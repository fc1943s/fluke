namespace Fluke.UI.Frontend

#nowarn "40"

open System
open Fluke.Shared
open Fluke.Shared.Domain
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fable.DateFunctions
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
//                Store.atomFamilyWithSync (
//                    $"{nameof Events}/{nameof events}",
//                    (fun (_eventId: EventId) -> Event.NoOp)
//                )

        let rec debug = Store.atomWithStorage $"{nameof debug}" (JS.isDebug ())

        let rec sessionRestored = Store.atom ($"{nameof sessionRestored}", false)
        let rec initialPeerSkipped = Store.atom ($"{nameof initialPeerSkipped}", false)
        let rec position = Store.atom ($"{nameof position}", None)
        let rec ctrlPressed = Store.atom ($"{nameof ctrlPressed}", false)
        let rec shiftPressed = Store.atom ($"{nameof shiftPressed}", false)


        let rec databaseIdSet = Store.atomWithSync ($"{nameof databaseIdSet}", (Set.empty: Set<DatabaseId>), [])

        let expandedDatabaseIdSetDefault : Set<DatabaseId> = Set.empty

        let rec expandedDatabaseIdSet =
            Store.atomWithSync ($"{nameof expandedDatabaseIdSet}", expandedDatabaseIdSetDefault, [])

        let selectedDatabaseIdSetDefault : Set<DatabaseId> = Set.empty

        let rec selectedDatabaseIdSet =
            Store.atomWithSync ($"{nameof selectedDatabaseIdSet}", selectedDatabaseIdSetDefault, [])

        let viewDefault = View.View.Information
        let rec view = Store.atomWithSync ($"{nameof view}", viewDefault, [])

        let languageDefault = Language.English
        let rec language = Store.atomWithSync ($"{nameof language}", languageDefault, [])

        let colorDefault = String.Format ("#{0:X6}", Random().Next 0x1000000)
        let rec color = Store.atomWithSync ($"{nameof color}", colorDefault, [])

        let weekStartDefault = DayOfWeek.Sunday
        let rec weekStart = Store.atomWithSync ($"{nameof weekStart}", weekStartDefault, [])

        let dayStartDefault = FlukeTime.Create 0 0
        let rec dayStart = Store.atomWithSync ($"{nameof dayStart}", dayStartDefault, [])

        let sessionDurationDefault = Minute 25
        let rec sessionDuration = Store.atomWithSync ($"{nameof sessionDuration}", sessionDurationDefault, [])

        let sessionBreakDurationDefault = Minute 5

        let rec sessionBreakDuration =
            Store.atomWithSync ($"{nameof sessionBreakDuration}", sessionBreakDurationDefault, [])

        let daysBeforeDefault = 7
        let rec daysBefore = Store.atomWithSync ($"{nameof daysBefore}", daysBeforeDefault, [])

        let daysAfterDefault = 7
        let rec daysAfter = Store.atomWithSync ($"{nameof daysAfter}", daysAfterDefault, [])

        let searchTextDefault = ""
        let rec searchText = Store.atomWithSync ($"{nameof searchText}", searchTextDefault, [])


        let cellSizeDefault = 23
        let rec cellSize = Store.atomWithSync ($"{nameof cellSize}", cellSizeDefault, [])

        let leftDockDefault : TempUI.DockType option = None
        let rec leftDock = Store.atomWithSync ($"{nameof leftDock}", leftDockDefault, [])

        let rightDockDefault : TempUI.DockType option = None
        let rec rightDock = Store.atomWithSync ($"{nameof rightDock}", rightDockDefault, [])

        let hideTemplatesDefault = false
        let rec hideTemplates = Store.atomWithSync ($"{nameof hideTemplates}", hideTemplatesDefault, [])

        let hideSchedulingOverlayDefault = false

        let rec hideSchedulingOverlay =
            Store.atomWithSync ($"{nameof hideSchedulingOverlay}", hideSchedulingOverlayDefault, [])

        let showViewOptionsDefault = false
        let rec showViewOptions = Store.atomWithSync ($"{nameof showViewOptions}", showViewOptionsDefault, [])

        let filterTasksByViewDefault = true
        let rec filterTasksByView = Store.atomWithSync ($"{nameof filterTasksByView}", filterTasksByViewDefault, [])

        let informationAttachmentMapDefault : Map<Information, Set<AttachmentId>> = Map.empty

        let rec informationAttachmentMap =
            Store.atomWithSync ($"{nameof informationAttachmentMap}", informationAttachmentMapDefault, [])


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

        let uiFlagDefault = UIFlag.None

        let rec uiFlag =
            Store.atomFamilyWithSync (
                $"{nameof uiFlag}",
                (fun (_uiFlagType: UIFlagType) -> uiFlagDefault),
                (fun (uiFlagType: UIFlagType) -> uiFlagType |> string |> List.singleton)
            )

        let uiVisibleFlagDefault = false

        let rec uiVisibleFlag =
            Store.atomFamilyWithSync (
                $"{nameof uiVisibleFlag}",
                (fun (_uiFlagType: UIFlagType) -> uiVisibleFlagDefault),
                (fun (uiFlagType: UIFlagType) -> uiFlagType |> string |> List.singleton)
            )

        let accordionFlagDefault : string [] = [||]

        let rec accordionFlag =
            Store.atomFamilyWithSync (
                $"{nameof accordionFlag}",
                (fun (_key: TextKey) -> accordionFlagDefault),
                (fun (key: TextKey) -> key |> TextKey.Value |> List.singleton)
            )


        module rec Database =
            let databaseIdIdentifier (databaseId: DatabaseId) =
                databaseId
                |> DatabaseId.Value
                |> string
                |> List.singleton

            let rec taskIdSet =
                Store.atomFamilyWithSync (
                    $"{nameof Database}/{nameof taskIdSet}",
                    (fun (_databaseId: DatabaseId) -> Set.empty: Set<TaskId>),
                    databaseIdIdentifier
                )

            let rec name =
                Store.atomFamilyWithSync (
                    $"{nameof Database}/{nameof name}",
                    (fun (_databaseId: DatabaseId) -> Database.Default.Name),
                    databaseIdIdentifier
                )

            let rec owner =
                Store.atomFamilyWithSync (
                    $"{nameof Database}/{nameof owner}",
                    (fun (_databaseId: DatabaseId) -> Database.Default.Owner),
                    databaseIdIdentifier
                )

            let rec sharedWith =
                Store.atomFamilyWithSync (
                    $"{nameof Database}/{nameof sharedWith}",
                    (fun (_databaseId: DatabaseId) -> Database.Default.SharedWith),
                    databaseIdIdentifier
                )

            let rec position =
                Store.atomFamilyWithSync (
                    $"{nameof Database}/{nameof position}",
                    (fun (_databaseId: DatabaseId) -> Database.Default.Position),
                    databaseIdIdentifier
                )


        module rec Attachment =
            let attachmentIdIdentifier (attachmentId: AttachmentId) =
                attachmentId
                |> AttachmentId.Value
                |> string
                |> List.singleton

            let rec timestamp =
                Store.atomFamilyWithSync (
                    $"{nameof Attachment}/{nameof timestamp}",
                    (fun (_attachmentId: AttachmentId) -> None: FlukeDateTime option),
                    attachmentIdIdentifier
                )

            let rec attachment =
                Store.atomFamilyWithSync (
                    $"{nameof Attachment}/{nameof attachment}",
                    (fun (_attachmentId: AttachmentId) -> None: Attachment option),
                    attachmentIdIdentifier
                )


        module rec Task =
            let taskIdIdentifier (taskId: TaskId) =
                taskId |> TaskId.Value |> string |> List.singleton

            let rec statusMap =
                Store.atomFamilyWithSync (
                    $"{nameof Task}/{nameof statusMap}",
                    (fun (_taskId: TaskId) -> Map.empty: Map<DateId, Username * ManualCellStatus>),
                    taskIdIdentifier
                )

            //            let rec databaseId =
