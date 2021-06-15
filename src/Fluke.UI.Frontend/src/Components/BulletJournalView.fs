namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fable.DateFunctions
open Fluke.Shared.Domain
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module BulletJournalView =
    open Domain.UserInteraction

    [<ReactComponent>]
    let BulletJournalView (input: {| Username: Username |}) =
        let weekCellsMap = Store.useValue (Selectors.BulletJournalView.weekCellsMap input.Username)

        Chakra.box
            (fun x -> x.flex <- "1")
            [
                yield!
                    weekCellsMap
                    |> List.map
                        (fun week ->
                            Chakra.flex
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
                                                        |> List.filter
                                                            (fun cell ->
                                                                not cell.Sessions.IsEmpty
                                                                || not cell.Attachments.IsEmpty
                                                                || cell.Status <> State.Disabled)

                                                    Chakra.box
                                                        (fun x ->
                                                            x.flex <- "1"
                                                            x.paddingLeft <- "10px"
                                                            x.paddingRight <- "10px")
                                                        [
                                                            Chakra.box
                                                                (fun x ->
                                                                    x.visibility <-
                                                                        if visibleCells.IsEmpty then
                                                                            "hidden"
                                                                        else
                                                                            "visible"

                                                                    x.marginBottom <- "3px"
                                                                    x.borderBottomWidth <- "1px"
                                                                    x.borderBottomColor <- "#333"
                                                                    x.fontSize <- "14px"
                                                                    x.lineHeight <- "14px"

                                                                    x.color <-
                                                                        if cells |> List.forall (fun x -> x.IsToday) then
                                                                            "#777"
                                                                        else
                                                                            "")
                                                                [
                                                                    (referenceDay |> FlukeDate.DateTime)
                                                                        .Format "EEEE, dd MMM yyyy"
                                                                    |> String.toLower
                                                                    |> str
                                                                ]

                                                            yield!
                                                                visibleCells
                                                                |> List.map
                                                                    (fun cell ->
                                                                        Chakra.flex
                                                                            (fun _ -> ())
                                                                            [
                                                                                Cell.Cell
                                                                                    {|
                                                                                        Username = input.Username
                                                                                        DateId = dateId
                                                                                        TaskId = cell.TaskId
                                                                                        SemiTransparent = false
                                                                                    |}
                                                                                Chakra.box
                                                                                    (fun x -> x.paddingLeft <- "4px")
                                                                                    [
                                                                                        TaskName.TaskName
                                                                                            {|
                                                                                                Username =
                                                                                                    input.Username
                                                                                                TaskId = cell.TaskId
                                                                                            |}
                                                                                    ]
                                                                            ])
                                                        ])
                                ])
            ]
