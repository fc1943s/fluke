namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings


module Cells =
    [<ReactComponent>]
    let Cells taskIdAtoms =
        UI.box
            (fun _ -> ())
            [
                yield! taskIdAtoms |> Array.mapi TaskCells.TaskCells
            ]
