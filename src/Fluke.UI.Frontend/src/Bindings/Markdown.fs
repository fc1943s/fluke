namespace Fluke.UI.Frontend.Bindings

open Fable.React
open Fable.Core.JsInterop


module Markdown =
    let reactMarkdown: obj -> obj = importDefault "react-markdown"

    let gfm: obj = importDefault "remark-gfm"

    let inline render text =
        ReactBindings.React.createElement (
            reactMarkdown,
            {|
                className = "markdown-container"
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
