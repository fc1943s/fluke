namespace FsUi.Components

open FsUi.Bindings


module InputLabelIconButton =
    let inline InputLabelIconButton props =
        UI.iconButton
            (fun x ->
                x.border <- "0"
                x.color <- "heliotrope"
                x.marginLeft <- "3px"
                x.marginTop <- "-2px"
                x.padding <- "2px"
                x.minWidth <- "0"
                x.width <- "auto"
                x.height <- "auto"
                props x)
            []
