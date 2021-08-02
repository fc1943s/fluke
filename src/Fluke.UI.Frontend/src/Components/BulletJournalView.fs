namespace Fluke.UI.Frontend.Components

open FsJs
open Fable.React
open Feliz
open Fluke.Shared.Domain
open Fluke.UI.Frontend.State
open FsStore
open FsUi.Bindings
open Fluke.Shared


module BulletJournalView =
    open Domain.UserInteraction

    [<ReactComponent>]
    let BulletJournalView () =
        let weekCellsMap = Store.useValue Selectors.BulletJournalView.bulletJournalWeekCellsMap

        UI.box
            (fun x -> x.flex <- "1")
            [
                yield!
                    weekCellsMap
                    |> List.map
                        (fun week ->
                            UI.flex
                                (fun x ->
                                    x.flex <- "1"
                                    x.marginTop <- "15px"
                                    x.marginBottom <- "15px")
                                [
                                    yield!
                                        week
                                        |> Map.keys
                                        |> Seq.map
                                            (fun dateId ->
                                                match dateId with
                                                | DateId referenceDay as dateId ->
                                                    let cells = week.[dateId]

                                                    let visibleCells =
                                                        cells
                                                        |> Array.filter
                                                            (fun cell ->
                                                                not cell.SessionList.IsEmpty
                                                                || not cell.AttachmentStateList.IsEmpty
                                                                || cell.Status <> State.Disabled)

                                                    UI.box
                                                        (fun x ->
                                                            x.flex <- "1"
                                                            x.paddingLeft <- "10px"
                                                            x.paddingRight <- "10px")
                                                        [
                                                            UI.box
                                                                (fun x ->
                                                                    x.visibility <-
                                                                        if visibleCells.Length = 0 then
                                                                            "hidden"
                                                                        else
                                                                            "visible"

                                                                    x.marginBottom <- "3px"
                                                                    x.borderBottomWidth <- "1px"
                                                                    x.borderBottomColor <- "gray.16"
                                                                    x.fontSize <- "14px"
                                                                    x.lineHeight <- "14px"

                                                                    x.color <-
                                                                        if cells |> Array.forall (fun x -> x.IsToday) then
                                                                            "gray.45"
                                                                        else
                                                                            "")
                                                                [
                                                                    referenceDay
                                                                    |> FlukeDate.DateTime
                                                                    |> DateTime.format "EEEE, dd MMM yyyy"
                                                                    |> String.toLower
                                                                    |> str
                                                                ]

                                                            yield!
                                                                visibleCells
                                                                |> Array.map
                                                                    (fun cell ->
                                                                        UI.flex
                                                                            (fun _ -> ())
                                                                            [
                                                                                Cell.Cell
                                                                                    {|
                                                                                        TaskIdAtom = cell.TaskIdAtom
                                                                                        DateIdAtom = cell.DateIdAtom
                                                                                        SemiTransparent = false
                                                                                    |}
                                                                                UI.box
                                                                                    (fun x -> x.paddingLeft <- "4px")
                                                                                    [
                                                                                        TaskName.TaskName
                                                                                            cell.TaskIdAtom
                                                                                    ]
                                                                            ])
                                                        ])
                                ])
            ]
