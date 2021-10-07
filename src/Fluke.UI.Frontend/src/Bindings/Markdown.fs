namespace Fluke.UI.Frontend.Bindings

open Fable.Core.JsInterop
open Fable.React
open Feliz
open FsUi.Components


module Markdown =
    let reactMarkdown: obj -> obj = importDefault "react-markdown"

    let gfm: obj = importDefault "remark-gfm"

    let inline render text =
        ReactBindings.React.createElement (
            reactMarkdown,
            {|
                className = "markdown-container"
                components =
                    {|
                        a =
                            fun (props: {| children: seq<ReactElement>
                                           href: string |}) ->
                                ExternalLink.ExternalLink
                                    {|
                                        Link = React.fragment props.children
                                        Href = props.href
                                        Props = fun _ -> ()
                                    |}
                    |}
                remarkPlugins =
                    [|
                        [|
                            unbox gfm
                            unbox {|  |}
                        |]
                    |]
            |},
            [
                str text
            ]
        )
