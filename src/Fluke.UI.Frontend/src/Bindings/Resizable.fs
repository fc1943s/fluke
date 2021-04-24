namespace Fluke.UI.Frontend.Bindings

open Fable.React
open Fable.Core.JsInterop


module Resizable =
    let private Resizable : obj -> obj = import "Resizable" "re-resizable"

    let resizable props children =
        ReactBindings.React.createElement (Resizable, props, children)
