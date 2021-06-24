namespace Fluke.UI.Frontend.Tests.Core

open Fable.Jester
open Fable.ReactTestingLibrary
open Fable.Core.JsInterop
open Feliz
open Fluke.UI.Frontend.Bindings
open Fable.React
open Fluke.UI.Frontend.Hooks
open Microsoft.FSharp.Core.Operators


module RTL =
    let inline sleep ms =
        promise {
            printfn $"RTL.sleep({ms})"

            for _ in 0 .. ms / 500 do
                do! RTL.waitFor (Promise.sleep 500)

            printfn $"RTL.sleep({ms}) end"
        }

module Setup =
    import "jest" "@jest/globals"

    let handlePromise promise = promise
    //        |> Promise.catch (fun ex -> Fable.Core.JS.console.error (box ex))

    Jest.afterAll (
        promise {
            Browser.Dom.window?exit <- true
            printfn "after all"
        }
    )

    [<ReactComponent>]
    let RootWrapper cmp =
        React.strictMode [
            //            Recoil.root [
//                root.children [
            React.ReactErrorBoundary.renderCatchFn
                (fun (error, info) -> printfn $"ReactErrorBoundary Error: {info.componentStack} {error}")
                (str "error")
                cmp
        //                ]
//            ]
        ]

    let render (cmp: ReactElement) =
        promise {
            //            let mutable peekFn : (CallbackMethods -> JS.Promise<unit>) -> JS.Promise<unit> =
//                fun _ -> failwith "called empty callback"
//
            let mutable callbacksRef : Store.GetFn * Store.SetFn = unbox null
            //
//            let cmpWrapper =
//                React.memo
//                    (fun () ->
//                        peekFn <-
//                            Recoil.useCallbackRef
//                                (fun (setter: CallbackMethods) (fn: CallbackMethods -> JS.Promise<unit>) ->
//                                    RTL.waitFor (fn setter |> handlePromise))
//
//                        cmp)

            let subject =
                RTL.render (
                    RootWrapper (
                        React.fragment [
                            (React.memo
                                (fun () ->
                                    let callbacks = Store.useCallbacks ()

                                    React.useEffect (
                                        (fun () ->
                                            promise {
                                                let! callbacksValue = callbacks ()
                                                callbacksRef <- callbacksValue
                                            }
                                            |> Promise.start),
                                        [|
                                            box callbacks
                                        |]
                                    )

                                    nothing)
                                ())
                            cmp
                        ]
                    )
                )

            //            RTL.act (fun () -> Jest.runAllTimers())

            do! RTL.waitFor id
            //            return subject, peekFn
            return subject, callbacksRef
        }

//    let waitForObj setter fn =
//        promise {
//            let mutable obj = None
//
//            let! result = fn setter
//            obj <- Some result
//
//            do!
//                JS.waitFor (fun () -> obj.IsSome)
//                |> Async.StartAsPromise
//
//            return obj.Value
//        }

//    let getSnapshot (fn: (MutableSnapshot -> unit) option) : Snapshot = emitJsExpr fn "snapshot_UNSTABLE($0)"
