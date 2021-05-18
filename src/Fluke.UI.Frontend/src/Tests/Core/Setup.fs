namespace Fluke.UI.Frontend.Tests.Core

open Fable.Core.JsInterop
open Fable.ReactTestingLibrary
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend
open Fluke.Shared.Domain
open Fluke.UI.Frontend.Tests.Core
open Fable.React
open Fluke.UI.Frontend.Hooks


module Setup =
    open State

    import "jest" "@jest/globals"

    let rootWrapper cmp =
        React.memo
            (fun () ->
                React.strictMode [
                    Recoil.root [ root.children [ cmp ] ]
                ]
                |> ReactErrorBoundary.renderCatchFn
                    (fun (error, info) -> printfn $"ReactErrorBoundary Error: {info.componentStack} {error}")
                    (str "error"))

    let handlePromise promise = promise
    //        |> Promise.catch (fun ex -> Fable.Core.JS.console.error (box ex))

    let render (cmp: ReactElement) =
        promise {
            let mutable peekFn : (CallbackMethods -> Fable.Core.JS.Promise<unit>) -> Fable.Core.JS.Promise<unit> =
                fun _ -> failwith "called empty callback"

            let cmpWrapper =
                React.memo
                    (fun () ->
                        peekFn <-
                            Recoil.useCallbackRef
                                (fun (setter: CallbackMethods) (fn: CallbackMethods -> Fable.Core.JS.Promise<unit>) ->
                                    RTL.waitFor (fn setter |> handlePromise))

                        cmp)

            let subject = RTL.render ((rootWrapper (cmpWrapper ())) ())
            do! RTL.waitFor id
            return subject, peekFn
        }
