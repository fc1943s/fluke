namespace Fluke.UI.Frontend.Tests.CellSelection

open Fable.ReactTestingLibrary
open Fable.Jester
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared
open Microsoft.FSharp.Core.Operators
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings; open FsStore; open FsUi.Bindings


module HorizontalShiftPressed =
    open CellSelectionSetup

    Jest.test (
        "cell selection - horizontal shift pressed",
        promise {
            let! cellMapGetter, (get, setFn) = initialize ()

            RTL.act (fun () -> Store.set setFn Atoms.Session.shiftPressed true)

            do! click (getCell (cellMapGetter, TaskName "2", FlukeDate.Create 2020 Month.January 9))
            do! click (getCell (cellMapGetter, TaskName "2", FlukeDate.Create 2020 Month.January 11))

            let! taskId_2 = taskIdByName cellMapGetter "2"

            do!
                [
                    taskId_2,
                    set [
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
