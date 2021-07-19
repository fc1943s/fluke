namespace Fluke.UI.Frontend

#nowarn "40"

open Fable.Extras
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

    [<RequireQualifiedAccess>]
    type DatabaseNodeType =
        | Template
        | Owned
        | Shared

    [<RequireQualifiedAccess>]
    type UIFlag =
        | None
        | Database of DatabaseId
        | Information of Information
        | Task of DatabaseId * TaskId
        | Cell of TaskId * DateId
        | File of FileId
        | RawImage of url: string

    [<RequireQualifiedAccess>]
    type AccordionType =
        | Settings
        | DatabaseForm
        | InformationForm
        | TaskForm
        | CellForm

    [<RequireQualifiedAccess>]
    type UIFlagType =
        | Database
        | Information
        | Task
        | Cell
        | File
        | RawImage

    type DeviceId = DeviceId of guid: Guid

    and DeviceId with
        static member inline NewId () = DeviceId (Guid.NewTicksGuid ())
        static member inline Value (DeviceId guid) = guid

    type Ping = Ping of ticksText: string

    and Ping with
        static member inline Value (Ping ticks) = int64 ticks

    type Color = Color of hex: string

    and Color with
        static member inline Value (Color hex) = hex
        static member inline Default = Color "#000000"

    let deviceId =
        match JS.window id with
        | Some window ->
            match window.localStorage.getItem "deviceId" with
            | String.ValidString deviceId -> DeviceId (Guid deviceId)
            | _ ->
                let deviceId = DeviceId.NewId ()
                window.localStorage.setItem ("deviceId", deviceId |> DeviceId.Value |> string)
                deviceId
        | None -> DeviceId.NewId ()


    let uiFlagDefault = UIFlag.None
    let uiVisibleFlagDefault = false
    let accordionHiddenFlagDefault: string [] = [||]

    type UserState =
        {
            Archive: bool option
            AccordionHiddenFlagMap: Map<AccordionType, string []>
            CellColorDisabled: Color
            CellColorSuggested: Color
            CellColorPending: Color
            CellColorMissed: Color
            CellColorMissedToday: Color
            CellColorPostponedUntil: Color
            CellColorPostponed: Color
            CellColorCompleted: Color
            CellColorDismissed: Color
            CellColorScheduled: Color
            CellSize: int
            ClipboardAttachmentIdMap: Map<AttachmentId, bool>
            ClipboardVisible: bool
            DarkMode: bool
            DaysAfter: int
            DaysBefore: int
            DayStart: FlukeTime
            EnableCellPopover: bool
            ExpandedDatabaseIdSet: Set<DatabaseId>
            FilterTasksByView: bool
            FilterTasksText: string
            FontSize: int
            HideSchedulingOverlay: bool
            HideTemplates: bool option
            Language: Language
            LastInformationDatabase: DatabaseId option
            LeftDock: TempUI.DockType option
            LeftDockSize: int
            RandomizeProject: bool
            RandomizeProjectAttachment: bool
            RandomizeArea: bool
            RandomizeAreaAttachment: bool
            RandomizeResource: bool
            RandomizeResourceAttachment: bool
            RandomizeProjectTask: bool
            RandomizeAreaTask: bool
            RandomizeProjectTaskAttachment: bool
            RandomizeAreaTaskAttachment: bool
            RandomizeCellAttachment: bool
            RightDock: TempUI.DockType option
            RightDockSize: int
            SearchText: string
            SelectedDatabaseIdSet: Set<DatabaseId>
            SessionBreakDuration: Minute
            SessionDuration: Minute
            SystemUiFont: bool
            UIFlagMap: Map<UIFlagType, UIFlag>
            UIVisibleFlagMap: Map<UIFlagType, bool>
            UserColor: Color option
            View: View
            WeekStart: DayOfWeek
        }

    type UserState with
        static member inline Default =
            {
                Archive = None
                AccordionHiddenFlagMap =
                    Union.ToList<AccordionType>
                    |> List.map (fun accordionType -> accordionType, accordionHiddenFlagDefault)
                    |> Map.ofList
                CellColorDisabled = Color "#595959"
                CellColorSuggested = Color "#4C664E"
                CellColorPending = Color "#262626"
                CellColorMissed = Color "#990022"
                CellColorMissedToday = Color "#530011"
                CellColorPostponedUntil = Color "#604800"
                CellColorPostponed = Color "#B08200"
                CellColorCompleted = Color "#339933"
                CellColorDismissed = Color "#673AB7"
                CellColorScheduled = Color "#003038"
                CellSize = 19
                ClipboardAttachmentIdMap = Map.empty
                ClipboardVisible = false
                DarkMode = false
                DaysAfter = 7
                DaysBefore = 7
                DayStart = FlukeTime.Create 0 0
                EnableCellPopover = true
                ExpandedDatabaseIdSet = Set.empty
                FilterTasksByView = false
                FilterTasksText = ""
                FontSize = 15
                HideSchedulingOverlay = false
                HideTemplates = None
                Language = Language.English
                LastInformationDatabase = None
                LeftDock = None
                LeftDockSize = 300
                RandomizeProject = true
                RandomizeProjectAttachment = true
                RandomizeArea = true
                RandomizeAreaAttachment = true
                RandomizeResource = true
                RandomizeResourceAttachment = true
                RandomizeProjectTask = true
                RandomizeAreaTask = true
                RandomizeProjectTaskAttachment = true
                RandomizeAreaTaskAttachment = true
                RandomizeCellAttachment = false
                RightDock = None
                RightDockSize = 300
                SearchText = ""
                SelectedDatabaseIdSet = Set.empty
                SessionBreakDuration = Minute 5
                SessionDuration = Minute 25
                SystemUiFont = true
                UIFlagMap =
                    Union.ToList<UIFlagType>
                    |> List.map (fun uiFlagType -> uiFlagType, uiFlagDefault)
                    |> Map.ofList
                UIVisibleFlagMap =
                    Union.ToList<UIFlagType>
                    |> List.map (fun uiFlagType -> uiFlagType, uiVisibleFlagDefault)
                    |> Map.ofList
                UserColor = None
                View = View.View.Information
                WeekStart = DayOfWeek.Sunday
            }


    module Atoms =


        //        module rec Events =
