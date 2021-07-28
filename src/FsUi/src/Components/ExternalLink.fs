namespace FsUi.Components

open FsUi.Bindings
open Fable.React


module ExternalLink =
    let externalLinkIcon =
        Icons.bi.BiLinkExternal
        |> Icons.renderWithProps
            (fun x ->
                x.display <- "inline"
                x.marginLeft <- "3px"
                x.marginTop <- "-2px")

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
