namespace Fluke.UI.Frontend.Components

open Fable.Core
open Fable.Core.JsInterop
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
        | PathChanged

    type ParsedSegments = { View: View.View option }

    let useLog () =
        let isTesting = false // Recoil.useValue Recoil.Atoms.isTesting
        Recoil.useCallbackRef (fun _ (str: string) -> if isTesting then printfn $"{str}")

    [<ReactComponent>]
    let RouterObserver () =
        React.useEffect (
            (fun () ->
                let redirect = Browser.Dom.window.sessionStorage?redirect
                JS.delete "sessionStorage.redirect"

                match redirect with
                | String.ValidString _ when redirect <> Browser.Dom.window.location.href -> Router.navigatePath redirect
                | _ -> ()),
            [||]
        )

        let log = useLog ()
        let sessionRestored, setSessionRestored = Recoil.useState Recoil.Atoms.sessionRestored
        let username = Recoil.useValue Recoil.Atoms.username
        let view, setView = Recoil.useStateDefault Recoil.Atoms.User.view username

        let onKeyDown =
            Recoil.useCallbackRef
                (fun _ (e: KeyboardEvent) ->
                    match e.ctrlKey, e.shiftKey, e.key with
                    | false, true, "H" -> setView View.View.HabitTracker
                    | false, true, "P" -> setView View.View.Priority
                    | false, true, "B" -> setView View.View.BulletJournal
                    | false, true, "I" ->
                        log "RouterObserver.onKeyDown() View.Information"
                        setView View.View.Information
                    | _ -> ())

        React.useListener.onKeyDown onKeyDown

        let step, setStep = React.useState Steps.Start

        let currentSegments, setCurrentSegments = React.useState (Router.currentPath ())
        let initialSegments, _ = React.useState currentSegments
        let restoringInitialSegments, setRestoringInitialSegments = React.useState false

        let isRootHosted =
            React.useMemo (
                (fun () ->
                    Browser.Dom.window.location.host.EndsWith "github.io"
                    |> not),
                [||]
            )

        let parseSegments =
            Recoil.useCallbackRef
                (fun _ segments ->
                    match (segments
                           |> List.skip (if isRootHosted then 0 else 1)) with
                    | [ "view"; "HabitTracker" ] -> { View = Some View.View.HabitTracker }
                    | [ "view"; "Priority" ] -> { View = Some View.View.Priority }
                    | [ "view"; "BulletJournal" ] -> { View = Some View.View.BulletJournal }
                    | [ "view"; "Information" ] -> { View = Some View.View.Information }
                    | _ -> { View = None })

        let pathPrefix, parsedSegments =
            React.useMemo (
                (fun () ->
                    let pathPrefix =
                        if not isRootHosted then
                            [|
                                initialSegments.[0]
                            |]
                        else
                            [||]

                    let parsedSegments = parseSegments currentSegments
                    pathPrefix, parsedSegments),
                [|
                    parseSegments
                    initialSegments
                    isRootHosted
                    currentSegments
                |]
            )

        React.useEffect (
            (fun () ->
                if restoringInitialSegments then
                    log "RouterObserver. #10"

                    if currentSegments <> initialSegments then
                        log "RouterObserver. #11"

                        match (initialSegments |> parseSegments).View with
                        | Some view -> setView view
                        | None -> ()

                        Router.navigatePath (initialSegments |> List.toArray)
                    else
                        log "RouterObserver. #12"

                        if step <> Steps.AwaitingChange then
                            log "RouterObserver. #12.1"
                            setStep Steps.AwaitingChange
                        else
                            log "RouterObserver. #12.2"
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

                    log
                        $"RouterObserver. #1 {
                                                  JS.JSON.stringify
                                                      {|
                                                          username = username
                                                          step = step
                                                          view = view
                                                          currentSegments = currentSegments
                                                          pathFromState = pathFromState
                                                          initialSegments = initialSegments
                                                          initialSegmentsView = (initialSegments |> parseSegments).View
                                                          parsedSegments = parsedSegments
                                                      |}
                        }"

                    match step, parsedSegments with
                    | _, { View = None } when pathFromState <> (currentSegments |> List.toArray) ->
                        setStep Steps.AwaitingChange
                        log "RouterObserver. #2"

                        match currentSegments with
                        | [ "login" ] when
                            not initialSegments.IsEmpty
                            && initialSegments.[0] <> "login" -> setRestoringInitialSegments true
                        | _ -> Router.navigatePath pathFromState

                    | Steps.Start, { View = Some pathView } ->
                        setStep Steps.Processing
                        setView pathView
                        log "RouterObserver. #3"

                    | Steps.AwaitingChange, { View = Some pathView } when view <> pathView ->
                        Router.navigatePath pathFromState
                        log "RouterObserver. #4"

                    | Steps.Processing, { View = Some pathView } when view = pathView ->
                        setStep Steps.AwaitingChange
                        log "RouterObserver. #5"

                    | Steps.PathChanged, { View = Some pathView } ->
                        setStep Steps.Processing
                        setView pathView
                        log "RouterObserver. #6"

                    | _ ->
                        if pathFromState <> (currentSegments |> List.toArray) then
                            Router.navigatePath pathFromState

                        log "RouterObserver. #7"
                else
                    log "RouterObserver. #8"

                    JS.setTimeout (fun () -> setSessionRestored true) 0
                    |> ignore),
            [|
                box pathPrefix
                box parseSegments
                box log
                box restoringInitialSegments
                box setRestoringInitialSegments
                box initialSegments
                box sessionRestored
                box setSessionRestored
                box currentSegments
                box username
                box view
                box setView
                box step
                box setStep
                box parsedSegments
            |]
        )


        React.router [
            router.pathMode
            router.onUrlChanged
                (fun newSegments ->
                    log
                        $"RouterObserver. onUrlChanged. {
                                                             JS.JSON.stringify
                                                                 {|
                                                                     username = username
                                                                     step = step
                                                                     view = view
                                                                     currentSegments = currentSegments
                                                                     newSegments = newSegments
                                                                     initialSegments = initialSegments
                                                                     parsedSegments = parsedSegments
                                                                 |}
                        }"

                    match step with
                    | Steps.AwaitingChange when currentSegments <> newSegments -> setStep Steps.PathChanged
                    | _ -> ()

                    if newSegments <> currentSegments then setCurrentSegments newSegments)
        ]