//            type EventId = EventId of position: float * guid: Guid
//
//            let newEventId () =
//                EventId (JS.Constructors.Date.now (), Guid.NewTicksGuid ())
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

        let rec debug = Store.atomWithStorageSync ($"{nameof debug}", JS.isDebug ())
        let rec sessionRestored = Store.atom ($"{nameof sessionRestored}", false)
        let rec initialPeerSkipped = Store.atom ($"{nameof initialPeerSkipped}", false)
        let rec position = Store.atom ($"{nameof position}", None)
        let rec ctrlPressed = Store.atom ($"{nameof ctrlPressed}", false)
        let rec shiftPressed = Store.atom ($"{nameof shiftPressed}", false)

        module User =
            let rec archive = Store.atomWithSync ($"{nameof User}/{nameof archive}", UserState.Default.Archive, [])

            let rec cellColorDisabled =
                Store.atomWithSync (
                    $"{nameof User}/{nameof cellColorDisabled}",
                    UserState.Default.CellColorDisabled,
                    []
                )

            let rec cellColorSuggested =
                Store.atomWithSync (
                    $"{nameof User}/{nameof cellColorSuggested}",
                    UserState.Default.CellColorSuggested,
                    []
                )

            let rec cellColorPending =
                Store.atomWithSync ($"{nameof User}/{nameof cellColorPending}", UserState.Default.CellColorPending, [])

            let rec cellColorMissed =
                Store.atomWithSync ($"{nameof User}/{nameof cellColorMissed}", UserState.Default.CellColorMissed, [])

            let rec cellColorMissedToday =
                Store.atomWithSync (
                    $"{nameof User}/{nameof cellColorMissedToday}",
                    UserState.Default.CellColorMissedToday,
                    []
                )

            let rec cellColorPostponedUntil =
                Store.atomWithSync (
                    $"{nameof User}/{nameof cellColorPostponedUntil}",
                    UserState.Default.CellColorPostponedUntil,
                    []
                )

            let rec cellColorPostponed =
                Store.atomWithSync (
                    $"{nameof User}/{nameof cellColorPostponed}",
                    UserState.Default.CellColorPostponed,
                    []
                )

            let rec cellColorCompleted =
                Store.atomWithSync (
                    $"{nameof User}/{nameof cellColorCompleted}",
                    UserState.Default.CellColorCompleted,
                    []
                )

            let rec cellColorDismissed =
                Store.atomWithSync (
                    $"{nameof User}/{nameof cellColorDismissed}",
                    UserState.Default.CellColorDismissed,
                    []
                )

            let rec cellColorScheduled =
                Store.atomWithSync (
                    $"{nameof User}/{nameof cellColorScheduled}",
                    UserState.Default.CellColorScheduled,
                    []
                )

            let rec cellSize = Store.atomWithSync ($"{nameof User}/{nameof cellSize}", UserState.Default.CellSize, [])

            let rec clipboardAttachmentIdMap =
                Store.atomWithSync (
                    $"{nameof User}/{nameof clipboardAttachmentIdMap}",
                    UserState.Default.ClipboardAttachmentIdMap,
                    []
                )

            let rec clipboardVisible =
                Store.atomWithSync ($"{nameof User}/{nameof clipboardVisible}", UserState.Default.ClipboardVisible, [])

            let rec darkMode =
                Store.atomWithStorageSync ($"{nameof User}/{nameof darkMode}", UserState.Default.DarkMode)

            let rec daysAfter =
                Store.atomWithSync ($"{nameof User}/{nameof daysAfter}", UserState.Default.DaysAfter, [])

            let rec daysBefore =
                Store.atomWithSync ($"{nameof User}/{nameof daysBefore}", UserState.Default.DaysBefore, [])

            let rec dayStart = Store.atomWithSync ($"{nameof User}/{nameof dayStart}", UserState.Default.DayStart, [])

            let rec enableCellPopover =
                Store.atomWithSync (
                    $"{nameof User}/{nameof enableCellPopover}",
                    UserState.Default.EnableCellPopover,
                    []
                )

            let rec expandedDatabaseIdSet =
                Store.atomWithSync (
                    $"{nameof User}/{nameof expandedDatabaseIdSet}",
                    UserState.Default.ExpandedDatabaseIdSet,
                    []
                )

            let rec filterTasksByView =
                Store.atomWithSync (
                    $"{nameof User}/{nameof filterTasksByView}",
                    UserState.Default.FilterTasksByView,
                    []
                )

            let rec filterTasksText =
                Store.atomWithSync ($"{nameof User}/{nameof filterTasksText}", UserState.Default.FilterTasksText, [])

            let rec fontSize =
                Store.atomWithStorageSync ($"{nameof User}/{nameof fontSize}", UserState.Default.FontSize)

            let rec hideSchedulingOverlay =
                Store.atomWithSync (
                    $"{nameof User}/{nameof hideSchedulingOverlay}",
                    UserState.Default.HideSchedulingOverlay,
                    []
                )

            let rec hideTemplates =
                Store.atomWithSync ($"{nameof User}/{nameof hideTemplates}", UserState.Default.HideTemplates, [])

            let rec language = Store.atomWithSync ($"{nameof User}/{nameof language}", UserState.Default.Language, [])

            let rec lastInformationDatabase =
                Store.atomWithSync (
                    $"{nameof User}/{nameof lastInformationDatabase}",
                    UserState.Default.LastInformationDatabase,
                    []
                )

            let rec leftDock = Store.atomWithSync ($"{nameof User}/{nameof leftDock}", UserState.Default.LeftDock, [])

            let rec leftDockSize =
                Store.atomWithSync ($"{nameof User}/{nameof leftDockSize}", UserState.Default.LeftDockSize, [])

            let rec randomizeProject =
                Store.atomWithSync ($"{nameof User}/{nameof randomizeProject}", UserState.Default.RandomizeProject, [])

            let rec randomizeProjectAttachment =
                Store.atomWithSync (
                    $"{nameof User}/{nameof randomizeProjectAttachment}",
                    UserState.Default.RandomizeProjectAttachment,
                    []
                )

            let rec randomizeArea =
                Store.atomWithSync ($"{nameof User}/{nameof randomizeArea}", UserState.Default.RandomizeArea, [])

            let rec randomizeAreaAttachment =
                Store.atomWithSync (
                    $"{nameof User}/{nameof randomizeAreaAttachment}",
                    UserState.Default.RandomizeAreaAttachment,
                    []
                )

            let rec randomizeResource =
                Store.atomWithSync (
                    $"{nameof User}/{nameof randomizeResource}",
                    UserState.Default.RandomizeResource,
                    []
                )

            let rec randomizeResourceAttachment =
                Store.atomWithSync (
                    $"{nameof User}/{nameof randomizeResourceAttachment}",
                    UserState.Default.RandomizeResourceAttachment,
                    []
                )

            let rec randomizeProjectTask =
                Store.atomWithSync (
                    $"{nameof User}/{nameof randomizeProjectTask}",
                    UserState.Default.RandomizeProjectTask,
                    []
                )

            let rec randomizeAreaTask =
                Store.atomWithSync (
                    $"{nameof User}/{nameof randomizeAreaTask}",
                    UserState.Default.RandomizeAreaTask,
                    []
                )

            let rec randomizeProjectTaskAttachment =
                Store.atomWithSync (
                    $"{nameof User}/{nameof randomizeProjectTaskAttachment}",
                    UserState.Default.RandomizeProjectTaskAttachment,
                    []
                )

            let rec randomizeAreaTaskAttachment =
                Store.atomWithSync (
                    $"{nameof User}/{nameof randomizeAreaTaskAttachment}",
                    UserState.Default.RandomizeAreaTaskAttachment,
                    []
                )

            let rec randomizeCellAttachment =
                Store.atomWithSync (
                    $"{nameof User}/{nameof randomizeCellAttachment}",
                    UserState.Default.RandomizeCellAttachment,
                    []
                )

            let rec rightDock =
                Store.atomWithSync ($"{nameof User}/{nameof rightDock}", UserState.Default.RightDock, [])

            let rec rightDockSize =
                Store.atomWithSync ($"{nameof User}/{nameof rightDockSize}", UserState.Default.RightDockSize, [])

            let rec searchText =
                Store.atomWithSync ($"{nameof User}/{nameof searchText}", UserState.Default.SearchText, [])

            let rec selectedDatabaseIdSet =
                Store.atomWithSync (
                    $"{nameof User}/{nameof selectedDatabaseIdSet}",
                    UserState.Default.SelectedDatabaseIdSet,
                    []
                )

            let rec sessionBreakDuration =
                Store.atomWithSync (
                    $"{nameof User}/{nameof sessionBreakDuration}",
                    UserState.Default.SessionBreakDuration,
                    []
                )

            let rec sessionDuration =
                Store.atomWithSync ($"{nameof User}/{nameof sessionDuration}", UserState.Default.SessionDuration, [])

            let rec systemUiFont =
                Store.atomWithStorageSync ($"{nameof User}/{nameof systemUiFont}", UserState.Default.SystemUiFont)

            let rec userColor = Store.atomWithSync ($"{nameof User}/{nameof userColor}", (None: Color option), [])

            let rec view = Store.atomWithSync ($"{nameof User}/{nameof view}", UserState.Default.View, [])

            let rec weekStart =
                Store.atomWithSync ($"{nameof User}/{nameof weekStart}", UserState.Default.WeekStart, [])

            let rec uiFlag =
                Store.atomFamilyWithSync (
                    $"{nameof User}/{nameof uiFlag}",
                    (fun (_uiFlagType: UIFlagType) -> uiFlagDefault),
                    (string >> List.singleton)
                )

            let rec uiVisibleFlag =
                Store.atomFamilyWithSync (
                    $"{nameof User}/{nameof uiVisibleFlag}",
                    (fun (_uiFlagType: UIFlagType) -> uiVisibleFlagDefault),
                    (string >> List.singleton)
                )

            let rec accordionHiddenFlag =
                Store.atomFamilyWithSync (
                    $"{nameof User}/{nameof accordionHiddenFlag}",
                    (fun (_accordionType: AccordionType) -> accordionHiddenFlagDefault),
                    (string >> List.singleton)
                )


        module rec Device =
            let rec devicePing =
                Store.atomFamilyWithSync (
                    $"{nameof Device}/{nameof devicePing}",
                    (fun (_deviceId: DeviceId) -> Ping "0"),
                    (DeviceId.Value >> string >> List.singleton)
                )


        module rec Database =
            let databaseIdIdentifier (databaseId: DatabaseId) =
                databaseId
                |> DatabaseId.Value
                |> string
                |> List.singleton

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

            let rec informationAttachmentIdMap =
                Store.atomFamilyWithSync (
                    $"{nameof Database}/{nameof informationAttachmentIdMap}",
                    (fun (_databaseId: DatabaseId) -> Map.empty: Map<Information, Set<AttachmentId>>),
                    databaseIdIdentifier
                )


        module rec File =
            let fileIdIdentifier (fileId: FileId) =
                fileId |> FileId.Value |> string |> List.singleton


            let rec chunkCount =
                Store.atomFamilyWithSync (
                    $"{nameof File}/{nameof chunkCount}",
                    (fun (_fileId: FileId) -> 0),
                    fileIdIdentifier
                )

            let rec chunk =
                Store.atomFamilyWithSync (
                    $"{nameof File}/{nameof chunk}",
                    (fun (_fileId: FileId, _index: int) -> ""),
                    (fun (fileId: FileId, index: int) ->
                        fileIdIdentifier fileId
                        @ [
                            string index
                        ])
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

            let rec archived =
                Store.atomFamilyWithSync (
                    $"{nameof Attachment}/{nameof archived}",
                    (fun (_attachmentId: AttachmentId) -> None: bool option),
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

            let rec databaseId =
                Store.atomFamilyWithSync (
                    $"{nameof Task}/{nameof databaseId}",
                    (fun (_taskId: TaskId) -> Database.Default.Id),
                    taskIdIdentifier
                )

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

            let rec cellAttachmentIdMap =
                Store.atomFamilyWithSync (
                    $"{nameof Task}/{nameof cellAttachmentIdMap}",
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

            let rec archived =
                Store.atomFamilyWithSync (
                    $"{nameof Task}/{nameof archived}",
                    (fun (_taskId: TaskId) -> None: bool option),
                    taskIdIdentifier
                )


        module rec Cell =
            let cellIdentifier (taskId: TaskId) (dateId: DateId) =
                [
                    taskId |> TaskId.Value |> string
                    dateId |> DateId.Value |> FlukeDate.Stringify
                ]


    module rec Selectors =
        let rec dateIdArray =
            Store.readSelector (
                $"{nameof dateIdArray}",
                (fun getter ->
                    let position = Store.value getter Atoms.position

                    match position with
                    | Some position ->
                        let daysBefore = Store.value getter Atoms.User.daysBefore
                        let daysAfter = Store.value getter Atoms.User.daysAfter
                        let dayStart = Store.value getter Atoms.User.dayStart
                        let dateId = dateId dayStart position
                        let (DateId referenceDay) = dateId

                        referenceDay
                        |> List.singleton
                        |> Rendering.getDateSequence (daysBefore, daysAfter)
                        |> List.map DateId
                        |> List.toArray
                    | _ -> [||])
            )

        let rec dateIdAtoms =
            Store.readSelector (
                $"{nameof dateIdAtoms}",
                (fun getter ->
                    dateIdArray
                    |> Jotai.jotaiUtils.splitAtom
                    |> Store.value getter)
            )

        let rec dateIdAtomsByMonth =
            Store.readSelector (
                $"{nameof dateIdAtomsByMonth}",
                (fun getter ->
                    let dateIdArray = Store.value getter dateIdArray
                    let dateIdAtoms = Store.value getter dateIdAtoms

                    dateIdArray
                    |> Array.indexed
                    |> Array.groupBy (fun (_, dateId) -> dateId |> DateId.Value |> fun date -> date.Month)
                    |> Array.map (fun (_, dates) -> dates |> Array.map (fun (i, _) -> dateIdAtoms.[i])))
            )

        let rec deviceInfo = Store.readSelector ($"{nameof deviceInfo}", (fun _ -> JS.deviceInfo))

        let rec asyncDatabaseIdAtoms =
            Store.selectAtomSyncKeys (
                $"{nameof asyncDatabaseIdAtoms}",
                Atoms.Database.name,
                Database.Default.Id,
                (Guid >> DatabaseId)
            )

        let rec asyncTaskIdAtoms =
            Store.selectAtomSyncKeys (
                $"{nameof asyncTaskIdAtoms}",
                Atoms.Task.databaseId,
                Task.Default.Id,
                (Guid >> TaskId)
            )

        let rec asyncDeviceIdAtoms =
            Store.selectAtomSyncKeys (
                $"{nameof asyncDeviceIdAtoms}",
                Atoms.Device.devicePing,
                deviceId,
                (Guid >> DeviceId)
            )

        let rec databaseIdAtoms =
            Store.readSelector (
                $"{nameof databaseIdAtoms}",
                (fun getter ->
                    let asyncDatabaseIdAtoms = Store.value getter asyncDatabaseIdAtoms
                    let hideTemplates = Store.value getter Atoms.User.hideTemplates

                    asyncDatabaseIdAtoms
                    |> Array.filter
                        (fun databaseIdAtom ->
                            let databaseId = Store.value getter databaseIdAtom
                            let database = Store.value getter (Selectors.Database.database databaseId)

                            let valid =
                                database.Name
                                |> DatabaseName.Value
                                |> String.IsNullOrWhiteSpace
                                |> not
                                && database.Owner
                                   |> Username.Value
                                   |> String.IsNullOrWhiteSpace
                                   |> not

                            if not valid then
                                false
                            else
                                let nodeType = Store.value getter (Selectors.Database.nodeType databaseId)

                                nodeType <> DatabaseNodeType.Template
                                || hideTemplates = Some false))
            )


        module User =
            let rec userState =
                Store.readSelector (
                    $"{nameof User}/{nameof userState}",
                    (fun getter ->
                        {
                            Archive = Store.value getter Atoms.User.archive
                            AccordionHiddenFlagMap =
                                Union.ToList<AccordionType>
                                |> List.map
                                    (fun accordionType ->
                                        accordionType, Store.value getter (Atoms.User.accordionHiddenFlag accordionType))
                                |> Map.ofList
                            CellColorDisabled = Store.value getter Atoms.User.cellColorDisabled
                            CellColorSuggested = Store.value getter Atoms.User.cellColorSuggested
                            CellColorPending = Store.value getter Atoms.User.cellColorPending
                            CellColorMissed = Store.value getter Atoms.User.cellColorMissed
                            CellColorMissedToday = Store.value getter Atoms.User.cellColorMissedToday
                            CellColorPostponedUntil = Store.value getter Atoms.User.cellColorPostponedUntil
                            CellColorPostponed = Store.value getter Atoms.User.cellColorPostponed
                            CellColorCompleted = Store.value getter Atoms.User.cellColorCompleted
                            CellColorDismissed = Store.value getter Atoms.User.cellColorDismissed
                            CellColorScheduled = Store.value getter Atoms.User.cellColorScheduled
                            CellSize = Store.value getter Atoms.User.cellSize
                            ClipboardAttachmentIdMap = Store.value getter Atoms.User.clipboardAttachmentIdMap
                            ClipboardVisible = Store.value getter Atoms.User.clipboardVisible
                            DarkMode = Store.value getter Atoms.User.darkMode
                            DaysAfter = Store.value getter Atoms.User.daysAfter
                            DaysBefore = Store.value getter Atoms.User.daysBefore
                            DayStart = Store.value getter Atoms.User.dayStart
                            EnableCellPopover = Store.value getter Atoms.User.enableCellPopover
                            ExpandedDatabaseIdSet = Store.value getter Atoms.User.expandedDatabaseIdSet
                            FilterTasksByView = Store.value getter Atoms.User.filterTasksByView
                            FilterTasksText = Store.value getter Atoms.User.filterTasksText
                            FontSize = Store.value getter Atoms.User.fontSize
                            HideSchedulingOverlay = Store.value getter Atoms.User.hideSchedulingOverlay
                            HideTemplates = Store.value getter Atoms.User.hideTemplates
                            Language = Store.value getter Atoms.User.language
                            LastInformationDatabase = Store.value getter Atoms.User.lastInformationDatabase
                            LeftDock = Store.value getter Atoms.User.leftDock
                            LeftDockSize = Store.value getter Atoms.User.leftDockSize
                            RandomizeProject = Store.value getter Atoms.User.randomizeProject
                            RandomizeProjectAttachment = Store.value getter Atoms.User.randomizeProjectAttachment
                            RandomizeArea = Store.value getter Atoms.User.randomizeArea
                            RandomizeAreaAttachment = Store.value getter Atoms.User.randomizeAreaAttachment
                            RandomizeResource = Store.value getter Atoms.User.randomizeResource
                            RandomizeResourceAttachment = Store.value getter Atoms.User.randomizeResourceAttachment
                            RandomizeProjectTask = Store.value getter Atoms.User.randomizeProjectTask
                            RandomizeAreaTask = Store.value getter Atoms.User.randomizeAreaTask
                            RandomizeProjectTaskAttachment =
                                Store.value getter Atoms.User.randomizeProjectTaskAttachment
                            RandomizeAreaTaskAttachment = Store.value getter Atoms.User.randomizeAreaTaskAttachment
                            RandomizeCellAttachment = Store.value getter Atoms.User.randomizeCellAttachment
                            RightDock = Store.value getter Atoms.User.rightDock
                            RightDockSize = Store.value getter Atoms.User.rightDockSize
                            SearchText = Store.value getter Atoms.User.searchText
                            SelectedDatabaseIdSet = Store.value getter Atoms.User.selectedDatabaseIdSet
                            SessionBreakDuration = Store.value getter Atoms.User.sessionBreakDuration
                            SessionDuration = Store.value getter Atoms.User.sessionDuration
                            SystemUiFont = Store.value getter Atoms.User.systemUiFont
                            UIFlagMap =
                                Union.ToList<UIFlagType>
                                |> List.map
                                    (fun uiFlagType -> uiFlagType, Store.value getter (Atoms.User.uiFlag uiFlagType))
                                |> Map.ofList
                            UIVisibleFlagMap =
                                Union.ToList<UIFlagType>
                                |> List.map
                                    (fun uiFlagType ->
                                        uiFlagType, Store.value getter (Atoms.User.uiVisibleFlag uiFlagType))
                                |> Map.ofList
                            UserColor = Store.value getter Atoms.User.userColor
                            View = Store.value getter Atoms.User.view
                            WeekStart = Store.value getter Atoms.User.weekStart
                        })
                )


        module Database =
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

            let rec nodeType =
                Store.readSelectorFamily (
                    $"{nameof Database}/{nameof nodeType}",
                    (fun (databaseId: DatabaseId) getter ->
                        let database = Store.value getter (database databaseId)
                        let username = Store.value getter Store.Atoms.username

                        match database.Owner with
                        | owner when owner = Templates.templatesUser.Username -> DatabaseNodeType.Template
                        | owner when Some owner = username -> DatabaseNodeType.Owned
                        | _ -> DatabaseNodeType.Shared)
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

            let rec taskIdAtoms =
                Store.readSelectorFamily (
                    $"{nameof Database}/{nameof taskIdAtoms}",
                    (fun (databaseId: DatabaseId) getter ->
                        asyncTaskIdAtoms
                        |> Store.value getter
                        |> Array.filter
                            (fun taskIdAtom ->
                                let taskId = Store.value getter taskIdAtom
                                let databaseId' = Store.value getter (Atoms.Task.databaseId taskId)
                                databaseId = databaseId'))
                )

            let rec unarchivedTaskIdAtoms =
                Store.readSelectorFamily (
                    $"{nameof Database}/{nameof unarchivedTaskIdAtoms}",
                    (fun (databaseId: DatabaseId) getter ->
                        let taskIdAtoms = Store.value getter (taskIdAtoms databaseId)

                        taskIdAtoms
                        |> Array.filter
                            (fun taskIdAtom ->
                                let taskId = Store.value getter taskIdAtom
                                let archived = Store.value getter (Atoms.Task.archived taskId)
                                archived = Some false))
                )

            let rec archivedTaskIdAtoms =
                Store.readSelectorFamily (
                    $"{nameof Database}/{nameof archivedTaskIdAtoms}",
                    (fun (databaseId: DatabaseId) getter ->
                        let taskIdAtoms = Store.value getter (taskIdAtoms databaseId)

                        taskIdAtoms
                        |> Array.filter
                            (fun taskIdAtom ->
                                let taskId = Store.value getter taskIdAtom
                                let archived = Store.value getter (Atoms.Task.archived taskId)
                                archived = Some true))
                )

            let rec taskIdAtomsByArchive =
                Store.readSelectorFamily (
                    $"{nameof Database}/{nameof taskIdAtomsByArchive}",
                    (fun (databaseId: DatabaseId) getter ->
                        let archive = Store.value getter Atoms.User.archive

                        databaseId
                        |> (if archive = Some true then
                                Database.archivedTaskIdAtoms
                            else
                                Database.unarchivedTaskIdAtoms)
                        |> Store.value getter)
                )

            let rec informationAttachmentIdMapByArchive =
                Store.readSelectorFamily (
                    $"{nameof Database}/{nameof informationAttachmentIdMapByArchive}",
                    (fun (databaseId: DatabaseId) getter ->
                        let archive = Store.value getter Atoms.User.archive

                        let informationAttachmentIdMap =
                            Store.value getter (Atoms.Database.informationAttachmentIdMap databaseId)

                        let attachmentIdArray =
                            informationAttachmentIdMap
                            |> Map.values
                            |> Seq.fold Set.union Set.empty
                            |> Seq.toArray

                        let archivedArray =
                            attachmentIdArray
                            |> Array.map Atoms.Attachment.archived
                            |> Store.waitForAll
                            |> Store.value getter

                        let archivedMap =
                            archivedArray
                            |> Array.zip attachmentIdArray
                            |> Map.ofArray

                        informationAttachmentIdMap
                        |> Map.map
                            (fun _ attachmentIdSet ->
                                attachmentIdSet
                                |> Set.filter (fun attachmentId -> archivedMap.[attachmentId] = archive)))
                )

            let rec databaseState =
                Store.readSelectorFamily (
                    $"{nameof Database}/{nameof databaseState}",
                    (fun (databaseId: DatabaseId) getter ->
                        let database = Store.value getter (Selectors.Database.database databaseId)

                        let taskIdAtoms = Store.value getter (Selectors.Database.taskIdAtoms databaseId)

                        let taskStateList: TaskState list =
                            taskIdAtoms
                            |> Array.toList
                            |> List.map (Store.value getter)
                            |> List.map Selectors.Task.taskState
                            |> List.map (Store.value getter)

                        let fileIdList =
                            taskStateList
                            |> List.collect
                                (fun taskState ->
                                    taskState.AttachmentStateList
                                    |> List.choose
                                        (fun attachmentState ->
                                            match attachmentState.Attachment with
                                            | Attachment.Image fileId -> Some fileId
                                            | _ -> None))

                        let hexStringList =
                            fileIdList
                            |> List.map Selectors.File.hexString
                            |> List.toArray
                            |> Store.waitForAll
                            |> Store.value getter

                        if hexStringList |> Array.contains None then
                            Error "Invalid files present"
                        else
                            let fileMap =
                                fileIdList
                                |> List.mapi (fun i fileId -> fileId, hexStringList.[i].Value)
                                |> Map.ofList

                            let informationAttachmentIdMap =
                                Store.value getter (Atoms.Database.informationAttachmentIdMap databaseId)

                            let informationStateMap =
                                informationAttachmentIdMap
                                |> Map.map
                                    (fun information attachmentIdSet ->
                                        let attachmentStateList =
                                            attachmentIdSet
                                            |> Set.toArray
                                            |> Array.map Selectors.Attachment.attachmentState
                                            |> Store.waitForAll
                                            |> Store.value getter
                                            |> Array.toList
                                            |> List.choose id

                                        {
                                            Information = information
                                            AttachmentStateList = attachmentStateList
                                            SortList = []
                                        })
                                |> Map.filter
                                    (fun _ informationState ->
                                        not informationState.AttachmentStateList.IsEmpty
                                        || not informationState.SortList.IsEmpty)

                            let taskStateMap =
                                taskStateList
                                |> List.map
                                    (fun taskState ->
                                        taskState.Task.Id,
                                        { taskState with
                                            CellStateMap =
                                                taskState.CellStateMap
                                                |> Map.map (fun _ cellState -> { cellState with SessionList = [] })
                                        })
                                |> Map.ofSeq

                            let databaseState =
                                {
                                    Database = database
                                    InformationStateMap = informationStateMap
                                    TaskStateMap = taskStateMap
                                    FileMap = fileMap
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
                                Error "Database is not fully synced"
                            else
                                Ok databaseState)
                )


        module File =
            let rec hexString =
                Store.readSelectorFamily (
                    $"{nameof File}/{nameof hexString}",
                    (fun (fileId: FileId) getter ->
                        let chunkCount = Store.value getter (Atoms.File.chunkCount fileId)

                        match chunkCount with
                        | 0 -> None
                        | _ ->
                            let chunks =
                                [|
                                    0 .. chunkCount - 1
                                |]
                                |> Array.map (fun i -> Atoms.File.chunk (fileId, i))
                                |> Store.waitForAll
                                |> Store.value getter

                            if chunks |> Array.contains "" then
                                JS.log
                                    (fun () ->
                                        $"File.blob
                                        incomplete blob. skipping
                                    chunkCount={chunkCount}
                                    chunks.Length={chunks.Length}
                                    chunks.[0].Length={if chunks.Length = 0 then unbox null else chunks.[0].Length}
                                    ")

                                None
                            else
                                JS.log
                                    (fun () ->
                                        $"File.blob
                                    chunkCount={chunkCount}
                                    chunks.Length={chunks.Length}
                                    chunks.[0].Length={if chunks.Length = 0 then unbox null else chunks.[0].Length}
                                    ")

                                Some (chunks |> String.concat ""))
                )

            let rec blob =
                Store.readSelectorFamily (
                    $"{nameof File}/{nameof blob}",
                    (fun (fileId: FileId) getter ->
                        let hexString = Store.value getter (hexString fileId)

                        hexString
                        |> Option.map JS.hexStringToByteArray
                        |> Option.map
                            (fun bytes -> JS.uint8ArrayToBlob (JSe.Uint8Array (unbox<uint8 []> bytes)) "image/png"))
                )

            let rec objectUrl =
                Store.readSelectorFamily (
                    $"{nameof File}/{nameof objectUrl}",
                    (fun (fileId: FileId) getter ->
                        let blob = Store.value getter (blob fileId)
                        blob |> Option.map Browser.Url.URL.createObjectURL)
                )


        module Attachment =
            let rec attachmentState =
                Store.readSelectorFamily (
                    $"{nameof Attachment}/{nameof attachmentState}",
                    (fun (attachmentId: AttachmentId) getter ->
                        let timestamp = Store.value getter (Atoms.Attachment.timestamp attachmentId)
                        let archived = Store.value getter (Atoms.Attachment.archived attachmentId)
                        let attachment = Store.value getter (Atoms.Attachment.attachment attachmentId)

                        match timestamp, archived, attachment with
                        | Some timestamp, Some archived, Some attachment ->
                            Some
                                {
                                    Timestamp = timestamp
                                    Archived = archived
                                    Attachment = attachment
                                }
                        | _ -> None)
                )


        module Information =
            let rec attachmentIdMap =
                Store.readSelectorFamily (
                    $"{nameof Information}/{nameof attachmentIdMap}",
                    (fun (information: Information) getter ->
                        let selectedDatabaseIdArray =
                            Store.value getter Atoms.User.selectedDatabaseIdSet
                            |> Set.toArray

                        let informationAttachmentIdMapByArchiveArray =
                            selectedDatabaseIdArray
                            |> Array.map Selectors.Database.informationAttachmentIdMapByArchive
                            |> Store.waitForAll
                            |> Store.value getter

                        informationAttachmentIdMapByArchiveArray
                        |> Array.mapi
                            (fun i informationAttachmentIdMapByArchive ->
                                selectedDatabaseIdArray.[i],
                                informationAttachmentIdMapByArchive
                                |> Map.tryFind information
                                |> Option.defaultValue Set.empty)
                        |> Map.ofSeq)
                )


        module Task =
            open Rendering

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

            let rec cellStateMap =
                Store.readSelectorFamily (
                    $"{nameof Task}/{nameof cellStateMap}",
                    (fun (taskId: TaskId) getter ->
                        let statusMap = Store.value getter (Atoms.Task.statusMap taskId)
                        let cellAttachmentIdMap = Store.value getter (Atoms.Task.cellAttachmentIdMap taskId)

                        let sessions = Store.value getter (Atoms.Task.sessions taskId)
                        let dayStart = Store.value getter Atoms.User.dayStart

                        let sessionMap =
                            sessions
                            |> List.map (fun (Session start as session) -> dateId dayStart start, session)
                            |> List.groupBy fst
                            |> Map.ofList
                            |> Map.mapValues (List.map snd)

                        let cellStateAttachmentMap =
                            cellAttachmentIdMap
                            |> Map.mapValues
                                (fun attachmentIdSet ->
                                    let attachmentStateList =
                                        attachmentIdSet
                                        |> Set.toArray
                                        |> Array.map Attachment.attachmentState
                                        |> Store.waitForAll
                                        |> Store.value getter
                                        |> Array.toList
                                        |> List.choose id
                                        |> List.sortByDescending
                                            (fun attachmentState ->
                                                attachmentState.Timestamp
                                                |> FlukeDateTime.DateTime)

                                    {
                                        Status = Disabled
                                        SessionList = []
                                        AttachmentStateList = attachmentStateList
                                    })

                        let cellStateSessionMap =
                            sessionMap
                            |> Map.mapValues
                                (fun sessions ->
                                    {
                                        Status = Disabled
                                        SessionList = sessions
                                        AttachmentStateList = []
                                    })

                        let newStatusMap =
                            statusMap
                            |> Map.mapValues
                                (fun status ->
                                    {
                                        Status = UserStatus status
                                        SessionList = []
                                        AttachmentStateList = []
                                    })

                        newStatusMap
                        |> mergeCellStateMap cellStateSessionMap
                        |> mergeCellStateMap cellStateAttachmentMap
                        |> Map.filter
                            (fun _ cellState ->
                                match cellState with
                                | { Status = UserStatus _ } -> true
                                | { SessionList = _ :: _ } -> true
                                | { AttachmentStateList = _ :: _ } -> true
                                | _ -> false))
                )

            let rec filteredCellStateMap =
                Store.readSelectorFamily (
                    $"{nameof Task}/{nameof filteredCellStateMap}",
                    (fun (taskId: TaskId) getter ->
                        let dateIdArray = Store.value getter dateIdArray
                        let cellStateMap = Store.value getter (cellStateMap taskId)

                        dateIdArray
                        |> Array.map
                            (fun dateId ->
                                let cellState =
                                    cellStateMap
                                    |> Map.tryFind dateId
                                    |> Option.defaultValue
                                        {
                                            Status = Disabled
                                            SessionList = []
                                            AttachmentStateList = []
                                        }

                                dateId, cellState)
                        |> Map.ofSeq
                        |> Map.filter
                            (fun _ cellState ->
                                match cellState with
                                | { Status = UserStatus _ } -> true
                                | { SessionList = _ :: _ } -> true
                                | { AttachmentStateList = _ :: _ } -> true
                                | _ -> false))
                )

            let rec taskState =
                Store.readSelectorFamily (
                    $"{nameof Task}/{nameof taskState}",
                    (fun (taskId: TaskId) getter ->

                        let task = Store.value getter (task taskId)
                        let sessions = Store.value getter (Atoms.Task.sessions taskId)
                        let archived = Store.value getter (Atoms.Task.archived taskId)
                        let cellStateMap = Store.value getter (cellStateMap taskId)
                        let attachmentIdSet = Store.value getter (Atoms.Task.attachmentIdSet taskId)

                        let attachmentStateList =
                            attachmentIdSet
                            |> Set.toArray
                            |> Array.map Attachment.attachmentState
                            |> Store.waitForAll
                            |> Store.value getter
                            |> Array.toList
                            |> List.choose id
                            |> List.sortByDescending
                                (fun attachmentState ->
                                    attachmentState.Timestamp
                                    |> FlukeDateTime.DateTime)

                        {
                            Task = task
                            Archived = archived |> Option.defaultValue false
                            SessionList = sessions
                            AttachmentStateList = attachmentStateList
                            SortList = []
                            CellStateMap = cellStateMap
                        })
                )

            let rec cellStatusMap =
                Store.readSelectorFamily (
                    $"{nameof Task}/{nameof cellStatusMap}",
                    (fun (taskId: TaskId) getter ->
                        let taskState = Store.value getter (taskState taskId)
                        let dateIdArray = Store.value getter dateIdArray
                        let dayStart = Store.value getter Atoms.User.dayStart

                        match dateIdArray with
                        | [||] -> Map.empty
                        | _ ->
                            let dateSequence =
                                dateIdArray
                                |> Array.map DateId.Value
                                |> Array.toList

                            let firstDateRange, lastDateRange, taskStateDateSequence =
                                taskStateDateSequence dayStart dateSequence taskState

                            let position = Store.value getter Atoms.position

                            let rec loop renderState =
                                function
                                | moment :: tail ->
                                    //                                    let result =
//                                        Store.value
//                                            getter
//                                            (Cell.internalSessionStatus (taskId, dateId dayStart moment, renderState))

                                    let result =
                                        match position with
                                        | Some position ->
                                            Some (
                                                internalSessionStatus
                                                    dayStart
                                                    position
                                                    taskState
                                                    (dateId dayStart moment)
                                                    renderState
                                            )
                                        | None -> None

                                    match result with
                                    | Some (status, renderState) -> (moment, status) :: loop renderState tail
                                    | None -> (moment, Disabled) :: loop renderState tail
                                | [] -> []

                            loop WaitingFirstEvent taskStateDateSequence
                            |> List.filter (fun (moment, _) -> moment >==< (firstDateRange, lastDateRange))
                            |> List.map (fun (moment, cellStatus) -> dateId dayStart moment, cellStatus)
                            |> Map.ofSeq)
                )

            let rec isReadWrite =
                Store.readSelectorFamily (
                    $"{nameof Task}/{nameof isReadWrite}",
                    (fun (taskId: TaskId) getter ->
                        let databaseId = Store.value getter (Atoms.Task.databaseId taskId)
                        Store.value getter (Database.isReadWrite databaseId))
                )

            let rec lastSession =
                Store.readSelectorFamily (
                    $"{nameof Task}/{nameof lastSession}",
                    (fun (taskId: TaskId) getter ->
                        let dateIdArray = Store.value getter dateIdArray
                        let cellStateMap = Store.value getter (cellStateMap taskId)

                        dateIdArray
                        |> Seq.rev
                        |> Seq.tryPick
                            (fun dateId ->
                                cellStateMap
                                |> Map.tryFind dateId
                                |> Option.map (fun cellState -> cellState.SessionList)
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
                            let sessionDuration = Store.value getter Atoms.User.sessionDuration
                            let sessionBreakDuration = Store.value getter Atoms.User.sessionBreakDuration

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
                        let cellStateMap = Store.value getter (cellStateMap taskId)

                        let usersCount =
                            cellStateMap
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
                        let dateIdArray = Store.value getter dateIdArray
                        let selectionSet = Store.value getter (Atoms.Task.selectionSet taskId)
                        dateIdArray |> Array.exists selectionSet.Contains)
                )


        module Cell =
            open Rendering

            let rec internalSessionStatus =
                Store.readSelectorFamily (
                    $"{nameof Cell}/{nameof internalSessionStatus}",
                    (fun (taskId: TaskId, dateId: DateId, renderState: LaneCellRenderState) getter ->
                        let dayStart = Store.value getter Atoms.User.dayStart
                        let position = Store.value getter Atoms.position
                        let taskState = Store.value getter (Task.taskState taskId)

                        match position with
                        | Some position ->
                            let status, renderState =
                                Rendering.internalSessionStatus dayStart position taskState dateId renderState

                            Some (status, renderState)
                        | None -> None)
                )

            let rec sessionStatus =
                Store.selectorFamily (
                    $"{nameof Cell}/{nameof sessionStatus}",
                    (fun (taskId: TaskId, dateId: DateId) getter ->
                        let hideSchedulingOverlay = Store.value getter Atoms.User.hideSchedulingOverlay

                        if hideSchedulingOverlay then
                            Store.value getter (Atoms.Task.statusMap taskId)
                            |> Map.tryFind dateId
                            |> Option.map UserStatus
                            |> Option.defaultValue Disabled
                        else
                            Store.value getter (Task.cellStatusMap taskId)
                            |> Map.tryFind dateId
                            |> Option.defaultValue Disabled),
                    (fun (taskId: TaskId, dateId: DateId) _getter setter newValue ->
                        Store.change
                            setter
                            (Atoms.Task.statusMap taskId)
                            (fun statusMap ->
                                match newValue with
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
                        let sessions = Store.value getter (Atoms.Task.sessions taskId)
                        let dayStart = Store.value getter Atoms.User.dayStart

                        sessions
                        |> List.filter (fun (Session start) -> isToday dayStart start dateId))
                )

            let rec sessionCount =
                Store.readSelectorFamily (
                    $"{nameof Cell}/{nameof sessionCount}",
                    (fun (taskId: TaskId, dateId: DateId) getter ->
                        let sessions = Store.value getter (sessions (taskId, dateId))
                        sessions.Length)
                )

            let rec attachmentIdSet =
                Store.readSelectorFamily (
                    $"{nameof Cell}/{nameof attachmentIdSet}",
                    (fun (taskId: TaskId, dateId: DateId) getter ->
                        let cellAttachmentIdMap = Store.value getter (Atoms.Task.cellAttachmentIdMap taskId)

                        cellAttachmentIdMap
                        |> Map.tryFind dateId
                        |> Option.defaultValue Set.empty)
                )


        module Session =
            let rec devicePingList =
                Store.readSelector (
                    $"{nameof Session}/{nameof devicePingList}",
                    (fun getter ->
                        let deviceIdArray =
                            asyncDeviceIdAtoms
                            |> Store.value getter
                            |> Store.waitForAll
                            |> Store.value getter

                        let pingArray =
                            deviceIdArray
                            |> Array.map Atoms.Device.devicePing
                            |> Store.waitForAll
                            |> Store.value getter

                        deviceIdArray
                        |> Array.toList
                        |> List.mapi (fun i deviceId -> deviceId, pingArray.[i]))
                )

            let rec selectedTaskIdAtoms =
                Store.readSelector (
                    $"{nameof Session}/{nameof selectedTaskIdAtoms}",
                    (fun getter ->
                        let selectedDatabaseIdSet = Store.value getter Atoms.User.selectedDatabaseIdSet

                        selectedDatabaseIdSet
                        |> Set.toArray
                        |> Array.map Database.taskIdAtoms
                        |> Store.waitForAll
                        |> Store.value getter
                        |> Array.collect id)
                )

            let rec selectedTaskIdListByArchive =
                Store.readSelector (
                    $"{nameof Session}/{nameof selectedTaskIdListByArchive}",
                    (fun getter ->
                        let selectedDatabaseIdSet = Store.value getter Atoms.User.selectedDatabaseIdSet

                        selectedDatabaseIdSet
                        |> Set.toArray
                        |> Array.map Database.taskIdAtomsByArchive
                        |> Store.waitForAll
                        |> Store.value getter
                        |> Array.collect id
                        |> Store.waitForAll
                        |> Store.value getter
                        |> Array.toList)
                )

            let rec informationSet =
                Store.readSelector (
                    $"{nameof Session}/{nameof informationSet}",
                    (fun getter ->
                        let selectedDatabaseIdSet = Store.value getter Atoms.User.selectedDatabaseIdSet

                        let informationAttachmentIdMapArray =
                            selectedDatabaseIdSet
                            |> Set.toArray
                            |> Array.map Atoms.Database.informationAttachmentIdMap
                            |> Store.waitForAll
                            |> Store.value getter
                            |> Array.collect (
                                Map.filter (fun _ attachmentIdSet -> attachmentIdSet |> Set.isEmpty |> not)
                                >> Map.keys
                                >> Seq.toArray
                            )

                        let selectedTaskIdAtoms = Store.value getter selectedTaskIdAtoms

                        let taskInformationArray =
                            selectedTaskIdAtoms
                            |> Array.map (Store.value getter)
                            |> Array.map Atoms.Task.information
                            |> Store.waitForAll
                            |> Store.value getter

                        let informationArray =
                            taskInformationArray
                            |> Array.append informationAttachmentIdMapArray

                        let projectAreas =
                            informationArray
                            |> Array.choose
                                (fun information ->
                                    match information with
                                    | Project project -> Some (Area project.Area)
                                    | _ -> None)

                        informationArray
                        |> Array.append projectAreas
                        |> Array.append informationAttachmentIdMapArray
                        |> Array.filter
                            (fun information ->
                                information
                                |> Information.Name
                                |> InformationName.Value
                                |> String.IsNullOrWhiteSpace
                                |> not)
                        |> Set.ofSeq)
                )

            let rec activeSessions =
                Store.readSelector (
                    $"{nameof Session}/{nameof activeSessions}",
                    (fun getter ->
                        let selectedTaskIdArray =
                            Store.value getter selectedTaskIdListByArchive
                            |> List.toArray

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
                        |> List.indexed
                        |> List.sortBy snd
                        |> List.choose
                            (fun (i, duration) ->
                                duration
                                |> Option.map
                                    (fun duration ->
                                        TempUI.ActiveSession (TaskName.Value nameArray.[i], Minute duration))))
                )

            let rec filteredTaskIdSet =
                Store.readSelector (
                    $"{nameof Session}/{nameof filteredTaskIdSet}",
                    (fun getter ->
                        let filterTasksByView = Store.value getter Atoms.User.filterTasksByView
                        let filterTasksText = Store.value getter Atoms.User.filterTasksText
                        let view = Store.value getter Atoms.User.view
                        let dateIdArray = Store.value getter dateIdArray

                        let selectedTaskIdArray =
                            Store.value getter selectedTaskIdListByArchive
                            |> List.toArray

                        let selectedTaskList =
                            selectedTaskIdArray
                            |> Array.map Task.task
                            |> Store.waitForAll
                            |> Store.value getter
                            |> Array.toList

                        let selectedTaskListSearch =
                            match filterTasksText with
                            | "" -> selectedTaskList
                            | _ ->
                                selectedTaskList
                                |> List.filter
                                    (fun task ->
                                        let check (text: string) = text.IndexOf filterTasksText >= 0

                                        (task.Name |> TaskName.Value |> check)
                                        || (task.Information
                                            |> Information.Name
                                            |> InformationName.Value
                                            |> check))

                        let filteredTaskList =
                            if filterTasksByView then
                                let dateSequence =
                                    dateIdArray
                                    |> Array.map DateId.Value
                                    |> Array.toList

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
                                $"filteredTaskList.Length={filteredTaskList.Length} taskListSearch.Length={selectedTaskListSearch.Length}")

                        filteredTaskList
                        |> List.map (fun task -> task.Id)
                        |> Set.ofSeq)
                )

            let rec filteredTaskIdCount =
                Store.readSelector (
                    $"{nameof Session}/{nameof filteredTaskIdCount}",
                    (fun getter ->
                        let filteredTaskIdSet = Store.value getter filteredTaskIdSet
                        filteredTaskIdSet.Count)
                )

            let rec sortedTaskIdArray =
                Store.readSelector (
                    $"{nameof Session}/{nameof sortedTaskIdArray}",
                    (fun getter ->
                        let position = Store.value getter Atoms.position

                        match position with
                        | Some position ->
                            let filteredTaskIdSet = Store.value getter filteredTaskIdSet

                            JS.log (fun () -> $"sortedTaskIdArray. filteredTaskIdSet.Count={filteredTaskIdSet.Count}")

                            let filteredTaskIdArray = filteredTaskIdSet |> Set.toArray

                            let statusMapArray =
                                filteredTaskIdArray
                                |> Array.map Task.cellStatusMap
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

                            let view = Store.value getter Atoms.User.view
                            let dayStart = Store.value getter Atoms.User.dayStart
                            let informationSet = Store.value getter Session.informationSet

                            let result =
                                sortLanes
                                    {|
                                        View = view
                                        DayStart = dayStart
                                        Position = position
                                        InformationSet = informationSet
                                        Lanes = lanes
                                    |}

                            JS.log (fun () -> $"sortedTaskIdArray. result.Length={result.Length}")

                            result
                            |> List.map (fun (taskState, _) -> taskState.Task.Id)
                            |> List.toArray
                        | _ -> [||])
                )

            let rec sortedTaskIdAtoms =
                Store.readSelector (
                    $"{nameof Session}/{nameof sortedTaskIdAtoms}",
                    (fun getter ->
                        sortedTaskIdArray
                        |> Jotai.jotaiUtils.splitAtom
                        |> Store.value getter)
                )

            let rec sortedTaskIdCount =
                Store.readSelector (
                    $"{nameof Session}/{nameof sortedTaskIdCount}",
                    (fun getter ->
                        let sortedTaskIdAtoms = Store.value getter sortedTaskIdAtoms
                        sortedTaskIdAtoms.Length)
                )

            let rec informationTaskIdArray =
                Store.readSelector (
                    $"{nameof Session}/{nameof informationTaskIdArray}",
                    (fun getter ->
                        let sortedTaskIdAtoms = Store.value getter sortedTaskIdAtoms

                        let informationSet = Store.value getter informationSet

                        let taskInformationArray =
                            sortedTaskIdAtoms
                            |> Store.waitForAll
                            |> Store.value getter
                            |> Array.map Atoms.Task.information
                            |> Store.waitForAll
                            |> Store.value getter

                        let taskMap =
                            sortedTaskIdAtoms
                            |> Array.mapi (fun i taskIdAtom -> taskInformationArray.[i], taskIdAtom)
                            |> Array.groupBy fst
                            |> Array.map (fun (information, taskIdAtoms) -> information, taskIdAtoms |> Array.map snd)
                            |> Map.ofSeq

                        informationSet
                        |> Set.toArray
                        |> Array.map
                            (fun information ->
                                let taskIdAtoms =
                                    taskMap
                                    |> Map.tryFind information
                                    |> Option.defaultValue [||]

                                information, taskIdAtoms)
                        |> Array.sortBy (fst >> Information.Name)
                        |> Array.sortBy (
                            fst
                            >> Option.ofObjUnbox
                            >> Option.map Information.toTag
                        ))
                )

            let rec informationTaskIdAtoms =
                Store.readSelector (
                    $"{nameof Session}/{nameof informationTaskIdAtoms}",
                    (fun getter ->
                        informationTaskIdArray
                        |> Jotai.jotaiUtils.splitAtom
                        |> Store.value getter)
                )

            let rec informationTaskIdArrayByKind =
                Store.readSelector (
                    $"{nameof Session}/{nameof informationTaskIdArrayByKind}",
                    (fun getter ->
                        let informationTaskIdAtoms = Store.value getter Selectors.Session.informationTaskIdAtoms
                        //                         let informationTaskIdArray = Store.value getter Selectors.Session.informationTaskIdArray
                        let informationTaskIdArray =
                            informationTaskIdAtoms
                            |> Store.waitForAll
                            |> Store.value getter

                        informationTaskIdArray
                        |> Array.indexed
                        |> Array.groupBy (fun (_, (information, _)) -> Information.toString information)
                        |> Array.map
                            (fun (informationKindName, groups) ->
                                informationKindName,
                                groups
                                |> Array.map (fun (i, _) -> informationTaskIdAtoms.[i])))
                )

            let rec informationTaskIdAtomsByKind =
                Store.readSelector (
                    $"{nameof Session}/{nameof informationTaskIdAtomsByKind}",
                    (fun getter ->
                        informationTaskIdArrayByKind
                        |> Jotai.jotaiUtils.splitAtom
                        |> Store.value getter)
                )

            let rec selectionSetMap =
                Store.readSelector (
                    $"{nameof Session}/{nameof selectionSetMap}",
                    (fun getter ->
                        let sortedTaskIdArray = Store.value getter sortedTaskIdArray

                        sortedTaskIdArray
                        |> Array.map Atoms.Task.selectionSet
                        |> Store.waitForAll
                        |> Store.value getter
                        |> Array.mapi (fun i dates -> sortedTaskIdArray.[i], dates)
                        |> Map.ofArray)
                )

            let rec cellSelectionMap =
                Store.readSelector (
                    $"{nameof Session}/{nameof cellSelectionMap}",
                    (fun getter ->
                        let selectionSetMap = Store.value getter selectionSetMap
                        let dateIdArray = Store.value getter dateIdArray

                        selectionSetMap
                        |> Map.keys
                        |> Seq.map
                            (fun taskId ->
                                let dates =
                                    dateIdArray
                                    |> Array.map (fun dateId -> dateId, selectionSetMap.[taskId].Contains dateId)
                                    |> Array.filter snd
                                    |> Array.map fst
                                    |> Set.ofSeq

                                taskId, dates)
                        |> Seq.filter (fun (_, dates) -> Set.isEmpty dates |> not)
                        |> Map.ofSeq)
                )


        module DateId =
            let isToday =
                Store.readSelectorFamily (
                    $"{nameof FlukeDate}/{nameof isToday}",
                    (fun (dateId: DateId) getter ->
                        let position = Store.value getter Atoms.position

                        match position with
                        | Some position ->
                            let dayStart = Store.value getter Atoms.User.dayStart

                            Domain.UserInteraction.isToday dayStart position dateId
                        | _ -> false)
                )

            let rec hasCellSelection =
                Store.readSelectorFamily (
                    $"{nameof FlukeDate}/{nameof hasCellSelection}",
                    (fun (dateId: DateId) getter ->
                        let cellSelectionMap = Store.value getter Session.cellSelectionMap

                        cellSelectionMap
                        |> Map.values
                        |> Seq.exists (Set.contains dateId))
                )


        module BulletJournalView =
            let rec weekCellsMap =
                Store.readSelector (
                    $"{nameof BulletJournalView}/{nameof weekCellsMap}",
                    (fun getter ->
                        let position = Store.value getter Atoms.position
                        let sortedTaskIdAtoms = Store.value getter Session.sortedTaskIdAtoms

                        let sortedTaskIdArray =
                            sortedTaskIdAtoms
                            |> Store.waitForAll
                            |> Store.value getter

                        let taskStateArray =
                            sortedTaskIdArray
                            |> Array.map Task.taskState
                            |> Store.waitForAll
                            |> Store.value getter

                        let taskStateMap =
                            sortedTaskIdArray
                            |> Array.mapi (fun i taskId -> taskId, taskStateArray.[i])
                            |> Map.ofSeq

                        let taskIdAtomMap =
                            sortedTaskIdArray
                            |> Array.mapi (fun i taskId -> taskId, sortedTaskIdAtoms.[i])
                            |> Map.ofSeq

                        match position with
                        | Some position ->
                            let dayStart = Store.value getter Atoms.User.dayStart
                            let weekStart = Store.value getter Atoms.User.weekStart

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
                                            sortedTaskIdArray
                                            |> Array.collect
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
                                                                        AttachmentStateList = []
                                                                        SessionList = []
                                                                    }

                                                            {|
                                                                DateId = dateId
                                                                TaskId = taskId
                                                                DateIdAtom = Jotai.jotai.atom dateId
                                                                TaskIdAtom = taskIdAtomMap.[taskId]
                                                                Status = cellState.Status
                                                                SessionList = cellState.SessionList
                                                                IsToday = isToday
                                                                AttachmentStateList = cellState.AttachmentStateList
                                                            |})
                                                    |> List.toArray)
                                            |> Array.groupBy (fun x -> x.DateId)
                                            |> Array.map
                                                (fun (dateId, cellsMetadata) ->
                                                    match dateId with
                                                    | DateId referenceDay as dateId ->
                                                        //                |> Sorting.sortLanesByTimeOfDay input.DayStart input.Position input.TaskOrderList
                                                        let taskSessionList =
                                                            cellsMetadata
                                                            |> Array.toList
                                                            |> List.collect (fun x -> x.SessionList)

                                                        let sortedTasksMap =
                                                            cellsMetadata
                                                            |> Array.map
                                                                (fun cellMetadata ->
                                                                    let taskState =
                                                                        { taskStateMap.[cellMetadata.TaskId] with
                                                                            SessionList = taskSessionList
                                                                        }

                                                                    taskState,
                                                                    [
                                                                        dateId, cellMetadata.Status
                                                                    ]
                                                                    |> Map.ofSeq)
                                                            |> Array.toList
                                                            |> Sorting.sortLanesByTimeOfDay
                                                                dayStart
                                                                (FlukeDateTime.Create (referenceDay, dayStart, Second 0))
                                                            |> List.indexed
                                                            |> List.map
                                                                (fun (i, (taskState, _)) -> taskState.Task.Id, i)
                                                            |> Map.ofSeq

                                                        let newCells =
                                                            cellsMetadata
                                                            |> Array.sortBy
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
