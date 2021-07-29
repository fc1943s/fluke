namespace FsUi.Components

open FsUi.Bindings


module Spinner =
    let inline Spinner props =
        UI.spinner
            (fun x ->
                x.size <- "xl"
                props x)
            []
