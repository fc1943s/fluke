namespace Fluke.UI.Frontend.Components

open Feliz
open FsUi.Bindings


module Cells =
    [<ReactComponent>]
    let Cells taskIdAtoms =
        Ui.box
            (fun _ -> ())
            [
                yield! taskIdAtoms |> Array.mapi TaskCells.TaskCells
            ]
