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
    let PositionUpdater () =
        let position, setPosition = Recoil.useState Atoms.position
        let selectedDatabaseIds = Recoil.useValue Atoms.selectedDatabaseIds

        let selectedDatabasePositions =
            selectedDatabaseIds
            |> Array.map (Some >> Atoms.Database.position)
            |> Recoil.waitForAll
            |> Recoil.useValue

        Scheduling.useScheduling
            Scheduling.Interval
            1000
            (fun _ ->
                promise {
                    let newPosition = FlukeDateTime.FromDateTime DateTime.Now

                    if Some newPosition <> position
                       && selectedDatabasePositions.Length > 0 then
                        if selectedDatabasePositions
                           |> Array.exists Option.isSome
                           |> not then
                            printfn $"Updating position newPosition={newPosition.Stringify ()}"
                            setPosition (Some newPosition)
                        else
                            printfn
                                $"Updating position selectedDatabasePositions.[0]={
                                                                                       selectedDatabasePositions.[0]
                                                                                       |> Option.map
                                                                                           (fun x -> x.Stringify ())
                                }"

                            setPosition selectedDatabasePositions.[0]
                    else
                        if position <> None then setPosition None

                        printfn
                            $"Skipping position update. position={position |> Option.map (fun x -> x.Stringify ())} newPosition={
                                                                                                                                     newPosition.Stringify
                                                                                                                                         ()
                            } selected={selectedDatabaseIds}"
                })

        nothing
