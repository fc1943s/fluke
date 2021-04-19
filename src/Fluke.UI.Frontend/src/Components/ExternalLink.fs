namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings
open Fable.React


module ExternalLink =
    [<ReactComponent>]
    let ExternalLink
        (input: {| Text: string
                   Props: Chakra.IChakraProps |})
        =
        Chakra.link
            (fun x -> x <+ input.Props)
            [
                str input.Text
                Chakra.Icons.externalLinkIcon
                    (fun x ->
                        x.marginLeft <- "3px"
                        x.marginTop <- "-1px")
                    []
            ]
