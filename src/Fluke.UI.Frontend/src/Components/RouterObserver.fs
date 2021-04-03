namespace Fluke.UI.Frontend.Components

open Browser.Types
open Feliz.Router
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.Shared
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module RouterObserver =

    [<RequireQualifiedAccess>]
    type Steps =
        | Start
        | Processing
        | AwaitingChange
        | UrlChanged

    type ParsedSegments = { View: View.View option }

    [<ReactComponent>]
    let RouterObserver () =
        let sessionRestored, setSessionRestored = Recoil.useState Recoil.Atoms.sessionRestored
        let username = Recoil.useValue Recoil.Atoms.username
        let view, setView = Recoil.useState Recoil.Atoms.view

        let onKeyDown =
            Recoil.useCallbackRef
                (fun _ (e: KeyboardEvent) ->
                    match e.ctrlKey, e.shiftKey, e.key with
                    | _, true, "H" -> setView View.View.HabitTracker
                    | _, true, "P" -> setView View.View.Priority
                    | _, true, "B" -> setView View.View.BulletJournal
                    | _, true, "I" -> setView View.View.Information
                    | _ -> ())

        React.useListener.onKeyDown onKeyDown

        let step, setStep = React.useState Steps.Start

        let segments, setSegments = React.useState (Router.currentUrl ())

        //            printfn
//                "RouterObserver.render. %A"
//                {|
//                    username = username
//                    step = step
//                    view = view
//                    segments = segments
//                |}

        let parsedSegments =
            React.useMemo (
                (fun () ->
                    match segments with
                    | [ "view"; "HabitTracker" ] -> { View = Some View.View.HabitTracker }
                    | [ "view"; "Priority" ] -> { View = Some View.View.Priority }
                    | [ "view"; "BulletJournal" ] -> { View = Some View.View.BulletJournal }
                    | [ "view"; "Information" ] -> { View = Some View.View.Information }
                    | _ -> { View = None }),
                [|
                    segments
                |]
            )

        React.useEffect (
            (fun () ->
                if sessionRestored then
                    let urlFromState =
                        match username with
                        | Some _ ->
                            [|
                                "view"
                                string view
                            |]
                        | None ->
                            [|
                                "login"
                            |]

                    match step, parsedSegments with
                    | _, { View = None } when urlFromState <> (segments |> List.toArray) ->
                        setStep Steps.AwaitingChange
                        Router.navigate urlFromState

                    | Steps.Start, { View = Some urlView } ->
                        setStep Steps.Processing
                        setView urlView

                    | Steps.AwaitingChange, { View = Some urlView } when view <> urlView -> Router.navigate urlFromState

                    | Steps.Processing, { View = Some urlView } when view = urlView -> setStep Steps.AwaitingChange

                    | Steps.UrlChanged, { View = Some urlView } ->
                        setStep Steps.Processing
                        setView urlView

                    | _ ->
                        if urlFromState <> (segments |> List.toArray) then
                            Router.navigate urlFromState
                else
                    Fable.Core.JS.setTimeout (fun () -> setSessionRestored true) 0
                    |> ignore),
            [|
                box sessionRestored
                box setSessionRestored
                box segments
                box username
                box view
                box setView
                box step
                box setStep
                box parsedSegments
            |]
        )


        React.router [
            router.onUrlChanged
                (fun newSegments ->
                    //                    printfn
//                        "onUrlChanged. %A"
//                        {|
//                            username = username
//                            step = step
//                            view = view
//                            segments = segments
//                            newSegments = newSegments
//                        |}

                    match step with
                    | Steps.AwaitingChange when segments <> newSegments -> setStep Steps.UrlChanged
                    | _ -> ()

                    setSegments newSegments
                    ())
        ]
