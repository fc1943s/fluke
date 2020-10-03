namespace Fluke.UI.Frontend.Bindings

open System.Collections.Generic
open Browser
open Fable.Core.JsInterop


module Dom =
    let private domRefs = Dictionary<string, obj> ()
    Dom.window?fluke <- domRefs
    let set key value = domRefs.[key] <- value
