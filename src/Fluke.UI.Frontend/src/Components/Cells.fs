namespace Fluke.UI.Frontend.Components

open Fluke.Shared.Domain.Model
open Feliz
open Fluke.UI.Frontend.Bindings


module Cells =
    [<ReactComponent>]
    let Cells (taskIdList: TaskId list) =
        Chakra.box
            (fun _ -> ())
            [
                yield! taskIdList |> List.mapi TaskCells.TaskCells
            ]
