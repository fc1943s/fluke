namespace Fluke.UI.Frontend.Hooks

open FSharpPlus
open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.Shared

module SessionDataLoader =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    let initializeSessionData username (setter: CallbackMethods) sessionData =
        let recoilInformationMap =
            sessionData.TaskList
            |> Seq.map (fun task -> task.Information)
            |> Seq.distinct
            |> Seq.map (fun information -> sessionData.InformationStateMap.[information])
            |> Seq.map (fun informationState ->

                let informationId = Recoil.Atoms.Information.informationId informationState.Information

                setter.set (Recoil.Atoms.Information.wrappedInformation informationId, informationState.Information)
                setter.set (Recoil.Atoms.Information.attachments informationId, informationState.Attachments)
                informationState.Information, informationId)
            |> Map.ofSeq

        Profiling.addTimestamp "state.set[1]"

        sessionData.TaskList
        |> List.map (fun task -> sessionData.TaskStateMap.[task])
        |> List.iter (fun taskState ->
            let taskId = Recoil.Atoms.Task.taskId taskState.Task
            setter.set (Recoil.Atoms.Task.name taskId, taskState.Task.Name)

            setter.set (Recoil.Atoms.Task.informationId taskId, recoilInformationMap.[taskState.Task.Information])

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
                setter.set (Recoil.Atoms.Cell.status (taskId, dateId), cellState.Status)
                setter.set (Recoil.Atoms.Cell.attachments (taskId, dateId), cellState.Attachments)
                setter.set (Recoil.Atoms.Cell.sessions (taskId, dateId), cellState.Sessions)
                setter.set (Recoil.Atoms.Cell.selected (taskId, dateId), false)))

        let taskIdList =
            sessionData.TaskList
            |> List.map Recoil.Atoms.Task.taskId

        setter.set (Recoil.Atoms.Session.taskIdList username, taskIdList)

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
                        Profiling.addTimestamp "dataLoader.loadStateCallback[0]"

                        match sessionData with
                        | Some sessionData ->
                            initializeSessionData input.Username setter sessionData

                            treeStateMap
                            |> Map.iter (fun id treeState ->
                                setter.set (Recoil.Atoms.Tree.name id, treeState.Name)
                                setter.set (Recoil.Atoms.Tree.owner id, Some treeState.Owner)
                                setter.set (Recoil.Atoms.Tree.sharedWith id, treeState.SharedWith)
                                setter.set (Recoil.Atoms.Tree.position id, treeState.Position))


                        //                                    printfn
//                                        "MainComponent.SessionDataLoader.hook.loadState -> Atoms.Session.taskIdList[.Length] <- %A"
//                                        taskIdList.Length

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


                        Profiling.addTimestamp "dataLoader.loadStateCallback[1]"

                        //                            setter.set (Recoil.Selectors.Session.state input.Username, state)

                        Profiling.addTimestamp "dataLoader.loadStateCallback[2]"
                    }
                    |> Async.StartImmediate)

            Profiling.addTimestamp "dataLoader render"
            React.useEffect
                ((fun () ->
                    Profiling.addTimestamp "dataLoader effect"
                    loadState ()),

                 // TODO: return a cleanup?
                 [|
                     sessionData :> obj
                 |])

            nothing)