//                Store.atomFamilyWithSync (
//                    $"{nameof Task}/{nameof databaseId}",
//                    (fun (_taskId: TaskId) -> Database.Default.Id),
//                    (fun (username: Username) ->
//                        [
//                            Store.gunEffect
//                                (Store.InputAtom.Atom
//                               (askIdIdentifier taskId)
//                        ])
//                )

            let rec sessions =
                Store.atomFamilyWithSync (
                    $"{nameof Task}/{nameof sessions}",
                    (fun (_taskId: TaskId) -> []: Session list),
                    taskIdIdentifier
                )

            let rec attachmentIdSet =
                Store.atomFamilyWithSync (
                    $"{nameof Task}/{nameof attachmentIdSet}",
                    (fun (_taskId: TaskId) -> Set.empty: Set<AttachmentId>),
                    taskIdIdentifier
                )

            let rec cellAttachmentMap =
                Store.atomFamilyWithSync (
                    $"{nameof Task}/{nameof cellAttachmentMap}",
                    (fun (_taskId: TaskId) -> Map.empty: Map<DateId, Set<AttachmentId>>),
                    taskIdIdentifier
                )

            let rec selectionSet =
                Store.atomFamilyWithSync (
                    $"{nameof Task}/{nameof selectionSet}",
                    (fun (_taskId: TaskId) -> Set.empty: Set<DateId>),
                    taskIdIdentifier
                )

            let rec information =
                Store.atomFamilyWithSync (
                    $"{nameof Task}/{nameof information}",
                    (fun (_taskId: TaskId) -> Task.Default.Information),
                    taskIdIdentifier
                )

            let rec name =
                Store.atomFamilyWithSync (
                    $"{nameof Task}/{nameof name}",
                    (fun (_taskId: TaskId) -> Task.Default.Name),
                    taskIdIdentifier
                )

            let rec scheduling =
                Store.atomFamilyWithSync (
                    $"{nameof Task}/{nameof scheduling}",
                    (fun (_taskId: TaskId) -> Task.Default.Scheduling),
                    taskIdIdentifier
                )

            let rec pendingAfter =
                Store.atomFamilyWithSync (
                    $"{nameof Task}/{nameof pendingAfter}",
                    (fun (_taskId: TaskId) -> Task.Default.PendingAfter),
                    taskIdIdentifier
                )

            let rec missedAfter =
                Store.atomFamilyWithSync (
                    $"{nameof Task}/{nameof missedAfter}",
                    (fun (_taskId: TaskId) -> Task.Default.MissedAfter),
                    taskIdIdentifier
                )

            let rec priority =
                Store.atomFamilyWithSync (
                    $"{nameof Task}/{nameof priority}",
                    (fun (_taskId: TaskId) -> Task.Default.Priority),
                    taskIdIdentifier
                )

            let rec duration =
                Store.atomFamilyWithSync (
                    $"{nameof Task}/{nameof duration}",
                    (fun (_taskId: TaskId) -> Task.Default.Duration),
                    taskIdIdentifier
                )



        //
