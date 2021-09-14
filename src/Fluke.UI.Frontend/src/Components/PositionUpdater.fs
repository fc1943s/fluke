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
        let deviceInfo = Store.useValue Selectors.Store.deviceInfo

        let hubCount, setHubCount = React.useState 0

        Scheduling.useScheduling
            Scheduling.Interval
            5000
            (fun getter setter ->
                promise {
                    let hub = Atom.get getter Selectors.Hub.hub
                    let hubUrl = Atom.get getter Atoms.hubUrl

                    match hubUrl with
                    | Some (String.Valid _) ->
                        match hub with
                        | Some hub when hub.connectionId = None ->
                            printfn
                                $"position timer. hub.connectionId={hub.connectionId}. triggering hubTrigger **skipped** window.hubTrigger()"

                            match Dom.window () with
                            | Some window -> window?hubTrigger <- fun () -> Atom.change setter Atoms.hubTrigger ((+) 1)
                            | None -> ()

                            if hubCount = 3 then
                                Atom.change setter Atoms.hubTrigger ((+) 1)
                                setHubCount 0
                            else
                                setHubCount (hubCount + 1)
                        | _ -> ()
                    | _ -> ()

                    Atom.set setter (Atoms.Device.devicePing deviceInfo.DeviceId) (Ping (string DateTime.Now.Ticks))
                })

        Scheduling.useScheduling
            Scheduling.Interval
            1000
            (fun getter setter ->
                promise {
                    let selectedDatabaseIdSet = Atom.get getter Atoms.User.selectedDatabaseIdSet

                    let selectedDatabasePositions =
                        selectedDatabaseIdSet
                        |> Set.toList
                        |> List.map Atoms.Database.position
                        |> List.map (Atom.get getter)

                    let pausedPosition =
                        selectedDatabasePositions
                        |> List.choose id
                        |> List.tryHead

                    let position = Atom.get getter Atoms.Session.position

                    let newPosition =
                        match selectedDatabasePositions, pausedPosition with
                        | [], _ -> if position <> None then None else position
                        | _, None ->
                            let newPosition =
                                { FlukeDateTime.FromDateTime DateTime.Now with
                                    Second = Second 0
                                }

                            let position = Atom.get getter Atoms.Session.position

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
                        Atom.set setter Atoms.Session.position newPosition
                })

        nothing
