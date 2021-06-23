namespace Fluke.UI.Frontend.Components

open System
open Fable.React
open Feliz
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings


module PositionUpdater =

    [<ReactComponent>]
    let PositionUpdater (input: {| Username: Username |}) =
        Scheduling.useScheduling
            Scheduling.Interval
            1000
            (fun get set ->
                promise {
                    let selectedDatabaseIdSet = Atoms.getAtomValue get (Atoms.User.selectedDatabaseIdSet input.Username)

                    let selectedDatabasePositions =
                        selectedDatabaseIdSet
                        |> Set.toList
                        |> List.map (fun databaseId -> Atoms.Database.position (input.Username, databaseId))
                        |> List.map (Atoms.getAtomValue get)

                    let pausedPosition =
                        selectedDatabasePositions
                        |> List.choose id
                        |> List.tryHead

                    let position = Atoms.getAtomValue get Atoms.position

                    let newPosition =
                        match selectedDatabasePositions, pausedPosition with
                        | [], _ -> if position <> None then None else position
                        | _, None ->
                            let newPosition = FlukeDateTime.FromDateTime DateTime.Now

                            let isTesting = Atoms.getAtomValue get Atoms.isTesting
                            let position = Atoms.getAtomValue get Atoms.position

                            if (not isTesting || position.IsNone)
                               && Some newPosition <> position then
                                printfn $"Updating position newPosition={newPosition |> FlukeDateTime.Stringify}"
                                Atoms.setAtomValue set Atoms.position (Some newPosition)
                                Some newPosition
                            else
                                position
                        | _, Some _ ->
                            printfn
                                $"Updating position selectedDatabasePositions.[0]={
                                                                                       pausedPosition
                                                                                       |> Option.map
                                                                                           FlukeDateTime.Stringify
                                }"

                            pausedPosition

                    if position <> newPosition then
                        Atoms.setAtomValue set Atoms.position newPosition
                })

        nothing
