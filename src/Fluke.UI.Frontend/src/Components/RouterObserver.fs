namespace Fluke.UI.Frontend.Components

open Fable.React
open Fable.Core
open Fable.Core.JsInterop
open Feliz.Router
open Feliz
open Fluke.Shared
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State


module RouterObserver =
    [<ReactComponent>]
    let RouterObserver () =
        React.useEffect (
            (fun () ->
                match JS.window id with
                | Some window ->
                    let redirect = window.sessionStorage?redirect
                    emitJsExpr () "delete sessionStorage.redirect"

                    match redirect with
                    | String.ValidString _ when redirect <> window.location.href ->
                        Router.navigatePath (redirect.Split "/" |> Array.skip 3)
                    | _ -> ()
                | None -> ()),
            [||]
        )

        let setSessionRestored = Store.useSetState Atoms.sessionRestored

        React.useEffect (
            (fun () -> setSessionRestored true),
            [|
                box setSessionRestored
            |]
        )

        nothing
