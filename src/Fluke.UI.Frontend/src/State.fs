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


    type TextKey = TextKey of key: string

    and TextKey with
        static member inline Value (TextKey key) = key

    [<RequireQualifiedAccess>]
    type Join =
        | Database of DatabaseId
        | Task of DatabaseId * TaskId


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
    let accordionFlagDefault: string [] = [||]

    type UserState =
        {
            Archive: bool option
            AccordionFlagMap: Map<AccordionType, string []>
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
            ClipboardAttachmentMap: Map<AttachmentId, bool>
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
                AccordionFlagMap =
                    Union.ToList<AccordionType>
                    |> List.map (fun accordionType -> accordionType, accordionFlagDefault)
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
                ClipboardAttachmentMap = Map.empty
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

        let rec debug = Store.atomWithStorageSync ($"{nameof debug}", JS.isDebug (), id)

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

            let rec clipboardAttachmentMap =
                Store.atomWithSync (
                    $"{nameof User}/{nameof clipboardAttachmentMap}",
                    UserState.Default.ClipboardAttachmentMap,
                    []
                )

            let rec clipboardVisible =
                Store.atomWithSync ($"{nameof User}/{nameof clipboardVisible}", UserState.Default.ClipboardVisible, [])

            let rec darkMode =
                Store.atomWithStorageSync ($"{nameof User}/{nameof darkMode}", UserState.Default.DarkMode, id)

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
                Store.atomWithStorageSync ($"{nameof User}/{nameof fontSize}", UserState.Default.FontSize, id)

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
                Store.atomWithStorageSync ($"{nameof User}/{nameof systemUiFont}", UserState.Default.SystemUiFont, id)

            let rec templatesDeleted = Store.atomWithSync ($"{nameof User}/{nameof templatesDeleted}", false, [])

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

            let rec accordionFlag =
                Store.atomFamilyWithSync (
                    $"{nameof User}/{nameof accordionFlag}",
                    (fun (_accordionType: AccordionType) -> accordionFlagDefault),
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

            //            let rec taskIdSet =
//                Store.atomFamilyWithSync (
//                    $"{nameof Database}/{nameof taskIdSet}",
//                    (fun (_databaseId: DatabaseId) -> Set.empty: Set<TaskId>),
//                    databaseIdIdentifier
//                )

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

            let rec informationAttachmentMap =
                Store.atomFamilyWithSync (
                    $"{nameof Database}/{nameof informationAttachmentMap}",
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

            let rec archived =
                Store.atomFamilyWithSync (
                    $"{nameof Task}/{nameof archived}",
                    (fun (_taskId: TaskId) -> None: bool option),
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


    module rec Selectors =
        let rec dateSequence =
            Store.readSelector (
                $"{nameof dateSequence}",
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
                    | _ -> [])
            )

        //        let rec taskIdMap =
//            Store.readSelector (
//                $"{nameof taskIdMap}",
//                (fun getter ->
//                    let databaseIdArray =
//                        Store.value getter Atoms.databaseIdSet
//                        |> Set.toArray
//
//                    databaseIdArray
//                    |> Array.map Atoms.Database.taskIdSet
//                    |> Store.waitForAll
//                    |> Store.value getter
//                    |> Array.mapi (fun i taskIdSet -> databaseIdArray.[i], taskIdSet)
//                    |> Map.ofArray)
//            )

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


        module User =
            let rec userState =
                Store.readSelector (
                    $"{nameof User}/{nameof userState}",
                    (fun getter ->
                        {
                            Archive = Store.value getter Atoms.User.archive
                            AccordionFlagMap =
                                Union.ToList<AccordionType>
                                |> List.map
                                    (fun accordionType ->
                                        accordionType, Store.value getter (Atoms.User.accordionFlag accordionType))
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
                            ClipboardAttachmentMap = Store.value getter Atoms.User.clipboardAttachmentMap
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

            let rec taskIdAtoms =
                Store.readSelectorFamily (
                    $"{nameof Database}/{nameof taskIdAtoms}",
                    (fun databaseId getter ->
                        let archive = Store.value getter Atoms.User.archive

                        match archive with
                        | Some archive ->
                            asyncTaskIdAtoms
                            |> Store.value getter
                            |> Array.filter
                                (fun taskIdAtom ->
                                    let taskId = Store.value getter taskIdAtom
                                    let databaseId' = Store.value getter (Atoms.Task.databaseId taskId)
                                    let archived = Store.value getter (Atoms.Task.archived taskId)

                                    match archived with
                                    | Some archived -> databaseId = databaseId' && archived = archive
                                    | None -> false)
                        | None -> [||])
                )

            let rec databaseState =
                Store.readSelectorFamily (
                    $"{nameof Database}/{nameof databaseState}",
                    (fun databaseId getter ->
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
                                    taskState.Attachments
                                    |> List.choose
                                        (fun (_, attachment) ->
                                            match attachment with
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

                            let informationAttachmentMap =
                                Store.value getter (Atoms.Database.informationAttachmentMap databaseId)

                            let informationStateMap =
                                informationAttachmentMap
                                |> Map.map
                                    (fun information attachmentIdSet ->
                                        let attachments =
                                            attachmentIdSet
                                            |> Set.toArray
                                            |> Array.map Selectors.Attachment.attachment
                                            |> Store.waitForAll
                                            |> Store.value getter
                                            |> Array.toList
                                            |> List.choose id

                                        {
                                            Information = information
                                            Attachments = attachments
                                            SortList = []
                                        })
                                |> Map.filter
                                    (fun _ informationState ->
                                        not informationState.Attachments.IsEmpty
                                        || not informationState.SortList.IsEmpty)

                            let taskStateMap =
                                taskStateList
                                |> List.map (fun taskState -> taskState.Task.Id, taskState)
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


        module Information =
            let rec attachmentIdMap =
                Store.readSelectorFamily (
                    $"{nameof Information}/{nameof attachmentIdMap}",
                    (fun (information: Information) getter ->
                        let selectedDatabaseIdArray =
                            Store.value getter Atoms.User.selectedDatabaseIdSet
                            |> Set.toArray

                        let informationAttachmentMapArray =
                            selectedDatabaseIdArray
                            |> Array.map Atoms.Database.informationAttachmentMap
                            |> Store.waitForAll
                            |> Store.value getter

                        informationAttachmentMapArray
                        |> Array.mapi
                            (fun i informationAttachmentMap ->
                                selectedDatabaseIdArray.[i],
                                informationAttachmentMap
                                |> Map.tryFind information
                                |> Option.defaultValue Set.empty)
                        |> Map.ofSeq)
                )


        module Task =
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
                        let cellAttachmentMap = Store.value getter (Atoms.Task.cellAttachmentMap taskId)

                        let sessions = Store.value getter (Atoms.Task.sessions taskId)
                        let dayStart = Store.value getter Atoms.User.dayStart

                        let sessionMap =
                            sessions
                            |> List.map (fun (Session start as session) -> dateId dayStart start, session)
                            |> List.groupBy fst
                            |> Map.ofList
                            |> Map.mapValues (List.map snd)

                        let newAttachmentMap =
                            cellAttachmentMap
                            |> Map.mapValues
                                (fun attachmentIdSet ->
                                    let attachments =
                                        attachmentIdSet
                                        |> Set.toArray
                                        |> Array.map Attachment.attachment
                                        |> Store.waitForAll
                                        |> Store.value getter
                                        |> Array.toList
                                        |> List.choose id
                                        |> List.sortByDescending (fst >> FlukeDateTime.DateTime)

                                    {
                                        Status = Disabled
                                        Sessions = []
                                        Attachments = attachments
                                    })

                        let newSessionMap =
                            sessionMap
                            |> Map.mapValues
                                (fun sessions ->
                                    {
                                        Status = Disabled
                                        Sessions = sessions
                                        Attachments = []
                                    })

                        let newStatusMap =
                            statusMap
                            |> Map.mapValues
                                (fun status ->
                                    {
                                        Status = UserStatus status
                                        Sessions = []
                                        Attachments = []
                                    })

                        newStatusMap
                        |> mergeCellStateMap newSessionMap
                        |> mergeCellStateMap newAttachmentMap
                        |> Map.filter
                            (fun _ cellState ->
                                match cellState with
                                | { Status = UserStatus _ } -> true
                                | { Sessions = _ :: _ } -> true
                                | { Attachments = _ :: _ } -> true
                                | _ -> false))
                )

            let rec filteredCellStateMap =
                Store.readSelectorFamily (
                    $"{nameof Task}/{nameof filteredCellStateMap}",
                    (fun (taskId: TaskId) getter ->
                        let dateSequence = Store.value getter dateSequence
                        let cellStateMap = Store.value getter (cellStateMap taskId)

                        dateSequence
                        |> List.map DateId
                        |> List.map
                            (fun dateId ->
                                let cellState =
                                    cellStateMap
                                    |> Map.tryFind dateId
                                    |> Option.defaultValue
                                        {
                                            Status = Disabled
                                            Sessions = []
                                            Attachments = []
                                        }

                                dateId, cellState)
                        |> Map.ofSeq
                        |> Map.filter
                            (fun _ cellState ->
                                match cellState with
                                | { Status = UserStatus _ } -> true
                                | { Sessions = _ :: _ } -> true
                                | { Attachments = _ :: _ } -> true
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

                        let attachments =
                            attachmentIdSet
                            |> Set.toArray
                            |> Array.map Attachment.attachment
                            |> Store.waitForAll
                            |> Store.value getter
                            |> Array.toList
                            |> List.choose id
                            |> List.sortByDescending (fst >> FlukeDateTime.DateTime)

                        {
                            Task = task
                            Archived = archived |> Option.defaultValue false
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
                        let dayStart = Store.value getter Atoms.User.dayStart

                        match position with
                        | Some position when not dateSequence.IsEmpty ->
                            Rendering.renderTaskStatusMap dayStart position dateSequence taskState
                        | _ -> Map.empty)
                )

            //            let rec databaseId =
//                Store.selectAtomFamily (
//                    $"{nameof Task}/{nameof databaseId}",
//                    taskIdMap,
//                    (fun (taskId: TaskId) taskIdMap ->
//                        let databaseIdList =
//                            taskIdMap
//                            |> Map.filter (fun _ taskIdSet -> taskIdSet.Contains taskId)
//                            |> Map.keys
//                            |> Seq.toList
//
//                        match databaseIdList with
//                        | [] -> Database.Default.Id
//                        | [ databaseId ] -> databaseId
//                        | _ -> failwith $"Error: task {taskId} exists in two databases ({databaseIdList})")
//                )

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
                        let dateSequence = Store.value getter dateSequence
                        let cellStateMap = Store.value getter (cellStateMap taskId)

                        dateSequence
                        |> List.rev
                        |> List.tryPick
                            (fun date ->
                                cellStateMap
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
                        let dateSequence = Store.value getter dateSequence
                        let selectionSet = Store.value getter (Atoms.Task.selectionSet taskId)

                        dateSequence
                        |> List.exists (DateId >> selectionSet.Contains))
                )


        module Cell =
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
                        let sessions = Store.value getter (Atoms.Task.sessions taskId)
                        let dayStart = Store.value getter Atoms.User.dayStart

                        sessions
                        |> List.filter (fun (Session start) -> isToday dayStart start dateId))
                )

            let rec attachmentIdSet =
                Store.readSelectorFamily (
                    $"{nameof Cell}/{nameof attachmentIdSet}",
                    (fun (taskId: TaskId, dateId: DateId) getter ->
                        let cellAttachmentMap = Store.value getter (Atoms.Task.cellAttachmentMap taskId)

                        cellAttachmentMap
                        |> Map.tryFind dateId
                        |> Option.defaultValue Set.empty)
                )

        //            let rec attachments =
//                Store.readSelectorFamily (
//                    $"{nameof Cell}/{nameof attachments}",
//                    (fun (taskId: TaskId, dateId: DateId) getter ->
//                        let attachmentIdSet = Store.value getter (attachmentIdSet (taskId, dateId))
//
//                        attachmentIdSet
//                        |> Map.tryFind dateId
//                        |> Option.map (fun x -> x.Attachments)
//                        |> Option.defaultValue [])
//                )


        module Session =
            //            let rec taskIdSet =
//                Store.readSelector (
//                    $"{nameof Session}/{nameof taskIdSet}",
//                    (fun getter ->
//                        let taskIdMap = Store.value getter taskIdMap
//
//                        if taskIdMap.IsEmpty then
//                            Set.empty
//                        else
//                            taskIdMap |> Map.values |> Seq.reduce Set.union)
//                )

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

            let rec selectedTaskIdList =
                Store.readSelector (
                    $"{nameof Session}/{nameof selectedTaskIdList}",
                    (fun getter ->
                        let selectedTaskIdAtoms = Store.value getter selectedTaskIdAtoms

                        selectedTaskIdAtoms
                        |> Store.waitForAll
                        |> Store.value getter
                        |> Array.toList)
                )

            //            let rec selectedTaskStateList =
//                Store.readSelector (
//                    $"{nameof Session}/{nameof selectedTaskStateList}",
//                    (fun getter ->
//                        let selectedTaskIdAtoms = Store.value getter selectedTaskIdAtoms
//
//                        selectedTaskIdAtoms
//                        |> Array.map (Store.value getter)
////                        |> Array.map Task.taskState
//                        |> Store.waitForAll
//                        |> Store.value getter
//                        |> Array.toList)
//                )

            let rec informationSet =
                Store.readSelector (
                    $"{nameof Session}/{nameof informationSet}",
                    (fun getter ->
                        let selectedDatabaseIdSet = Store.value getter Atoms.User.selectedDatabaseIdSet

                        let informationAttachmentMapArray =
                            selectedDatabaseIdSet
                            |> Set.toArray
                            |> Array.map Atoms.Database.informationAttachmentMap
                            |> Store.waitForAll
                            |> Store.value getter
                            |> Array.collect (
                                Map.filter (fun _ attachments -> attachments |> Set.isEmpty |> not)
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
                            |> Array.append informationAttachmentMapArray

                        let projectAreas =
                            informationArray
                            |> Array.choose
                                (fun information ->
                                    match information with
                                    | Project project -> Some (Area project.Area)
                                    | _ -> None)

                        informationArray
                        |> Array.append projectAreas
                        |> Array.append informationAttachmentMapArray
                        |> Array.filter
                            (fun information ->
                                information
                                |> Information.Name
                                |> InformationName.Value
                                |> String.IsNullOrWhiteSpace
                                |> not)
                        |> Set.ofSeq)
                )

            //            let rec informationStateList =
//                Store.readSelector (
//                    $"{nameof Session}/{nameof informationStateList}",
//                    (fun getter ->
//                        let informationSet = Store.value getter informationSet
//
//                        informationSet
//                        |> Set.toArray
//                        |> Array.map Information.informationState
//                        |> Store.waitForAll
//                        |> Store.value getter
//                        |> Array.toList)
//                )

            let rec activeSessions =
                Store.readSelector (
                    $"{nameof Session}/{nameof activeSessions}",
                    (fun getter ->
                        let selectedTaskIdAtoms = Store.value getter selectedTaskIdAtoms

                        let selectedTaskIdArray =
                            selectedTaskIdAtoms
                            |> Array.map (Store.value getter)

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
                        let filterTasksByView = Store.value getter Atoms.User.filterTasksByView
                        let filterTasksText = Store.value getter Atoms.User.filterTasksText
                        let view = Store.value getter Atoms.User.view
                        let dateSequence = Store.value getter dateSequence

                        let selectedTaskIdAtoms = Store.value getter selectedTaskIdAtoms

                        let selectedTaskIdArray =
                            selectedTaskIdAtoms
                            |> Array.map (Store.value getter)

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

                        let informationSet = Store.value getter informationSet

                        let taskInformationArray =
                            sortedTaskIdList
                            |> List.toArray
                            |> Array.map Atoms.Task.information
                            |> Store.waitForAll
                            |> Store.value getter

                        let taskMap =
                            sortedTaskIdList
                            |> List.mapi (fun i taskId -> taskInformationArray.[i], taskId)
                            |> List.groupBy fst
                            |> List.map (fun (information, tasks) -> information, tasks |> List.map snd)
                            |> Map.ofList

                        informationSet
                        |> Set.toList
                        |> List.map
                            (fun information ->
                                let tasks =
                                    taskMap
                                    |> Map.tryFind information
                                    |> Option.defaultValue []

                                information, tasks)
                        |> List.sortBy (fun (information, _) -> information |> Information.Name)
                        |> List.groupBy (fun (information, _) -> Information.toString information)
                        |> List.sortBy (
                            snd
                            >> List.tryHead
                            >> Option.map (
                                fst
                                >> fun information ->
                                    if informationSet.Contains information then
                                        information |> Information.toTag
                                    else
                                        -1
                            )
                            >> Option.defaultValue -1
                        ))
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


        module FlukeDate =
            let isToday =
                Store.readSelectorFamily (
                    $"{nameof FlukeDate}/{nameof isToday}",
                    (fun (date: FlukeDate) getter ->
                        let position = Store.value getter Atoms.position

                        match position with
                        | Some position ->
                            let dayStart = Store.value getter Atoms.User.dayStart

                            Domain.UserInteraction.isToday dayStart position (DateId date)
                        | _ -> false)
                )

            let rec hasCellSelection =
                Store.readSelectorFamily (
                    $"{nameof FlukeDate}/{nameof hasCellSelection}",
                    (fun (date: FlukeDate) getter ->
                        let cellSelectionMap = Store.value getter Session.cellSelectionMap

                        cellSelectionMap
                        |> Map.values
                        |> Seq.exists (Set.contains date))
                )


        module BulletJournalView =
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
                                                                        { taskStateMap.[cellMetadata.TaskId] with
                                                                            Sessions = taskSessions
                                                                        }

                                                                    taskState,
                                                                    [
                                                                        dateId, cellMetadata.Status
                                                                    ]
                                                                    |> Map.ofSeq)
                                                            |> Sorting.sortLanesByTimeOfDay
                                                                dayStart
                                                                (FlukeDateTime.Create (referenceDay, dayStart, Second 0))
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
