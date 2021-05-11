namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings
open Fable.React


module ExternalLink =
    let externalLinkIcon =
        Chakra.Icons.externalLinkIcon
            (fun x ->
                x.marginLeft <- "3px"
                x.marginTop <- "-1px")
            []

    [<ReactComponent>]
    let ExternalLink
        (input: {| Link: ReactElement
                   Href: string
                   Props: Chakra.IChakraProps -> unit |})
        =
        Tooltip.wrap
            (str input.Href)
            [
                Chakra.link
                    (fun x ->
                        x.href <- input.Href
                        x.isExternal <- true
                        input.Props x)
                    [
                        input.Link
                        externalLinkIcon
                    ]
            ]
