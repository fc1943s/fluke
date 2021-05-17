namespace Fluke.UI.Frontend.Components

open System
open Fable.React
open Feliz
open Feliz.Recoil
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings


module PositionUpdater =

    [<ReactComponent>]
    let PositionUpdater (input: {| Username: Username |}) =
        let position, setPosition = Recoil.useState Atoms.position
        let selectedDatabaseIdList = Recoil.useValue (Atoms.User.selectedDatabaseIdList input.Username)

        let selectedDatabasePositions =
            selectedDatabaseIdList
            |> List.map (Some >> Atoms.Database.position)
            |> Recoil.waitForAll
            |> Recoil.useValue

        Scheduling.useScheduling
            Scheduling.Interval
            1000
            (fun _ ->
                promise {
                    let pausedPosition =
                        selectedDatabasePositions
                        |> List.choose id
                        |> List.tryHead

                    if selectedDatabasePositions.Length > 0 then
                        if pausedPosition.IsNone then
                            let newPosition = FlukeDateTime.FromDateTime DateTime.Now

                            if Some newPosition <> position then
                                printfn $"Updating position newPosition={newPosition |> FlukeDateTime.Stringify}"
                                setPosition (Some newPosition)
                        else if position <> pausedPosition then
                            printfn
                                $"Updating position selectedDatabasePositions.[0]={
                                                                                       pausedPosition
                                                                                       |> Option.map
                                                                                           FlukeDateTime.Stringify
                                }"

                            setPosition pausedPosition
                    elif position <> None then
                        setPosition None
                })

        nothing
