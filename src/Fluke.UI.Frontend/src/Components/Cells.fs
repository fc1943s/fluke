namespace Fluke.UI.Frontend.Components

open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Feliz
open Fluke.UI.Frontend.Bindings


module Cells =
    [<ReactComponent>]
    let Cells
        (input: {| Username: Username
                   TaskIdList: TaskId list |})
        =
        Profiling.addTimestamp "cells.render"

        Chakra.box
            (fun _ -> ())
            [
                yield!
                    input.TaskIdList
                    |> List.mapi
                        (fun i taskId ->
                            TaskCells.TaskCells
                                {|
                                    Username = input.Username
                                    TaskId = taskId
                                    Index = i
                                |})
            ]
