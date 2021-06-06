namespace Fluke.UI.Frontend.Tests.CellSelection

open Fable.ReactTestingLibrary
open Fable.Jester
open Feliz.Recoil
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared
open Microsoft.FSharp.Core.Operators
open Fluke.UI.Frontend.State


module SingleCellToggle =
    open CellSelectionSetup

    Jest.test (
        "single cell toggle",
        promise {
            let! cellMap, setter = initialize ()

            RTL.act (fun () -> setter.current().set (Atoms.ctrlPressed, true))

            do! click (getCell (cellMap, TaskName "2", FlukeDate.Create 2020 Month.January 9))

            do!
                [
                    taskIdByName cellMap "2",
                    set [
                        FlukeDate.Create 2020 Month.January 9
                    ]
                ]
                |> Map.ofList
                |> expectSelection setter

            do! click (getCell (cellMap, TaskName "2", FlukeDate.Create 2020 Month.January 9))

            do! [] |> Map.ofList |> expectSelection setter

            do! click (getCell (cellMap, TaskName "2", FlukeDate.Create 2020 Month.January 11))

            do!
                [
                    taskIdByName cellMap "2",
                    set [
                        FlukeDate.Create 2020 Month.January 11
                    ]
                ]
                |> Map.ofList
                |> expectSelection setter
        },
        20000
    )
