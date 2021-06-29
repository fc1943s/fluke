namespace Fluke.UI.Frontend

#nowarn "40"

open Fable.Core.JsInterop
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

        let rec debug = Store.atomWithStorageSync ($"{nameof debug}", JS.isDebug (), id)

        let rec lastTotal = Store.atomWithSync ($"{nameof lastTotal}", (Some -1: int option), [])

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

        let rec color = Store.atomWithSync ($"{nameof color}", (None: string option), [])

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

        let fontSizeDefault = 12
        let rec fontSize = Store.atomWithStorageSync ($"{nameof fontSize}", fontSizeDefault, id)

        let darkModeDefault = true
        let rec darkMode = Store.atomWithStorageSync ($"{nameof darkMode}", darkModeDefault, id)

        let systemUiFontDefault = false
        let rec systemUiFont = Store.atomWithStorageSync ($"{nameof systemUiFont}", systemUiFontDefault, id)

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
                (fun getter ->
                    let position = Store.value getter Atoms.position

                    match position with
                    | Some position ->
                        let daysBefore = Store.value getter Atoms.daysBefore
                        let daysAfter = Store.value getter Atoms.daysAfter
                        let dayStart = Store.value getter Atoms.dayStart
                        let dateId = dateId dayStart position
                        let (DateId referenceDay) = dateId

                        referenceDay
                        |> List.singleton
                        |> Rendering.getDateSequence (daysBefore, daysAfter)
                    | _ -> [])
            )

        let rec taskIdMap =
            Store.readSelector (
                $"{nameof taskIdMap}",
                (fun getter ->
                    let databaseIdArray =
                        Store.value getter Atoms.databaseIdSet
                        |> Set.toArray

                    databaseIdArray
                    |> Array.map Atoms.Database.taskIdSet
                    |> Store.waitForAll
                    |> Store.value getter
                    |> Array.mapi (fun i taskIdSet -> databaseIdArray.[i], taskIdSet)
                    |> Map.ofArray)
            )

        let rec deviceInfo = Store.readSelector ($"{nameof deviceInfo}", (fun _ -> JS.deviceInfo))


        module rec Database =
            //            let rec taskIdSet =
