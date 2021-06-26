namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings


module Spinner =

    [<ReactComponent>]
    let Spinner (input: {| Props: Chakra.IChakraProps -> unit |}) =
        Chakra.spinner
            (fun x ->
                x.size <- "xl"
                input.Props x)
            []
