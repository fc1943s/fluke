namespace Fluke.UI.Frontend.State.Atoms

open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.State.State
open FsCore.BaseModel
open FsStore



module rec User =
    let collection = Collection (nameof User)

    let inline atomWithSync name defaultValueFn =
        Store.atomWithSync
            {
                StoreRoot = Fluke.root
                Collection = Some collection
                Keys = []
                Name = name
            }
            defaultValueFn

    let rec archive = atomWithSync (nameof archive) UserState.Default.Archive
    let rec cellColorDisabled = atomWithSync (nameof cellColorDisabled) UserState.Default.CellColorDisabled
    let rec cellColorSuggested = atomWithSync (nameof cellColorSuggested) UserState.Default.CellColorSuggested
    let rec cellColorPending = atomWithSync (nameof cellColorPending) UserState.Default.CellColorPending
    let rec cellColorMissed = atomWithSync (nameof cellColorMissed) UserState.Default.CellColorMissed
    let rec cellColorMissedToday = atomWithSync (nameof cellColorMissedToday) UserState.Default.CellColorMissedToday

    let rec cellColorPostponedUntil =
        atomWithSync (nameof cellColorPostponedUntil) UserState.Default.CellColorPostponedUntil

    let rec cellColorPostponed = atomWithSync (nameof cellColorPostponed) UserState.Default.CellColorPostponed
    let rec cellColorCompleted = atomWithSync (nameof cellColorCompleted) UserState.Default.CellColorCompleted
    let rec cellColorDismissed = atomWithSync (nameof cellColorDismissed) UserState.Default.CellColorDismissed
    let rec cellColorScheduled = atomWithSync (nameof cellColorScheduled) UserState.Default.CellColorScheduled
    let rec cellSize = atomWithSync (nameof cellSize) UserState.Default.CellSize

    let rec clipboardAttachmentIdMap =
        atomWithSync (nameof clipboardAttachmentIdMap) UserState.Default.ClipboardAttachmentIdMap

    let rec clipboardVisible = atomWithSync (nameof clipboardVisible) UserState.Default.ClipboardVisible
    let rec daysAfter = atomWithSync (nameof daysAfter) UserState.Default.DaysAfter
    let rec daysBefore = atomWithSync (nameof daysBefore) UserState.Default.DaysBefore
    let rec dayStart = atomWithSync (nameof dayStart) UserState.Default.DayStart
    let rec enableCellPopover = atomWithSync (nameof enableCellPopover) UserState.Default.EnableCellPopover
    let rec expandedDatabaseIdSet = atomWithSync (nameof expandedDatabaseIdSet) UserState.Default.ExpandedDatabaseIdSet
    let rec filter = atomWithSync (nameof filter) UserState.Default.Filter
    let rec hideSchedulingOverlay = atomWithSync (nameof hideSchedulingOverlay) UserState.Default.HideSchedulingOverlay
    let rec hideTemplates = atomWithSync (nameof hideTemplates) UserState.Default.HideTemplates
    let rec language = atomWithSync (nameof language) UserState.Default.Language
    let rec lastDatabaseSelected = atomWithSync (nameof lastDatabaseSelected) UserState.Default.LastDatabaseSelected
    let rec leftDock = atomWithSync (nameof leftDock) UserState.Default.LeftDock
    let rec leftDockSize = atomWithSync (nameof leftDockSize) UserState.Default.LeftDockSize
    let rec randomizeProject = atomWithSync (nameof randomizeProject) UserState.Default.RandomizeProject

    let rec randomizeProjectAttachment =
        atomWithSync (nameof randomizeProjectAttachment) UserState.Default.RandomizeProjectAttachment

    let rec randomizeArea = atomWithSync (nameof randomizeArea) UserState.Default.RandomizeArea

    let rec randomizeAreaAttachment =
        atomWithSync (nameof randomizeAreaAttachment) UserState.Default.RandomizeAreaAttachment

    let rec randomizeResource = atomWithSync (nameof randomizeResource) UserState.Default.RandomizeResource

    let rec randomizeResourceAttachment =
        atomWithSync (nameof randomizeResourceAttachment) UserState.Default.RandomizeResourceAttachment

    let rec randomizeProjectTask = atomWithSync (nameof randomizeProjectTask) UserState.Default.RandomizeProjectTask
    let rec randomizeAreaTask = atomWithSync (nameof randomizeAreaTask) UserState.Default.RandomizeAreaTask

    let rec randomizeProjectTaskAttachment =
        atomWithSync (nameof randomizeProjectTaskAttachment) UserState.Default.RandomizeProjectTaskAttachment

    let rec randomizeAreaTaskAttachment =
        atomWithSync (nameof randomizeAreaTaskAttachment) UserState.Default.RandomizeAreaTaskAttachment

    let rec randomizeCellAttachment =
        atomWithSync (nameof randomizeCellAttachment) UserState.Default.RandomizeCellAttachment

    let rec rightDock = atomWithSync (nameof rightDock) UserState.Default.RightDock
    let rec rightDockSize = atomWithSync (nameof rightDockSize) UserState.Default.RightDockSize
    let rec searchText = atomWithSync (nameof searchText) UserState.Default.SearchText
    let rec selectedDatabaseIdSet = atomWithSync (nameof selectedDatabaseIdSet) UserState.Default.SelectedDatabaseIdSet
    let rec sessionBreakDuration = atomWithSync (nameof sessionBreakDuration) UserState.Default.SessionBreakDuration
    let rec sessionDuration = atomWithSync (nameof sessionDuration) UserState.Default.SessionDuration
    let rec userColor = atomWithSync (nameof userColor) (None: Color option)
    let rec view = atomWithSync (nameof view) UserState.Default.View
    let rec weekStart = atomWithSync (nameof weekStart) UserState.Default.WeekStart

    let rec uiFlag =
        Store.atomFamilyWithSync
            Fluke.root
            collection
            (nameof uiFlag)
            (fun (_: UIFlagType) -> uiFlagDefault)
            (string >> List.singleton)

    let rec uiVisibleFlag =
        Store.atomFamilyWithSync
            Fluke.root
            collection
            (nameof uiVisibleFlag)
            (fun (_: UIFlagType) -> uiVisibleFlagDefault)
            (string >> List.singleton)

    let rec accordionHiddenFlag =
        Store.atomFamilyWithSync
            Fluke.root
            collection
            (nameof accordionHiddenFlag)
            (fun (_: AccordionType) -> accordionHiddenFlagDefault)
            (string >> List.singleton)
