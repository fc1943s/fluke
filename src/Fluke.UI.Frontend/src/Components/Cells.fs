namespace Fluke.UI.Frontend.Components

open Fluke.Shared.Domain.Model
open Feliz
open Fluke.UI.Frontend.Bindings


module Cells =
    [<ReactComponent>]
    let Cells (input: {| TaskIdList: TaskId list |}) =
        Chakra.box
            (fun _ -> ())
            [
                yield!
                    input.TaskIdList
                    |> List.mapi (fun i taskId -> TaskCells.TaskCells {| TaskId = taskId; Index = i |})
            ]
