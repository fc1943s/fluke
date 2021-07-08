namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings
open Fable.React


module ExternalLink =
    let externalLinkIcon =
        UI.Icons.externalLinkIcon
            (fun x ->
                x.marginLeft <- "3px"
                x.marginTop <- "-1px")
            []

    let inline ExternalLink
        (input: {| Link: ReactElement
                   Href: string
                   Props: UI.IChakraProps -> unit |})
        =
        Tooltip.wrap
            (str input.Href)
            [
                UI.link
                    (fun x ->
                        x.href <- input.Href
                        x.isExternal <- true
                        input.Props x)
                    [
                        input.Link
                        externalLinkIcon
                    ]
            ]
