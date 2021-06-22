namespace Fluke.UI.Frontend.Hooks

open Browser.Types
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend
open Fluke.Shared
open Fable.SimpleHttp
open System
open Fluke.Shared.Domain.UserInteraction
open Fable.Core


module Hydrate =
    let hydrateDatabase _get set (username, atomScope, database: Database) =
        promise {
            Store.scopedSet set username atomScope (Atoms.Database.name, (username, database.Id), database.Name)
            Store.scopedSet set username atomScope (Atoms.Database.owner, (username, database.Id), database.Owner)

            Store.scopedSet
                set
                username
                atomScope
                (Atoms.Database.sharedWith, (username, database.Id), database.SharedWith)

            Store.scopedSet set username atomScope (Atoms.Database.position, (username, database.Id), database.Position)
        }

    let useHydrateDatabase () =
        Store.useCallback (hydrateDatabase, [||])

    let hydrateTask _get set (username, atomScope, _databaseId, task: Task) =
        promise {
            //            setter.scopedSet username atomScope (Atoms.Task.databaseId, (username, task.Id), databaseId)
            Store.scopedSet set username atomScope (Atoms.Task.name, (username, task.Id), task.Name)
            Store.scopedSet set username atomScope (Atoms.Task.information, (username, task.Id), task.Information)
            Store.scopedSet set username atomScope (Atoms.Task.duration, (username, task.Id), task.Duration)
            Store.scopedSet set username atomScope (Atoms.Task.pendingAfter, (username, task.Id), task.PendingAfter)
            Store.scopedSet set username atomScope (Atoms.Task.missedAfter, (username, task.Id), task.MissedAfter)
            Store.scopedSet set username atomScope (Atoms.Task.scheduling, (username, task.Id), task.Scheduling)
            Store.scopedSet set username atomScope (Atoms.Task.priority, (username, task.Id), task.Priority)
            Store.scopedSet set username atomScope (Atoms.Task.selectionSet, (username, task.Id), Set.empty)
        }

    let useHydrateTask () = Store.useCallback (hydrateTask, [||])

    let hydrateTaskState get set (username, atomScope, databaseId, taskState) =
        promise {
            do! hydrateTask get set (username, atomScope, databaseId, taskState.Task)

            Store.scopedSet
                set
                username
                atomScope
                (Atoms.Task.statusMap,
                 (username, taskState.Task.Id),
                 (taskState.CellStateMap
                  |> Seq.choose
                      (function
                      | KeyValue (dateId, { Status = UserStatus (_, userStatus) }) -> Some (dateId, userStatus)
                      | _ -> None)
                  |> Map.ofSeq))

            Store.scopedSet
                set
                username
                atomScope
                (Atoms.Task.attachmentIdSet,
                 (username, taskState.Task.Id),
                 taskState.Attachments
                 |> List.map
                     (fun (timestamp, attachment) ->
                         let attachmentId = AttachmentId.NewId ()

                         Store.scopedSet
                             set
                             username
                             atomScope
                             (Atoms.Attachment.timestamp, (username, attachmentId), Some timestamp)

                         Store.scopedSet
                             set
                             username
                             atomScope
                             (Atoms.Attachment.attachment, (username, attachmentId), Some attachment)

                         attachmentId)
                 |> Set.ofList)

            Store.scopedSet
                set
                username
                atomScope
                (Atoms.Task.cellAttachmentMap,
                 (username, taskState.Task.Id),
                 taskState.CellStateMap
                 |> Seq.map
                     (fun (KeyValue (dateId, { Attachments = attachments })) ->
                         dateId,
                         attachments
                         |> List.choose
                             (fun (timestamp, attachment) ->
                                 let attachmentId = AttachmentId.NewId ()

                                 Store.scopedSet
                                     set
                                     username
                                     atomScope
                                     (Atoms.Attachment.timestamp, (username, attachmentId), Some timestamp)

                                 Store.scopedSet
                                     set
                                     username
                                     atomScope
                                     (Atoms.Attachment.attachment, (username, attachmentId), Some attachment)

                                 Some attachmentId)
                         |> Set.ofList)
                 |> Map.ofSeq)

            Store.scopedSet
                set
                username
                atomScope
                (Atoms.Task.sessions, (username, taskState.Task.Id), taskState.Sessions)
        }

    let useHydrateTaskState () =
        Store.useCallback (hydrateTaskState, [||])


    let useHydrateDatabaseState () =
        let hydrateDatabase = useHydrateDatabase ()
        let hydrateTaskState = useHydrateTaskState ()

        Store.useCallback (
            (fun _get set (username, atomScope, databaseState) ->
                promise {
                    do! hydrateDatabase (username, atomScope, databaseState.Database)

                    Atoms.setAtomValue set (Atoms.Session.databaseIdSet username) (Set.add databaseState.Database.Id)

                    databaseState.InformationStateMap
                    |> Map.values
                    |> Seq.iter
                        (fun informationState ->
                            informationState.Attachments
                            |> List.iter
                                (fun (timestamp, attachment) ->
                                    let attachmentId = AttachmentId.NewId ()

                                    Store.scopedSet
                                        set
                                        username
                                        atomScope
                                        (Atoms.Attachment.timestamp, (username, attachmentId), Some timestamp)

                                    Store.scopedSet
                                        set
                                        username
                                        atomScope
                                        (Atoms.Attachment.attachment, (username, attachmentId), Some attachment)))

                    do!
                        databaseState.TaskStateMap
                        |> Map.values
                        |> Seq.map
                            (fun taskState ->
                                promise {
                                    do! hydrateTaskState (username, atomScope, databaseState.Database.Id, taskState)

                                    Atoms.setAtomValue
                                        set
                                        (Atoms.Task.statusMap (username, taskState.Task.Id))
                                        (fun _ ->
                                            (taskState.CellStateMap
                                             |> Seq.choose
                                                 (function
                                                 | KeyValue (dateId, { Status = UserStatus (_, userStatus) }) ->
                                                     Some (dateId, userStatus)
                                                 | _ -> None)
                                             |> Map.ofSeq))

                                    Atoms.setAtomValue
                                        set
                                        (Atoms.Database.taskIdSet (username, databaseState.Database.Id))
                                        (Set.add taskState.Task.Id)
                                })
                        |> Promise.Parallel
                        |> Promise.ignore
                }),
            [|
                box hydrateDatabase
                box hydrateTaskState
            |]
        )

    let useHydrateTemplates () =
        let hydrateDatabaseState = useHydrateDatabaseState ()

        Store.useCallback (
            (fun _ _ username ->
                promise {
                    TestUser.fetchTemplatesDatabaseStateMap ()
                    |> Map.values
                    |> Seq.iter
                        (fun databaseState ->
                            hydrateDatabaseState (username, JotaiTypes.AtomScope.ReadOnly, databaseState)
                            |> Promise.start)
                }),
            [|
                box hydrateDatabaseState
            |]
        )

    let useExportDatabase () =
        let toast = Chakra.useToast ()

        Store.useCallback (
            (fun get _set (username, databaseId) ->
                promise {
                    let database = Atoms.getAtomValue get (Selectors.Database.database (username, databaseId))

                    let taskIdSet = Atoms.getAtomValue get (Selectors.Database.taskIdSet (username, databaseId))

                    let taskStateList =
                        taskIdSet
                        |> Set.toList
                        |> List.map (fun taskId -> Selectors.Task.taskState (username, taskId))
                        |> List.map (Atoms.getAtomValue get)

                    let informationStateList = Atoms.getAtomValue get (Selectors.Session.informationStateList username)

                    let databaseState =
                        {
                            Database = database
                            InformationStateMap =
                                informationStateList
                                |> List.map (fun informationState -> informationState.Information, informationState)
                                |> Map.ofList
                            TaskStateMap =
                                taskStateList
                                |> List.map (fun taskState -> taskState.Task.Id, taskState)
                                |> Map.ofList
                        }

                    if databaseState.TaskStateMap
                       |> Map.exists
                           (fun _ taskState ->
                               taskState.Task.Name
                               |> TaskName.Value
                               |> String.IsNullOrWhiteSpace
                               || taskState.Task.Information
                                  |> Information.Name
                                  |> InformationName.Value
                                  |> String.IsNullOrWhiteSpace) then
                        toast (fun x -> x.description <- "Database is not fully synced")
                    else

                        let json = databaseState |> Gun.jsonEncode

                        let timestamp =
                            (FlukeDateTime.FromDateTime DateTime.Now)
                            |> FlukeDateTime.Stringify

                        JS.download json $"{database.Name |> DatabaseName.Value}-{timestamp}.json" "application/json"

                        toast
                            (fun x ->
                                x.description <- "Database exported successfully"
                                x.title <- "Success"
                                x.status <- "success")
                }),
            [|
                box toast
            |]
        )

    let useImportDatabase () =
        let hydrateDatabaseState = useHydrateDatabaseState ()
        let toast = Chakra.useToast ()

        Store.useCallback (
            (fun _get _set (username, files) ->
                promise {
                    match files with
                    | Some (files: FileList) ->
                        let! files =
                            files
                            |> Seq.ofItems
                            |> Seq.map
                                (fun file ->
                                    async {
                                        let! content = FileReader.readFileAsText file
                                        return content
                                    })
                            |> Async.Parallel
                            |> Async.StartAsPromise

                        try
                            do!
                                files
                                |> Array.map
                                    (fun content ->
                                        let databaseState = Gun.jsonDecode<DatabaseState> content

                                        let database =
                                            let databaseName =
                                                let databaseName = databaseState.Database.Name |> DatabaseName.Value

                                                let timestamp =
                                                    DateTime.Now
                                                    |> FlukeDateTime.FromDateTime
                                                    |> FlukeDateTime.Stringify

                                                DatabaseName $"{databaseName}_{timestamp}"

                                            {
                                                Id = DatabaseId.NewId ()
                                                Name = databaseName
                                                Owner = username
                                                SharedWith = DatabaseAccess.Private []
                                                Position = databaseState.Database.Position
                                            }

                                        hydrateDatabaseState (
                                            username,
                                            JotaiTypes.AtomScope.ReadOnly,
                                            { databaseState with
                                                Database = database
                                                TaskStateMap =
                                                    databaseState.TaskStateMap
                                                    |> Map.toSeq
                                                    |> Seq.map
                                                        (fun (_, taskState) ->
                                                            let taskId = TaskId.NewId ()

                                                            taskId,
                                                            { taskState with
                                                                Task = { taskState.Task with Id = taskId }
                                                            })
                                                    |> Map.ofSeq
                                            }
                                        ))
                                |> Promise.Parallel
                                |> Promise.ignore

                            toast
                                (fun x ->
                                    x.description <- "Database imported successfully"
                                    x.title <- "Success"
                                    x.status <- "success")
                        with ex -> toast (fun x -> x.description <- $"Error importing database: ${ex.Message}")
                    | _ -> toast (fun x -> x.description <- "No files selected")
                }),
            [|
                box toast
                box hydrateDatabaseState
            |]
        )
