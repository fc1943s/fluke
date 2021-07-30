namespace Fluke.UI.Frontend.State.Atoms

open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.State.State
open FsCore.Model
open FsStore


module rec User =
    let inline atomWithSync atomPath defaultValueFn =
        Store.atomWithSync Fluke.collection atomPath defaultValueFn []

    let rec archive = atomWithSync $"{nameof User}/{nameof archive}" UserState.Default.Archive

    let rec cellColorDisabled =
        atomWithSync $"{nameof User}/{nameof cellColorDisabled}" UserState.Default.CellColorDisabled

    let rec cellColorSuggested =
        atomWithSync $"{nameof User}/{nameof cellColorSuggested}" UserState.Default.CellColorSuggested

    let rec cellColorPending =
        atomWithSync $"{nameof User}/{nameof cellColorPending}" UserState.Default.CellColorPending

    let rec cellColorMissed = atomWithSync $"{nameof User}/{nameof cellColorMissed}" UserState.Default.CellColorMissed

    let rec cellColorMissedToday =
        atomWithSync $"{nameof User}/{nameof cellColorMissedToday}" UserState.Default.CellColorMissedToday

    let rec cellColorPostponedUntil =
        atomWithSync $"{nameof User}/{nameof cellColorPostponedUntil}" UserState.Default.CellColorPostponedUntil

    let rec cellColorPostponed =
        atomWithSync $"{nameof User}/{nameof cellColorPostponed}" UserState.Default.CellColorPostponed

    let rec cellColorCompleted =
        atomWithSync $"{nameof User}/{nameof cellColorCompleted}" UserState.Default.CellColorCompleted

    let rec cellColorDismissed =
        atomWithSync $"{nameof User}/{nameof cellColorDismissed}" UserState.Default.CellColorDismissed

    let rec cellColorScheduled =
        atomWithSync $"{nameof User}/{nameof cellColorScheduled}" UserState.Default.CellColorScheduled

    let rec cellSize = atomWithSync $"{nameof User}/{nameof cellSize}" UserState.Default.CellSize

    let rec clipboardAttachmentIdMap =
        atomWithSync $"{nameof User}/{nameof clipboardAttachmentIdMap}" UserState.Default.ClipboardAttachmentIdMap

    let rec clipboardVisible =
        atomWithSync $"{nameof User}/{nameof clipboardVisible}" UserState.Default.ClipboardVisible

    let rec daysAfter = atomWithSync $"{nameof User}/{nameof daysAfter}" UserState.Default.DaysAfter

    let rec daysBefore = atomWithSync $"{nameof User}/{nameof daysBefore}" UserState.Default.DaysBefore

    let rec dayStart = atomWithSync $"{nameof User}/{nameof dayStart}" UserState.Default.DayStart

    let rec enableCellPopover =
        atomWithSync $"{nameof User}/{nameof enableCellPopover}" UserState.Default.EnableCellPopover

    let rec expandedDatabaseIdSet =
        atomWithSync $"{nameof User}/{nameof expandedDatabaseIdSet}" UserState.Default.ExpandedDatabaseIdSet

    let rec filter = atomWithSync $"{nameof User}/{nameof filter}" UserState.Default.Filter

    let rec hideSchedulingOverlay =
        atomWithSync $"{nameof User}/{nameof hideSchedulingOverlay}" UserState.Default.HideSchedulingOverlay

    let rec hideTemplates = atomWithSync $"{nameof User}/{nameof hideTemplates}" UserState.Default.HideTemplates

    let rec language = atomWithSync $"{nameof User}/{nameof language}" UserState.Default.Language

    let rec lastDatabaseSelected =
        atomWithSync $"{nameof User}/{nameof lastDatabaseSelected}" UserState.Default.LastDatabaseSelected

    let rec leftDock = atomWithSync $"{nameof User}/{nameof leftDock}" UserState.Default.LeftDock

    let rec leftDockSize = atomWithSync $"{nameof User}/{nameof leftDockSize}" UserState.Default.LeftDockSize

    let rec randomizeProject =
        atomWithSync $"{nameof User}/{nameof randomizeProject}" UserState.Default.RandomizeProject

    let rec randomizeProjectAttachment =
        atomWithSync $"{nameof User}/{nameof randomizeProjectAttachment}" UserState.Default.RandomizeProjectAttachment

    let rec randomizeArea = atomWithSync $"{nameof User}/{nameof randomizeArea}" UserState.Default.RandomizeArea

    let rec randomizeAreaAttachment =
        atomWithSync $"{nameof User}/{nameof randomizeAreaAttachment}" UserState.Default.RandomizeAreaAttachment

    let rec randomizeResource =
        atomWithSync $"{nameof User}/{nameof randomizeResource}" UserState.Default.RandomizeResource

    let rec randomizeResourceAttachment =
        atomWithSync $"{nameof User}/{nameof randomizeResourceAttachment}" UserState.Default.RandomizeResourceAttachment

    let rec randomizeProjectTask =
        atomWithSync $"{nameof User}/{nameof randomizeProjectTask}" UserState.Default.RandomizeProjectTask

    let rec randomizeAreaTask =
        atomWithSync $"{nameof User}/{nameof randomizeAreaTask}" UserState.Default.RandomizeAreaTask

    let rec randomizeProjectTaskAttachment =
        atomWithSync
            $"{nameof User}/{nameof randomizeProjectTaskAttachment}"
            UserState.Default.RandomizeProjectTaskAttachment

    let rec randomizeAreaTaskAttachment =
        atomWithSync $"{nameof User}/{nameof randomizeAreaTaskAttachment}" UserState.Default.RandomizeAreaTaskAttachment

    let rec randomizeCellAttachment =
        atomWithSync $"{nameof User}/{nameof randomizeCellAttachment}" UserState.Default.RandomizeCellAttachment

    let rec rightDock = atomWithSync $"{nameof User}/{nameof rightDock}" UserState.Default.RightDock

    let rec rightDockSize = atomWithSync $"{nameof User}/{nameof rightDockSize}" UserState.Default.RightDockSize

    let rec searchText = atomWithSync $"{nameof User}/{nameof searchText}" UserState.Default.SearchText

    let rec selectedDatabaseIdSet =
        atomWithSync $"{nameof User}/{nameof selectedDatabaseIdSet}" UserState.Default.SelectedDatabaseIdSet

    let rec sessionBreakDuration =
        atomWithSync $"{nameof User}/{nameof sessionBreakDuration}" UserState.Default.SessionBreakDuration

    let rec sessionDuration = atomWithSync $"{nameof User}/{nameof sessionDuration}" UserState.Default.SessionDuration

    let rec systemUiFont =
        Store.atomWithStorageSync Fluke.collection $"{nameof User}/{nameof systemUiFont}" UserState.Default.SystemUiFont

    let rec userColor = atomWithSync $"{nameof User}/{nameof userColor}" (None: Color option)

    let rec view = atomWithSync $"{nameof User}/{nameof view}" UserState.Default.View

    let rec weekStart = atomWithSync $"{nameof User}/{nameof weekStart}" UserState.Default.WeekStart

    let rec uiFlag =
        Store.atomFamilyWithSync
            Fluke.collection
            $"{nameof User}/{nameof uiFlag}"
            (fun (_uiFlagType: UIFlagType) -> uiFlagDefault)
            (string >> List.singleton)

    let rec uiVisibleFlag =
        Store.atomFamilyWithSync
            Fluke.collection
            $"{nameof User}/{nameof uiVisibleFlag}"
            (fun (_uiFlagType: UIFlagType) -> uiVisibleFlagDefault)
            (string >> List.singleton)

    let rec accordionHiddenFlag =
        Store.atomFamilyWithSync
            Fluke.collection
            $"{nameof User}/{nameof accordionHiddenFlag}"
            (fun (_accordionType: AccordionType) -> accordionHiddenFlagDefault)
            (string >> List.singleton)
