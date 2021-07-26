namespace Fluke.UI.Frontend.State.Selectors

#nowarn "40"

open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.State.State
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.UserInteraction


module rec User =
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
                    Filter = Store.value getter Atoms.User.filter
                    FontSize = Store.value getter Atoms.User.fontSize
                    HideSchedulingOverlay = Store.value getter Atoms.User.hideSchedulingOverlay
                    HideTemplates = Store.value getter Atoms.User.hideTemplates
                    Language = Store.value getter Atoms.User.language
                    LastDatabaseSelected = Store.value getter Atoms.User.lastDatabaseSelected
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
                    RandomizeProjectTaskAttachment = Store.value getter Atoms.User.randomizeProjectTaskAttachment
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
                        |> List.map (fun uiFlagType -> uiFlagType, Store.value getter (Atoms.User.uiFlag uiFlagType))
                        |> Map.ofList
                    UIVisibleFlagMap =
                        Union.ToList<UIFlagType>
                        |> List.map
                            (fun uiFlagType -> uiFlagType, Store.value getter (Atoms.User.uiVisibleFlag uiFlagType))
                        |> Map.ofList
                    UserColor = Store.value getter Atoms.User.userColor
                    View = Store.value getter Atoms.User.view
                    WeekStart = Store.value getter Atoms.User.weekStart
                })
        )
