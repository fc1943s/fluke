namespace Fluke.UI.Frontend.Bindings

open Fable.React
open Fable.Core.JsInterop
open Fable.Core
open Feliz


module Resizable =
    let private Resizable : obj -> obj = import "Resizable" "re-resizable"

    let resizable props children =
        if JS.deviceInfo.IsMobile then
            React.fragment children
        else
            ReactBindings.React.createElement (Resizable, props, children)
