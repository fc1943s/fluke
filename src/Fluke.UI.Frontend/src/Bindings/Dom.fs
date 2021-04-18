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
        if false then
            let viewport = Browser.Dom.window.document.querySelector "meta[name=viewport]"
            let initialContent : string = viewport?content

            printfn
                $"initial={initialContent}
            devicePixelRatio={Browser.Dom.window.devicePixelRatio}
            availWidth/clientWidth={
                                        Browser.Dom.window.screen.availWidth
                                        / Browser.Dom.document.documentElement.clientWidth
                }
            "

            viewport?content <- initialContent.Replace ("initial-scale=1, maximum-scale=10", "user-scalable=no")

            JS.setTimeout (fun () -> viewport?content <- initialContent) 1000
            |> ignore
