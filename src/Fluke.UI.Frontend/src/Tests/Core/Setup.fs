namespace Fluke.UI.Frontend.Tests.Core

open Feliz
open FsStore
open FsStore.Model
open FsUi.Bindings
open Fable.Jester
open Fable.ReactTestingLibrary
open Fable.Core.JsInterop
open Fable.React
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

    let inline handlePromise promise = promise
    //        |> Promise.catch (fun ex -> Fable.Core.JS.console.error (box ex))

    Jest.afterAll (
        promise {
            printfn "after all"
            Browser.Dom.window?exit <- true

        //            JS.setTimeout (fun () -> emitJsExpr () "process.exit()") 1000
//            |> ignore
        }
    )


    let inline render (cmp: ReactElement) =
        promise {
            //            let mutable peekFn : (CallbackMethods -> JS.Promise<unit>) -> JS.Promise<unit> =
//                fun _ -> failwith "called empty callback"
//
            let mutable callbacksRef: GetFn * SetFn = unbox null
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
                    React.ErrorBoundary [
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
                    ]
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
