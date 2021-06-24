namespace Fluke.UI.Frontend.Hooks

open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend
open Fluke.Shared
open Browser.Types
open Fable.SimpleHttp
open System
open Fluke.Shared.Domain.UserInteraction
open Fable.Core


module Hydrate =
    let hydrateDatabase _get set (atomScope, database: Database) =
        promise {
            Store.scopedSet set atomScope (Atoms.Database.name, database.Id, database.Name)
            Store.scopedSet set atomScope (Atoms.Database.owner, database.Id, database.Owner)

            Store.scopedSet set atomScope (Atoms.Database.sharedWith, database.Id, database.SharedWith)

            Store.scopedSet set atomScope (Atoms.Database.position, database.Id, database.Position)
        }

    let useHydrateDatabase () =
        Store.useCallback (hydrateDatabase, [||])

    let hydrateTask _get set (atomScope, _databaseId, task: Task) =
        promise {
            //            setter.scopedSet username atomScope (Atoms.Task.databaseId, task.Id, databaseId)
            Store.scopedSet set atomScope (Atoms.Task.name, task.Id, task.Name)
            Store.scopedSet set atomScope (Atoms.Task.information, task.Id, task.Information)
            Store.scopedSet set atomScope (Atoms.Task.duration, task.Id, task.Duration)
            Store.scopedSet set atomScope (Atoms.Task.pendingAfter, task.Id, task.PendingAfter)
            Store.scopedSet set atomScope (Atoms.Task.missedAfter, task.Id, task.MissedAfter)
            Store.scopedSet set atomScope (Atoms.Task.scheduling, task.Id, task.Scheduling)
            Store.scopedSet set atomScope (Atoms.Task.priority, task.Id, task.Priority)
            Store.scopedSet set atomScope (Atoms.Task.selectionSet, task.Id, Set.empty)
        }

    let useHydrateTask () = Store.useCallback (hydrateTask, [||])

    let hydrateTaskState get set (atomScope, databaseId, taskState) =
        promise {
            do! hydrateTask get set (atomScope, databaseId, taskState.Task)

            Store.scopedSet
                set
                atomScope
                (Atoms.Task.statusMap,
                 taskState.Task.Id,
                 (taskState.CellStateMap
                  |> Seq.choose
                      (function
                      | KeyValue (dateId,
                                  {
                                      Status = UserStatus (username, userStatus)
                                  }) -> Some (dateId, (username, userStatus))
                      | _ -> None)
                  |> Map.ofSeq))

            Store.scopedSet
                set
                atomScope
                (Atoms.Task.attachmentIdSet,
                 taskState.Task.Id,
                 taskState.Attachments
                 |> List.map
                     (fun (timestamp, attachment) ->
                         let attachmentId = AttachmentId.NewId ()

                         Store.scopedSet set atomScope (Atoms.Attachment.timestamp, attachmentId, Some timestamp)

                         Store.scopedSet set atomScope (Atoms.Attachment.attachment, attachmentId, Some attachment)

                         attachmentId)
                 |> Set.ofSeq)

            Store.scopedSet
                set
                atomScope
                (Atoms.Task.cellAttachmentMap,
                 taskState.Task.Id,
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
                                     atomScope
                                     (Atoms.Attachment.timestamp, attachmentId, Some timestamp)

                                 Store.scopedSet
                                     set
                                     atomScope
                                     (Atoms.Attachment.attachment, attachmentId, Some attachment)

                                 Some attachmentId)
                         |> Set.ofSeq)
                 |> Map.ofSeq)

            Store.scopedSet set atomScope (Atoms.Task.sessions, taskState.Task.Id, taskState.Sessions)
        }

    let useHydrateTaskState () =
        Store.useCallback (hydrateTaskState, [||])


    let useHydrateDatabaseState () =
        let hydrateDatabase = useHydrateDatabase ()
        let hydrateTaskState = useHydrateTaskState ()

        Store.useCallback (
            (fun _get set (atomScope, databaseState) ->
                promise {
                    do! hydrateDatabase (atomScope, databaseState.Database)

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
                                        atomScope
                                        (Atoms.Attachment.timestamp, attachmentId, Some timestamp)

                                    Store.scopedSet
                                        set
                                        atomScope
                                        (Atoms.Attachment.attachment, attachmentId, Some attachment)))

                    do!
                        databaseState.TaskStateMap
                        |> Map.values
                        |> Seq.map (fun taskState -> hydrateTaskState (atomScope, databaseState.Database.Id, taskState))
                        |> Promise.Parallel
                        |> Promise.ignore

                    Atoms.setAtomValue
                        set
                        (Atoms.Database.taskIdSet databaseState.Database.Id)
                        (databaseState.TaskStateMap
                         |> Map.keys
                         |> Set.ofSeq)
                }),
            [|
                box hydrateDatabase
                box hydrateTaskState
            |]
        )

    let useHydrateTemplates () =
        let hydrateDatabaseState = useHydrateDatabaseState ()

        Store.useCallback (
            (fun _ set () ->
                promise {
                    let databaseStateMap = TestUser.fetchTemplatesDatabaseStateMap ()

                    do!
                        databaseStateMap
                        |> Map.values
                        |> Seq.map (fun databaseState -> hydrateDatabaseState (Store.AtomScope.ReadOnly, databaseState))
                        |> Promise.Parallel
                        |> Promise.ignore

                    Atoms.setAtomValuePrev
                        set
                        Atoms.databaseIdSet
                        (Set.union (databaseStateMap |> Map.keys |> Set.ofSeq))
                }),
            [|
                box hydrateDatabaseState
            |]
        )

    let useExportDatabase () =
        let toast = Chakra.useToast ()

        Store.useCallback (
            (fun get _set databaseId ->
                promise {
                    let database = Atoms.getAtomValue get (Selectors.Database.database databaseId)

                    let taskIdSet = Atoms.getAtomValue get (Atoms.Database.taskIdSet databaseId)

                    let taskStateList =
                        taskIdSet
                        |> Set.toList
                        |> List.map Selectors.Task.taskState
                        |> List.map (Atoms.getAtomValue get)

                    let informationStateList = Atoms.getAtomValue get Selectors.Session.informationStateList

                    let databaseState =
                        {
                            Database = database
                            InformationStateMap =
                                informationStateList
                                |> List.map (fun informationState -> informationState.Information, informationState)
                                |> Map.ofSeq
                            TaskStateMap =
                                taskStateList
                                |> List.map (fun taskState -> taskState.Task.Id, taskState)
                                |> Map.ofSeq
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
            (fun get set files ->
                promise {
                    let username = Atoms.getAtomValue get Atoms.username

                    match username, files with
                    | Some username, Some (files: FileList) ->
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
                                        promise {
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

                                            do!
                                                hydrateDatabaseState (
                                                    Store.AtomScope.ReadOnly,
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
                                                )

                                            Atoms.setAtomValuePrev set Atoms.databaseIdSet (Set.add database.Id)
                                        })
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
