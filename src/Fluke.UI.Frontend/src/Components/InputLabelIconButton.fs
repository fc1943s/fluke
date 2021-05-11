namespace Fluke.UI.Frontend.Components

open Fluke.UI.Frontend.Bindings


module InputLabelIconButton =
    let inline InputLabelIconButton (input: {| Props: Chakra.IChakraProps -> unit |}) =
        Chakra.iconButton
            (fun x ->
                x.border <- "0"
                x.color <- "heliotrope"
                x.marginLeft <- "3px"
                x.marginTop <- "-2px"
                x.padding <- "2px"
                x.minWidth <- "0"
                x.width <- "auto"
                x.height <- "auto"
                input.Props x)
            []
