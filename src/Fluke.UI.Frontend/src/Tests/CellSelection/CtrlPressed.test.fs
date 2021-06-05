namespace Fluke.UI.Frontend.Tests.CellSelection

open Fable.ReactTestingLibrary
open Fable.Jester
open Feliz.Recoil
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared
open Microsoft.FSharp.Core.Operators
open Fluke.UI.Frontend.State


module CtrlPressed =
    open CellSelectionSetup

    Jest.test (
        "ctrl pressed",
        promise {
            let! cellMap, setter = initialize ()

            RTL.act (fun () -> setter.current().set (Atoms.ctrlPressed, true))

            do! click (getCell (cellMap, TaskName "2", FlukeDate.Create 2020 Month.January 9))
            do! click (getCell (cellMap, TaskName "2", FlukeDate.Create 2020 Month.January 11))

            do!
                [
                    taskIdByName cellMap "2",
                    set [
                        FlukeDate.Create 2020 Month.January 9
                        FlukeDate.Create 2020 Month.January 11
                    ]
                ]
                |> Map.ofList
                |> expectSelection setter
        },
        15000
    )
