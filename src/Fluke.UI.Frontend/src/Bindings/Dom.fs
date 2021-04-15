namespace Fluke.UI.Frontend.Bindings

open System.Collections.Generic
open Browser
open Fable.Core.JsInterop
open Fable.Core

[<AutoOpen>]
module Operators =
    [<Emit("Object.assign({}, $0, $1)")>]
    let (++) _o1 _o2 : obj = jsNative


module Dom =
    let private domRefs = Dictionary<string, obj> ()
    Dom.window?fluke <- domRefs
    let set key value = domRefs.[key] <- value

    [<Emit "new Event($0, $1)">]
    let createEvent _eventType _props = jsNative

    let newObj fn = jsOptions<_> fn
