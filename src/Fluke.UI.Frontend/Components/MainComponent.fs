namespace Fluke.UI.Frontend.Components

open System
open Browser
open Fable.Core
open Browser.Types
open FSharpPlus
open Fable.React
open Suigetsu.UI.Frontend.React
open Suigetsu.Core
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.Shared
open Fable.Core.JsInterop


module MainComponent =
    let globalShortcutHandler =
        React.memo (fun () ->
            let selection, setSelection =
                Recoil.useState Recoil.Selectors.selection

            let ctrlPressed, setCtrlPressed = Recoil.useState Recoil.Atoms.ctrlPressed

            let shiftPressed, setShiftPressed =
                Recoil.useState Recoil.Atoms.shiftPressed

            let keyEvent (e: KeyboardEvent) =
                if e.ctrlKey <> ctrlPressed then
                    setCtrlPressed e.ctrlKey

                if e.shiftKey <> shiftPressed then
                    setShiftPressed e.shiftKey

                if e.key = "Escape" && not selection.IsEmpty then
                    setSelection Map.empty

            React.useListener.onKeyDown keyEvent
            React.useListener.onKeyUp keyEvent

            nothing)

    let positionUpdater =
        React.memo (fun () ->
            let resetPosition =
                Recoil.useResetState Recoil.Selectors.position

            Scheduling.useScheduling Scheduling.Interval (60 * 1000) resetPosition
            //        Scheduling.useScheduling Scheduling.Interval (10 * 1000) resetPosition

            nothing)

    let dataLoader =
        React.memo (fun () ->
            let view = Recoil.useValue Recoil.Selectors.view

            let loadTreeSelection =
                Recoil.useCallbackRef (fun setter ->
                    async {
                        Recoil.Profiling.addTimestamp "dataLoader.loadTreeCallback[0]"
                        let! treeSelection = setter.snapshot.getAsync (Recoil.Selectors.treeSelectionAsync view)

                        printfn "dataLoader.TREESELECTION=None:%A" (treeSelection = None)

                        Browser.Dom.window?treeSelection <- treeSelection

                        //Uncaught (in promise) Promise {<fulfilled>: List}
                        Recoil.Profiling.addTimestamp "dataLoader.loadTreeCallback[1]"

                        setter.set (Recoil.Selectors.treeSelection, treeSelection)

                        Recoil.Profiling.addTimestamp "dataLoader.loadTreeCallback[2]"
                    }
                    |> Async.StartImmediate)

            Recoil.Profiling.addTimestamp "dataLoader render"
            React.useEffect
                ((fun () ->
                    Recoil.Profiling.addTimestamp "dataLoader effect"
                    loadTreeSelection ()),

                 // TODO: return a cleanup?
                 [|
                     view :> obj
                 |])

            nothing)

    let soundPlayer =
        React.memo (fun () ->
            let oldActiveSessions = React.useRef []

            let user = Recoil.useValue Recoil.Selectors.user

            let activeSessions =
                Recoil.useValue Recoil.Selectors.activeSessions

            React.useEffect
                ((fun () ->
                    oldActiveSessions.current
                    |> List.map (fun (Model.ActiveSession (oldTaskName, (Model.Minute oldDuration), _, _)) ->
                        let newSession =
                            activeSessions
                            |> List.tryFind (fun (Model.ActiveSession (taskName, (Model.Minute duration), _, _)) ->
                                taskName = oldTaskName
                                && duration = oldDuration + 1.)

                        match newSession with
                        | Some (Model.ActiveSession (_, (Model.Minute newDuration), _, _)) when oldDuration = -1.
                                                                                                && newDuration = 0. ->
                            Temp.Sound.playTick
                        | Some (Model.ActiveSession (_, newDuration, totalDuration, _)) when newDuration = totalDuration ->
                            Temp.Sound.playDing
                        | None ->
                            match user with
                            | Some { SessionLength = Model.Minute sessionLength;
                                     SessionBreakLength = Model.Minute sessionBreakLength } when oldDuration =
                                                                                                     sessionLength
                                                                                                 + sessionBreakLength
                                                                                                 - 1. ->
                                Temp.Sound.playDing
                            | _ -> fun () -> ()
                        | _ -> fun () -> ())
                    |> List.iter (fun x -> x ())

                    oldActiveSessions.current <- activeSessions),
                 [|
                     user :> obj
                     activeSessions :> obj
                 |])

            nothing)

    let autoReload_TEMP =
        React.memo (fun () ->
            let reload =
                React.useCallback (fun () -> Dom.window.location.reload true)

            Scheduling.useScheduling Scheduling.Timeout (60 * 60 * 1000) reload

            nothing)

    let diag =
        React.memo (fun () ->
            let text, setText = React.useState ""
            let oldJson, setOldJson = React.useState ""
            let debug = Recoil.useValue Recoil.Atoms.debug

            Scheduling.useScheduling Scheduling.Interval 100 (fun () ->
                if not debug then
                    ()
                else
                    let indent n = String (' ', n)

                    let json =
                        Recoil.Profiling.profilingState
                        |> Fable.SimpleJson.SimpleJson.stringify
                        |> JS.JSON.parse
                        |> fun obj -> JS.JSON.stringify (obj, unbox null, 4)
                        |> String.replace (sprintf ",\n%s" (indent 3)) ""
                        |> String.replace (indent 1) ""
                        |> String.replace "][\n" ""
                        |> String.replace "\"" " "

                    if json = oldJson then
                        ()
                    else
                        setText json
                        setOldJson json)

            if not debug then
                nothing
            else
                React.fragment [
                    Html.pre [
                        prop.id "diag"
                        prop.style [
                            style.custom ("width", "min-content")
                            style.custom ("height", "80%")
                            style.position.fixedRelativeToWindow
                            style.right 0
                            style.bottom 0
                            style.fontSize 9
                            style.backgroundColor "#44444488"
                            style.zIndex 100
                        ]
                        prop.children
                            [
                                str text
                            ]
                    ]

                    Html.div [
                        prop.id "test1"
                        prop.style [
                            style.position.absolute
                            style.width 100
                            style.height 100
                            style.top 0
                            style.right 0
                            style.backgroundColor "#ccc3"
                            style.zIndex 100
                        ]
                        prop.children
                            [
                                str "test1"
                            ]
                    ]
                ])

    let render =
        React.memo (fun () ->
            React.fragment [
                diag ()
                globalShortcutHandler ()
                positionUpdater ()
                autoReload_TEMP ()

                React.suspense
                    ([
                        dataLoader ()
                        soundPlayer ()

                        NavBarComponent.render ()
                        PanelsComponent.render ()
                     ],
                     PageLoaderComponent.render ())
            ])
