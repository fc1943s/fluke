namespace Fluke.UI.Frontend

#nowarn "40"

open System
open Fluke.Shared
open Fluke.Shared.Domain
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fable.DateFunctions
open Fable.Core.JsInterop
open Fable.Core


module State =
    open Model
    open Domain.UserInteraction
    open Domain.State
    open View

    type TextKey = TextKey of key: string

    and TextKey with
        static member inline Value (TextKey key) = key

    [<RequireQualifiedAccess>]
    type Join =
        | Database of DatabaseId
        | Task of DatabaseId * TaskId

    module Atoms =
        let rec debug = JotaiUtils.atomWithStorage $"{nameof debug}" JS.isDebug
        let rec sessionRestored = Store.atomWithProfiling ($"{nameof sessionRestored}", false)
        let rec initialPeerSkipped = Store.atomWithProfiling ($"{nameof initialPeerSkipped}", false)
        let rec position = Store.atomWithProfiling ($"{nameof position}", None)
        let rec ctrlPressed = Store.atomWithProfiling ($"{nameof ctrlPressed}", false)
        let rec shiftPressed = Store.atomWithProfiling ($"{nameof shiftPressed}", false)


        //        module rec Events =
//            type EventId = EventId of position: float * guid: Guid
//
//            let newEventId () =
//                EventId (JS.Constructors.Date.now (), Guid.NewGuid ())
//
//            [<RequireQualifiedAccess>]
//            type Event =
//                | AddDatabase of id: EventId * name: DatabaseName * dayStart: FlukeTime
//                | AddTask of id: EventId * name: TaskName
//                | NoOp
//
//            let rec events =
//                Store.atomFamilyWithProfiling (
//                    $"{nameof Events}/{nameof events}",
//                    (fun (_eventId: EventId) -> Event.NoOp)
//                )


        module rec User =
            let rec expandedDatabaseIdSet =
                Store.atomFamilyWithProfiling (
                    $"{nameof User}/{nameof expandedDatabaseIdSet}",
                    (fun (_username: Username) -> Set.empty: Set<DatabaseId>),
                    (fun (username: Username) -> Some (username, []))
                )

            let rec selectedDatabaseIdSet =
                Store.atomFamilyWithProfiling (
                    $"{nameof User}/{nameof selectedDatabaseIdSet}",
                    (fun (_username: Username) -> Set.empty: Set<DatabaseId>),
                    (fun (username: Username) -> Some (username, []))
                )

            let rec view =
                Store.atomFamilyWithProfiling (
                    $"{nameof User}/{nameof view}",
                    (fun (_username: Username) -> TempUI.defaultView),
                    (fun (username: Username) -> Some (username, []))
                )

            let rec language =
                Store.atomFamilyWithProfiling (
                    $"{nameof User}/{nameof language}",
                    (fun (_username: Username) -> Language.English),
                    (fun (username: Username) -> Some (username, []))
                )

            let rec color =
                Store.atomFamilyWithProfiling (
                    $"{nameof User}/{nameof color}",
                    (fun (_username: Username) -> String.Format ("#{0:X6}", Random().Next 0x1000000)),
                    (fun (username: Username) -> Some (username, []))
                )

            let rec weekStart =
                Store.atomFamilyWithProfiling (
                    $"{nameof User}/{nameof weekStart}",
                    (fun (_username: Username) -> DayOfWeek.Sunday),
                    (fun (username: Username) -> Some (username, []))
                )

            let rec dayStart =
                Store.atomFamilyWithProfiling (
                    $"{nameof User}/{nameof dayStart}",
                    (fun (_username: Username) -> FlukeTime.Create 0 0),
                    (fun (username: Username) -> Some (username, []))
                )

            let rec sessionDuration =
                Store.atomFamilyWithProfiling (
                    $"{nameof User}/{nameof sessionDuration}",
                    (fun (_username: Username) -> Minute 25),
                    (fun (username: Username) -> Some (username, []))
                )

            let rec sessionBreakDuration =
                Store.atomFamilyWithProfiling (
                    $"{nameof User}/{nameof sessionBreakDuration}",
                    (fun (_username: Username) -> Minute 5),
                    (fun (username: Username) -> Some (username, []))
                )

            let rec daysBefore =
                Store.atomFamilyWithProfiling (
                    $"{nameof User}/{nameof daysBefore}",
                    (fun (_username: Username) -> 7),
                    (fun (username: Username) -> Some (username, []))

                )

            let rec daysAfter =
                Store.atomFamilyWithProfiling (
                    $"{nameof User}/{nameof daysAfter}",
                    (fun (_username: Username) -> 7),
                    (fun (username: Username) -> Some (username, []))
                )

            let rec searchText =
                Store.atomFamilyWithProfiling (
                    $"{nameof User}/{nameof searchText}",
                    (fun (_username: Username) -> ""),
                    (fun (username: Username) -> Some (username, []))
                )

            let rec cellSize =
                Store.atomFamilyWithProfiling (
                    $"{nameof User}/{nameof cellSize}",
                    (fun (_username: Username) -> 23),
                    (fun (username: Username) -> Some (username, []))
                )

            let rec leftDock =
                Store.atomFamilyWithProfiling (
                    $"{nameof User}/{nameof leftDock}",
                    (fun (_username: Username) -> None: TempUI.DockType option),
                    (fun (username: Username) -> Some (username, []))
                )

            let rec rightDock =
                Store.atomFamilyWithProfiling (
                    $"{nameof User}/{nameof rightDock}",
                    (fun (_username: Username) -> None: TempUI.DockType option),
                    (fun (username: Username) -> Some (username, []))
                )

            let rec hideTemplates =
                Store.atomFamilyWithProfiling (
                    $"{nameof User}/{nameof hideTemplates}",
                    (fun (_username: Username) -> false),
                    (fun (username: Username) -> Some (username, []))
                )

            let rec hideSchedulingOverlay =
                Store.atomFamilyWithProfiling (
                    $"{nameof User}/{nameof hideSchedulingOverlay}",
                    (fun (_username: Username) -> false),
                    (fun (username: Username) -> Some (username, []))
                )

            let rec showViewOptions =
                Store.atomFamilyWithProfiling (
                    $"{nameof User}/{nameof showViewOptions}",
                    (fun (_username: Username) -> false),
                    (fun (username: Username) -> Some (username, []))
                )

            let rec filterTasksByView =
                Store.atomFamilyWithProfiling (
                    $"{nameof User}/{nameof filterTasksByView}",
                    (fun (_username: Username) -> true),
                    (fun (username: Username) -> Some (username, []))
                )

            let rec informationAttachmentMap =
                Store.atomFamilyWithProfiling (
                    $"{nameof User}/{nameof informationAttachmentMap}",
                    (fun (_username: Username) -> Map.empty: Map<Information, Set<AttachmentId>>),
                    (fun (username: Username) -> Some (username, []))
                )

            [<RequireQualifiedAccess>]
            type UIFlag =
                | None
                | Database of DatabaseId
                | Information of Information
                | Task of DatabaseId * TaskId
                | Cell of TaskId * DateId

            [<RequireQualifiedAccess>]
            type UIFlagType =
                | Database
                | Information
                | Task
                | Cell

            let rec uiFlag =
                Store.atomFamilyWithProfiling (
                    $"{nameof User}/{nameof uiFlag}",
                    (fun (_username: Username, _uiFlagType: UIFlagType) -> UIFlag.None: UIFlag),
                    (fun (username: Username, uiFlagType: UIFlagType) ->
                        Some (username, uiFlagType |> string |> List.singleton))
                )

            let rec uiVisibleFlag =
                Store.atomFamilyWithProfiling (
                    $"{nameof User}/{nameof uiVisibleFlag}",
                    (fun (_username: Username, _uiFlagType: UIFlagType) -> false),
                    (fun (username: Username, uiFlagType: UIFlagType) ->
                        Some (username, uiFlagType |> string |> List.singleton))
                )

            let rec accordionFlag =
                Store.atomFamilyWithProfiling (
                    $"{nameof User}/{nameof accordionFlag}",
                    (fun (_username: Username, _key: TextKey) -> [||]: string []),
                    (fun (username: Username, key: TextKey) -> Some (username, key |> TextKey.Value |> List.singleton))
                )


        module rec Database =
            let databaseIdIdentifier (databaseId: DatabaseId) =
                databaseId
                |> DatabaseId.Value
                |> string
                |> List.singleton

            let rec taskIdSet =
                Store.atomFamilyWithProfiling (
                    $"{nameof Database}/{nameof taskIdSet}",
                    (fun (_username: Username, _databaseId: DatabaseId) -> Set.empty: Set<TaskId>),
                    (fun (username: Username, databaseId: DatabaseId) ->
                        Some (username, databaseIdIdentifier databaseId))
                )

            let rec name =
                Store.atomFamilyWithProfiling (
                    $"{nameof Database}/{nameof name}",
                    (fun (_username: Username, _databaseId: DatabaseId) -> Database.Default.Name),
                    (fun (username: Username, databaseId: DatabaseId) ->
                        Some (username, databaseIdIdentifier databaseId))
                )

            let rec owner =
                Store.atomFamilyWithProfiling (
                    $"{nameof Database}/{nameof owner}",
                    (fun (_username: Username, _databaseId: DatabaseId) -> Database.Default.Owner),
                    (fun (username: Username, databaseId: DatabaseId) ->
                        Some (username, databaseIdIdentifier databaseId))
                )

            let rec sharedWith =
                Store.atomFamilyWithProfiling (
                    $"{nameof Database}/{nameof sharedWith}",
                    (fun (_username: Username, _databaseId: DatabaseId) -> Database.Default.SharedWith),
                    (fun (username: Username, databaseId: DatabaseId) ->
                        Some (username, databaseIdIdentifier databaseId))
                )

            let rec position =
                Store.atomFamilyWithProfiling (
                    $"{nameof Database}/{nameof position}",
                    (fun (_username: Username, _databaseId: DatabaseId) -> Database.Default.Position),
                    (fun (username: Username, databaseId: DatabaseId) ->
                        Some (username, databaseIdIdentifier databaseId))
                )


        module rec Attachment =
            let attachmentIdIdentifier (attachmentId: AttachmentId) =
                attachmentId
                |> AttachmentId.Value
                |> string
                |> List.singleton

            let rec timestamp =
                Store.atomFamilyWithProfiling (
                    $"{nameof Attachment}/{nameof timestamp}",
                    (fun (_username: Username, _attachmentId: AttachmentId) -> None: FlukeDateTime option),
                    (fun (username: Username, attachmentId: AttachmentId) ->
                        Some (username, attachmentIdIdentifier attachmentId))
                )

            let rec attachment =
                Store.atomFamilyWithProfiling (
                    $"{nameof Attachment}/{nameof attachment}",
                    (fun (_username: Username, _attachmentId: AttachmentId) -> None: Attachment option),
                    (fun (username: Username, attachmentId: AttachmentId) ->
                        Some (username, attachmentIdIdentifier attachmentId))
                )


        module rec Task =
            let taskIdIdentifier (taskId: TaskId) =
                taskId |> TaskId.Value |> string |> List.singleton

            let rec statusMap =
                Store.atomFamilyWithProfiling (
                    $"{nameof Task}/{nameof statusMap}",
                    (fun (_username: Username, _taskId: TaskId) -> Map.empty: Map<DateId, ManualCellStatus>),
                    (fun (username: Username, taskId: TaskId) -> Some (username, taskIdIdentifier taskId))
                )

            //            let rec databaseId =
