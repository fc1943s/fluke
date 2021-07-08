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
    let hydrateDatabase _ setter (atomScope, database: Database) =
        promise {
            Store.scopedSet setter atomScope (Atoms.Database.name, database.Id, database.Name)
            Store.scopedSet setter atomScope (Atoms.Database.owner, database.Id, database.Owner)
            Store.scopedSet setter atomScope (Atoms.Database.sharedWith, database.Id, database.SharedWith)
            Store.scopedSet setter atomScope (Atoms.Database.position, database.Id, database.Position)
        }

    let useHydrateDatabase () =
        Store.useCallback (hydrateDatabase, [||])

    let hydrateTask _ setter (atomScope, databaseId, task: Task) =
        promise {
            Store.scopedSet setter atomScope (Atoms.Task.databaseId, task.Id, databaseId)
            Store.scopedSet setter atomScope (Atoms.Task.name, task.Id, task.Name)
            Store.scopedSet setter atomScope (Atoms.Task.information, task.Id, task.Information)
            Store.scopedSet setter atomScope (Atoms.Task.duration, task.Id, task.Duration)
            Store.scopedSet setter atomScope (Atoms.Task.pendingAfter, task.Id, task.PendingAfter)
            Store.scopedSet setter atomScope (Atoms.Task.missedAfter, task.Id, task.MissedAfter)
            Store.scopedSet setter atomScope (Atoms.Task.scheduling, task.Id, task.Scheduling)
            Store.scopedSet setter atomScope (Atoms.Task.priority, task.Id, task.Priority)
            Store.scopedSet setter atomScope (Atoms.Task.selectionSet, task.Id, Set.empty)
        }

    let useHydrateTask () = Store.useCallback (hydrateTask, [||])

    let hydrateAttachment _getter setter (atomScope, (timestamp, attachment)) =
        let attachmentId = AttachmentId.NewId ()
        Store.scopedSet setter atomScope (Atoms.Attachment.timestamp, attachmentId, Some timestamp)
        Store.scopedSet setter atomScope (Atoms.Attachment.attachment, attachmentId, Some attachment)
        attachmentId

    let hydrateFile _getter setter (atomScope: Store.AtomScope, hexString: string) =
        let chunkSize = 16000
        let chunkCount = int (Math.Ceiling (float hexString.Length / float chunkSize))

        let chunks =
            JS.chunkString
                hexString
                {|
                    size = chunkSize
                    unicodeAware = false
                |}

        JS.log
            (fun () ->
                $"hydrateFile.
        base64.Length={hexString.Length}
        chunkCount={chunkCount}
        chunks.[0].Length={chunks.[0].Length}
        ")

        let fileId = FileId.NewId ()
        Store.set setter (Atoms.File.chunkCount fileId) chunkCount

        chunks
        |> Array.iteri (fun i chunk -> Store.scopedSet setter atomScope (Atoms.File.chunk, (fileId, i), chunk))

        fileId


    let hydrateTaskState getter setter (atomScope, databaseId, taskState) =
        promise {
            do! hydrateTask getter setter (atomScope, databaseId, taskState.Task)

            Store.scopedSet
                setter
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
                setter
                atomScope
                (Atoms.Task.attachmentIdSet,
                 taskState.Task.Id,
                 taskState.Attachments
                 |> List.map
                     (fun (timestamp, attachment) ->
                         hydrateAttachment getter setter (atomScope, (timestamp, attachment)))
                 |> Set.ofSeq)

            Store.scopedSet
                setter
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
                                 Some (hydrateAttachment getter setter (atomScope, (timestamp, attachment))))
                         |> Set.ofSeq)
                 |> Map.ofSeq)

            Store.scopedSet setter atomScope (Atoms.Task.sessions, taskState.Task.Id, taskState.Sessions)
        }

    let useHydrateTaskState () =
        Store.useCallback (hydrateTaskState, [||])


    let useHydrateDatabaseState () =
        let hydrateDatabase = useHydrateDatabase ()
        let hydrateTaskState = useHydrateTaskState ()

        Store.useCallback (
            (fun getter setter (atomScope, databaseState) ->
                promise {
                    do! hydrateDatabase (atomScope, databaseState.Database)

                    let informationAttachmentMap =
                        databaseState.InformationStateMap
                        |> Map.values
                        |> Seq.map
                            (fun informationState ->
                                let attachmentIdList =
                                    informationState.Attachments
                                    |> List.map
                                        (fun (timestamp, attachment) ->
                                            hydrateAttachment getter setter (atomScope, (timestamp, attachment)))

                                informationState.Information, (attachmentIdList |> Set.ofList))
                        |> Map.ofSeq

                    Store.scopedSet
                        setter
                        atomScope
                        (Atoms.Database.informationAttachmentMap, databaseState.Database.Id, informationAttachmentMap)

                    let newFileIdMap =
                        databaseState.FileMap
                        |> Map.toList
                        |> List.map
                            (fun (fileId, hexString) -> fileId, hydrateFile getter setter (atomScope, hexString))
                        |> Map.ofList

                    do!
                        databaseState.TaskStateMap
                        |> Map.values
                        |> Seq.map
                            (fun taskState ->
                                let newTaskState =
                                    { taskState with
                                        Attachments =
                                            taskState.Attachments
                                            |> List.map
                                                (fun (moment, attachment) ->
                                                    moment,
                                                    match attachment with
                                                    | Attachment.Image fileId -> Attachment.Image newFileIdMap.[fileId]
                                                    | _ -> attachment)
                                    }

                                hydrateTaskState (atomScope, databaseState.Database.Id, newTaskState))
                        |> Promise.Parallel
                        |> Promise.ignore

                //
//                    Store.set
//                        setter
//                        (Atoms.Database.taskIdSet databaseState.Database.Id)
//                        (databaseState.TaskStateMap
//                         |> Map.keys
//                         |> Set.ofSeq)
                }),
            [|
                box hydrateDatabase
                box hydrateTaskState
            |]
        )

    let useHydrateTemplates () =
        let hydrateDatabaseState = useHydrateDatabaseState ()

        Store.useCallback (
            (fun _ _ () ->
                promise {
                    let databaseStateMap = TestUser.fetchTemplatesDatabaseStateMap ()

                    do!
                        databaseStateMap
                        |> Map.values
                        |> Seq.map (fun databaseState -> hydrateDatabaseState (Store.AtomScope.Current, databaseState))
                        |> Promise.Parallel
                        |> Promise.ignore

                //                    Store.change set Atoms.databaseIdSet (Set.union (databaseStateMap |> Map.keys |> Set.ofSeq))
                }),
            [|
                box hydrateDatabaseState
            |]
        )

    let useExportDatabase () =
        let toast = Chakra.useToast ()

        Store.useCallback (
            (fun getter _ databaseId ->
                promise {
                    toast
                        (fun x ->
                            x.description <- "Fetching data..."
                            x.title <- "Loading"
                            x.status <- "warning")

                    let _firstFetch = Store.value getter (Selectors.Database.databaseState databaseId)

                    do! Promise.sleep 4000

                    let secondFetch = Store.value getter (Selectors.Database.databaseState databaseId)

                    match secondFetch with
                    | Ok databaseState ->
                        let json = databaseState |> Json.encodeFormatted

                        let timestamp =
                            (FlukeDateTime.FromDateTime DateTime.Now)
                            |> FlukeDateTime.Stringify

                        JS.download
                            json
                            $"{databaseState.Database.Name |> DatabaseName.Value}-{timestamp}.json"
                            "application/json"

                        toast
                            (fun x ->
                                x.description <- "Database exported successfully"
                                x.title <- "Success"
                                x.status <- "success")

                    | Error error -> toast (fun x -> x.description <- error)
                }),
            [|
                box toast
            |]
        )

    let useImportDatabase () =
        let hydrateDatabaseState = useHydrateDatabaseState ()
        let toast = Chakra.useToast ()

        Store.useCallback (
            (fun getter _ files ->
                promise {
                    let username = Store.value getter Store.Atoms.username

                    match username, files with
                    | Some username, Some (files: FileList) when files.length > 0 ->
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
                                            let databaseState = Json.decode<DatabaseState> content

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
                                                    Store.AtomScope.Current,
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
                                                                        CellStateMap =
                                                                            taskState.CellStateMap
                                                                            |> Map.mapValues
                                                                                (function
                                                                                | { Status = UserStatus (_, status) } as cellState ->
                                                                                    { cellState with
                                                                                        Status =
                                                                                            UserStatus (
                                                                                                username,
                                                                                                status
                                                                                            )
                                                                                    }
                                                                                | cellState -> cellState)
                                                                    })
                                                            |> Map.ofSeq
                                                    }
                                                )
                                        //                                            Store.change setter Atoms.databaseIdSet (Set.add database.Id)
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
                //                                            Store.change setter Atoms.databaseIdSet (Set.add database.Id)
                }),
            [|
                box toast
                box hydrateDatabaseState
            |]
        )
