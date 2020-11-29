namespace Fluke.UI.Frontend.Components

open FSharpPlus
open Fluke.Shared.Domain.UserInteraction
open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module Cells =

    [<ReactComponent>]
    let cells (input: {| Username: Username
                         TaskIdList: Recoil.Atoms.Task.TaskId list |}) =
        Profiling.addTimestamp "cells.render"

        let dateSequence = Recoil.useValue Recoil.Selectors.dateSequence

        Chakra.box
            {|  |}
            [
                yield!
                    input.TaskIdList
                    |> List.mapi (fun i taskId ->
                        Chakra.flex
                            {|  |}
                            [
                                yield!
                                    dateSequence
                                    |> List.map (fun date ->
                                        Cell.cell
                                            {|
                                                Username = input.Username
                                                TaskId = taskId
                                                DateId = DateId date
                                                SemiTransparent = i % 2 <> 0
                                            |})
                            ])
            ]
