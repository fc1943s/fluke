namespace Fluke.UI.Frontend.Bindings

open Fable.React
open Fable.Core.JsInterop

module Markdown =
    let private reactMarkdown : obj -> obj = importDefault "react-markdown"

    let render text =
        ReactBindings.React.createElement (
            reactMarkdown,
            {|  |},
            [
                str text
            ]
        )
