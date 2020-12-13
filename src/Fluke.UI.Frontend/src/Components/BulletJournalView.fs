namespace Fluke.UI.Frontend.Components

open FSharpPlus
open Fable.React
open Feliz
open Feliz.Recoil
open Fable.DateFunctions
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module BulletJournalView =
    open Domain.UserInteraction

    [<ReactComponent>]
    let BulletJournalView (input: {| Username: Username |}) =
        let weekCellsMap = Recoil.useValue (Recoil.Selectors.Session.weekCellsMap input.Username)

        Chakra.box
            ()
            [
                yield!
                    weekCellsMap
                    |> List.map (fun week ->
                        Chakra.flex
                            {| marginTop = "15px"; marginBottom = "15px" |}
                            [
                                yield!
                                    week
                                    |> Map.keys
                                    |> Seq.map (fun ((DateId referenceDay) as dateId) ->
                                        let cells = week.[dateId]

                                        Chakra.box
                                            {| paddingLeft = "10px"; paddingRight = "10px" |}
                                            [
                                                Chakra.box
                                                    {|
                                                        marginBottom = "3px"
                                                        borderBottom = "1px solid #333"
                                                        fontSize = "14px"
                                                        color =
                                                            if cells |> List.forall (fun x -> x.IsToday) then
                                                                "#777"
                                                            else
                                                                ""
                                                    |}
                                                    [
                                                        referenceDay.DateTime.Format "EEEE, dd MMM yyyy"
                                                        |> String.toLower
                                                        |> str
                                                    ]


                                                yield!
                                                    cells
                                                    |> List.map (fun cell ->
                                                        Chakra.flex
                                                            ()
                                                            [
                                                                Cell.Cell
                                                                    {|
                                                                        Username = input.Username
                                                                        DateId = dateId
                                                                        TaskId = cell.TaskId
                                                                        SemiTransparent = false
                                                                    |}
                                                                Chakra.box
                                                                    {| paddingLeft = "4px" |}
                                                                    [
                                                                        TaskName.TaskName {| TaskId = cell.TaskId |}
                                                                    ]
                                                            ])
                                            ])
                            ])
            ]
