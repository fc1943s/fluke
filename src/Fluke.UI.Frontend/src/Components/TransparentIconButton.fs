namespace Fluke.UI.Frontend.Components

open Fluke.UI.Frontend.Bindings


module TransparentIconButton =
    let inline TransparentIconButton (input: {| Props: Chakra.IChakraProps |}) =
        Chakra.iconButton
            (fun x ->
                x <+ input.Props
                x.backgroundColor <- "transparent"
                x.variant <- "outline"
                x.border <- "0"
                x.fontSize <- "18px"
                x.width <- "30px"
                x.height <- "30px"
                x.borderRadius <- "0")
            []
