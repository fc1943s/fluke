namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings


module Checkbox =
    [<ReactComponent>]
    let Checkbox (label: string option) (props: Chakra.IChakraProps -> unit) =
        Chakra.checkbox
            (fun x ->
                x.colorScheme <- "purple"
                x.size <- "lg"
                props x)
            [
                match label with
                | Some label ->
                    Chakra.box
                        (fun x -> x.fontSize <- "main")
                        [
                            str label
                        ]
                | _ -> nothing
            ]
