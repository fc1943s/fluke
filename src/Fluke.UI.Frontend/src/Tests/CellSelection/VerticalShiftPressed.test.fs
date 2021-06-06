namespace Fluke.UI.Frontend.Tests.CellSelection

open Fable.ReactTestingLibrary
open Fable.Jester
open Feliz.Recoil
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared
open Microsoft.FSharp.Core.Operators
open Fluke.UI.Frontend.State


module VerticalShiftPressed =
    open CellSelectionSetup

    Jest.test (
        "cell selection - vertical shift pressed",
        promise {
            let! cellMap, setter = initialize ()

            RTL.act (fun () -> setter.current().set (Atoms.shiftPressed, true))

            do! click (getCell (cellMap, TaskName "2", FlukeDate.Create 2020 Month.January 9))

            do! click (getCell (cellMap, TaskName "3", FlukeDate.Create 2020 Month.January 9))

            do!
                [
                    taskIdByName cellMap "2",
                    set [
                        FlukeDate.Create 2020 Month.January 9
                    ]

                    taskIdByName cellMap "3",
                    set [
                        FlukeDate.Create 2020 Month.January 9
                    ]
                ]
                |> Map.ofList
                |> expectSelection setter
        },
        maxTimeout
    )
