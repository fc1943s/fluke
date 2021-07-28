namespace FsUi.Components

open Feliz
open FsUi.Bindings


module Spinner =

    [<ReactComponent>]
    let Spinner props =
        UI.spinner
            (fun x ->
                x.size <- "xl"
                props x)
            []
