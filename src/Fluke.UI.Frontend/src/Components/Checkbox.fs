namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings


module Checkbox =
    [<ReactComponent>]
    let Checkbox (input: {| Props: Chakra.IChakraProps -> unit |}) =
        Chakra.checkbox
            (fun x ->
                x.colorScheme <- "purple"
                input.Props x)
            []
