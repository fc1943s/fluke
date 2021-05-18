namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Fable.DateFunctions
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module BulletJournalView =
    open Domain.UserInteraction

    [<ReactComponent>]
    let BulletJournalView (input: {| Username: Username |}) =
        let weekCellsMap = Recoil.useValue (Selectors.BulletJournalView.weekCellsMap input.Username)

        Chakra.box
            (fun _ -> ())
            [
                yield!
                    weekCellsMap
                    |> List.map
                        (fun week ->
                            Chakra.flex
                                (fun x ->
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

                                                    Chakra.box
                                                        (fun x ->
                                                            x.paddingLeft <- "10px"
                                                            x.paddingRight <- "10px")
                                                        [
                                                            Chakra.box
                                                                (fun x ->
                                                                    x.marginBottom <- "3px"
                                                                    x.borderBottomWidth <- "1px"
                                                                    x.borderBottomColor <- "#333"
                                                                    x.fontSize <- "14px"

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
                                                                cells
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
                                                                                            {| TaskId = cell.TaskId |}
                                                                                    ]
                                                                            ])
                                                        ])
                                ])
            ]
