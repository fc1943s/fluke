namespace Fluke.UI.Frontend.Components

open FsStore.State
open System
open Fable.React
open Feliz
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.State
open FsCore.BaseModel
open FsStore
open FsStore.Hooks
open FsUi.Hooks
open FsJs
open FsCore
open Fluke.Shared
open Fable.Core.JsInterop


module PositionUpdater =
    [<ReactComponent>]
    let PositionUpdater () =
        let deviceInfo = Store.useValue Selectors.deviceInfo

        Scheduling.useScheduling
            Scheduling.Interval
            5000
            (fun getter setter ->
                promise {
                    let hub = Store.value getter Selectors.Hub.hub
                    let hubUrl = Store.value getter Atoms.hubUrl

                    match hubUrl with
                    | Some (String.Valid _) ->
                        match hub with
                        | Some hub when hub.connectionId = None ->
                            printfn
                                $"position timer. hub.connectionId={hub.connectionId}. triggering hubTrigger **skipped** window.hubTrigger()"

                            match Dom.window () with
                            | Some window -> window?hubTrigger <- fun () -> Store.change setter Atoms.hubTrigger ((+) 1)
                            | None -> ()
                        //                            Store.change setter Atoms.hubTrigger ((+) 1)
                        | _ -> ()
                    | _ -> ()

                    Store.set setter (Atoms.Device.devicePing deviceInfo.DeviceId) (Ping (string DateTime.Now.Ticks))
                })

        Scheduling.useScheduling
            Scheduling.Interval
            1000
            (fun getter setter ->
                promise {
                    let selectedDatabaseIdSet = Store.value getter Atoms.User.selectedDatabaseIdSet

                    let selectedDatabasePositions =
                        selectedDatabaseIdSet
                        |> Set.toList
                        |> List.map Atoms.Database.position
                        |> List.map (Store.value getter)

                    let pausedPosition =
                        selectedDatabasePositions
                        |> List.choose id
                        |> List.tryHead

                    let position = Store.value getter Atoms.Session.position

                    let newPosition =
                        match selectedDatabasePositions, pausedPosition with
                        | [], _ -> if position <> None then None else position
                        | _, None ->
                            let newPosition =
                                { FlukeDateTime.FromDateTime DateTime.Now with
                                    Second = Second 0
                                }

                            let position = Store.value getter Atoms.Session.position

                            if (not deviceInfo.IsTesting || position.IsNone)
                               && Some newPosition <> position then
                                Some newPosition
                            else
                                position
                        | _, Some _ -> pausedPosition

                    //                    printfn $"PositionUpdater
//                        selectedDatabaseIdSet={selectedDatabaseIdSet}
//                        selectedDatabasePositions={selectedDatabasePositions}
//                        pausedPosition={pausedPosition}
//                        position={position}
//                        newPosition={newPosition}
//                    "

                    if position <> newPosition then
                        printfn $"Updating position newPosition={newPosition |> Option.map FlukeDateTime.Stringify}"
                        Store.set setter Atoms.Session.position newPosition
                })

        nothing
