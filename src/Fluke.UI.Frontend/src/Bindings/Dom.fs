namespace Fluke.UI.Frontend.Bindings

open System.Collections.Generic
open Fable.Core.JsInterop
open Fable.Core


module Dom =
    let private domRefs = Dictionary<string, obj> ()

    if jsTypeof Browser.Dom.window <> "undefined" then
        Browser.Dom.window?domRefs <- domRefs

    let set key value = domRefs.[key] <- value

    [<Emit "new Event($0, $1)">]
    let inline createEvent _eventType _props = jsNative