//                Store.readSelectorFamily (
//                    $"{nameof Database}/{nameof taskIdSet}",
//                    (fun (databaseId: DatabaseId) getter ->
//                        //                        let taskIdSet = getter.get (Atoms.Session.taskIdSet username)
////
////                        taskIdSet
////                        |> Set.filter
////                            (fun taskId ->
////                                let databaseId' = getter.get (Atoms.Task.databaseId taskId)
////                                databaseId' = databaseId)
//
//                        Store.value getter (Atoms.Database.taskIdSet databaseId))
//                )

            let rec database =
                Store.readSelectorFamily (
                    $"{nameof Database}/{nameof database}",
                    (fun (databaseId: DatabaseId) getter ->
                        {
                            Id = databaseId
                            Name = Store.value getter (Atoms.Database.name databaseId)
                            Owner = Store.value getter (Atoms.Database.owner databaseId)
                            SharedWith = Store.value getter (Atoms.Database.sharedWith databaseId)
                            Position = Store.value getter (Atoms.Database.position databaseId)
                        })
                )

            let rec isReadWrite =
                Store.readSelectorFamily (
                    $"{nameof Database}/{nameof isReadWrite}",
                    (fun (databaseId: DatabaseId) getter ->
                        let username = Store.value getter Store.Atoms.username

                        let access =
                            match username with
                            | Some username ->
                                let database = Store.value getter (database databaseId)

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
                    (fun (attachmentId: AttachmentId) getter ->
                        let timestamp = Store.value getter (Atoms.Attachment.timestamp attachmentId)
                        let attachment = Store.value getter (Atoms.Attachment.attachment attachmentId)

                        match timestamp, attachment with
                        | Some timestamp, Some attachment -> Some (timestamp, attachment)
                        | _ -> None)
                )


        module rec Information =
            let rec attachments =
                Store.readSelectorFamily (
                    $"{nameof Information}/{nameof attachments}",
                    (fun (information: Information) getter ->
                        Store.value getter Atoms.informationAttachmentMap
                        |> Map.tryFind information
                        |> Option.defaultValue Set.empty
                        |> Set.toArray
                        |> Array.map Attachment.attachment
                        |> Store.waitForAll
                        |> Store.value getter
                        |> Array.toList
                        |> List.choose id)
                )

            let rec informationState =
                Store.readSelectorFamily (
                    $"{nameof Information}/{nameof informationState}",
                    (fun (information: Information) getter ->
                        {
                            Information = information
                            Attachments = Store.value getter (attachments information)
                            SortList = []
                        })
                )


        module rec Task =
            let rec task =
                Store.readSelectorFamily (
                    $"{nameof Task}/{nameof task}",
                    (fun (taskId: TaskId) getter ->
                        {
                            Id = taskId
                            Name = Store.value getter (Atoms.Task.name taskId)
                            Information = Store.value getter (Atoms.Task.information taskId)
                            PendingAfter = Store.value getter (Atoms.Task.pendingAfter taskId)
                            MissedAfter = Store.value getter (Atoms.Task.missedAfter taskId)
                            Scheduling = Store.value getter (Atoms.Task.scheduling taskId)
                            Priority = Store.value getter (Atoms.Task.priority taskId)
                            Duration = Store.value getter (Atoms.Task.duration taskId)
                        })
                )

            let rec taskState =
                Store.readSelectorFamily (
                    $"{nameof Task}/{nameof taskState}",
                    (fun (taskId: TaskId) getter ->
                        let task = Store.value getter (task taskId)
                        let dateSequence = Store.value getter dateSequence
                        let statusMap = Store.value getter (Atoms.Task.statusMap taskId)
                        let sessions = Store.value getter (Atoms.Task.sessions taskId)
                        let attachmentIdSet = Store.value getter (Atoms.Task.attachmentIdSet taskId)
                        let cellAttachmentMap = Store.value getter (Atoms.Task.cellAttachmentMap taskId)

                        let attachments =
                            attachmentIdSet
                            |> Set.toArray
                            |> Array.map Attachment.attachment
                            |> Store.waitForAll
                            |> Store.value getter
                            |> Array.toList
                            |> List.choose id
                            |> List.sortByDescending (fst >> FlukeDateTime.DateTime)

                        let cellStateMapWithoutStatus =
                            let dayStart = Store.value getter Atoms.dayStart

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
                                            let attachmentIdArray =
                                                cellAttachmentMap
                                                |> Map.tryFind dateId
                                                |> Option.defaultValue Set.empty
                                                |> Set.toArray

                                            let timestampList =
                                                attachmentIdArray
                                                |> Array.map Atoms.Attachment.timestamp
                                                |> Store.waitForAll
                                                |> Store.value getter
                                                |> Array.toList

                                            let attachmentArray =
                                                attachmentIdArray
                                                |> Array.map Atoms.Attachment.attachment
                                                |> Store.waitForAll
                                                |> Store.value getter

                                            timestampList
                                            |> List.mapi
                                                (fun i timestamp ->
                                                    match timestamp, attachmentArray.[i] with
                                                    | Some timestamp, Some attachment -> Some (timestamp, attachment)
                                                    | _ -> None)
                                            |> List.choose id
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
                    (fun (taskId: TaskId) getter ->
                        let position = Store.value getter Atoms.position
                        let taskState = Store.value getter (taskState taskId)
                        let dateSequence = Store.value getter dateSequence
                        let dayStart = Store.value getter Atoms.dayStart

                        match position with
                        | Some position when not dateSequence.IsEmpty ->
                            Rendering.renderTaskStatusMap dayStart position dateSequence taskState
                        | _ -> Map.empty)
                )

            let rec databaseId =
                Store.selectAtomFamily (
                    $"{nameof Task}/{nameof databaseId}",
                    taskIdMap,
                    (fun (taskId: TaskId) taskIdMap ->
                        let databaseIdList =
                            taskIdMap
                            |> Map.filter (fun _ taskIdSet -> taskIdSet.Contains taskId)
                            |> Map.keys
                            |> Seq.toList

                        match databaseIdList with
                        | [] -> Database.Default.Id
                        | [ databaseId ] -> databaseId
                        | _ -> failwith $"Error: task {taskId} exists in two databases ({databaseIdList})")
                )

            let rec isReadWrite =
                Store.readSelectorFamily (
                    $"{nameof Task}/{nameof isReadWrite}",
                    (fun (taskId: TaskId) getter ->
                        let databaseId = Store.value getter (databaseId taskId)
                        Store.value getter (Database.isReadWrite databaseId))
                )

            let rec lastSession =
                Store.readSelectorFamily (
                    $"{nameof Task}/{nameof lastSession}",
                    (fun (taskId: TaskId) getter ->
                        let dateSequence = Store.value getter dateSequence
                        let taskState = Store.value getter (taskState taskId)

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
                    (fun (taskId: TaskId) getter ->
                        let position = Store.value getter Atoms.position
                        let lastSession = Store.value getter (lastSession taskId)

                        match position, lastSession with
                        | Some position, Some lastSession ->
                            let sessionDuration = Store.value getter Atoms.sessionDuration
                            let sessionBreakDuration = Store.value getter Atoms.sessionBreakDuration

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
                    (fun (taskId: TaskId) getter ->
                        let taskState = Store.value getter (taskState taskId)

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
                    (fun (taskId: TaskId) getter ->
                        let dateSequence = Store.value getter dateSequence
                        let selectionSet = Store.value getter (Atoms.Task.selectionSet taskId)

                        dateSequence
                        |> List.exists (DateId >> selectionSet.Contains))
                )


        module rec Cell =
            let rec sessionStatus =
                Store.selectorFamily (
                    $"{nameof Cell}/{nameof sessionStatus}",
                    (fun (taskId: TaskId, dateId: DateId) getter ->
                        let hideSchedulingOverlay = Store.value getter Atoms.hideSchedulingOverlay

                        if hideSchedulingOverlay then
                            Store.value getter (Atoms.Task.statusMap taskId)
                            |> Map.tryFind dateId
                            |> Option.map UserStatus
                            |> Option.defaultValue Disabled
                        else
                            Store.value getter (Task.statusMap taskId)
                            |> Map.tryFind dateId
                            |> Option.defaultValue Disabled),
                    (fun (taskId: TaskId, dateId: DateId) getter setter newValue ->
                        let statusMap = Store.value getter (Atoms.Task.statusMap taskId)

                        Store.set
                            setter
                            (Atoms.Task.statusMap taskId)
                            (match newValue with
                             | UserStatus (username, status) -> statusMap |> Map.add dateId (username, status)
                             | _ -> statusMap |> Map.remove dateId))
                )

            let rec selected =
                Store.selectorFamily (
                    $"{nameof Cell}/{nameof selected}",
                    (fun (taskId: TaskId, dateId: DateId) getter ->
                        let selectionSet = Store.value getter (Atoms.Task.selectionSet taskId)
                        selectionSet.Contains dateId),
                    (fun (taskId: TaskId, dateId: DateId) _ setter newValue ->
                        Store.change
                            setter
                            (Atoms.Task.selectionSet taskId)
                            ((if newValue then Set.add else Set.remove) dateId))
                )

            let rec sessions =
                Store.readSelectorFamily (
                    $"{nameof Cell}/{nameof sessions}",
                    (fun (taskId: TaskId, dateId: DateId) getter ->
                        let taskState = Store.value getter (Task.taskState taskId)

                        taskState.CellStateMap
                        |> Map.tryFind dateId
                        |> Option.map (fun x -> x.Sessions)
                        |> Option.defaultValue [])
                )

            let rec attachments =
                Store.readSelectorFamily (
                    $"{nameof Cell}/{nameof attachments}",
                    (fun (taskId: TaskId, dateId: DateId) getter ->
                        let taskState = Store.value getter (Task.taskState taskId)

                        taskState.CellStateMap
                        |> Map.tryFind dateId
                        |> Option.map (fun x -> x.Attachments)
                        |> Option.defaultValue [])
                )


        module rec Session =
            let rec taskIdSet =
                Store.readSelector (
                    $"{nameof Session}/{nameof taskIdSet}",
                    (fun getter ->
                        let taskIdMap = Store.value getter taskIdMap

                        if taskIdMap.IsEmpty then
                            Set.empty
                        else
                            taskIdMap |> Map.values |> Seq.reduce Set.union)
                )

            let rec taskStateList =
                Store.readSelector (
                    $"{nameof Session}/{nameof taskStateList}",
                    (fun getter ->
                        let taskIdSet = Store.value getter taskIdSet

                        taskIdSet
                        |> Set.toArray
                        |> Array.map Task.taskState
                        |> Store.waitForAll
                        |> Store.value getter
                        |> Array.toList)
                )

            let rec informationSet =
                Store.readSelector (
                    $"{nameof Session}/{nameof informationSet}",
                    (fun getter ->
                        let taskIdSet = Store.value getter taskIdSet

                        taskIdSet
                        |> Set.toArray
                        |> Array.map Atoms.Task.information
                        |> Store.waitForAll
                        |> Store.value getter
                        |> Array.filter
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
                    (fun getter ->
                        let selectedDatabaseIdSet = Store.value getter Atoms.selectedDatabaseIdSet

                        let taskIdArray = Store.value getter taskIdSet |> Set.toArray

                        taskIdArray
                        |> Array.map Task.databaseId
                        |> Store.waitForAll
                        |> Store.value getter
                        |> Array.indexed
                        |> Array.filter (fun (_, databaseId) -> selectedDatabaseIdSet |> Set.contains databaseId)
                        |> Array.map (fun (i, _) -> taskIdArray.[i])
                        |> Set.ofSeq)
                )

            let rec informationStateList =
                Store.readSelector (
                    $"{nameof Session}/{nameof informationStateList}",
                    (fun getter ->
                        let informationSet = Store.value getter informationSet

                        informationSet
                        |> Set.toArray
                        |> Array.map Information.informationState
                        |> Store.waitForAll
                        |> Store.value getter
                        |> Array.toList)
                )

            let rec activeSessions =
                Store.readSelector (
                    $"{nameof Session}/{nameof activeSessions}",
                    (fun getter ->
                        let selectedTaskIdSet = Store.value getter selectedTaskIdSet
                        let selectedTaskIdArray = selectedTaskIdSet |> Set.toArray

                        let durationArray =
                            selectedTaskIdArray
                            |> Array.map Task.activeSession
                            |> Store.waitForAll
                            |> Store.value getter

                        let nameArray =
                            selectedTaskIdArray
                            |> Array.map Atoms.Task.name
                            |> Store.waitForAll
                            |> Store.value getter

                        durationArray
                        |> Array.toList
                        |> List.sortBy id
                        |> List.mapi
                            (fun i ->
                                Option.map
                                    (fun duration ->
                                        TempUI.ActiveSession (TaskName.Value nameArray.[i], Minute duration)))
                        |> List.choose id)
                )

            let rec filteredTaskIdSet =
                Store.readSelector (
                    $"{nameof Session}/{nameof filteredTaskIdSet}",
                    (fun getter ->
                        let filterTasksByView = Store.value getter Atoms.filterTasksByView
                        let searchText = Store.value getter Atoms.searchText
                        let view = Store.value getter Atoms.view
                        let dateSequence = Store.value getter dateSequence
                        let selectedTaskIdSet = Store.value getter selectedTaskIdSet

                        let selectedTaskList =
                            selectedTaskIdSet
                            |> Set.toArray
                            |> Array.map Task.task
                            |> Store.waitForAll
                            |> Store.value getter
                            |> Array.toList

                        let selectedTaskListSearch =
                            match searchText with
                            | "" -> selectedTaskList
                            | _ ->
                                selectedTaskList
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
                                selectedTaskListSearch
                                |> List.map (fun task -> Task.taskState task.Id)
                                |> List.toArray
                                |> Store.waitForAll
                                |> Store.value getter
                                |> filterTaskStateSeq view dateSequence
                                |> Seq.toList
                                |> List.map (fun taskState -> taskState.Task)
                            else
                                selectedTaskListSearch


                        JS.log
                            (fun () ->
                                $"filteredTaskList.Length={filteredTaskList.Length} taskListSearch.Length={
                                                                                                               selectedTaskListSearch.Length
                                }")

                        filteredTaskList
                        |> List.map (fun task -> task.Id)
                        |> Set.ofSeq)
                )

            let rec sortedTaskIdList =
                Store.readSelector (
                    $"{nameof Session}/{nameof sortedTaskIdList}",
                    (fun getter ->
                        let position = Store.value getter Atoms.position

                        match position with
                        | Some position ->
                            let filteredTaskIdSet = Store.value getter filteredTaskIdSet

                            JS.log (fun () -> $"sortedTaskIdList. filteredTaskIdSet.Count={filteredTaskIdSet.Count}")

                            let filteredTaskIdArray = filteredTaskIdSet |> Set.toArray

                            let statusMapArray =
                                filteredTaskIdArray
                                |> Array.map Task.statusMap
                                |> Store.waitForAll
                                |> Store.value getter

                            let taskStateArray =
                                filteredTaskIdArray
                                |> Array.map Task.taskState
                                |> Store.waitForAll
                                |> Store.value getter

                            let lanes =
                                statusMapArray
                                |> Array.zip taskStateArray
                                |> Array.toList

                            let view = Store.value getter Atoms.view
                            let dayStart = Store.value getter Atoms.dayStart
                            let informationStateList = Store.value getter Session.informationStateList

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
                    (fun getter ->
                        let sortedTaskIdList = Store.value getter sortedTaskIdList

                        let informationArray =
                            sortedTaskIdList
                            |> List.toArray
                            |> Array.map Atoms.Task.information
                            |> Store.waitForAll
                            |> Store.value getter

                        sortedTaskIdList
                        |> List.indexed
                        |> List.groupBy (fun (i, _) -> informationArray.[i])
                        |> List.sortBy (fun (information, _) -> information |> Information.Name)
                        |> List.groupBy (fun (information, _) -> Information.toString information)
                        |> List.sortBy (
                            snd
                            >> List.tryHead
                            >> Option.map (fst >> Information.toTag)
                            >> Option.defaultValue -1
                        )
                        |> List.map (fun (a, b) -> a, b |> List.map (fun (c, d) -> c, d |> List.map snd)))
                )

            let rec selectionSetMap =
                Store.readSelector (
                    $"{nameof Session}/{nameof selectionSetMap}",
                    (fun getter ->
                        let sortedTaskIdList = Store.value getter sortedTaskIdList

                        sortedTaskIdList
                        |> List.toArray
                        |> Array.map Atoms.Task.selectionSet
                        |> Store.waitForAll
                        |> Store.value getter
                        |> Array.mapi (fun i dates -> sortedTaskIdList.[i], dates)
                        |> Map.ofArray)
                )

            let rec cellSelectionMap =
                Store.readSelector (
                    $"{nameof Session}/{nameof cellSelectionMap}",
                    (fun getter ->
                        let selectionSetMap = Store.value getter selectionSetMap
                        let dateSequence = Store.value getter dateSequence

                        selectionSetMap
                        |> Map.keys
                        |> Seq.map
                            (fun taskId ->
                                let dates =
                                    dateSequence
                                    |> List.map (fun date -> date, selectionSetMap.[taskId].Contains (DateId date))
                                    |> List.filter snd
                                    |> List.map fst
                                    |> Set.ofSeq

                                taskId, dates)
                        |> Seq.filter (fun (_, dates) -> Set.isEmpty dates |> not)
                        |> Map.ofSeq)
                )


        module rec FlukeDate =
            let isToday =
                Store.readSelectorFamily (
                    $"{nameof FlukeDate}/{nameof isToday}",
                    (fun (date: FlukeDate) getter ->
                        let position = Store.value getter Atoms.position

                        match position with
                        | Some position ->
                            let dayStart = Store.value getter Atoms.dayStart

                            Domain.UserInteraction.isToday dayStart position (DateId date)
                        | _ -> false)
                )

            let rec hasCellSelection =
                Store.readSelectorFamily (
                    $"{nameof FlukeDate}/{nameof hasCellSelection}",
                    (fun (date: FlukeDate) getter ->
                        JS.log (fun () -> $"hasCellSelection date={date |> FlukeDate.Stringify}")

                        Browser.Dom.window?lastDate <- date

                        let cellSelectionMap = Store.value getter Session.cellSelectionMap

                        cellSelectionMap
                        |> Map.values
                        |> Seq.exists (Set.contains date))
                )


        module rec BulletJournalView =
            let rec weekCellsMap =
                Store.readSelector (
                    $"{nameof BulletJournalView}/{nameof weekCellsMap}",
                    (fun getter ->
                        let position = Store.value getter Atoms.position
                        let sortedTaskIdList = Store.value getter Session.sortedTaskIdList

                        let taskStateArray =
                            sortedTaskIdList
                            |> List.map Task.taskState
                            |> List.toArray
                            |> Store.waitForAll
                            |> Store.value getter

                        let taskStateMap =
                            sortedTaskIdList
                            |> List.mapi (fun i taskId -> taskId, taskStateArray.[i])
                            |> Map.ofSeq

                        match position with
                        | Some position ->
                            let dayStart = Store.value getter Atoms.dayStart
                            let weekStart = Store.value getter Atoms.weekStart

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

                                        let result =
                                            sortedTaskIdList
                                            |> List.collect
                                                (fun taskId ->
                                                    dateIdSequence
                                                    |> List.map
                                                        (fun dateId ->
                                                            let isToday = isToday dayStart position dateId

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
