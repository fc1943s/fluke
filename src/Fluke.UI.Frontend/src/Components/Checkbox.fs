namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings


module Checkbox =
    [<ReactComponent>]
    let Checkbox
        (input: {| Label: string option
                   Props: Chakra.IChakraProps -> unit |})
        =
        Chakra.checkbox
            (fun x ->
                x.colorScheme <- "purple"
                x.size <- "lg"
                input.Props x)
            [
                match input.Label with
                | Some label ->
                    Chakra.box
                        (fun x -> x.fontSize <- "main")
                        [
                            str label
                        ]
                | _ -> nothing
            ]
