namespace Fluke.UI.Frontend.State.Atoms

open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.State.State
open Fluke.Shared.Domain.UserInteraction


module rec User =
    let rec archive = Store.atomWithSync ($"{nameof User}/{nameof archive}", UserState.Default.Archive, [])

    let rec cellColorDisabled =
        Store.atomWithSync ($"{nameof User}/{nameof cellColorDisabled}", UserState.Default.CellColorDisabled, [])

    let rec cellColorSuggested =
        Store.atomWithSync ($"{nameof User}/{nameof cellColorSuggested}", UserState.Default.CellColorSuggested, [])

    let rec cellColorPending =
        Store.atomWithSync ($"{nameof User}/{nameof cellColorPending}", UserState.Default.CellColorPending, [])

    let rec cellColorMissed =
        Store.atomWithSync ($"{nameof User}/{nameof cellColorMissed}", UserState.Default.CellColorMissed, [])

    let rec cellColorMissedToday =
        Store.atomWithSync ($"{nameof User}/{nameof cellColorMissedToday}", UserState.Default.CellColorMissedToday, [])

    let rec cellColorPostponedUntil =
        Store.atomWithSync (
            $"{nameof User}/{nameof cellColorPostponedUntil}",
            UserState.Default.CellColorPostponedUntil,
            []
        )

    let rec cellColorPostponed =
        Store.atomWithSync ($"{nameof User}/{nameof cellColorPostponed}", UserState.Default.CellColorPostponed, [])

    let rec cellColorCompleted =
        Store.atomWithSync ($"{nameof User}/{nameof cellColorCompleted}", UserState.Default.CellColorCompleted, [])

    let rec cellColorDismissed =
        Store.atomWithSync ($"{nameof User}/{nameof cellColorDismissed}", UserState.Default.CellColorDismissed, [])

    let rec cellColorScheduled =
        Store.atomWithSync ($"{nameof User}/{nameof cellColorScheduled}", UserState.Default.CellColorScheduled, [])

    let rec cellSize = Store.atomWithSync ($"{nameof User}/{nameof cellSize}", UserState.Default.CellSize, [])

    let rec clipboardAttachmentIdMap =
        Store.atomWithSync (
            $"{nameof User}/{nameof clipboardAttachmentIdMap}",
            UserState.Default.ClipboardAttachmentIdMap,
            []
        )

    let rec clipboardVisible =
        Store.atomWithSync ($"{nameof User}/{nameof clipboardVisible}", UserState.Default.ClipboardVisible, [])

    let rec darkMode = Store.atomWithStorageSync ($"{nameof User}/{nameof darkMode}", UserState.Default.DarkMode)

    let rec daysAfter = Store.atomWithSync ($"{nameof User}/{nameof daysAfter}", UserState.Default.DaysAfter, [])

    let rec daysBefore = Store.atomWithSync ($"{nameof User}/{nameof daysBefore}", UserState.Default.DaysBefore, [])

    let rec dayStart = Store.atomWithSync ($"{nameof User}/{nameof dayStart}", UserState.Default.DayStart, [])

    let rec enableCellPopover =
        Store.atomWithSync ($"{nameof User}/{nameof enableCellPopover}", UserState.Default.EnableCellPopover, [])

    let rec expandedDatabaseIdSet =
        Store.atomWithSync (
            $"{nameof User}/{nameof expandedDatabaseIdSet}",
            UserState.Default.ExpandedDatabaseIdSet,
            []
        )

    let rec filter = Store.atomWithSync ($"{nameof User}/{nameof filter}", UserState.Default.Filter, [])

    let rec fontSize = Store.atomWithStorageSync ($"{nameof User}/{nameof fontSize}", UserState.Default.FontSize)

    let rec hideSchedulingOverlay =
        Store.atomWithSync (
            $"{nameof User}/{nameof hideSchedulingOverlay}",
            UserState.Default.HideSchedulingOverlay,
            []
        )

    let rec hideTemplates =
        Store.atomWithSync ($"{nameof User}/{nameof hideTemplates}", UserState.Default.HideTemplates, [])

    let rec language = Store.atomWithSync ($"{nameof User}/{nameof language}", UserState.Default.Language, [])

    let rec lastDatabaseSelected =
        Store.atomWithSync (
            $"{nameof User}/{nameof lastDatabaseSelected}",
            UserState.Default.LastDatabaseSelected,
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
        Store.atomWithSync ($"{nameof User}/{nameof randomizeResource}", UserState.Default.RandomizeResource, [])

    let rec randomizeResourceAttachment =
        Store.atomWithSync (
            $"{nameof User}/{nameof randomizeResourceAttachment}",
            UserState.Default.RandomizeResourceAttachment,
            []
        )

    let rec randomizeProjectTask =
        Store.atomWithSync ($"{nameof User}/{nameof randomizeProjectTask}", UserState.Default.RandomizeProjectTask, [])

    let rec randomizeAreaTask =
        Store.atomWithSync ($"{nameof User}/{nameof randomizeAreaTask}", UserState.Default.RandomizeAreaTask, [])

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

    let rec rightDock = Store.atomWithSync ($"{nameof User}/{nameof rightDock}", UserState.Default.RightDock, [])

    let rec rightDockSize =
        Store.atomWithSync ($"{nameof User}/{nameof rightDockSize}", UserState.Default.RightDockSize, [])

    let rec searchText = Store.atomWithSync ($"{nameof User}/{nameof searchText}", UserState.Default.SearchText, [])

    let rec selectedDatabaseIdSet =
        Store.atomWithSync (
            $"{nameof User}/{nameof selectedDatabaseIdSet}",
            UserState.Default.SelectedDatabaseIdSet,
            []
        )

    let rec sessionBreakDuration =
        Store.atomWithSync ($"{nameof User}/{nameof sessionBreakDuration}", UserState.Default.SessionBreakDuration, [])

    let rec sessionDuration =
        Store.atomWithSync ($"{nameof User}/{nameof sessionDuration}", UserState.Default.SessionDuration, [])

    let rec systemUiFont =
        Store.atomWithStorageSync ($"{nameof User}/{nameof systemUiFont}", UserState.Default.SystemUiFont)

    let rec userColor = Store.atomWithSync ($"{nameof User}/{nameof userColor}", (None: Color option), [])

    let rec view = Store.atomWithSync ($"{nameof User}/{nameof view}", UserState.Default.View, [])

    let rec weekStart = Store.atomWithSync ($"{nameof User}/{nameof weekStart}", UserState.Default.WeekStart, [])

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
