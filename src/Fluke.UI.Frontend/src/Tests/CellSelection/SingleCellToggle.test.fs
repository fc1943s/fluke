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
            let! cellMapGetter, setter = initialize ()

            RTL.act (fun () -> setter.current().set (Atoms.ctrlPressed, true))

            do! click (getCell (cellMapGetter, TaskName "2", FlukeDate.Create 2020 Month.January 9))

            let! taskId_2 = taskIdByName cellMapGetter "2"

            do!
                [
                    taskId_2,
                    set [
                        FlukeDate.Create 2020 Month.January 9
                    ]
                ]
                |> Map.ofList
                |> expectSelection setter

            do! click (getCell (cellMapGetter, TaskName "2", FlukeDate.Create 2020 Month.January 9))

            do! [] |> Map.ofList |> expectSelection setter

            do! click (getCell (cellMapGetter, TaskName "2", FlukeDate.Create 2020 Month.January 11))

            do!
                [
                    taskId_2,
                    set [
                        FlukeDate.Create 2020 Month.January 11
                    ]
                ]
                |> Map.ofList
                |> expectSelection setter
        },
        maxTimeout
    )
