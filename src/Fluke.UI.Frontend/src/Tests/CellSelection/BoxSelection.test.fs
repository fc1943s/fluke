namespace Fluke.UI.Frontend.Tests.CellSelection

open Fable.ReactTestingLibrary
open Fable.Jester
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared
open Microsoft.FSharp.Core.Operators
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings; open FsStore; open FsUi.Bindings


module BoxSelection =
    open CellSelectionSetup

    Jest.test (
        "box selection",
        promise {
            let! cellMapGetter, (get, setFn) = initialize ()

            RTL.act (fun () -> Store.set setFn Atoms.Session.shiftPressed true)

            do! click (getCell (cellMapGetter, TaskName "2", FlukeDate.Create 2020 Month.January 9))
            do! click (getCell (cellMapGetter, TaskName "3", FlukeDate.Create 2020 Month.January 10))

            let! taskId_1 = taskIdByName cellMapGetter "1"
            let! taskId_2 = taskIdByName cellMapGetter "2"
            let! taskId_3 = taskIdByName cellMapGetter "3"
            let! taskId_4 = taskIdByName cellMapGetter "4"

            do!
                [
                    taskId_2,
                    set [
                        FlukeDate.Create 2020 Month.January 9
                        FlukeDate.Create 2020 Month.January 10
                    ]

                    taskId_3,
                    set [
                        FlukeDate.Create 2020 Month.January 9
                        FlukeDate.Create 2020 Month.January 10
                    ]
                ]
                |> Map.ofSeq
                |> expectSelection get


            do! click (getCell (cellMapGetter, TaskName "4", FlukeDate.Create 2020 Month.January 11))

            do!
                [
                    taskId_2,
                    set [
                        FlukeDate.Create 2020 Month.January 9
                        FlukeDate.Create 2020 Month.January 10
                        FlukeDate.Create 2020 Month.January 11
                    ]

                    taskId_3,
                    set [
                        FlukeDate.Create 2020 Month.January 9
                        FlukeDate.Create 2020 Month.January 10
                        FlukeDate.Create 2020 Month.January 11
                    ]

                    taskId_4,
                    set [
                        FlukeDate.Create 2020 Month.January 9
                        FlukeDate.Create 2020 Month.January 10
                        FlukeDate.Create 2020 Month.January 11
                    ]
                ]
                |> Map.ofSeq
                |> expectSelection get

            do! click (getCell (cellMapGetter, TaskName "1", FlukeDate.Create 2020 Month.January 8))

            do!
                [
                    taskId_1,
                    set [
                        FlukeDate.Create 2020 Month.January 8
                        FlukeDate.Create 2020 Month.January 9
                        FlukeDate.Create 2020 Month.January 10
                        FlukeDate.Create 2020 Month.January 11
                    ]

                    taskId_2,
                    set [
                        FlukeDate.Create 2020 Month.January 8
                        FlukeDate.Create 2020 Month.January 9
                        FlukeDate.Create 2020 Month.January 10
                        FlukeDate.Create 2020 Month.January 11
                    ]

                    taskId_3,
                    set [
                        FlukeDate.Create 2020 Month.January 8
                        FlukeDate.Create 2020 Month.January 9
                        FlukeDate.Create 2020 Month.January 10
                        FlukeDate.Create 2020 Month.January 11
                    ]

                    taskId_4,
                    set [
                        FlukeDate.Create 2020 Month.January 8
                        FlukeDate.Create 2020 Month.January 9
                        FlukeDate.Create 2020 Month.January 10
                        FlukeDate.Create 2020 Month.January 11
                    ]
                ]
                |> Map.ofSeq
                |> expectSelection get
        },
        maxTimeout
    )
