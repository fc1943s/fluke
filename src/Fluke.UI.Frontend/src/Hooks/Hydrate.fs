namespace Fluke.UI.Frontend.Hooks

open FsStore
open FsStore.Bindings.Gun
open FsStore.State
open FsStore.Hooks
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
open FsCore.BaseModel
open FsJs
open FsStore.Model
open FsStore.Utils
open FsUi.Model
open FsUi.State
open FsUi.Bindings
open FsUi.Hooks
open Feliz


module Result =
    let inline choose result =
        match result with
        | Ok value -> Some value
        | _ -> None

    let inline chooseError result =
        match result with
        | Error error -> Some error
        | _ -> None

    let inline isError result =
        match result with
        | Error _ -> true
        | _ -> false


module Hydrate =
    let inline hydrateUiState _ setter (uiState: UiState) =
        promise {
            let set atom value = Atom.set setter atom value

            set Atoms.Ui.darkMode uiState.DarkMode
            set Atoms.Ui.fontSize uiState.FontSize
        }

    let inline hydrateUserState _ setter (userState: UserState) =
        promise {
            let set atom value = Atom.set setter atom value

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
            |> Map.iter
                (fun accordionType values ->
                    set (Atoms.User.accordionHiddenFlag accordionType) (values |> Array.toList))

            userState.UIFlagMap
            |> Map.iter (Atoms.User.uiFlag >> set)

            userState.UIVisibleFlagMap
            |> Map.iter (Atoms.User.uiVisibleFlag >> set)

            Logger.logInfo (fun () -> $"setting usercolor: {userState.UserColor}")

            set Atoms.User.userColor userState.UserColor
        }

    let inline hydrateDatabase _ setter (atomScope, database: Database) =
        promise {
            TempValue.setWithScope setter atomScope (Atoms.Database.name, database.Id, database.Name)
            TempValue.setWithScope setter atomScope (Atoms.Database.owner, database.Id, database.Owner)
            TempValue.setWithScope setter atomScope (Atoms.Database.sharedWith, database.Id, database.SharedWith)
            TempValue.setWithScope setter atomScope (Atoms.Database.position, database.Id, database.Position)
        }

    let inline hydrateTask _ setter (atomScope, databaseId, task: Task) =
        promise {
            TempValue.setWithScope setter atomScope (Atoms.Task.databaseId, task.Id, databaseId)
            TempValue.setWithScope setter atomScope (Atoms.Task.name, task.Id, task.Name)
            TempValue.setWithScope setter atomScope (Atoms.Task.information, task.Id, task.Information)
            TempValue.setWithScope setter atomScope (Atoms.Task.duration, task.Id, task.Duration)
            TempValue.setWithScope setter atomScope (Atoms.Task.pendingAfter, task.Id, task.PendingAfter)
            TempValue.setWithScope setter atomScope (Atoms.Task.missedAfter, task.Id, task.MissedAfter)
            TempValue.setWithScope setter atomScope (Atoms.Task.scheduling, task.Id, task.Scheduling)
            TempValue.setWithScope setter atomScope (Atoms.Task.priority, task.Id, task.Priority)
        }

    let inline hydrateAttachmentState _getter setter (atomScope, parent, attachmentState) =
        let attachmentId = AttachmentId.NewId ()
        TempValue.setWithScope setter atomScope (Atoms.Attachment.parent, attachmentId, Some parent)

        TempValue.setWithScope
            setter
            atomScope
            (Atoms.Attachment.timestamp, attachmentId, Some attachmentState.Timestamp)

        TempValue.setWithScope setter atomScope (Atoms.Attachment.archived, attachmentId, Some attachmentState.Archived)

        TempValue.setWithScope
            setter
            atomScope
            (Atoms.Attachment.attachment, attachmentId, Some attachmentState.Attachment)

        attachmentId



    let inline hydrateTaskState getter setter (atomScope, databaseId, taskState: TaskState) =
        promise {
            do! hydrateTask getter setter (atomScope, databaseId, taskState.Task)

            TempValue.setWithScope setter atomScope (Atoms.Task.selectionSet, taskState.Task.Id, Set.empty)
            TempValue.setWithScope setter atomScope (Atoms.Task.archived, taskState.Task.Id, Some taskState.Archived)

            TempValue.setWithScope
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

            TempValue.setWithScope setter atomScope (Atoms.Task.sessions, taskState.Task.Id, taskState.SessionList)
        }

    let inline hydrateDatabaseState getter setter (atomScope, databaseState: DatabaseState) =
        promise {
            do! hydrateDatabase getter setter (atomScope, databaseState.Database)

            let hydrateOperationList =
                databaseState.FileMap
                |> Map.toList
                |> List.map
                    (fun (fileId, hexString) ->
                        let newFileId = Hydrate.hydrateFile setter hexString

                        match newFileId with
                        | Some newFileId -> Ok (fileId, newFileId)
                        | None -> Error $"Error hydrating file {fileId}")

            match hydrateOperationList |> List.filter Result.isError with
            | _ :: _ as errors -> return errors |> List.choose Result.chooseError |> Error
            | _ ->
                let newFileIdMap =
                    hydrateOperationList
                    |> List.choose Result.choose
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

                return Ok ()
        }

    let inline hydrateTemplates getter setter =
        promise {
            let databaseStateMap = TestUser.fetchTemplatesDatabaseStateMap ()

            return!
                databaseStateMap
                |> Map.values
                |> Seq.map (fun databaseState -> hydrateDatabaseState getter setter (AtomScope.Current, databaseState))
                |> Promise.all
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
                            let databaseState = Atom.get getter (Selectors.Database.databaseState databaseId)

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
                    let alias = Atom.get getter Selectors.Gun.alias

                    match alias with
                    | Some (Alias alias) ->
                        let privateKeys = Atom.get getter Selectors.Gun.privateKeys
                        let json = privateKeys |> Json.encodeFormatted

                        let timestamp =
                            (FlukeDateTime.FromDateTime DateTime.Now)
                            |> FlukeDateTime.Stringify

                        Dom.download json $"{alias}-{timestamp}-keys.json" "application/json"

                        toast
                            (fun x ->
                                x.description <- "User keys exported successfully"
                                x.title <- "Success"
                                x.status <- "success")
                    | None -> eprintf $"invalid username: {alias}"
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

                    let alias = Atom.get getter Selectors.Gun.alias

                    match alias with
                    | Some (Alias alias) ->
                        let _ = Atom.get getter Selectors.User.userState
                        let _ = Atom.get getter Selectors.Ui.uiState

                        do! Promise.sleep 4000

                        let userState = Atom.get getter Selectors.User.userState
                        let uiState = Atom.get getter Selectors.Ui.uiState

                        let json =
                            { Ui = uiState; User = userState }
                            |> Json.encodeFormatted

                        let timestamp =
                            (FlukeDateTime.FromDateTime DateTime.Now)
                            |> FlukeDateTime.Stringify

                        Dom.download json $"{alias}-{timestamp}-settings.json" "application/json"

                        toast
                            (fun x ->
                                x.description <- "User settings exported successfully"
                                x.title <- "Success"
                                x.status <- "success")
                    | None -> eprintf $"invalid alias: {alias}"
                })

    let inline useImportUserSettings () =
        let toast = Ui.useToast ()

        Store.useCallbackRef
            (fun getter setter files ->
                promise {
                    let alias = Atom.get getter Selectors.Gun.alias

                    match alias, files with
                    | Some _alias, Some (files: FileList) when files.length = 1 ->
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
                    let alias = Atom.get getter Selectors.Gun.alias

                    match alias, files with
                    | Some (Alias alias), Some (files: FileList) when files.length > 0 ->
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
                                                    Owner = Username alias
                                                    SharedWith = DatabaseAccess.Private []
                                                    Position = databaseState.Database.Position
                                                }

                                            return!
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
                                                                                                 Username alias,
                                                                                                 status
                                                                                             )
                                                                                     }
                                                                                 | cellState -> cellState)
                                                                     })
                                                             |> Map.ofSeq
                                                     })
                                        //                                            Atom.change setter Atoms.databaseIdSet (Set.add database.Id)
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
                //                                            Atom.change setter Atoms.databaseIdSet (Set.add database.Id)
                })

    let inline useHydrateTemplates () =
        let hydrateTemplatesPending, setHydrateTemplatesPending = Store.useState Atoms.Session.hydrateTemplatesPending
        let toast = Ui.useToast ()
        let alias = Store.useValue Selectors.Gun.alias

        let hydrate =
            Store.useCallbackRef
                (fun getter setter _ ->
                    promise {
                        if hydrateTemplatesPending then
                            setHydrateTemplatesPending false

                            let! hydrateResult = hydrateTemplates getter setter

                            match hydrateResult
                                  |> Array.choose Result.chooseError
                                  |> Array.toList
                                  |> List.collect id with
                            | [] ->
                                do! hydrateUiState getter setter UiState.Default

                                do!
                                    hydrateUserState
                                        getter
                                        setter
                                        { UserState.Default with
                                            Archive = Some false
                                            HideTemplates = Some false
                                            UserColor =
                                                String.Format ("#{0:X6}", Random().Next 0x1000000)
                                                |> Color
                                                |> Some
                                        }

                                toast
                                    (fun x ->
                                        x.title <- "Success"
                                        x.status <- "success"
                                        x.description <- "User registered successfully")

                                return true
                            | errors ->
                                toast (fun x -> x.description <- $"Sign up hydrate error. errors={errors}")
                                return false
                        else
                            return false
                    })

        React.useEffect (
            (fun () ->
                promise {
                    if alias.IsSome && hydrateTemplatesPending then
                        let! hydrateResult = hydrate ()
                        printfn $"hydrateResult={hydrateResult}"
                }
                |> Promise.start),
            [|
                box alias
                box hydrateTemplatesPending
                box hydrate
            |]
        )

    let inline deleteRecord getter collection guid =
        Engine.delete
            getter
            (StoreAtomPath.RecordAtomPath (
                Fluke.root,
                collection,
                [
                    guid |> string |> AtomKeyFragment
                ]
            ))
