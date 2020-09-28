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
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    module GlobalShortcutHandler =
        let hook =
            React.memo (fun () ->
                let selection, setSelection = Recoil.useState Recoil.Selectors.selection
                let ctrlPressed, setCtrlPressed = Recoil.useState Recoil.Atoms.ctrlPressed
                let shiftPressed, setShiftPressed = Recoil.useState Recoil.Atoms.shiftPressed

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

    module PositionUpdater =
        let hook =
            React.memo (fun () ->
                let resetPosition = Recoil.useResetState Recoil.Selectors.position

                Scheduling.useScheduling Scheduling.Interval (60 * 1000) resetPosition
                //        Scheduling.useScheduling Scheduling.Interval (10 * 1000) resetPosition

                nothing)

    module SessionDataLoader =
        let hook =
            React.memo (fun (input: {| Username: Username |}) ->
                //            let position = Recoil.useValue Recoil.Selectors.position
//                let treeSelectionIds = Recoil.useValue (Recoil.Atoms.Session.treeSelectionIds input.Username)
                let sessionData = Recoil.useValue (Recoil.Selectors.Session.sessionData input.Username)
                let treeStateMap = Recoil.useValue Recoil.Atoms.treeStateMap
//                let dateSequence = Recoil.useValue Recoil.Selectors.dateSequence

                //                printfn "MainComponent.dataLoader -> Atoms.Session.treeSelectionIds = %A" treeSelectionIds
//                printfn
//                    "MainComponent.SessionDataLoader.hook -> Selectors.Session.sessionData.IsSome = %A"
//                    sessionData.IsSome

                //                let treeSelectionIdsMemo =
//                    React.useMemo (fun () ->
//                        printfn
//                            "MainComponent.dataLoader -> treeSelectionIdsMemo -> Atoms.Session.treeSelectionIds = %A"
//                            treeSelectionIds
//                        treeSelectionIds,
//                        [|
//                            treeSelectionIds
//                        |])


                let loadState =
                    Recoil.useCallbackRef (fun setter ->
                        async {
                            //                        do! Async.Sleep 1000
                            Recoil.Profiling.addTimestamp "dataLoader.loadStateCallback[0]"

                            match sessionData with
                            | Some sessionData ->
                                printfn
                                    "MainComponent.SessionDataLoader.hook -> loadState -> (sessionData.TaskList.Length) = %A"
                                    sessionData.TaskList.Length
                                //                                        let! treeStateMap = setter.snapshot.getAsync Recoil.Atoms.treeStateMap
//                                        let! dateSequence = setter.snapshot.getAsync Recoil.Selectors.dateSequence

                                let a () =
                                    let recoilInformationMap =
                                        sessionData.TaskList
                                        |> Seq.map (fun task -> task.Information)
                                        |> Seq.distinct
                                        |> Seq.map (fun information -> sessionData.InformationStateMap.[information])
                                        |> Seq.map (fun informationState ->

                                            let informationId =
                                                Recoil.Atoms.Information.informationId informationState.Information

                                            setter.set
                                                (Recoil.Atoms.Information.wrappedInformation informationId,
                                                 informationState.Information)
                                            setter.set
                                                (Recoil.Atoms.Information.attachments informationId,
                                                 informationState.Attachments)
                                            informationState.Information, informationId)
                                        |> Map.ofSeq

                                    Recoil.Profiling.addTimestamp "state.set[1]"

                                    sessionData.TaskList
                                    |> List.map (fun task -> sessionData.TaskStateMap.[task])
                                    |> List.iter (fun taskState ->
                                        let taskId = Recoil.Atoms.Task.taskId taskState.Task
                                        setter.set (Recoil.Atoms.Task.name taskId, taskState.Task.Name)

                                        setter.set
                                            (Recoil.Atoms.Task.informationId taskId,
                                             recoilInformationMap.[taskState.Task.Information])

                                        setter.set (Recoil.Atoms.Task.pendingAfter taskId, taskState.Task.PendingAfter)

                                        setter.set (Recoil.Atoms.Task.missedAfter taskId, taskState.Task.MissedAfter)

                                        setter.set (Recoil.Atoms.Task.scheduling taskId, taskState.Task.Scheduling)

                                        setter.set (Recoil.Atoms.Task.priority taskId, taskState.Task.Priority)

                                        setter.set (Recoil.Atoms.Task.attachments taskId, taskState.Attachments)

                                        setter.set (Recoil.Atoms.Task.duration taskId, taskState.Task.Duration)

                                        //                                        dateSequence
//                                        |> List.iter (fun date ->
//                                            let cellId = Recoil.Atoms.Cell.cellId taskId (DateId date)
//
//                                            setter.set (Recoil.Atoms.Cell.taskId cellId, taskId)
//                                            setter.set (Recoil.Atoms.Cell.date cellId, date))

                                        taskState.CellStateMap
                                        |> Map.filter (fun dateId cellState ->
                                            (<>) cellState.Status Disabled
                                            || not cellState.Attachments.IsEmpty
                                            || not cellState.Sessions.IsEmpty)
                                        |> Map.iter (fun dateId cellState ->
                                            let cellId = Recoil.Atoms.Cell.cellId taskId dateId

                                            setter.set (Recoil.Atoms.Cell.status cellId, cellState.Status)
                                            setter.set (Recoil.Atoms.Cell.attachments cellId, cellState.Attachments)
                                            setter.set (Recoil.Atoms.Cell.sessions cellId, cellState.Sessions)
                                            setter.set (Recoil.Atoms.Cell.selected cellId, false)))

                                    treeStateMap
                                    |> Map.values
                                    |> Seq.iter (fun treeState ->
                                        setter.set (Recoil.Atoms.Tree.name treeState.Id, treeState.Name)
                                        setter.set (Recoil.Atoms.Tree.owner treeState.Id, Some treeState.Owner)
                                        setter.set (Recoil.Atoms.Tree.sharedWith treeState.Id, treeState.SharedWith)
                                        setter.set (Recoil.Atoms.Tree.position treeState.Id, treeState.Position))
                                    //
                                    let taskIdList =
                                        sessionData.TaskList
                                        |> List.map Recoil.Atoms.Task.taskId

                                    setter.set (Recoil.Atoms.Session.taskIdList input.Username, taskIdList)

//                                    printfn
//                                        "MainComponent.SessionDataLoader.hook.loadState -> Atoms.Session.taskIdList[.Length] <- %A"
//                                        taskIdList.Length

                                a ()
                            | None -> ()






                            //                            let! state = setter.snapshot.getAsync (Recoil.Selectors.Session.state input.Username)
                            //
                            //                            printfn
                            //                                "MainComponent.dataLoader -> loadState -> let! state = %A"
                            //                                (if state.IsNone then
                            //                                    "None"
                            //                                 else
                            //                                     "Some ?")
                            //
                            //                            Ext.setDom (nameof state) state


                            Recoil.Profiling.addTimestamp "dataLoader.loadStateCallback[1]"

                            //                            setter.set (Recoil.Selectors.Session.state input.Username, state)

                            Recoil.Profiling.addTimestamp "dataLoader.loadStateCallback[2]"
                        }
                        |> Async.StartImmediate)

                Recoil.Profiling.addTimestamp "dataLoader render"
                React.useEffect
                    ((fun () ->
                        Recoil.Profiling.addTimestamp "dataLoader effect"
                        loadState ()),

                     // TODO: return a cleanup?
                     [|
                         sessionData :> obj
                     |])

                nothing)

    module SoundPlayer =
        let hook =
            React.memo (fun (input: {| Username: Username |}) ->
                let oldActiveSessions = React.useRef []
                let (Minute sessionLength) = Recoil.useValue (Recoil.Atoms.User.sessionLength input.Username)

                let (Minute sessionBreakLength) = Recoil.useValue (Recoil.Atoms.User.sessionBreakLength input.Username)

                let activeSessions = Recoil.useValue (Recoil.Selectors.Session.activeSessions input.Username)

                React.useEffect
                    ((fun () ->
                        oldActiveSessions.current
                        |> List.map (fun (Model.ActiveSession (oldTaskName, (Minute oldDuration), _, _)) ->
                            let newSession =
                                activeSessions
                                |> List.tryFind (fun (Model.ActiveSession (taskName, (Minute duration), _, _)) ->
                                    taskName = oldTaskName
                                    && duration = oldDuration + 1.)

                            match newSession with
                            | Some (Model.ActiveSession (_, (Minute newDuration), _, _)) when oldDuration = -1.
                                                                                              && newDuration = 0. ->
                                Temp.Sound.playTick
                            | Some (Model.ActiveSession (_, newDuration, totalDuration, _)) when newDuration =
                                                                                                     totalDuration ->
                                Temp.Sound.playDing
                            | None ->
                                if oldDuration = sessionLength
                                   + sessionBreakLength
                                   - 1. then
                                    Temp.Sound.playDing
                                else
                                    id
                            | _ -> id)
                        |> List.iter (fun x -> x ())

                        oldActiveSessions.current <- activeSessions),
                     [|
                         sessionLength :> obj
                         sessionBreakLength :> obj
                         activeSessions :> obj
                     |])

                nothing)

    module AutoReload =
        let hook_TEMP =
            React.memo (fun () ->
                let reload = React.useCallback (fun () -> Dom.window.location.reload true)

                Scheduling.useScheduling Scheduling.Timeout (60 * 60 * 1000) reload

                nothing)

    module Debug =
        let render =
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

                if debug then
                    DebugOverlay.render {| BottomRightText = text |}
                else
                    nothing)

    module Content =
        let render =
            React.memo (fun () ->
                let username = Recoil.useValue Recoil.Atoms.username
                let treeStateMap = Recoil.useValue Recoil.Selectors.treeStateMap

                let loadUser =
                    Recoil.useCallbackRef (fun setter ->
                        async {
                            //                            let! treeStateMap = setter.snapshot.getAsync Recoil.Selectors.treeStateMap

                            match treeStateMap with
                            | Some (user, treeStateMap) ->
                                let availableTreeIds =
                                    treeStateMap
                                    |> Map.values
                                    |> Seq.sortBy (fun treeState -> treeState.Name)
                                    |> Seq.map (fun treeState -> treeState.Id)
                                    |> Seq.toList

                                setter.set (Recoil.Atoms.username, Some user.Username)
                                setter.set (Recoil.Atoms.Session.user user.Username, Some user)
                                setter.set (Recoil.Atoms.Session.availableTreeIds user.Username, availableTreeIds)
                                setter.set (Recoil.Atoms.treeStateMap, treeStateMap)
                            | None -> ()
                        }
                        |> Async.StartImmediate)

                React.useEffect
                    ((fun () ->
                        match username with
                        | Some _ -> ()
                        | None -> loadUser ()),
                     [|
                         username :> obj
                     |])

                match username with
                | Some username ->
                    React.suspense
                        ([
                            SessionDataLoader.hook {| Username = username |}
                            SoundPlayer.hook {| Username = username |}

                            NavBarComponent.render {| Username = username |}
                            PanelsComponent.render ()
                         ],
                         PageLoaderComponent.render ())

                | None -> nothing)

    let render =
        React.memo (fun () ->
            React.fragment [
                GlobalShortcutHandler.hook ()
                //                PositionUpdater.hook ()
//                AutoReload.hook_TEMP ()
                Debug.render ()
                Content.render ()
            ])
