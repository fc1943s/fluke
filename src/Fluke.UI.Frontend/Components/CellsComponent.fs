namespace Fluke.UI.Frontend.Components

open FSharpPlus
open Suigetsu.Core
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend


module CellsComponent =
    let render =
        React.memo (fun (input: {| TaskIdList: Recoil.Atoms.RecoilTask.TaskId list |}) ->
            Recoil.Profiling.addTimestamp "cells.render"

            let dateSequence = Recoil.useValue Recoil.Selectors.dateSequence

            Html.div [
                prop.className Css.laneContainer
                prop.children
                    [
                        yield! input.TaskIdList
                               |> List.map (fun taskId ->
                                   Html.div
                                       [
                                           yield! dateSequence
                                                  |> List.map (fun date ->
                                                      CellComponent.render {| TaskId = taskId; Date = date |})
                                       ])
                    ]
            ])
