namespace Fluke.UI.Frontend.Bindings

open System.Collections.Generic
open Fable.Core.JsInterop
open Fable.Core

[<AutoOpen>]
module Operators =
    [<Emit("Object.assign({}, $0, $1)")>]
    let (++) _o1 _o2 : obj = jsNative


module Dom =
    let private domRefs = Dictionary<string, obj> ()
    Browser.Dom.window?fluke <- domRefs
    let set key value = domRefs.[key] <- value

    [<Emit "new Event($0, $1)">]
    let createEvent _eventType _props = jsNative

    let newObj fn = jsOptions<_> fn

    let resetZoom () =
        let viewport = Browser.Dom.window.document.querySelector "meta[name=viewport]"
        let initialContent : string = viewport?content
        printfn $"initial={initialContent}"
        viewport?content <- initialContent.Replace ("maximum-scale=10", "maximum-scale=1")

        JS.setTimeout (fun () -> viewport?content <- initialContent) 100
        |> ignore
