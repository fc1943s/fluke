namespace Fluke.UI.Frontend.Components

open FsCore
open FsJs
open Fable.React
open Fable.Core.JsInterop
open Feliz.Router
open Feliz
open Fluke.Shared
open FsStore
open FsUi.Bindings
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State


module RouterObserver =
    [<ReactComponent>]
    let RouterObserver () =
        React.useEffect (
            (fun () ->
                match Dom.window () with
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

        let setSessionRestored = Store.useSetState Atoms.Session.sessionRestored

        React.useEffect (
            (fun () -> setSessionRestored true),
            [|
                box setSessionRestored
            |]
        )

        nothing
