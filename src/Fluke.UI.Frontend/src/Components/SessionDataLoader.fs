namespace Fluke.UI.Frontend.Components

open System
open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.Shared
open Fluke.UI.Frontend.Bindings
open Fable.Core

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
                |> Map.filter
                    (fun _dateId cellState ->
                        (<>) cellState.Status Disabled
                        || not cellState.Attachments.IsEmpty
                        || not cellState.Sessions.IsEmpty)
                |> Map.iter
                    (fun dateId cellState ->
                        setter.set (Recoil.Atoms.Cell.status (taskId, dateId), cellState.Status)
                        setter.set (Recoil.Atoms.Cell.attachments (taskId, dateId), cellState.Attachments)
                        setter.set (Recoil.Atoms.Cell.sessions (taskId, dateId), cellState.Sessions)
                        //                setter.set (Recoil.Atoms.Cell.selected (taskId, dateId), false)
                        ))

        let taskIdList =
            sessionData.TaskList
            |> List.map Recoil.Atoms.Task.taskId

        setter.set (Recoil.Atoms.Session.taskIdList username, taskIdList)

    let getDatabaseIdFromName name =
        name
        |> Crypto.sha3
        |> string
        |> String.take 16
        |> System.Text.Encoding.UTF8.GetBytes
        |> Guid
        |> DatabaseId

    let fetchDatabaseStateMap (setter: CallbackMethods) username =
        promise {
            let! position = setter.snapshot.getPromise Recoil.Selectors.position

            return!
                match position with
                | Some position ->
                    promise {
                        let! api = setter.snapshot.getPromise Recoil.Atoms.api

                        let! databaseStateList =
                            api
                            |> Option.bind (fun api -> Some (api.databaseStateList username position))
                            |> Sync.handleRequest
                            |> Async.StartAsPromise

                        let templates =
                            Templates.getDatabaseMap TempData.testUser
                            |> Map.toList
                            |> List.map
                                (fun (templateName, dslTemplate) ->
                                    Templates.databaseStateFromDslTemplate
                                        TempData.testUser
                                        (DatabaseId (Guid.NewGuid ()))
                                        templateName
                                        dslTemplate)


                        let newDatabaseStateList =
                            databaseStateList
                            |> Option.defaultValue []
                            |> List.append templates


                        let databaseStateMap =
                            newDatabaseStateList
                            |> List.map
                                (fun databaseState ->
                                    let (DatabaseName databaseName) = databaseState.Database.Name
                                    let id = getDatabaseIdFromName databaseName
                                    id, databaseState)
                            |> Map.ofList

                        return databaseStateMap
                    }
                | _ -> promise { return Map.empty }
        }


    [<ReactComponent>]
    let SessionDataLoader (username: Username) =

        let databaseStateMapCache = Recoil.useValue (Recoil.Atoms.Session.databaseStateMapCache username)

        let loaded, setLoaded = React.useState false

        let update =
            Recoil.useCallbackRef
                (fun getter ->
                    promise {
                        if not loaded then
                            let! databaseStateMap = fetchDatabaseStateMap getter username

                            printfn
                                $"SessionDataLoader.updateDatabaseStateMap():
                databaseStateMapCache.Count={databaseStateMapCache.Count}
                newDatabaseStateMapCache.Count={databaseStateMap.Count}"

                            if databaseStateMapCache.Count
                               <> databaseStateMap.Count then
                                getter.set (
                                    Recoil.Atoms.Session.databaseStateMapCache username,
                                    TempData.mergeDatabaseStateMap databaseStateMapCache databaseStateMap
                                )

                                setLoaded true

                    })

        React.useEffect (
            (fun () -> update () |> Promise.start),
            [|
                box loaded
                box setLoaded
                box databaseStateMapCache
                box update
            |]
        )

        let sessionData = Recoil.useValue (Recoil.Selectors.Session.sessionData username)

        let loadState =
            Recoil.useCallbackRef
                (fun setter ->
                    promise {
                        Profiling.addTimestamp "dataLoader.loadStateCallback[0]"

                        match sessionData with
                        | Some sessionData ->
                            let availableDatabaseIds =
                                databaseStateMapCache
                                |> Map.toList
                                |> List.sortBy (fun (_id, databaseState) -> databaseState.Database.Name)
                                |> List.map fst

                            setter.set (Recoil.Atoms.Session.availableDatabaseIds username, availableDatabaseIds)

                            initializeSessionData username setter sessionData

                            databaseStateMapCache
                            |> Map.iter
                                (fun id databaseState ->
                                    setter.set (Recoil.Atoms.Database.name id, databaseState.Database.Name)
                                    setter.set (Recoil.Atoms.Database.owner id, Some databaseState.Database.Owner)
                                    setter.set (Recoil.Atoms.Database.sharedWith id, databaseState.Database.SharedWith)
                                    setter.set (Recoil.Atoms.Database.position id, databaseState.Database.Position))

                        | _ -> ()

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
