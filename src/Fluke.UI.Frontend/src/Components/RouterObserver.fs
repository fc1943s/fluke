namespace Fluke.UI.Frontend.Components

open Fable.Core
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
        let isTesting = Recoil.useValue Recoil.Atoms.isTesting
        Recoil.useCallbackRef (fun _ (str: string) -> if isTesting then printfn $"{str}")

    [<ReactComponent>]
    let RouterObserver () =
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

        let parseSegments segments =
            match segments with
            | [ "view"; "HabitTracker" ] -> { View = Some View.View.HabitTracker }
            | [ "view"; "Priority" ] -> { View = Some View.View.Priority }
            | [ "view"; "BulletJournal" ] -> { View = Some View.View.BulletJournal }
            | [ "view"; "Information" ] -> { View = Some View.View.Information }
            | _ -> { View = None }

        let parsedSegments =
            React.useMemo (
                (fun () -> parseSegments currentSegments),
                [|
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
                        log "RouterObjserverRouterObserver. #12"

                        if step <> Steps.AwaitingChange then
                            setStep Steps.AwaitingChange
                        else
                            log "RouterObjserverRouterObserver. #12.1"
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

                    log
                        $"RouterObjserverRouterObserver. #1 {
                                                                 JS.JSON.stringify
                                                                     {|
                                                                         username = username
                                                                         step = step
                                                                         view = view
                                                                         currentSegments = currentSegments
                                                                         pathFromState = pathFromState
                                                                         initialSegments = initialSegments
                                                                         initialSegmentsView =
                                                                             (initialSegments |> parseSegments).View
                                                                         parsedSegments = parsedSegments
                                                                     |}
                        }"

                    match step, parsedSegments with
                    | _, { View = None } when pathFromState <> (currentSegments |> List.toArray) ->
                        setStep Steps.AwaitingChange
                        log "RouterObjserverRouterObserver. #2"

                        match currentSegments with
                        | [ "login" ] when not initialSegments.IsEmpty -> setRestoringInitialSegments true
                        | _ -> Router.navigatePath pathFromState

                    | Steps.Start, { View = Some pathView } ->
                        setStep Steps.Processing
                        setView pathView
                        log "RouterObjserverRouterObserver. #3"

                    | Steps.AwaitingChange, { View = Some pathView } when view <> pathView ->
                        Router.navigatePath pathFromState
                        log "RouterObjserverRouterObserver. #4"

                    | Steps.Processing, { View = Some pathView } when view = pathView ->
                        setStep Steps.AwaitingChange
                        log "RouterObjserverRouterObserver. #5"

                    | Steps.PathChanged, { View = Some pathView } ->
                        setStep Steps.Processing
                        setView pathView
                        log "RouterObjserverRouterObserver. #6"

                    | _ ->
                        if pathFromState <> (currentSegments |> List.toArray) then
                            Router.navigatePath pathFromState

                        log "RouterObjserverRouterObserver. #7"
                else
                    log "RouterObjserverRouterObserver. #8"

                    JS.setTimeout (fun () -> setSessionRestored true) 0
                    |> ignore),
            [|
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
                        $"RouterObjserverRouterObserver. onUrlChanged. {
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
