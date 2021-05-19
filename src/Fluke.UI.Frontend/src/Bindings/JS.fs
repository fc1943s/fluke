namespace Fluke.UI.Frontend.Bindings

open Fable.Core
open Fable.Core.JsInterop


[<AutoOpen>]
module Operators =
    [<Emit("Object.assign({}, $0, $1)")>]
    let (++) _o1 _o2 : obj = jsNative

    [<Emit("Object.assign($0, $1)")>]
    let (<+) _o1 _o2 : unit = jsNative

module JS =
    let isProduction = Browser.Dom.window.location.host.EndsWith "github.io"

    [<Emit "process.env.JEST_WORKER_ID">]
    let jestWorkerId : bool = jsNative

    let isTesting = jestWorkerId || Browser.Dom.window?Cypress <> null

    let inline log fn =
        if not isProduction && not isTesting then
            printfn $"[log] {fn ()}"
        else
            ()

    [<Emit "(w => $0 instanceof w[$1])(window)">]
    let instanceof (_obj: obj, _typeName: string) : bool = jsNative

    [<Emit "(() => { var audio = new Audio($0); audio.volume = 0.5; return audio; })().play();">]
    let playAudio (_file: string) : unit = jsNative

    let newObj fn = jsOptions<_> fn
    let cloneDeep<'T> (_: 'T) : 'T = importDefault "lodash.clonedeep"
    let cloneObj<'T> (obj: 'T) (fn: 'T -> 'T) = fn (cloneDeep obj)
    let toJsArray a = a |> Array.toList |> List.toArray

    let rec waitFor fn =
        async {
            let ok = fn ()

            if ok then
                return ()
            else
                log (fun () -> "waitForObject: null. waiting...")
                do! Async.Sleep 100
                return! waitFor fn
        }

    let rec waitForObject fn =
        async {
            let obj = fn ()

            if box obj <> null then
                return obj
            else
                log (fun () -> "waitForObject: null. waiting...")
                do! Async.Sleep 100
                return! waitForObject fn
        }

    let ofObjDefault def obj =
        if obj = null
           || (jsTypeof obj = "object"
               && (JS.Constructors.Object.keys obj).Count = 0) then
            def
        else
            obj
