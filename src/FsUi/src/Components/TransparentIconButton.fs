namespace FsUi.Components

open FsUi.Bindings


module TransparentIconButton =
    let inline TransparentIconButton (input: {| Props: UI.IChakraProps -> unit |}) =
        UI.iconButton
            (fun x ->
                x.backgroundColor <- "transparent"
                x.variant <- "outline"
                x.border <- "0"
                x.width <- "30px"
                x.height <- "30px"
                x.borderRadius <- "0"
                x.textAlign <- "-webkit-center"
                input.Props x)
            []
