namespace Fluke.UI.Frontend.Components

open Browser.Types
open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.Shared
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module ViewUpdater =
    let render =
        React.memo (fun () ->
            let path = Recoil.useValue Recoil.Atoms.path
            let setView = Recoil.useSetState Recoil.Atoms.view

            React.useListener.onKeyDown (fun (e: KeyboardEvent) ->
                match e.ctrlKey, e.shiftKey, e.key with
                | _, true, "H" -> setView View.View.HabitTracker
                | _, true, "P" -> setView View.View.Priority
                | _, true, "B" -> setView View.View.BulletJournal
                | _, true, "I" -> setView View.View.Information
                | _ -> ())

            let updateView () =
                let view =
                    match path with
                    | [ "view"; "HabitTracker" ] -> Some View.View.HabitTracker
                    | [ "view"; "Priority" ] -> Some View.View.Priority
                    | [ "view"; "BulletJournal" ] -> Some View.View.BulletJournal
                    | [ "view"; "Information" ] -> Some View.View.Information
                    | _ -> None

                match view with
                | Some view -> setView view
                | None -> ()

            React.useEffect
                (updateView,
                 [|
                     box path
                     box setView
                 |])

            nothing)
