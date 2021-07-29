namespace FsUi.Hooks

open FsCore
open Feliz
open Fable.Core.JsInterop


module React =
    let shadowedUseEffectFn (fn, deps) =
        (emitJsExpr (React.useEffect, fn, deps) "$0($1,$2)")

    let inline useIsMounted () =
        let isMounted = React.useRef false

        React.useEffect (
            (fun () ->
                isMounted.current <- true
                Object.newDisposable (fun () -> isMounted.current <- false)),
            [|
                box isMounted
            |]
        )

        isMounted

    let inline useDisposableEffect (effect, deps) =
        let disposed = React.useRef false

        shadowedUseEffectFn (
            (fun () ->
                if disposed.current then
                    printfn $"calling effect after dispose. {effect}"

                effect disposed.current

                Object.newDisposable
                    (fun () ->
                        disposed.current <- true
                        effect disposed.current)),
            Array.concat [
                deps
                [|
                    box effect
                |]
            ]
        )
