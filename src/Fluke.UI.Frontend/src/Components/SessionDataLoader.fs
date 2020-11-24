namespace Fluke.UI.Frontend.Components

open FSharpPlus
open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.Shared

module SessionDataLoader =
    open Domain.Model
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

            taskState.CellStateMap
            |> Map.filter (fun dateId cellState ->
                (<>) cellState.Status Disabled
                || not cellState.Attachments.IsEmpty
                || not cellState.Sessions.IsEmpty)
            |> Map.iter (fun dateId cellState ->
                setter.set (Recoil.Atoms.Cell.status (taskId, dateId), cellState.Status)
                setter.set (Recoil.Atoms.Cell.attachments (taskId, dateId), cellState.Attachments)
                setter.set (Recoil.Atoms.Cell.sessions (taskId, dateId), cellState.Sessions)
                //                setter.set (Recoil.Atoms.Cell.selected (taskId, dateId), false)
                ))

        let taskIdList =
            sessionData.TaskList
            |> List.map Recoil.Atoms.Task.taskId

        setter.set (Recoil.Atoms.Session.taskIdList username, taskIdList)

    let render =
        React.memo (fun (input: {| Username: Username |}) ->
            let sessionData = Recoil.useValue (Recoil.Selectors.Session.sessionData input.Username)

            let loadState =
                Recoil.useCallbackRef (fun setter ->
                    async {
                        Profiling.addTimestamp "dataLoader.loadStateCallback[0]"

                        match sessionData with
                        | Some sessionData ->
                            let! databaseStateMap =
                                setter.snapshot.getAsync (Recoil.Selectors.Session.databaseStateMap input.Username)

                            let availableDatabaseIds =
                                databaseStateMap
                                |> Map.toList
                                |> List.sortBy (fun (id, databaseState) -> databaseState.Database.Name)
                                |> List.map fst

                            setter.set (Recoil.Atoms.Session.availableDatabaseIds input.Username, availableDatabaseIds)


                            initializeSessionData input.Username setter sessionData

                            databaseStateMap
                            |> Map.iter (fun id databaseState ->
                                setter.set (Recoil.Atoms.Database.name id, databaseState.Database.Name)
                                setter.set (Recoil.Atoms.Database.owner id, Some databaseState.Database.Owner)
                                setter.set (Recoil.Atoms.Database.sharedWith id, databaseState.Database.SharedWith)
                                setter.set (Recoil.Atoms.Database.position id, databaseState.Database.Position))

                        | None -> ()

                        Profiling.addTimestamp "dataLoader.loadStateCallback[1]"
                    }
                    |> Async.StartImmediate)

            Profiling.addTimestamp "dataLoader render"

            React.useEffect
                ((fun () ->
                    Profiling.addTimestamp "dataLoader effect"
                    loadState ()),

                 // TODO: return a cleanup?
                 [|
                     box sessionData
                 |])

            nothing)
