namespace Fluke.UI.Frontend.Bindings

open Fable.React
open Fable.Core.JsInterop

module Markdown =
    let private reactMarkdown : obj -> obj = importDefault "react-markdown"

    let gfm : obj = importDefault "remark-gfm"

    let render text =
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