//            let rec taskIdSet =
//                Store.atomFamilyWithSync (
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
            Store.readSelector (
                $"{nameof dateSequence}",
                (fun get ->
                    let position = Atoms.getAtomValue get Atoms.position

                    match position with
                    | Some position ->
                        let daysBefore = Atoms.getAtomValue get Atoms.daysBefore
                        let daysAfter = Atoms.getAtomValue get Atoms.daysAfter
                        let dayStart = Atoms.getAtomValue get Atoms.dayStart
                        let dateId = dateId dayStart position
                        let (DateId referenceDay) = dateId

                        referenceDay
                        |> List.singleton
                        |> Rendering.getDateSequence (daysBefore, daysAfter)
                    | _ -> [])
            )

        let rec deviceInfo = Store.readSelector ($"{nameof deviceInfo}", (fun _getter -> JS.deviceInfo))


        module rec Database =
            //            let rec taskIdSet =
//                Store.readSelectorFamily (
//                    $"{nameof Database}/{nameof taskIdSet}",
//                    (fun (databaseId: DatabaseId) get ->
//                        //                        let taskIdSet = getter.get (Atoms.Session.taskIdSet username)
////
////                        taskIdSet
////                        |> Set.filter
////                            (fun taskId ->
////                                let databaseId' = getter.get (Atoms.Task.databaseId taskId)
////                                databaseId' = databaseId)
//
//                        Atoms.getAtomValue get (Atoms.Database.taskIdSet databaseId))
//                )

            let rec database =
                Store.readSelectorFamily (
                    $"{nameof Database}/{nameof database}",
                    (fun (databaseId: DatabaseId) get ->
                        {
                            Id = databaseId
                            Name = Atoms.getAtomValue get (Atoms.Database.name databaseId)
                            Owner = Atoms.getAtomValue get (Atoms.Database.owner databaseId)
                            SharedWith = Atoms.getAtomValue get (Atoms.Database.sharedWith databaseId)
                            Position = Atoms.getAtomValue get (Atoms.Database.position databaseId)
                        })
                )

            let rec isReadWrite =
                Store.readSelectorFamily (
                    $"{nameof Database}/{nameof isReadWrite}",
                    (fun (databaseId: DatabaseId) get ->
                        let username = Atoms.getAtomValue get Atoms.username

                        let access =
                            match username with
                            | Some username ->
                                let database = Atoms.getAtomValue get (database databaseId)

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
                Store.readSelectorFamily (
                    $"{nameof Attachment}/{nameof attachment}",
                    (fun (attachmentId: AttachmentId) get ->
                        let timestamp = Atoms.getAtomValue get (Atoms.Attachment.timestamp attachmentId)
                        let attachment = Atoms.getAtomValue get (Atoms.Attachment.attachment attachmentId)

                        match timestamp, attachment with
                        | Some timestamp, Some attachment -> Some (timestamp, attachment)
                        | _ -> None)
                )


        module rec Information =
            let rec attachments =
                Store.readSelectorFamily (
                    $"{nameof Information}/{nameof attachments}",
                    (fun (information: Information) get ->
                        Atoms.getAtomValue get Atoms.informationAttachmentMap
                        |> Map.tryFind information
                        |> Option.defaultValue Set.empty
                        |> Set.toList
                        |> List.choose (fun attachmentId -> Atoms.getAtomValue get (Attachment.attachment attachmentId)))
                )

            let rec informationState =
                Store.readSelectorFamily (
                    $"{nameof Information}/{nameof informationState}",
                    (fun (information: Information) get ->
                        {
                            Information = information
                            Attachments = Atoms.getAtomValue get (attachments information)
                            SortList = []
                        })
                )


        module rec Task =
            let rec task =
                Store.readSelectorFamily (
                    $"{nameof Task}/{nameof task}",
                    (fun (taskId: TaskId) get ->

                        {
                            Id = taskId
                            Name = Atoms.getAtomValue get (Atoms.Task.name taskId)
                            Information = Atoms.getAtomValue get (Atoms.Task.information taskId)
                            PendingAfter = Atoms.getAtomValue get (Atoms.Task.pendingAfter taskId)
                            MissedAfter = Atoms.getAtomValue get (Atoms.Task.missedAfter taskId)
                            Scheduling = Atoms.getAtomValue get (Atoms.Task.scheduling taskId)
                            Priority = Atoms.getAtomValue get (Atoms.Task.priority taskId)
                            Duration = Atoms.getAtomValue get (Atoms.Task.duration taskId)
                        })
                )

            let rec taskState =
                Store.readSelectorFamily (
                    $"{nameof Task}/{nameof taskState}",
                    (fun (taskId: TaskId) get ->
                        let task = Atoms.getAtomValue get (task taskId)
                        let dateSequence = Atoms.getAtomValue get dateSequence
                        let statusMap = Atoms.getAtomValue get (Atoms.Task.statusMap taskId)
                        let sessions = Atoms.getAtomValue get (Atoms.Task.sessions taskId)
                        let attachmentIdSet = Atoms.getAtomValue get (Atoms.Task.attachmentIdSet taskId)
                        let cellAttachmentMap = Atoms.getAtomValue get (Atoms.Task.cellAttachmentMap taskId)

                        let attachments =
                            attachmentIdSet
                            |> Set.toList
                            |> List.choose
                                (fun attachmentId -> Atoms.getAtomValue get (Attachment.attachment attachmentId))
                            |> List.sortByDescending (fst >> FlukeDateTime.DateTime)

                        let cellStateMapWithoutStatus =
                            let dayStart = Atoms.getAtomValue get Atoms.dayStart

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
                                                        Atoms.getAtomValue get (Atoms.Attachment.timestamp attachmentId)

                                                    let attachment =
                                                        Atoms.getAtomValue
                                                            get
                                                            (Atoms.Attachment.attachment attachmentId)

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
                            |> Map.ofSeq

                        let cellStateMap =
                            statusMap
                            |> Map.mapValues
                                (fun manualCellStatus ->
                                    {
                                        Status = UserStatus manualCellStatus
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
                Store.readSelectorFamily (
                    $"{nameof Task}/{nameof statusMap}",
                    (fun (taskId: TaskId) get ->
                        let position = Atoms.getAtomValue get Atoms.position
                        let taskState = Atoms.getAtomValue get (taskState taskId)
                        let dateSequence = Atoms.getAtomValue get dateSequence
                        let dayStart = Atoms.getAtomValue get Atoms.dayStart

                        match position with
                        | Some position when not dateSequence.IsEmpty ->
                            Rendering.renderTaskStatusMap dayStart position dateSequence taskState
                        | _ -> Map.empty)
                )

            let rec databaseId =
                Store.selectorFamily (
                    $"{nameof Task}/{nameof databaseId}",
                    (fun (taskId: TaskId) get ->
                        let databaseIdSet = Atoms.getAtomValue get Atoms.databaseIdSet

                        let databaseIdSet =
                            databaseIdSet
                            |> Set.choose
                                (fun databaseId ->
                                    let taskIdSet = Atoms.getAtomValue get (Atoms.Database.taskIdSet databaseId)

                                    if taskIdSet.Contains taskId then Some databaseId else None)

                        match databaseIdSet |> Set.toList with
                        | [] -> Database.Default.Id
                        | [ databaseId ] -> databaseId
                        | _ -> failwith $"Error: task {taskId} exists in two databases ({databaseIdSet})"),
                    (fun (taskId: TaskId) get set newValue ->
                        let databaseId = Atoms.getAtomValue get (databaseId taskId)

                        if databaseId <> newValue then
                            let taskIdSet = Atoms.getAtomValue get (Atoms.Database.taskIdSet databaseId)

                            Atoms.setAtomValue
                                set
                                (Atoms.Database.taskIdSet databaseId)
                                (taskIdSet |> Set.remove taskId)

                        Atoms.setAtomValuePrev set (Atoms.Database.taskIdSet newValue) (Set.add taskId))
                )

            let rec isReadWrite =
                Store.readSelectorFamily (
                    $"{nameof Task}/{nameof isReadWrite}",
                    (fun (taskId: TaskId) get ->
                        let databaseId = Atoms.getAtomValue get (databaseId taskId)
                        Atoms.getAtomValue get (Database.isReadWrite databaseId))
                )

            let rec lastSession =
                Store.readSelectorFamily (
                    $"{nameof Task}/{nameof lastSession}",
                    (fun (taskId: TaskId) get ->
                        let dateSequence = Atoms.getAtomValue get dateSequence
                        let taskState = Atoms.getAtomValue get (taskState taskId)

                        dateSequence
                        |> List.rev
                        |> List.tryPick
                            (fun date ->
                                taskState.CellStateMap
                                |> Map.tryFind (DateId date)
                                |> Option.map (fun cellState -> cellState.Sessions)
                                |> Option.defaultValue []
                                |> List.sortByDescending (fun (Session start) -> start |> FlukeDateTime.DateTime)
                                |> List.tryHead))
                )

            let rec activeSession =
                Store.readSelectorFamily (
                    $"{nameof Task}/{nameof activeSession}",
                    (fun (taskId: TaskId) get ->
                        let position = Atoms.getAtomValue get Atoms.position
                        let lastSession = Atoms.getAtomValue get (lastSession taskId)

                        match position, lastSession with
                        | Some position, Some lastSession ->
                            let sessionDuration = Atoms.getAtomValue get Atoms.sessionDuration
                            let sessionBreakDuration = Atoms.getAtomValue get Atoms.sessionBreakDuration

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
                Store.readSelectorFamily (
                    $"{nameof Task}/{nameof showUser}",
                    (fun (taskId: TaskId) get ->
                        let taskState = Atoms.getAtomValue get (taskState taskId)

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
                Store.readSelectorFamily (
                    $"{nameof Task}/{nameof hasSelection}",
                    (fun (taskId: TaskId) get ->
                        let dateSequence = Atoms.getAtomValue get dateSequence
                        let selectionSet = Atoms.getAtomValue get (Atoms.Task.selectionSet taskId)

                        dateSequence
                        |> List.exists (DateId >> selectionSet.Contains))
                )

        module rec Cell =
            let rec sessionStatus =
                Store.selectorFamily (
                    $"{nameof Cell}/{nameof sessionStatus}",
                    (fun (taskId: TaskId, dateId: DateId) get ->
                        let hideSchedulingOverlay = Atoms.getAtomValue get Atoms.hideSchedulingOverlay

                        if hideSchedulingOverlay then
                            Atoms.getAtomValue get (Atoms.Task.statusMap taskId)
                            |> Map.tryFind dateId
                            |> Option.map UserStatus
                            |> Option.defaultValue Disabled
                        else
                            Atoms.getAtomValue get (Task.statusMap taskId)
                            |> Map.tryFind dateId
                            |> Option.defaultValue Disabled),
                    (fun (taskId: TaskId, dateId: DateId) get set newValue ->
                        let statusMap = Atoms.getAtomValue get (Atoms.Task.statusMap taskId)

                        Atoms.setAtomValue
                            set
                            (Atoms.Task.statusMap taskId)
                            (match newValue with
                             | UserStatus (username, status) -> statusMap |> Map.add dateId (username, status)
                             | _ -> statusMap |> Map.remove dateId))
                )

            let rec selected =
                Store.selectorFamily (
                    $"{nameof Cell}/{nameof selected}",
                    (fun (taskId: TaskId, dateId: DateId) get ->
                        let selectionSet = Atoms.getAtomValue get (Atoms.Task.selectionSet taskId)
                        selectionSet.Contains dateId),
                    (fun (taskId: TaskId, dateId: DateId) _get set newValue ->
                        Atoms.setAtomValuePrev
                            set
                            (Atoms.Task.selectionSet taskId)
                            ((if newValue then Set.add else Set.remove) dateId))
                )

            let rec sessions =
                Store.readSelectorFamily (
                    $"{nameof Cell}/{nameof sessions}",
                    (fun (taskId: TaskId, dateId: DateId) get ->
                        let taskState = Atoms.getAtomValue get (Task.taskState taskId)

                        taskState.CellStateMap
                        |> Map.tryFind dateId
                        |> Option.map (fun x -> x.Sessions)
                        |> Option.defaultValue [])
                )

            let rec attachments =
                Store.readSelectorFamily (
                    $"{nameof Cell}/{nameof attachments}",
                    (fun (taskId: TaskId, dateId: DateId) get ->
                        let taskState = Atoms.getAtomValue get (Task.taskState taskId)

                        taskState.CellStateMap
                        |> Map.tryFind dateId
                        |> Option.map (fun x -> x.Attachments)
                        |> Option.defaultValue [])
                )


        module rec Session =
            let rec taskIdSet =
                Store.readSelector (
                    $"{nameof Session}/{nameof taskIdSet}",
                    (fun get ->
                        let databaseIdSet = Atoms.getAtomValue get Atoms.databaseIdSet

                        databaseIdSet
                        |> Set.collect (fun databaseId -> Atoms.getAtomValue get (Atoms.Database.taskIdSet databaseId)))
                )

            let rec informationSet =
                Store.readSelector (
                    $"{nameof Session}/{nameof informationSet}",
                    (fun get ->
                        let taskIdSet = Atoms.getAtomValue get taskIdSet

                        taskIdSet
                        |> Set.toList
                        |> List.map (fun taskId -> Atoms.getAtomValue get (Atoms.Task.information taskId))
                        |> List.filter
                            (fun information ->
                                information
                                |> Information.Name
                                |> InformationName.Value
                                |> String.IsNullOrWhiteSpace
                                |> not)
                        |> Set.ofSeq)
                )

            let rec selectedTaskIdSet =
                Store.readSelector (
                    $"{nameof Session}/{nameof selectedTaskIdSet}",
                    (fun get ->
                        let selectedDatabaseIdSet = Atoms.getAtomValue get Atoms.selectedDatabaseIdSet

                        let taskIdSet = Atoms.getAtomValue get taskIdSet

                        taskIdSet
                        |> Set.toList
                        |> List.map (fun taskId -> taskId, Atoms.getAtomValue get (Task.databaseId taskId))
                        |> List.filter (fun (_, databaseId) -> selectedDatabaseIdSet |> Set.contains databaseId)
                        |> List.map fst
                        |> Set.ofSeq)
                )

            let rec informationStateList =
                Store.readSelector (
                    $"{nameof Session}/{nameof informationStateList}",
                    (fun get ->
                        let informationSet = Atoms.getAtomValue get informationSet

                        informationSet
                        |> Set.toList
                        |> List.map
                            (fun information -> Atoms.getAtomValue get (Information.informationState information)))
                )

            let rec activeSessions =
                Store.readSelector (
                    $"{nameof Session}/{nameof activeSessions}",
                    (fun get ->
                        let selectedTaskIdSet = Atoms.getAtomValue get selectedTaskIdSet

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
                                        let (TaskName taskName) = Atoms.getAtomValue get (Atoms.Task.name taskId)

                                        TempUI.ActiveSession (taskName, Minute duration))))
                )

            let rec filteredTaskIdSet =
                Store.readSelector (
                    $"{nameof Session}/{nameof filteredTaskIdSet}",
                    (fun get ->
                        let filterTasksByView = true
                        // TODO: !!
                        //getter.get (Atoms.User.filterTasksByView username)

                        let searchText = Atoms.getAtomValue get Atoms.searchText
                        let view = Atoms.getAtomValue get Atoms.view
                        let dateSequence = Atoms.getAtomValue get dateSequence
                        let selectedTaskIdSet = Atoms.getAtomValue get selectedTaskIdSet

                        let taskList =
                            selectedTaskIdSet
                            |> Set.toList
                            |> List.map (fun taskId -> Atoms.getAtomValue get (Task.task taskId))

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
                                |> List.map (fun task -> Atoms.getAtomValue get (Task.taskState task.Id))
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
                        |> Set.ofSeq)
                )

            let rec sortedTaskIdList =
                Store.readSelector (
                    $"{nameof Session}/{nameof sortedTaskIdList}",
                    (fun get ->
                        let position = Atoms.getAtomValue get Atoms.position

                        match position with
                        | Some position ->
                            let view = Atoms.getAtomValue get Atoms.view
                            let dayStart = Atoms.getAtomValue get Atoms.dayStart
                            let filteredTaskIdSet = Atoms.getAtomValue get filteredTaskIdSet
                            let informationStateList = Atoms.getAtomValue get Session.informationStateList

                            JS.log (fun () -> $"sortedTaskIdList. filteredTaskIdSet.Count={filteredTaskIdSet.Count}")

                            let lanes =
                                filteredTaskIdSet
                                |> Set.toList
                                |> List.map
                                    (fun taskId ->
                                        let statusMap = Atoms.getAtomValue get (Task.statusMap taskId)
                                        let taskState = Atoms.getAtomValue get (Task.taskState taskId)
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
                Store.readSelector (
                    $"{nameof Session}/{nameof tasksByInformationKind}",
                    (fun get ->
                        let sortedTaskIdList = Atoms.getAtomValue get sortedTaskIdList

                        sortedTaskIdList
                        |> List.groupBy (fun taskId -> Atoms.getAtomValue get (Atoms.Task.information taskId))
                        |> List.sortBy (fun (information, _) -> information |> Information.Name)
                        |> List.groupBy (fun (information, _) -> Information.toString information)
                        |> List.sortBy (snd >> List.head >> fst >> Information.toTag))
                )

            let rec cellSelectionMap =
                Store.readSelector (
                    $"{nameof Session}/{nameof cellSelectionMap}",
                    (fun get ->
                        let sortedTaskIdList = Atoms.getAtomValue get sortedTaskIdList
                        let dateSequence = Atoms.getAtomValue get dateSequence

                        sortedTaskIdList
                        |> List.map
                            (fun taskId ->
                                let selectionSet = Atoms.getAtomValue get (Atoms.Task.selectionSet taskId)

                                let dates =
                                    dateSequence
                                    |> List.map (fun date -> date, selectionSet.Contains (DateId date))
                                    |> List.filter snd
                                    |> List.map fst
                                    |> Set.ofSeq

                                taskId, dates)
                        |> List.filter (fun (_, dates) -> Set.isEmpty dates |> not)
                        |> Map.ofSeq)
                )


        module rec FlukeDate =
            let isToday =
                Store.readSelectorFamily (
                    $"{nameof FlukeDate}/{nameof isToday}",
                    (fun (date: FlukeDate) get ->
                        let position = Atoms.getAtomValue get Atoms.position

                        match position with
                        | Some position ->
                            let dayStart = Atoms.getAtomValue get Atoms.dayStart

                            Domain.UserInteraction.isToday dayStart position (DateId date)
                        | _ -> false)
                )

            let rec hasCellSelection =
                Store.readSelectorFamily (
                    $"{nameof FlukeDate}/{nameof hasCellSelection}",
                    (fun (date: FlukeDate) get ->
                        let cellSelectionMap = Atoms.getAtomValue get Session.cellSelectionMap

                        cellSelectionMap
                        |> Map.values
                        |> Seq.exists (Set.contains date))
                )


        module rec BulletJournalView =
            let rec weekCellsMap =
                Store.readSelector (
                    $"{nameof BulletJournalView}/{nameof weekCellsMap}",
                    (fun get ->
                        let position = Atoms.getAtomValue get Atoms.position
                        let sortedTaskIdList = Atoms.getAtomValue get Session.sortedTaskIdList

                        match position with
                        | Some position ->
                            let dayStart = Atoms.getAtomValue get Atoms.dayStart
                            let weekStart = Atoms.getAtomValue get Atoms.weekStart

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
                                                (fun taskId -> taskId, Atoms.getAtomValue get (Task.taskState taskId))
                                            |> Map.ofSeq

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
                                                                    |> Map.ofSeq)
                                                            |> Sorting.sortLanesByTimeOfDay
                                                                dayStart
                                                                (FlukeDateTime.Create (referenceDay, dayStart))
                                                            |> List.indexed
                                                            |> List.map
                                                                (fun (i, (taskState, _)) -> taskState.Task.Id, i)
                                                            |> Map.ofSeq

                                                        let newCells =
                                                            cellsMetadata
                                                            |> List.sortBy
                                                                (fun cell ->
                                                                    sortedTasksMap
                                                                    |> Map.tryFind cell.TaskId
                                                                    |> Option.defaultValue -1)

                                                        dateId, newCells)
                                            |> Map.ofSeq

                                        result)
                            weeks
                        | _ -> [])
                )

module X =
    let a = 6
