namespace Fluke.UI.Frontend.Hooks

open System
open Feliz
open Fluke.UI.Frontend.Bindings
open Fable.Core.JsInterop


module React =
    let shadowedEffectFn (fn, deps) =
        (emitJsExpr (React.useEffect, fn, deps) "$0($1,$2)")

    let useDisposableEffect (effect, deps) =
        let disposed = React.useRef false

        shadowedEffectFn (
            (fun () ->
                if disposed.current then
                    printfn $"calling effect after dispose. {effect}"

                effect disposed.current

                { new IDisposable with
                    member _.Dispose () =
                        disposed.current <- true
                        effect disposed.current
                }),
            Array.concat [
                deps
                [|
                    box effect
                |]
            ]
        )
