namespace Fluke.UI.Frontend.Tests.CellSelection

open Fable.ReactTestingLibrary
open Fable.Jester
open Feliz.Recoil
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared
open Microsoft.FSharp.Core.Operators
open Fluke.UI.Frontend.State


module BoxSelection =
    open CellSelectionSetup

    Jest.test (
        "box selection",
        promise {
            let! cellMap, setter = initialize ()

            RTL.act (fun () -> setter.current().set (Atoms.shiftPressed, true))


            do! click (getCell (cellMap, TaskName "2", FlukeDate.Create 2020 Month.January 9))
            do! click (getCell (cellMap, TaskName "3", FlukeDate.Create 2020 Month.January 10))

            do!
                [
                    taskIdByName cellMap "2",
                    set [
                        FlukeDate.Create 2020 Month.January 9
                        FlukeDate.Create 2020 Month.January 10
                    ]

                    taskIdByName cellMap "3",
                    set [
                        FlukeDate.Create 2020 Month.January 9
                        FlukeDate.Create 2020 Month.January 10
                    ]
                ]
                |> Map.ofList
                |> expectSelection setter


            do! click (getCell (cellMap, TaskName "4", FlukeDate.Create 2020 Month.January 11))

            do!
                [
                    taskIdByName cellMap "2",
                    set [
                        FlukeDate.Create 2020 Month.January 9
                        FlukeDate.Create 2020 Month.January 10
                        FlukeDate.Create 2020 Month.January 11
                    ]

                    taskIdByName cellMap "3",
                    set [
                        FlukeDate.Create 2020 Month.January 9
                        FlukeDate.Create 2020 Month.January 10
                        FlukeDate.Create 2020 Month.January 11
                    ]

                    taskIdByName cellMap "4",
                    set [
                        FlukeDate.Create 2020 Month.January 9
                        FlukeDate.Create 2020 Month.January 10
                        FlukeDate.Create 2020 Month.January 11
                    ]
                ]
                |> Map.ofList
                |> expectSelection setter

            do! click (getCell (cellMap, TaskName "1", FlukeDate.Create 2020 Month.January 8))

            do!
                [
                    taskIdByName cellMap "1",
                    set [
                        FlukeDate.Create 2020 Month.January 8
                        FlukeDate.Create 2020 Month.January 9
                        FlukeDate.Create 2020 Month.January 10
                        FlukeDate.Create 2020 Month.January 11
                    ]

                    taskIdByName cellMap "2",
                    set [
                        FlukeDate.Create 2020 Month.January 8
                        FlukeDate.Create 2020 Month.January 9
                        FlukeDate.Create 2020 Month.January 10
                        FlukeDate.Create 2020 Month.January 11
                    ]

                    taskIdByName cellMap "3",
                    set [
                        FlukeDate.Create 2020 Month.January 8
                        FlukeDate.Create 2020 Month.January 9
                        FlukeDate.Create 2020 Month.January 10
                        FlukeDate.Create 2020 Month.January 11
                    ]

                    taskIdByName cellMap "4",
                    set [
                        FlukeDate.Create 2020 Month.January 8
                        FlukeDate.Create 2020 Month.January 9
                        FlukeDate.Create 2020 Month.January 10
                        FlukeDate.Create 2020 Month.January 11
                    ]
                ]
                |> Map.ofList
                |> expectSelection setter
        }
    )
