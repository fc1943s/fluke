namespace FsUi.Hooks

open System
open Feliz
open Fable.Core.JsInterop


module React =
    let shadowedUseEffectFn (fn, deps) =
        (emitJsExpr (React.useEffect, fn, deps) "$0($1,$2)")

    let useIsMounted () =
        let isMounted = React.useRef false

        React.useEffect (
            (fun () ->
                isMounted.current <- true

                { new IDisposable with
                    member _.Dispose () = isMounted.current <- false
                }),
            [|
                box isMounted
            |]
        )

        isMounted

    let useDisposableEffect (effect, deps) =
        let disposed = React.useRef false

        shadowedUseEffectFn (
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
