namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings
open Fable.React


module ExternalLink =
    [<ReactComponent>]
    let ExternalLink
        (input: {| href: string
                   isExternal: bool
                   text: string |})
        =
        Chakra.link
            {|
                isExternal = input.isExternal
                href = input.href
            |}
            [
                str input.text
                Chakra.Icons.externalLinkIcon
                    {|
                        marginLeft = "3px"
                        marginTop = "-1px"
                    |}
                    []
            ]
