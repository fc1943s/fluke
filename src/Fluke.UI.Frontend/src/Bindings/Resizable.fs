namespace Fluke.UI.Frontend.Bindings

open Fable.React
open Fable.Core.JsInterop
open Feliz
open FsJs


module Resizable =
    let Resizable: obj -> obj = import "Resizable" "re-resizable"

    let inline resizable props children =
        if Dom.deviceInfo.IsMobile then
            React.fragment children
        else
            ReactBindings.React.createElement (Resizable, props, children)
