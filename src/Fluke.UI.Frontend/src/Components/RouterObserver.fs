namespace Fluke.UI.Frontend.Components

open Fable.Core
open Fable.Core.JsInterop
open Feliz.Recoil
open Browser.Types
open Feliz.Router
open Feliz
open Feliz.UseListener
open Fluke.Shared
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State


module RouterObserver =

    [<RequireQualifiedAccess>]
    type Steps =
        | Start
        | Processing
        | AwaitingChange
        | PathChanged

    type ParsedSegments = { View: View.View option }

    let parseSegments (deviceInfo: JS.DeviceInfo) segments =
        match (segments
               |> List.skip (if deviceInfo.GitHubPages then 1 else 0)) with
        | [ "view"; "Information" ] -> { View = Some View.View.Information }
        | [ "view"; "HabitTracker" ] -> { View = Some View.View.HabitTracker }
        | [ "view"; "Priority" ] -> { View = Some View.View.Priority }
        | [ "view"; "BulletJournal" ] -> { View = Some View.View.BulletJournal }
        | _ -> { View = None }

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
            [|
            |]
        )

        let sessionRestored, setSessionRestored = Store.useState Atoms.sessionRestored
        let username = Store.useValue Atoms.username
        let deviceInfo = Store.useValue Selectors.deviceInfo

        let view, setView = Recoil.useStateKeyDefault Atoms.User.view username TempUI.defaultView

        let onKeyDown =
            Recoil.useCallbackRef
                (fun _ (e: KeyboardEvent) ->
                    match e.ctrlKey, e.shiftKey, e.key with
                    | false, true, "I" ->
                        JS.log (fun () -> "RouterObserver.onKeyDown() View.Information")
                        setView View.View.Information
                    | false, true, "H" -> setView View.View.HabitTracker
                    | false, true, "P" -> setView View.View.Priority
                    | false, true, "B" -> setView View.View.BulletJournal
                    | _ -> ())

        React.useListener.onKeyDown onKeyDown

        let step, setStep = React.useState Steps.Start

        let currentSegments, setCurrentSegments =
            React.useState (
                match Router.currentPath () with
                | [ path ] when path.Contains ".htm" -> []
                | path -> path
            )

        let initialSegments, _ = React.useState currentSegments

        let restoringInitialSegments, setRestoringInitialSegments = React.useState false


        let pathPrefix, parsedSegments =
            React.useMemo (
                (fun () ->
                    let pathPrefix =
                        if deviceInfo.GitHubPages then
                            [|
                                initialSegments.[0]
                            |]
                        else
                            [||]

                    let parsedSegments = parseSegments deviceInfo currentSegments
                    pathPrefix, parsedSegments),
                [|
                    box initialSegments
                    box deviceInfo
                    box currentSegments
                |]
            )

        React.useEffect (
            (fun () ->
                JS.log (fun () -> "RouterObserver effect. #00")

                if restoringInitialSegments then
                    JS.log (fun () -> "RouterObserver. #10")

                    if currentSegments <> initialSegments then
                        JS.log (fun () -> "RouterObserver. #11")

                        match (initialSegments |> parseSegments deviceInfo).View with
                        | Some view -> setView view
                        | _ -> ()

                        Router.navigatePath (initialSegments |> List.toArray)
                    else
                        JS.log (fun () -> "RouterObserver. #12")

                        if step <> Steps.AwaitingChange then
                            JS.log (fun () -> "RouterObserver. #12.1")
                            setStep Steps.AwaitingChange
                        else
                            JS.log (fun () -> "RouterObserver. #12.2")
                            setRestoringInitialSegments false
                elif sessionRestored then
                    let pathFromState =
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
                        |> Array.append pathPrefix

                    JS.log
                        (fun () ->
                            $"RouterObserver. #1 {
                                                      JS.JSON.stringify
                                                          {|
                                                              username = username
                                                              step = step
                                                              view = view
                                                              currentSegments = currentSegments
                                                              pathFromState = pathFromState
                                                              initialSegments = initialSegments
                                                              initialSegmentsView =
                                                                  (initialSegments |> parseSegments deviceInfo).View
                                                              parsedSegments = parsedSegments
                                                          |}
                            }")

                    match step, parsedSegments with
                    | _, { View = None } when pathFromState <> (currentSegments |> List.toArray) ->
                        setStep Steps.AwaitingChange
                        JS.log (fun () -> "RouterObserver. #2")

                        match currentSegments with
                        | [ "login" ] when
                            not initialSegments.IsEmpty
                            && initialSegments.[0] <> "login" -> setRestoringInitialSegments true
                        | _ -> Router.navigatePath pathFromState

                    | Steps.Start, { View = Some pathView } ->
                        setStep Steps.Processing
                        setView pathView

                        JS.log (fun () -> "RouterObserver. #3")

                    | Steps.AwaitingChange, { View = Some pathView } when view <> pathView ->
                        Router.navigatePath pathFromState
                        JS.log (fun () -> "RouterObserver. #4")

                    | Steps.Processing, { View = Some pathView } when view = pathView ->
                        setStep Steps.AwaitingChange
                        JS.log (fun () -> "RouterObserver. #5")

                    | Steps.PathChanged, { View = Some pathView } ->
                        setStep Steps.Processing
                        setView pathView

                        JS.log (fun () -> "RouterObserver. #6")

                    | _ ->
                        if pathFromState <> (currentSegments |> List.toArray) then
                            Router.navigatePath pathFromState

                        JS.log (fun () -> "RouterObserver. #7")
                else
                    JS.log (fun () -> "RouterObserver. #8")

                    setSessionRestored true),
            [|
                box setView
                box view
                box pathPrefix
                box deviceInfo
                box restoringInitialSegments
                box setRestoringInitialSegments
                box initialSegments
                box sessionRestored
                box setSessionRestored
                box currentSegments
                box username
                box step
                box setStep
                box parsedSegments
            |]
        )

        React.router [
            router.pathMode
            router.onUrlChanged
                (fun newSegments ->
                    JS.log
                        (fun () ->
                            $"RouterObserver. onUrlChanged. {
                                                                 JS.JSON.stringify
                                                                     {|
                                                                         username = username
                                                                         step = step
                                                                         currentSegments = currentSegments
                                                                         view = view
                                                                         newSegments = newSegments
                                                                         initialSegments = initialSegments
                                                                         parsedSegments = parsedSegments
                                                                     |}
                            }")

                    match step with
                    | Steps.AwaitingChange when currentSegments <> newSegments -> setStep Steps.PathChanged
                    | _ -> ()

                    if newSegments <> currentSegments then setCurrentSegments newSegments)
        ]