//                Store.atomFamilyWithProfiling (
//                    $"{nameof Task}/{nameof databaseId}",
//                    (fun (_username: Username, _taskId: TaskId) -> Database.Default.Id),
//                    (fun (username: Username, taskId: TaskId) ->
//                        [
//                            Store.gunEffect
//                                (Store.InputAtom.Atom
//                               (askIdIdentifier taskId)
//                        ])
//                )

            let rec sessions =
                Store.atomFamilyWithProfiling (
                    $"{nameof Task}/{nameof sessions}",
                    (fun (_username: Username, _taskId: TaskId) -> []: Session list),
                    (fun (username: Username, taskId: TaskId) -> Some (username, taskIdIdentifier taskId))
                )

            let rec attachmentIdSet =
                Store.atomFamilyWithProfiling (
                    $"{nameof Task}/{nameof attachmentIdSet}",
                    (fun (_username: Username, _taskId: TaskId) -> Set.empty: Set<AttachmentId>),
                    (fun (username: Username, taskId: TaskId) -> Some (username, taskIdIdentifier taskId))
                )

            let rec cellAttachmentMap =
                Store.atomFamilyWithProfiling (
                    $"{nameof Task}/{nameof cellAttachmentMap}",
                    (fun (_username: Username, _taskId: TaskId) -> Map.empty: Map<DateId, Set<AttachmentId>>),
                    (fun (username: Username, taskId: TaskId) -> Some (username, taskIdIdentifier taskId))
                )

            let rec selectionSet =
                Store.atomFamilyWithProfiling (
                    $"{nameof Task}/{nameof selectionSet}",
                    (fun (_username: Username, _taskId: TaskId) -> Set.empty: Set<DateId>),
                    (fun (username: Username, taskId: TaskId) -> Some (username, taskIdIdentifier taskId))
                )

            let rec information =
                Store.atomFamilyWithProfiling (
                    $"{nameof Task}/{nameof information}",
                    (fun (_username: Username, _taskId: TaskId) -> Task.Default.Information),
                    (fun (username: Username, taskId: TaskId) -> Some (username, taskIdIdentifier taskId))
                )

            let rec name =
                Store.atomFamilyWithProfiling (
                    $"{nameof Task}/{nameof name}",
                    (fun (_username: Username, _taskId: TaskId) -> Task.Default.Name),
                    (fun (username: Username, taskId: TaskId) -> Some (username, taskIdIdentifier taskId))
                )

            let rec scheduling =
                Store.atomFamilyWithProfiling (
                    $"{nameof Task}/{nameof scheduling}",
                    (fun (_username: Username, _taskId: TaskId) -> Task.Default.Scheduling),
                    (fun (username: Username, taskId: TaskId) -> Some (username, taskIdIdentifier taskId))
                )

            let rec pendingAfter =
                Store.atomFamilyWithProfiling (
                    $"{nameof Task}/{nameof pendingAfter}",
                    (fun (_username: Username, _taskId: TaskId) -> Task.Default.PendingAfter),
                    (fun (username: Username, taskId: TaskId) -> Some (username, taskIdIdentifier taskId))
                )

            let rec missedAfter =
                Store.atomFamilyWithProfiling (
                    $"{nameof Task}/{nameof missedAfter}",
                    (fun (_username: Username, _taskId: TaskId) -> Task.Default.MissedAfter),
                    (fun (username: Username, taskId: TaskId) -> Some (username, taskIdIdentifier taskId))
                )

            let rec priority =
                Store.atomFamilyWithProfiling (
                    $"{nameof Task}/{nameof priority}",
                    (fun (_username: Username, _taskId: TaskId) -> Task.Default.Priority),
                    (fun (username: Username, taskId: TaskId) -> Some (username, taskIdIdentifier taskId))
                )

            let rec duration =
                Store.atomFamilyWithProfiling (
                    $"{nameof Task}/{nameof duration}",
                    (fun (_username: Username, _taskId: TaskId) -> Task.Default.Duration),
                    (fun (username: Username, taskId: TaskId) -> Some (username, taskIdIdentifier taskId))
                )


        module rec Session =
            let rec databaseIdSet =
                Store.atomFamilyWithProfiling (
                    $"{nameof Session}/{nameof databaseIdSet}",
                    (fun (_username: Username) -> Set.empty: Set<DatabaseId>),
                    (fun (username: Username) -> Some (username, []))
                )

        //
