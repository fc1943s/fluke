namespace Fluke.UI.Frontend.Components

open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain.State
open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module Cells =
    [<ReactComponent>]
    let Cells
        (input: {| Username: Username
                   TaskIdList: TaskId list |})
        =
        Profiling.addTimestamp "cells.render"

        let dateSequence = Recoil.useValue Recoil.Selectors.dateSequence

        Chakra.box
            {|  |}
            [
                yield!
                    input.TaskIdList
                    |> List.mapi
                        (fun i taskId ->
                            Chakra.flex
                                {|  |}
                                [
                                    yield!
                                        dateSequence
                                        |> List.map
                                            (fun date ->
                                                Cell.Cell
                                                    {|
                                                        Username = input.Username
                                                        TaskId = taskId
                                                        DateId = DateId date
                                                        SemiTransparent = i % 2 <> 0
                                                    |})
                                ])
            ]
