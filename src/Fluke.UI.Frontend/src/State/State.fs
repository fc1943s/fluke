namespace Fluke.UI.Frontend.State

open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.View
open System
open Fluke.Shared
open Fluke.Shared.Domain
open Fluke.UI.Frontend
open FsCore.BaseModel
open FsCore


module Fluke =
    let root = StoreRoot (nameof Fluke)


module State =
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

    [<RequireQualifiedAccess>]
    type AttachmentParent =
        | None
        | Information of databaseId: DatabaseId * Information: Information
        | Task of taskId: TaskId
        | Cell of taskId: TaskId * dateId: DateId

    let uiFlagDefault = UIFlag.None
    let uiVisibleFlagDefault = false
    let accordionHiddenFlagDefault: string [] = [||]

    type Filter =
        {
            Moment: FlukeDateTime option
            Filter: string
            Information: {| Database: Database option
                            Information: Information |}
            Scheduling: Scheduling option
            Task: Task
            CellStatus: CellStatus
        }

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
            DaysAfter: int
            DaysBefore: int
            DayStart: FlukeTime
            EnableCellPopover: bool
            ExpandedDatabaseIdSet: Set<DatabaseId>
            Filter: Filter
            HideSchedulingOverlay: bool
            HideTemplates: bool option
            Language: Language
            LastDatabaseSelected: DatabaseId option
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
                    Reflection.unionCases<AccordionType>
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
                DaysAfter = 7
                DaysBefore = 7
                DayStart = FlukeTime.Create 0 0
                EnableCellPopover = true
                ExpandedDatabaseIdSet = Set.empty
                Filter =
                    {
                        Moment = None
                        Filter = ""
                        Information =
                            {|
                                Database = None
                                Information = Task.Default.Information
                            |}
                        Scheduling = None
                        Task = Task.Default
                        CellStatus = Disabled
                    }
                HideSchedulingOverlay = false
                HideTemplates = None
                Language = Language.English
                LastDatabaseSelected = None
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
                UIFlagMap =
                    Reflection.unionCases<UIFlagType>
                    |> List.map (fun uiFlagType -> uiFlagType, uiFlagDefault)
                    |> Map.ofList
                UIVisibleFlagMap =
                    Reflection.unionCases<UIFlagType>
                    |> List.map (fun uiFlagType -> uiFlagType, uiVisibleFlagDefault)
                    |> Map.ofList
                UserColor = None
                View = View.View.Information
                WeekStart = DayOfWeek.Sunday
            }
