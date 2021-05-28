namespace Fluke.UI.Frontend.Tests.Core

open Fable.Core.JsInterop
open Fable.ReactTestingLibrary
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Tests.Core
open Fable.React
open Fluke.UI.Frontend.Hooks
open Microsoft.FSharp.Core.Operators


module Setup =
    import "jest" "@jest/globals"
    import "snapshot_UNSTABLE" "recoil"

    let handlePromise promise = promise
    //        |> Promise.catch (fun ex -> Fable.Core.JS.console.error (box ex))

    let rootWrapper cmp =
        React.memo
            (fun () ->
                React.strictMode [
                    Recoil.root [ root.children [ cmp ] ]
                ]
                |> ReactErrorBoundary.renderCatchFn
                    (fun (error, info) -> printfn $"ReactErrorBoundary Error: {info.componentStack} {error}")
                    (str "error"))
            ()

    let render (cmp: ReactElement) =
        promise {
            //            let mutable peekFn : (CallbackMethods -> JS.Promise<unit>) -> JS.Promise<unit> =
//                fun _ -> failwith "called empty callback"
//
            let mutable setterFn : unit -> CallbackMethods = fun _ -> failwith "called empty callback"
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
                    rootWrapper (
                        React.fragment [
                            (React.memo
                                (fun () ->
                                    let setter = Recoil.useCallbackRef id

                                    React.useEffect (
                                        (fun () ->
                                            setterFn <- setter
                                            ()),
                                        [|
                                            box setter
                                        |]
                                    )

                                    nothing)
                                ())
                            cmp
                        ]
                    )
                )

//            do! RTL.waitFor id
//            do! RTL.waitFor id
            //            return subject, peekFn
            return subject, setterFn
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

    let getSnapshot (fn: (MutableSnapshot -> unit) option) : Snapshot = emitJsExpr fn "snapshot_UNSTABLE($0)"