//            let rec taskIdSet =
//                Store.atomFamilyWithProfiling (
//                    $"{nameof Session}/{nameof taskIdSet}",
//                    (fun (_username: Username) -> Set.empty: Set<TaskId>),
//                    (fun (username: Username) ->
//                        [
//                            Store.gunKeyEffect
//                                (Store.InputAtom.AtomFamily (username, Task.name, (username, Task.Default.Id)))
//                                (Recoil.parseValidGuid TaskId)
//                        ])
//                )


        module rec Cell =
            let cellIdentifier (taskId: TaskId) (dateId: DateId) =
                [
                    taskId |> TaskId.Value |> string
                    dateId |> DateId.Value |> FlukeDate.Stringify
                ]


    module Selectors =
        let rec dateSequence =
            Store.selectorWithProfiling (
                $"{nameof dateSequence}",
                (fun get ->
                    let username = Atoms.getAtomValue get Atoms.username
                    let position = Atoms.getAtomValue get Atoms.position

                    match position, username with
                    | Some position, Some username ->
                        let daysBefore = Atoms.getAtomValue get (Atoms.User.daysBefore username)
                        let daysAfter = Atoms.getAtomValue get (Atoms.User.daysAfter username)
                        let dayStart = Atoms.getAtomValue get (Atoms.User.dayStart username)
                        let dateId = dateId dayStart position
                        let (DateId referenceDay) = dateId

                        referenceDay
                        |> List.singleton
                        |> Rendering.getDateSequence (daysBefore, daysAfter)
                    | _ -> [])
            )

        let rec deviceInfo =
            Store.selectorWithProfiling (
                $"{Store.selectorWithProfiling}/{nameof deviceInfo}",
                (fun _getter -> JS.deviceInfo)
            )


        module rec Database =
            let rec taskIdSet =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Database}/{nameof taskIdSet}",
                    (fun (username: Username, databaseId: DatabaseId) get ->
                        //                        let taskIdSet = getter.get (Atoms.Session.taskIdSet username)
//
//                        taskIdSet
//                        |> Set.filter
//                            (fun taskId ->
//                                let databaseId' = getter.get (Atoms.Task.databaseId (username, taskId))
//                                databaseId' = databaseId)

                        Atoms.getAtomValue get (Atoms.Database.taskIdSet (username, databaseId)))
                )

            let rec database =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Database}/{nameof database}",
                    (fun (username: Username, databaseId: DatabaseId) get ->

                        {
                            Id = databaseId
                            Name = Atoms.getAtomValue get (Atoms.Database.name (username, databaseId))
                            Owner = Atoms.getAtomValue get (Atoms.Database.owner (username, databaseId))
                            SharedWith = Atoms.getAtomValue get (Atoms.Database.sharedWith (username, databaseId))
                            Position = Atoms.getAtomValue get (Atoms.Database.position (username, databaseId))
                        })
                )

            let rec isReadWrite =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Database}/{nameof isReadWrite}",
                    (fun (databaseId: DatabaseId) get ->
                        let username = Atoms.getAtomValue get Atoms.username

                        let access =
                            match username with
                            | Some username ->
                                let database = Atoms.getAtomValue get (database (username, databaseId))

                                if username <> Templates.templatesUser.Username
                                   && database.Owner = Templates.templatesUser.Username then
                                    None
                                else
                                    getAccess database username
                            | None -> None

                        access = Some Access.ReadWrite)
                )


        module rec Attachment =
            let rec attachment =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Attachment}/{nameof attachment}",
                    (fun (username: Username, attachmentId: AttachmentId) get ->
                        let timestamp = Atoms.getAtomValue get (Atoms.Attachment.timestamp (username, attachmentId))
                        let attachment = Atoms.getAtomValue get (Atoms.Attachment.attachment (username, attachmentId))

                        match timestamp, attachment with
                        | Some timestamp, Some attachment -> Some (timestamp, attachment)
                        | _ -> None)
                )


        module rec Information =
            let rec attachments =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Information}/{nameof attachments}",
                    (fun (username: Username, information: Information) get ->
                        Atoms.getAtomValue get (Atoms.User.informationAttachmentMap username)
                        |> Map.tryFind information
                        |> Option.defaultValue Set.empty
                        |> Set.toList
                        |> List.choose
                            (fun attachmentId ->
                                Atoms.getAtomValue get (Attachment.attachment (username, attachmentId))))
                )

            let rec informationState =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Information}/{nameof informationState}",
                    (fun (username: Username, information: Information) get ->
                        {
                            Information = information
                            Attachments = Atoms.getAtomValue get (attachments (username, information))
                            SortList = []
                        })
                )


        module rec Task =
            let rec task =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Task}/{nameof task}",
                    (fun (username: Username, taskId: TaskId) get ->

                        {
                            Id = taskId
                            Name = Atoms.getAtomValue get (Atoms.Task.name (username, taskId))
                            Information = Atoms.getAtomValue get (Atoms.Task.information (username, taskId))
                            PendingAfter = Atoms.getAtomValue get (Atoms.Task.pendingAfter (username, taskId))
                            MissedAfter = Atoms.getAtomValue get (Atoms.Task.missedAfter (username, taskId))
                            Scheduling = Atoms.getAtomValue get (Atoms.Task.scheduling (username, taskId))
                            Priority = Atoms.getAtomValue get (Atoms.Task.priority (username, taskId))
                            Duration = Atoms.getAtomValue get (Atoms.Task.duration (username, taskId))
                        })
                )

            let rec taskState =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Task}/{nameof taskState}",
                    (fun (username: Username, taskId: TaskId) get ->
                        let task = Atoms.getAtomValue get (task (username, taskId))
                        let dateSequence = Atoms.getAtomValue get dateSequence
                        let dayStart = Atoms.getAtomValue get (Atoms.User.dayStart username)
                        let statusMap = Atoms.getAtomValue get (Atoms.Task.statusMap (username, taskId))
                        let sessions = Atoms.getAtomValue get (Atoms.Task.sessions (username, taskId))
                        let attachmentIdSet = Atoms.getAtomValue get (Atoms.Task.attachmentIdSet (username, taskId))
                        let cellAttachmentMap = Atoms.getAtomValue get (Atoms.Task.cellAttachmentMap (username, taskId))

                        let attachments =
                            attachmentIdSet
                            |> Set.toList
                            |> List.choose
                                (fun attachmentId ->
                                    Atoms.getAtomValue get (Attachment.attachment (username, attachmentId)))
                            |> List.sortByDescending (fst >> FlukeDateTime.DateTime)

                        let cellStateMapWithoutStatus =
                            dateSequence
                            |> List.map DateId
                            |> List.map
                                (fun dateId ->
                                    let cellSessions =
                                        match dateSequence with
                                        | firstVisibleDate :: _ when firstVisibleDate <= (dateId |> DateId.Value) ->
                                            sessions
                                            |> List.filter (fun (Session start) -> isToday dayStart start dateId)
                                        | _ -> []

                                    let cellAttachments =
                                        match dateSequence with
                                        | firstVisibleDate :: _ when firstVisibleDate <= (dateId |> DateId.Value) ->
                                            cellAttachmentMap
                                            |> Map.tryFind dateId
                                            |> Option.defaultValue Set.empty
                                            |> Set.toList
                                            |> List.map
                                                (fun attachmentId ->

                                                    let timestamp =
                                                        Atoms.getAtomValue
                                                            get
                                                            (Atoms.Attachment.timestamp (username, attachmentId))

                                                    let attachment =
                                                        Atoms.getAtomValue
                                                            get
                                                            (Atoms.Attachment.attachment (username, attachmentId))

                                                    timestamp, attachment)
                                            |> List.choose
                                                (function
                                                | Some timestamp, Some attachment -> Some (timestamp, attachment)
                                                | _ -> None)
                                            |> List.sortByDescending (fst >> FlukeDateTime.DateTime)
                                        | _ -> []

                                    let cellState =
                                        {
                                            Status = Disabled
                                            Sessions = cellSessions
                                            Attachments = cellAttachments
                                        }

                                    dateId, cellState)
                            |> Map.ofList

                        let cellStateMap =
                            statusMap
                            |> Map.mapValues
                                (fun manualCellStatus ->
                                    {
                                        Status = UserStatus (username, manualCellStatus)
                                        Attachments = []
                                        Sessions = []
                                    })
                            |> mergeCellStateMap cellStateMapWithoutStatus
                            |> Map.filter
                                (fun _ cellState ->
                                    match cellState with
                                    | { Status = UserStatus _ } -> true
                                    | { Sessions = _ :: _ } -> true
                                    | { Attachments = _ :: _ } -> true
                                    | _ -> false)


                        {
                            Task = task
                            Sessions = sessions
                            Attachments = attachments
                            SortList = []
                            CellStateMap = cellStateMap
                        })
                )

            let rec statusMap =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Task}/{nameof statusMap}",
                    (fun (username: Username, taskId: TaskId) get ->
                        let dayStart = Atoms.getAtomValue get (Atoms.User.dayStart username)
                        let position = Atoms.getAtomValue get Atoms.position
                        let taskState = Atoms.getAtomValue get (taskState (username, taskId))
                        let dateSequence = Atoms.getAtomValue get dateSequence

                        match position with
                        | Some position when not dateSequence.IsEmpty ->
                            Rendering.renderTaskStatusMap dayStart position dateSequence taskState
                        | _ -> Map.empty)
                )

            let rec databaseId =
                Store.selectorFamilySetterWithProfiling (
                    $"{nameof Task}/{nameof databaseId}",
                    (fun (username: Username, taskId: TaskId) get ->
                        let databaseIdSet = Atoms.getAtomValue get (Atoms.Session.databaseIdSet username)

                        let databaseIdSet =
                            databaseIdSet
                            |> Set.choose
                                (fun databaseId ->
                                    let taskIdSet =
                                        Atoms.getAtomValue get (Atoms.Database.taskIdSet (username, databaseId))

                                    if taskIdSet.Contains taskId then Some databaseId else None)

                        match databaseIdSet |> Set.toList with
                        | [] -> Database.Default.Id
                        | [ databaseId ] -> databaseId
                        | _ -> failwith $"Error: task {taskId} exists in two databases ({databaseIdSet})"),
                    (fun (username: Username, taskId: TaskId) get set newValue ->
                        let databaseId = Atoms.getAtomValue get (databaseId (username, taskId))

                        if databaseId <> newValue then
                            let taskIdSet = Atoms.getAtomValue get (Atoms.Database.taskIdSet (username, databaseId))

                            Atoms.setAtomValue
                                set
                                (Atoms.Database.taskIdSet (username, databaseId))
                                (taskIdSet |> Set.remove taskId)

                        Atoms.setAtomValuePrev set (Atoms.Database.taskIdSet (username, newValue)) (Set.add taskId))
                )

            let rec isReadWrite =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Task}/{nameof isReadWrite}",
                    (fun (username: Username, taskId: TaskId) get ->
                        let databaseId = Atoms.getAtomValue get (databaseId (username, taskId))
                        Atoms.getAtomValue get (Database.isReadWrite databaseId))
                )

            let rec lastSession =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Task}/{nameof lastSession}",
                    (fun (taskId: TaskId) get ->
                        let dateSequence = Atoms.getAtomValue get dateSequence
                        let username = Atoms.getAtomValue get Atoms.username

                        match username with
                        | Some username ->
                            let taskState = Atoms.getAtomValue get (taskState (username, taskId))

                            dateSequence
                            |> List.rev
                            |> List.tryPick
                                (fun date ->
                                    taskState.CellStateMap
                                    |> Map.tryFind (DateId date)
                                    |> Option.map (fun cellState -> cellState.Sessions)
                                    |> Option.defaultValue []
                                    |> List.sortByDescending (fun (Session start) -> start |> FlukeDateTime.DateTime)
                                    |> List.tryHead)
                        | _ -> None)
                )

            let rec activeSession =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Task}/{nameof activeSession}",
                    (fun (taskId: TaskId) get ->
                        let username = Atoms.getAtomValue get Atoms.username
                        let position = Atoms.getAtomValue get Atoms.position
                        let lastSession = Atoms.getAtomValue get (lastSession taskId)

                        match username, position, lastSession with
                        | Some username, Some position, Some lastSession ->
                            let sessionDuration = Atoms.getAtomValue get (Atoms.User.sessionDuration username)
                            let sessionBreakDuration = Atoms.getAtomValue get (Atoms.User.sessionBreakDuration username)

                            let (Session start) = lastSession

                            let currentDuration =
                                ((position |> FlukeDateTime.DateTime)
                                 - (start |> FlukeDateTime.DateTime))
                                    .TotalMinutes
                                |> int

                            let active =
                                currentDuration < (sessionDuration |> Minute.Value)
                                                  + (sessionBreakDuration |> Minute.Value)

                            match active with
                            | true -> Some currentDuration
                            | false -> None
                        | _ -> None)
                )

            let rec showUser =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Task}/{nameof showUser}",
                    (fun (username: Username, taskId: TaskId) get ->
                        let taskState = Atoms.getAtomValue get (taskState (username, taskId))

                        let usersCount =
                            taskState.CellStateMap
                            |> Map.values
                            |> Seq.choose
                                (function
                                | { Status = UserStatus (user, _) } -> Some user
                                | _ -> None)
                            |> Seq.distinct
                            |> Seq.length

                        usersCount > 1)
                )

            let rec hasSelection =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Task}/{nameof hasSelection}",
                    (fun (taskId: TaskId) get ->
                        let dateSequence = Atoms.getAtomValue get dateSequence
                        let username = Atoms.getAtomValue get Atoms.username

                        match username with
                        | Some username ->
                            let selectionSet = Atoms.getAtomValue get (Atoms.Task.selectionSet (username, taskId))

                            dateSequence
                            |> List.exists (DateId >> selectionSet.Contains)
                        | None -> false)
                )

        module rec Cell =
            let rec sessionStatus =
                Store.selectorFamilySetterWithProfiling (
                    $"{nameof Cell}/{nameof sessionStatus}",
                    (fun (username: Username, taskId: TaskId, dateId: DateId) get ->
                        let hideSchedulingOverlay = Atoms.getAtomValue get (Atoms.User.hideSchedulingOverlay username)

                        if hideSchedulingOverlay then
                            Atoms.getAtomValue get (Atoms.Task.statusMap (username, taskId))
                            |> Map.tryFind dateId
                            |> Option.map (fun manualUserStatus -> UserStatus (username, manualUserStatus))
                            |> Option.defaultValue Disabled
                        else
                            Atoms.getAtomValue get (Task.statusMap (username, taskId))
                            |> Map.tryFind dateId
                            |> Option.defaultValue Disabled),
                    (fun (username: Username, taskId: TaskId, dateId: DateId) get set newValue ->
                        let statusMap = Atoms.getAtomValue get (Atoms.Task.statusMap (username, taskId))

                        Atoms.setAtomValue
                            set
                            (Atoms.Task.statusMap (username, taskId))
                            (match newValue with
                             | UserStatus (_, status) -> statusMap |> Map.add dateId status
                             | _ -> statusMap |> Map.remove dateId))
                )

            let rec selected =
                Store.selectorFamilySetterWithProfiling (
                    $"{nameof Cell}/{nameof selected}",
                    (fun (username: Username, taskId: TaskId, dateId: DateId) get ->
                        let selectionSet = Atoms.getAtomValue get (Atoms.Task.selectionSet (username, taskId))
                        selectionSet.Contains dateId),
                    (fun (username: Username, taskId: TaskId, dateId: DateId) _get set newValue ->
                        Atoms.setAtomValuePrev
                            set
                            (Atoms.Task.selectionSet (username, taskId))
                            ((if newValue then Set.add else Set.remove) dateId))
                )

            let rec sessions =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Cell}/{nameof sessions}",
                    (fun (username: Username, taskId: TaskId, dateId: DateId) get ->
                        let taskState = Atoms.getAtomValue get (Task.taskState (username, taskId))

                        taskState.CellStateMap
                        |> Map.tryFind dateId
                        |> Option.map (fun x -> x.Sessions)
                        |> Option.defaultValue [])
                )

            let rec attachments =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Cell}/{nameof attachments}",
                    (fun (username: Username, taskId: TaskId, dateId: DateId) get ->
                        let taskState = Atoms.getAtomValue get (Task.taskState (username, taskId))

                        taskState.CellStateMap
                        |> Map.tryFind dateId
                        |> Option.map (fun x -> x.Attachments)
                        |> Option.defaultValue [])
                )


        module rec Session =
            let rec taskIdSet =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Session}/{nameof taskIdSet}",
                    (fun (username: Username) get ->
                        let databaseIdSet = Atoms.getAtomValue get (Atoms.Session.databaseIdSet username)

                        databaseIdSet
                        |> Set.collect
                            (fun databaseId -> Atoms.getAtomValue get (Atoms.Database.taskIdSet (username, databaseId))))
                )

            let rec informationSet =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Session}/{nameof informationSet}",
                    (fun (username: Username) get ->
                        let taskIdSet = Atoms.getAtomValue get (taskIdSet username)

                        taskIdSet
                        |> Set.toList
                        |> List.map (fun taskId -> Atoms.getAtomValue get (Atoms.Task.information (username, taskId)))
                        |> List.filter
                            (fun information ->
                                information
                                |> Information.Name
                                |> InformationName.Value
                                |> String.IsNullOrWhiteSpace
                                |> not)
                        |> Set.ofList)
                )

            let rec selectedTaskIdSet =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Session}/{nameof selectedTaskIdSet}",
                    (fun (username: Username) get ->
                        let selectedDatabaseIdSet = Atoms.getAtomValue get (Atoms.User.selectedDatabaseIdSet username)
                        let taskIdSet = Atoms.getAtomValue get (taskIdSet username)

                        taskIdSet
                        |> Set.toList
                        |> List.map (fun taskId -> taskId, Atoms.getAtomValue get (Task.databaseId (username, taskId)))
                        |> List.filter (fun (_, databaseId) -> selectedDatabaseIdSet |> Set.contains databaseId)
                        |> List.map fst
                        |> Set.ofList)
                )

            let rec informationStateList =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Session}/{nameof informationStateList}",
                    (fun (username: Username) get ->
                        let informationSet = Atoms.getAtomValue get (informationSet username)

                        informationSet
                        |> Set.toList
                        |> List.map
                            (fun information ->
                                Atoms.getAtomValue get (Information.informationState (username, information))))
                )

            let rec activeSessions =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Session}/{nameof activeSessions}",
                    (fun (username: Username) get ->
                        let selectedTaskIdSet = Atoms.getAtomValue get (selectedTaskIdSet username)

                        selectedTaskIdSet
                        |> Set.toList
                        |> List.map
                            (fun taskId ->
                                let duration = Atoms.getAtomValue get (Task.activeSession taskId)
                                taskId, duration)
                        |> List.sortBy fst
                        |> List.choose
                            (fun (taskId, duration) ->
                                duration
                                |> Option.map
                                    (fun duration ->
                                        let (TaskName taskName) =
                                            Atoms.getAtomValue get (Atoms.Task.name (username, taskId))

                                        TempUI.ActiveSession (taskName, Minute duration))))
                )

            let rec filteredTaskIdSet =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Session}/{nameof filteredTaskIdSet}",
                    (fun (username: Username) get ->
                        let filterTasksByView = true
                        // TODO: !!
                        //getter.get (Atoms.User.filterTasksByView username)
                        let searchText = Atoms.getAtomValue get (Atoms.User.searchText username)
                        let view = Atoms.getAtomValue get (Atoms.User.view username)
                        let dateSequence = Atoms.getAtomValue get dateSequence
                        let selectedTaskIdSet = Atoms.getAtomValue get (selectedTaskIdSet username)

                        let taskList =
                            selectedTaskIdSet
                            |> Set.toList
                            |> List.map (fun taskId -> Atoms.getAtomValue get (Task.task (username, taskId)))

                        let taskListSearch =
                            match searchText with
                            | "" -> taskList
                            | _ ->
                                taskList
                                |> List.filter
                                    (fun task ->
                                        let check (text: string) = text.IndexOf searchText >= 0

                                        (task.Name |> TaskName.Value |> check)
                                        || (task.Information
                                            |> Information.Name
                                            |> InformationName.Value
                                            |> check))

                        let filteredTaskList =
                            if filterTasksByView then
                                taskListSearch
                                |> List.map (fun task -> Atoms.getAtomValue get (Task.taskState (username, task.Id)))
                                |> filterTaskStateSeq view dateSequence
                                |> Seq.toList
                                |> List.map (fun taskState -> taskState.Task)
                            else
                                taskListSearch


                        JS.log
                            (fun () ->
                                $"filteredTaskList.Length={filteredTaskList.Length} taskListSearch.Length={
                                                                                                               taskListSearch.Length
                                }")

                        filteredTaskList
                        |> List.map (fun task -> task.Id)
                        |> Set.ofList)
                )

            let rec sortedTaskIdList =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Session}/{nameof sortedTaskIdList}",
                    (fun (username: Username) get ->
                        let position = Atoms.getAtomValue get Atoms.position

                        match position with
                        | Some position ->
                            let view = Atoms.getAtomValue get (Atoms.User.view username)
                            let dayStart = Atoms.getAtomValue get (Atoms.User.dayStart username)
                            let filteredTaskIdSet = Atoms.getAtomValue get (filteredTaskIdSet username)

                            let informationStateList = Atoms.getAtomValue get (Session.informationStateList username)

                            JS.log (fun () -> $"sortedTaskIdList. filteredTaskIdSet.Count={filteredTaskIdSet.Count}")

                            let lanes =
                                filteredTaskIdSet
                                |> Set.toList
                                |> List.map
                                    (fun taskId ->
                                        let statusMap = Atoms.getAtomValue get (Task.statusMap (username, taskId))
                                        let taskState = Atoms.getAtomValue get (Task.taskState (username, taskId))
                                        taskState, statusMap)

                            let result =
                                sortLanes
                                    {|
                                        View = view
                                        DayStart = dayStart
                                        Position = position
                                        InformationStateList = informationStateList
                                        Lanes = lanes
                                    |}

                            JS.log (fun () -> $"sortedTaskIdList. result.Length={result.Length}")

                            result
                            |> List.map (fun (taskState, _) -> taskState.Task.Id)
                        | _ -> [])
                )

            let rec tasksByInformationKind =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Session}/{nameof tasksByInformationKind}",
                    (fun (username: Username) get ->
                        let sortedTaskIdList = Atoms.getAtomValue get (sortedTaskIdList username)

                        sortedTaskIdList
                        |> List.groupBy
                            (fun taskId -> Atoms.getAtomValue get (Atoms.Task.information (username, taskId)))
                        |> List.sortBy (fun (information, _) -> information |> Information.Name)
                        |> List.groupBy (fun (information, _) -> Information.toString information)
                        |> List.sortBy (snd >> List.head >> fst >> Information.toTag))
                )

            let rec cellSelectionMap =
                Store.selectorFamilyWithProfiling (
                    $"{nameof Session}/{nameof cellSelectionMap}",
                    (fun (username: Username) get ->
                        let sortedTaskIdList = Atoms.getAtomValue get (sortedTaskIdList username)
                        let dateSequence = Atoms.getAtomValue get dateSequence

                        sortedTaskIdList
                        |> List.map
                            (fun taskId ->
                                let selectionSet = Atoms.getAtomValue get (Atoms.Task.selectionSet (username, taskId))

                                let dates =
                                    dateSequence
                                    |> List.map (fun date -> date, selectionSet.Contains (DateId date))
                                    |> List.filter snd
                                    |> List.map fst
                                    |> Set.ofList

                                taskId, dates)
                        |> List.filter (fun (_, dates) -> Set.isEmpty dates |> not)
                        |> Map.ofList)
                )


        module rec FlukeDate =
            let isToday =
                Store.selectorFamilyWithProfiling (
                    $"{nameof FlukeDate}/{nameof isToday}",
                    (fun (date: FlukeDate) get ->
                        let username = Atoms.getAtomValue get Atoms.username
                        let position = Atoms.getAtomValue get Atoms.position

                        match username, position with
                        | Some username, Some position ->
                            let dayStart = Atoms.getAtomValue get (Atoms.User.dayStart username)

                            Domain.UserInteraction.isToday dayStart position (DateId date)
                        | _ -> false)
                )

            let rec hasCellSelection =
                Store.selectorFamilyWithProfiling (
                    $"{nameof FlukeDate}/{nameof hasCellSelection}",
                    (fun (date: FlukeDate) get ->
                        let username = Atoms.getAtomValue get Atoms.username

                        match username with
                        | Some username ->
                            let cellSelectionMap = Atoms.getAtomValue get (Session.cellSelectionMap username)

                            cellSelectionMap
                            |> Map.values
                            |> Seq.exists (Set.contains date)
                        | None -> false)
                )


        module rec BulletJournalView =
            let rec weekCellsMap =
                Store.selectorFamilyWithProfiling (
                    $"{nameof BulletJournalView}/{nameof weekCellsMap}",
                    (fun (username: Username) get ->
                        let position = Atoms.getAtomValue get Atoms.position
                        let sortedTaskIdList = Atoms.getAtomValue get (Session.sortedTaskIdList username)

                        match position with
                        | Some position ->
                            let dayStart = Atoms.getAtomValue get (Atoms.User.dayStart username)
                            let weekStart = Atoms.getAtomValue get (Atoms.User.weekStart username)

                            let weeks =
                                [
                                    -1 .. 1
                                ]
                                |> List.map
                                    (fun weekOffset ->
                                        let dateIdSequence =
                                            let rec getWeekStart (date: DateTime) =
                                                if date.DayOfWeek = weekStart then
                                                    date
                                                else
                                                    getWeekStart (date.AddDays -1)

                                            let startDate =
                                                dateId dayStart position
                                                |> fun (DateId referenceDay) ->
                                                    (referenceDay |> FlukeDate.DateTime)
                                                        .AddDays (7 * weekOffset)
                                                |> getWeekStart

                                            [
                                                0 .. 6
                                            ]
                                            |> List.map startDate.AddDays
                                            |> List.map FlukeDateTime.FromDateTime
                                            |> List.map (dateId dayStart)

                                        let taskStateMap =
                                            sortedTaskIdList
                                            |> List.map
                                                (fun taskId ->
                                                    taskId, Atoms.getAtomValue get (Task.taskState (username, taskId)))
                                            |> Map.ofList

                                        let result =
                                            sortedTaskIdList
                                            |> List.collect
                                                (fun taskId ->
                                                    dateIdSequence
                                                    |> List.map
                                                        (fun dateId ->
                                                            match dateId with
                                                            | DateId referenceDay as dateId ->
                                                                let isToday =
                                                                    Atoms.getAtomValue
                                                                        get
                                                                        (FlukeDate.isToday referenceDay)

                                                                let cellState =
                                                                    taskStateMap
                                                                    |> Map.tryFind taskId
                                                                    |> Option.bind
                                                                        (fun taskState ->
                                                                            taskState.CellStateMap |> Map.tryFind dateId)
                                                                    |> Option.defaultValue
                                                                        {
                                                                            Status = CellStatus.Disabled
                                                                            Attachments = []
                                                                            Sessions = []
                                                                        }

                                                                {|
                                                                    DateId = dateId
                                                                    TaskId = taskId
                                                                    Status = cellState.Status
                                                                    Sessions = cellState.Sessions
                                                                    IsToday = isToday
                                                                    Attachments = cellState.Attachments
                                                                |}))
                                            |> List.groupBy (fun x -> x.DateId)
                                            |> List.map
                                                (fun (dateId, cellsMetadata) ->
                                                    match dateId with
                                                    | DateId referenceDay as dateId ->
                                                        //                |> Sorting.sortLanesByTimeOfDay input.DayStart input.Position input.TaskOrderList
                                                        let taskSessions =
                                                            cellsMetadata
                                                            |> List.collect (fun x -> x.Sessions)

                                                        let sortedTasksMap =
                                                            cellsMetadata
                                                            |> List.map
                                                                (fun cellMetadata ->
                                                                    let taskState =
                                                                        { TaskState.Default with
                                                                            Task =
                                                                                taskStateMap.[cellMetadata.TaskId].Task
                                                                            Sessions = taskSessions
                                                                        }

                                                                    taskState,
                                                                    [
                                                                        dateId, cellMetadata.Status
                                                                    ]
                                                                    |> Map.ofList)
                                                            |> Sorting.sortLanesByTimeOfDay
                                                                dayStart
                                                                (FlukeDateTime.Create (referenceDay, dayStart))
                                                            |> List.indexed
                                                            |> List.map
                                                                (fun (i, (taskState, _)) -> taskState.Task.Id, i)
                                                            |> Map.ofList

                                                        let newCells =
                                                            cellsMetadata
                                                            |> List.sortBy
                                                                (fun cell ->
                                                                    sortedTasksMap
                                                                    |> Map.tryFind cell.TaskId
                                                                    |> Option.defaultValue -1)

                                                        dateId, newCells)
                                            |> Map.ofList

                                        result)

                            weeks
                        | _ -> [])
                )
