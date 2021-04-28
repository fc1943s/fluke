namespace Fluke.UI.Frontend.Components

open Fluke.UI.Frontend.Bindings


module TransparentIconButton =
    let inline TransparentIconButton (input: {| Props: Chakra.IChakraProps |}) =
        Chakra.iconButton
            (fun x ->
                x.backgroundColor <- "transparent"
                x.variant <- "outline"
                x.border <- "0"
                x.width <- "30px"
                x.height <- "30px"
                x.borderRadius <- "0"
                x.textAlign <- "-webkit-center"
                x <+ input.Props)
            []
