namespace Fluke.UI.Frontend.Components

open System
open Fable.React
open Feliz
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.State
open FsStore
open FsUi.Hooks


module PositionUpdater =
    [<ReactComponent>]
    let PositionUpdater () =
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

                            let isTesting = Store.value getter Atoms.isTesting
                            let position = Store.value getter Atoms.Session.position

                            if (not isTesting || position.IsNone)
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
