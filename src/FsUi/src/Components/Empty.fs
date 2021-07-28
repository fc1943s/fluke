namespace FsUi.Components

open Fable.React
open Feliz
open FsUi.Bindings


module Empty =
    [<ReactComponent>]
    let Empty props =
        UI.box
            (fun x -> props x)
            [
                str ""
            ]
