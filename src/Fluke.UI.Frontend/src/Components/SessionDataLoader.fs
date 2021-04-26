namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.Shared
open Fluke.UI.Frontend.Bindings

module SessionDataLoader =

    open Domain.Model
    open Domain.UserInteraction
    open Domain.State


    let fetchTemplatesDatabaseStateMap () =
        let templates =
            Templates.getDatabaseMap TempData.testUser
            |> Map.toList
            |> List.map
                (fun (templateName, dslTemplate) ->
                    let databaseId =
                        templateName
                        |> Crypto.getTextGuidHash
                        |> DatabaseId

                    Templates.databaseStateFromDslTemplate TempData.testUser databaseId templateName dslTemplate)
            |> List.map
                (fun databaseState ->
                    { databaseState with
                        TaskStateMap =
                            databaseState.TaskStateMap
                            |> Map.map
                                (fun { Name = TaskName taskName } taskState ->
                                    { taskState with
                                        TaskId = taskName |> Crypto.getTextGuidHash |> TaskId
                                    })
                    })

        let databaseStateMap =
            templates
            |> List.map (fun databaseState -> databaseState.Database.Id, databaseState)
            |> Map.ofList

        databaseStateMap

    let fetchLegacyDatabaseStateMap (setter: CallbackMethods) username =
        promise {
            let! position = setter.snapshot.getPromise Recoil.Atoms.position

            return!
                match position with
                | Some position ->
                    promise {
                        let! api = setter.snapshot.getPromise Recoil.Atoms.api

                        let! databaseStateList =
                            api
                            |> Option.bind (fun api -> Some (api.databaseStateList username position))
                            |> Sync.handleRequest

                        let databaseStateMap =
                            databaseStateList
                            |> Result.defaultValue []
                            |> List.map (fun databaseState -> databaseState.Database.Id, databaseState)
                            |> Map.ofList

                        return databaseStateMap
                    }
                | _ -> promise { return Map.empty }
        }

    let initializeSessionData username (setter: CallbackMethods) sessionData =
        let recoilInformationMap =
            sessionData.TaskList
            |> Seq.map (fun task -> task.Information)
            |> Seq.distinct
            |> Seq.map (fun information -> sessionData.InformationStateMap.[information])
            |> Seq.map
                (fun informationState ->

                    let informationId = Recoil.Atoms.Information.informationId informationState.Information

                    setter.set (Recoil.Atoms.Information.wrappedInformation informationId, informationState.Information)
                    setter.set (Recoil.Atoms.Information.attachments informationId, informationState.Attachments)
                    informationState.Information, informationId)
            |> Map.ofSeq

        Profiling.addTimestamp "state.set[1]"

        sessionData.TaskList
        |> List.map (fun task -> sessionData.TaskStateMap.[task])
        |> List.iter
            (fun taskState ->
                let taskId = Some taskState.TaskId
                setter.set (Recoil.Atoms.Task.task taskId, taskState.Task)
                setter.set (Recoil.Atoms.Task.name taskId, taskState.Task.Name)
                setter.set (Recoil.Atoms.Task.informationId taskId, recoilInformationMap.[taskState.Task.Information])
                setter.set (Recoil.Atoms.Task.pendingAfter taskId, taskState.Task.PendingAfter)
                setter.set (Recoil.Atoms.Task.missedAfter taskId, taskState.Task.MissedAfter)
                setter.set (Recoil.Atoms.Task.scheduling taskId, taskState.Task.Scheduling)
                setter.set (Recoil.Atoms.Task.priority taskId, taskState.Task.Priority)
                setter.set (Recoil.Atoms.Task.attachments taskId, taskState.Attachments)
                setter.set (Recoil.Atoms.Task.duration taskId, taskState.Task.Duration)

                taskState.CellStateMap
                |> Map.filter
                    (fun _dateId cellState ->
                        (<>) cellState.Status Disabled
                        || not cellState.Attachments.IsEmpty
                        || not cellState.Sessions.IsEmpty)
                |> Map.iter
                    (fun dateId cellState ->
                        setter.set (Recoil.Atoms.Cell.status (taskState.TaskId, dateId), cellState.Status)
                        setter.set (Recoil.Atoms.Cell.attachments (taskState.TaskId, dateId), cellState.Attachments)
                        setter.set (Recoil.Atoms.Cell.sessions (taskState.TaskId, dateId), cellState.Sessions)
                        //                setter.set (Recoil.Atoms.Cell.selected (taskId, dateId), false)
                        ))

        let taskIdList =
            sessionData.TaskList
            |> List.choose
                (fun task ->
                    sessionData.TaskStateMap
                    |> Map.tryFind task
                    |> Option.map (fun x -> x.TaskId))

        setter.set (Recoil.Atoms.Session.taskIdList username, taskIdList)

    [<ReactComponent>]
    let SessionDataLoader (input: {| Username: Username |}) =
        let update =
            Recoil.useCallbackRef
                (fun (getter: CallbackMethods) (databaseStateMapCache: Map<DatabaseId, DatabaseState>) (databaseStateMap: Map<DatabaseId, DatabaseState>) ->
                    promise {
                        printfn
                            $"SessionDataLoader.updateDatabaseStateMap():
                databaseStateMapCache.Count={databaseStateMapCache.Count}
                newDatabaseStateMapCache.Count={databaseStateMap.Count}"

                        if databaseStateMapCache.Count
                           <> databaseStateMap.Count then
                            getter.set (
                                Recoil.Atoms.Session.databaseStateMapCache input.Username,
                                TempData.mergeDatabaseStateMap databaseStateMapCache databaseStateMap
                            )


                    })


        let databaseStateMapCache = Recoil.useValue (Recoil.Atoms.Session.databaseStateMapCache input.Username)
        let loaded, setLoaded = React.useState false

        let loadTemplates =
            Recoil.useCallbackRef
                (fun _ ->
                    promise {
                        if not loaded then
                            let templatesDatabaseStateMap = fetchTemplatesDatabaseStateMap ()
                            do! update databaseStateMapCache templatesDatabaseStateMap
                            setLoaded true
                    })

        React.useEffect (
            (fun () -> loadTemplates () |> Promise.start),
            [|
                box loadTemplates
            |]
        )

        let databaseStateMapCache = Recoil.useValue (Recoil.Atoms.Session.databaseStateMapCache input.Username)
        let loaded, setLoaded = React.useState false

        let loadLegacy =
            Recoil.useCallbackRef
                (fun getter ->
                    promise {
                        if not loaded then

                            let! legacyDatabaseStateMap = fetchLegacyDatabaseStateMap getter input.Username
                            do! update databaseStateMapCache legacyDatabaseStateMap
                            setLoaded true
                    })

        React.useEffect (
            (fun () -> loadLegacy () |> Promise.start),
            [|
                box loadLegacy
            |]
        )

        let sessionData = Recoil.useValue (Recoil.Selectors.Session.sessionData input.Username)

        let loadState =
            Recoil.useCallbackRef
                (fun setter ->
                    promise {
                        Profiling.addTimestamp "dataLoader.loadStateCallback[0]"

                        let availableDatabaseIds =
                            databaseStateMapCache
                            |> Map.toList
                            |> List.sortBy (fun (_id, databaseState) -> databaseState.Database.Name)
                            |> List.map fst

                        setter.set (Recoil.Atoms.Session.availableDatabaseIds input.Username, availableDatabaseIds)

                        initializeSessionData input.Username setter sessionData

                        databaseStateMapCache
                        |> Map.iter
                            (fun id databaseState ->
                                setter.set (Recoil.Atoms.Database.name (Some id), databaseState.Database.Name)
                                setter.set (Recoil.Atoms.Database.owner (Some id), Some databaseState.Database.Owner)

                                setter.set (
                                    Recoil.Atoms.Database.sharedWith (Some id),
                                    databaseState.Database.SharedWith
                                )

                                setter.set (Recoil.Atoms.Database.position (Some id), databaseState.Database.Position))


                        Profiling.addTimestamp "dataLoader.loadStateCallback[1]"
                    })


        Profiling.addTimestamp "dataLoader render"

        React.useEffect (
            (fun () ->
                Profiling.addTimestamp "dataLoader effect"
                loadState () |> Promise.start),

            // TODO: return a cleanup?
            [|
                box databaseStateMapCache
                box loadState
                box sessionData
            |]
        )

        nothing
