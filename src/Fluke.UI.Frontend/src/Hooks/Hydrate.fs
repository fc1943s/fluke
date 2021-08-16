namespace Fluke.UI.Frontend.Hooks

open FsStore
open FsCore
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.State.State
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend
open Fluke.Shared
open Browser.Types
open Fable.SimpleHttp
open System
open Fluke.Shared.Domain.UserInteraction
open Fable.Core
open FsCore.Model
open FsJs
open FsStore.Hooks
open FsStore.Model
open FsUi.Model
open FsUi.State
open FsUi.Bindings


module Hydrate =
    let inline hydrateUiState _ setter (uiState: UiState) =
        promise {
            let set atom value = Store.set setter atom value

            set Atoms.Ui.darkMode uiState.DarkMode
            set Atoms.Ui.fontSize uiState.FontSize
        }

    let inline hydrateUserState _ setter (userState: UserState) =
        promise {
            let set atom value = Store.set setter atom value

            set Atoms.User.archive userState.Archive
            set Atoms.User.cellColorDisabled userState.CellColorDisabled
            set Atoms.User.cellColorSuggested userState.CellColorSuggested
            set Atoms.User.cellColorPending userState.CellColorPending
            set Atoms.User.cellColorMissed userState.CellColorMissed
            set Atoms.User.cellColorMissedToday userState.CellColorMissedToday
            set Atoms.User.cellColorPostponedUntil userState.CellColorPostponedUntil
            set Atoms.User.cellColorPostponed userState.CellColorPostponed
            set Atoms.User.cellColorCompleted userState.CellColorCompleted
            set Atoms.User.cellColorDismissed userState.CellColorDismissed
            set Atoms.User.cellColorScheduled userState.CellColorScheduled
            set Atoms.User.cellSize userState.CellSize
            set Atoms.User.clipboardAttachmentIdMap userState.ClipboardAttachmentIdMap
            set Atoms.User.clipboardVisible userState.ClipboardVisible
            set Atoms.User.daysAfter userState.DaysAfter
            set Atoms.User.daysBefore userState.DaysBefore
            set Atoms.User.dayStart userState.DayStart
            set Atoms.User.enableCellPopover userState.EnableCellPopover
            set Atoms.User.expandedDatabaseIdSet userState.ExpandedDatabaseIdSet
            set Atoms.User.filter userState.Filter
            set Atoms.User.hideSchedulingOverlay userState.HideSchedulingOverlay
            set Atoms.User.hideTemplates userState.HideTemplates
            set Atoms.User.language userState.Language
            set Atoms.User.lastDatabaseSelected userState.LastDatabaseSelected
            set Atoms.User.leftDock userState.LeftDock
            set Atoms.User.leftDockSize userState.LeftDockSize
            set Atoms.User.randomizeProject userState.RandomizeProject
            set Atoms.User.randomizeProjectAttachment userState.RandomizeProjectAttachment
            set Atoms.User.randomizeArea userState.RandomizeArea
            set Atoms.User.randomizeAreaAttachment userState.RandomizeAreaAttachment
            set Atoms.User.randomizeResource userState.RandomizeResource
            set Atoms.User.randomizeResourceAttachment userState.RandomizeResourceAttachment
            set Atoms.User.randomizeProjectTask userState.RandomizeProjectTask
            set Atoms.User.randomizeAreaTask userState.RandomizeAreaTask
            set Atoms.User.randomizeProjectTaskAttachment userState.RandomizeProjectTaskAttachment
            set Atoms.User.randomizeAreaTaskAttachment userState.RandomizeAreaTaskAttachment
            set Atoms.User.randomizeCellAttachment userState.RandomizeCellAttachment
            set Atoms.User.rightDock userState.RightDock
            set Atoms.User.rightDockSize userState.RightDockSize
            set Atoms.User.searchText userState.SearchText
            set Atoms.User.selectedDatabaseIdSet userState.SelectedDatabaseIdSet
            set Atoms.User.sessionBreakDuration userState.SessionBreakDuration
            set Atoms.User.sessionDuration userState.SessionDuration
            set Atoms.User.view userState.View
            set Atoms.User.weekStart userState.WeekStart

            userState.AccordionHiddenFlagMap
            |> Map.iter (Atoms.User.accordionHiddenFlag >> set)

            userState.UIFlagMap
            |> Map.iter (Atoms.User.uiFlag >> set)

            userState.UIVisibleFlagMap
            |> Map.iter (Atoms.User.uiVisibleFlag >> set)

            JS.setTimeout (fun () -> set Atoms.User.userColor userState.UserColor) 0
            |> ignore
        }

    let inline hydrateDatabase _ setter (atomScope, database: Database) =
        promise {
            Store.scopedSet setter atomScope (Atoms.Database.name, database.Id, database.Name)
            Store.scopedSet setter atomScope (Atoms.Database.owner, database.Id, database.Owner)
            Store.scopedSet setter atomScope (Atoms.Database.sharedWith, database.Id, database.SharedWith)
            Store.scopedSet setter atomScope (Atoms.Database.position, database.Id, database.Position)
        }

    let inline hydrateTask _ setter (atomScope, databaseId, task: Task) =
        promise {
            Store.scopedSet setter atomScope (Atoms.Task.databaseId, task.Id, databaseId)
            Store.scopedSet setter atomScope (Atoms.Task.name, task.Id, task.Name)
            Store.scopedSet setter atomScope (Atoms.Task.information, task.Id, task.Information)
            Store.scopedSet setter atomScope (Atoms.Task.duration, task.Id, task.Duration)
            Store.scopedSet setter atomScope (Atoms.Task.pendingAfter, task.Id, task.PendingAfter)
            Store.scopedSet setter atomScope (Atoms.Task.missedAfter, task.Id, task.MissedAfter)
            Store.scopedSet setter atomScope (Atoms.Task.scheduling, task.Id, task.Scheduling)
            Store.scopedSet setter atomScope (Atoms.Task.priority, task.Id, task.Priority)
        }

    let inline hydrateAttachmentState _getter setter (atomScope, parent, attachmentState) =
        let attachmentId = AttachmentId.NewId ()
        Store.scopedSet setter atomScope (Atoms.Attachment.parent, attachmentId, Some parent)
        Store.scopedSet setter atomScope (Atoms.Attachment.timestamp, attachmentId, Some attachmentState.Timestamp)
        Store.scopedSet setter atomScope (Atoms.Attachment.archived, attachmentId, Some attachmentState.Archived)
        Store.scopedSet setter atomScope (Atoms.Attachment.attachment, attachmentId, Some attachmentState.Attachment)
        attachmentId



    let inline hydrateTaskState getter setter (atomScope, databaseId, taskState: TaskState) =
        promise {
            do! hydrateTask getter setter (atomScope, databaseId, taskState.Task)

            Store.scopedSet setter atomScope (Atoms.Task.selectionSet, taskState.Task.Id, Set.empty)
            Store.scopedSet setter atomScope (Atoms.Task.archived, taskState.Task.Id, Some taskState.Archived)

            Store.scopedSet
                setter
                atomScope
                (Atoms.Task.statusMap,
                 taskState.Task.Id,
                 (taskState.CellStateMap
                  |> Map.choose
                      (function
                      | dateId,
                        {
                            Status = UserStatus (username, userStatus)
                        } -> Some (dateId, (username, userStatus))
                      | _ -> None)))

            let _attachmentIdList =
                taskState.AttachmentStateList
                |> List.map
                    (fun attachmentState ->
                        hydrateAttachmentState
                            getter
                            setter
                            (atomScope, AttachmentParent.Task taskState.Task.Id, attachmentState))

            let _attachmentIdList =
                taskState.CellStateMap
                |> Map.toList
                |> List.map
                    (fun (dateId, cellState) ->
                        dateId,
                        cellState.AttachmentStateList
                        |> List.choose
                            (fun attachmentState ->
                                Some (
                                    hydrateAttachmentState
                                        getter
                                        setter
                                        (atomScope, AttachmentParent.Cell (taskState.Task.Id, dateId), attachmentState)
                                ))
                        |> Set.ofSeq)

            Store.scopedSet setter atomScope (Atoms.Task.sessions, taskState.Task.Id, taskState.SessionList)
        }

    let inline hydrateDatabaseState getter setter (atomScope, databaseState: DatabaseState) =
        promise {
            do! hydrateDatabase getter setter (atomScope, databaseState.Database)

            let newFileIdMap =
                databaseState.FileMap
                |> Map.toList
                |> List.map
                    (fun (fileId, hexString) ->
                        let newFileId = Hydrate.hydrateFile setter (atomScope, hexString)
                        fileId, newFileId)
                |> Map.ofList

            let changeFileIds attachmentStateList =
                attachmentStateList
                |> List.map
                    (fun attachmentState ->
                        { attachmentState with
                            Attachment =
                                match attachmentState.Attachment with
                                | Attachment.Image fileId -> Attachment.Image newFileIdMap.[fileId]
                                | _ -> attachmentState.Attachment
                        })

            let _newInformationAttachmentIdMap =
                databaseState.InformationStateMap
                |> Map.values
                |> Seq.map
                    (fun informationState ->
                        let attachmentIdList =
                            informationState.AttachmentStateList
                            |> changeFileIds
                            |> List.map
                                (fun attachmentState ->
                                    hydrateAttachmentState
                                        getter
                                        setter
                                        (atomScope,
                                         AttachmentParent.Information (
                                             databaseState.Database.Id,
                                             informationState.Information
                                         ),
                                         attachmentState))

                        informationState.Information, (attachmentIdList |> Set.ofList))
                |> Map.ofSeq

            do!
                databaseState.TaskStateMap
                |> Map.values
                |> Seq.map
                    (fun taskState ->
                        let newTaskState =
                            { taskState with
                                CellStateMap =
                                    taskState.CellStateMap
                                    |> Map.map
                                        (fun _ cellState ->
                                            { cellState with
                                                AttachmentStateList = cellState.AttachmentStateList |> changeFileIds
                                            })
                                AttachmentStateList = taskState.AttachmentStateList |> changeFileIds
                            }

                        hydrateTaskState getter setter (atomScope, databaseState.Database.Id, newTaskState))
                |> Promise.all
                |> Promise.ignore
        }

    let inline hydrateTemplates getter setter =
        promise {
            let databaseStateMap = TestUser.fetchTemplatesDatabaseStateMap ()

            do!
                databaseStateMap
                |> Map.values
                |> Seq.map (fun databaseState -> hydrateDatabaseState getter setter (AtomScope.Current, databaseState))
                |> Promise.all
                |> Promise.ignore
        }

    let inline useExportDatabase () =
        let toast = Ui.useToast ()

        Store.useCallbackRef
            (fun getter _ databaseId ->
                promise {
                    toast
                        (fun x ->
                            x.description <- "Fetching data..."
                            x.title <- "Loading"
                            x.status <- "warning")

                    let attempts = 20

                    let rec loop attemptsLeft =
                        promise {
                            let databaseState = Store.value getter (Selectors.Database.databaseState databaseId)

                            match databaseState with
                            | _ when attemptsLeft = attempts ->
                                do! Promise.sleep 4000
                                return! loop (attemptsLeft - 1)
                            | Ok databaseState -> return Ok databaseState
                            | Error error ->
                                printfn $"attemptsLeft={attemptsLeft} error={error}"

                                if attemptsLeft = 0 then
                                    return Error error
                                else
                                    do! Promise.sleep 4000
                                    return! loop (attemptsLeft - 1)
                        }

                    match! loop attempts with
                    | Ok databaseState ->
                        let newDatabaseState =
                            {|
                                Database = databaseState.Database
                                InformationStateMap = databaseState.InformationStateMap
                                TaskStateMap =
                                    databaseState.TaskStateMap
                                    |> Map.toList
                                    |> List.sortBy (fun (_, taskState) -> taskState.Task.Name |> TaskName.Value)
                                FileMap = databaseState.FileMap
                            |}

                        let json = newDatabaseState |> Json.encodeFormatted

                        let timestamp =
                            (FlukeDateTime.FromDateTime DateTime.Now)
                            |> FlukeDateTime.Stringify

                        Dom.download
                            json
                            $"{databaseState.Database.Name |> DatabaseName.Value}-{timestamp}.json"
                            "application/json"

                        toast
                            (fun x ->
                                x.description <- "Database exported successfully"
                                x.title <- "Success"
                                x.status <- "success")

                    | Error error -> toast (fun x -> x.description <- error)
                })

    type SettingsState = { Ui: UiState; User: UserState }

    let inline useExportUserKey () =
        let toast = Ui.useToast ()

        Store.useCallbackRef
            (fun getter _ () ->
                promise {
                    let username = Store.value getter Atoms.username

                    match username with
                    | Some username ->
                        let gunKeys = Store.value getter Atoms.gunKeys
                        let json = gunKeys |> Json.encodeFormatted

                        let timestamp =
                            (FlukeDateTime.FromDateTime DateTime.Now)
                            |> FlukeDateTime.Stringify

                        Dom.download
                            json
                            $"{username |> Username.ValueOrDefault}-{timestamp}-keys.json"
                            "application/json"

                        toast
                            (fun x ->
                                x.description <- "User keys exported successfully"
                                x.title <- "Success"
                                x.status <- "success")
                    | None -> eprintf $"invalid username: {username}"
                })

    let inline useExportUserSettings () =
        let toast = Ui.useToast ()

        Store.useCallbackRef
            (fun getter _ () ->
                promise {
                    toast
                        (fun x ->
                            x.description <- "Fetching data..."
                            x.title <- "Loading"
                            x.status <- "warning")

                    let username = Store.value getter Atoms.username

                    match username with
                    | Some username ->
                        let _ = Store.value getter Selectors.User.userState
                        let _ = Store.value getter Selectors.Ui.uiState

                        do! Promise.sleep 4000

                        let userState = Store.value getter Selectors.User.userState
                        let uiState = Store.value getter Selectors.Ui.uiState

                        let json =
                            { Ui = uiState; User = userState }
                            |> Json.encodeFormatted

                        let timestamp =
                            (FlukeDateTime.FromDateTime DateTime.Now)
                            |> FlukeDateTime.Stringify

                        Dom.download
                            json
                            $"{username |> Username.ValueOrDefault}-{timestamp}-settings.json"
                            "application/json"

                        toast
                            (fun x ->
                                x.description <- "User settings exported successfully"
                                x.title <- "Success"
                                x.status <- "success")
                    | None -> eprintf $"invalid username: {username}"
                })

    let inline useImportUserSettings () =
        let toast = Ui.useToast ()

        Store.useCallbackRef
            (fun getter setter files ->
                promise {
                    let username = Store.value getter Atoms.username

                    match username, files with
                    | Some _username, Some (files: FileList) when files.length = 1 ->
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
                            let content = files.[0]

                            let state = Json.decode<SettingsState> content

                            do! hydrateUiState getter setter state.Ui
                            do! hydrateUserState getter setter state.User

                            toast
                                (fun x ->
                                    x.description <- "User settings imported successfully"
                                    x.title <- "Success"
                                    x.status <- "success")
                        with
                        | ex -> toast (fun x -> x.description <- $"Error importing settings: ${ex.Message}")
                    | _ -> toast (fun x -> x.description <- "No files selected")
                })

    let inline useImportDatabase () =
        let toast = Ui.useToast ()

        Store.useCallbackRef
            (fun getter setter files ->
                promise {
                    let username = Store.value getter Atoms.username

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
                                                hydrateDatabaseState
                                                    getter
                                                    setter
                                                    (AtomScope.Current,
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
                                                     })
                                        //                                            Store.change setter Atoms.databaseIdSet (Set.add database.Id)
                                        })
                                |> Promise.all
                                |> Promise.ignore

                            toast
                                (fun x ->
                                    x.description <- "Database imported successfully"
                                    x.title <- "Success"
                                    x.status <- "success")
                        with
                        | ex -> toast (fun x -> x.description <- $"Error importing database: ${ex.Message}")
                    | _ -> toast (fun x -> x.description <- "No files selected")
                //                                            Store.change setter Atoms.databaseIdSet (Set.add database.Id)
                })
