namespace Fluke.UI.Frontend.State.Atoms

open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.State.State
open FsCore.Model
open FsStore


module rec User =
    let rec archive =
        Store.atomWithSync (State.collection, $"{nameof User}/{nameof archive}", UserState.Default.Archive, [])

    let rec cellColorDisabled =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof cellColorDisabled}",
            UserState.Default.CellColorDisabled,
            []
        )

    let rec cellColorSuggested =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof cellColorSuggested}",
            UserState.Default.CellColorSuggested,
            []
        )

    let rec cellColorPending =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof cellColorPending}",
            UserState.Default.CellColorPending,
            []
        )

    let rec cellColorMissed =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof cellColorMissed}",
            UserState.Default.CellColorMissed,
            []
        )

    let rec cellColorMissedToday =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof cellColorMissedToday}",
            UserState.Default.CellColorMissedToday,
            []
        )

    let rec cellColorPostponedUntil =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof cellColorPostponedUntil}",
            UserState.Default.CellColorPostponedUntil,
            []
        )

    let rec cellColorPostponed =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof cellColorPostponed}",
            UserState.Default.CellColorPostponed,
            []
        )

    let rec cellColorCompleted =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof cellColorCompleted}",
            UserState.Default.CellColorCompleted,
            []
        )

    let rec cellColorDismissed =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof cellColorDismissed}",
            UserState.Default.CellColorDismissed,
            []
        )

    let rec cellColorScheduled =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof cellColorScheduled}",
            UserState.Default.CellColorScheduled,
            []
        )

    let rec cellSize =
        Store.atomWithSync (State.collection, $"{nameof User}/{nameof cellSize}", UserState.Default.CellSize, [])

    let rec clipboardAttachmentIdMap =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof clipboardAttachmentIdMap}",
            UserState.Default.ClipboardAttachmentIdMap,
            []
        )

    let rec clipboardVisible =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof clipboardVisible}",
            UserState.Default.ClipboardVisible,
            []
        )

    let rec daysAfter =
        Store.atomWithSync (State.collection, $"{nameof User}/{nameof daysAfter}", UserState.Default.DaysAfter, [])

    let rec daysBefore =
        Store.atomWithSync (State.collection, $"{nameof User}/{nameof daysBefore}", UserState.Default.DaysBefore, [])

    let rec dayStart =
        Store.atomWithSync (State.collection, $"{nameof User}/{nameof dayStart}", UserState.Default.DayStart, [])

    let rec enableCellPopover =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof enableCellPopover}",
            UserState.Default.EnableCellPopover,
            []
        )

    let rec expandedDatabaseIdSet =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof expandedDatabaseIdSet}",
            UserState.Default.ExpandedDatabaseIdSet,
            []
        )

    let rec filter =
        Store.atomWithSync (State.collection, $"{nameof User}/{nameof filter}", UserState.Default.Filter, [])

    let rec hideSchedulingOverlay =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof hideSchedulingOverlay}",
            UserState.Default.HideSchedulingOverlay,
            []
        )

    let rec hideTemplates =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof hideTemplates}",
            UserState.Default.HideTemplates,
            []
        )

    let rec language =
        Store.atomWithSync (State.collection, $"{nameof User}/{nameof language}", UserState.Default.Language, [])

    let rec lastDatabaseSelected =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof lastDatabaseSelected}",
            UserState.Default.LastDatabaseSelected,
            []
        )

    let rec leftDock =
        Store.atomWithSync (State.collection, $"{nameof User}/{nameof leftDock}", UserState.Default.LeftDock, [])

    let rec leftDockSize =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof leftDockSize}",
            UserState.Default.LeftDockSize,
            []
        )

    let rec randomizeProject =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof randomizeProject}",
            UserState.Default.RandomizeProject,
            []
        )

    let rec randomizeProjectAttachment =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof randomizeProjectAttachment}",
            UserState.Default.RandomizeProjectAttachment,
            []
        )

    let rec randomizeArea =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof randomizeArea}",
            UserState.Default.RandomizeArea,
            []
        )

    let rec randomizeAreaAttachment =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof randomizeAreaAttachment}",
            UserState.Default.RandomizeAreaAttachment,
            []
        )

    let rec randomizeResource =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof randomizeResource}",
            UserState.Default.RandomizeResource,
            []
        )

    let rec randomizeResourceAttachment =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof randomizeResourceAttachment}",
            UserState.Default.RandomizeResourceAttachment,
            []
        )

    let rec randomizeProjectTask =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof randomizeProjectTask}",
            UserState.Default.RandomizeProjectTask,
            []
        )

    let rec randomizeAreaTask =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof randomizeAreaTask}",
            UserState.Default.RandomizeAreaTask,
            []
        )

    let rec randomizeProjectTaskAttachment =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof randomizeProjectTaskAttachment}",
            UserState.Default.RandomizeProjectTaskAttachment,
            []
        )

    let rec randomizeAreaTaskAttachment =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof randomizeAreaTaskAttachment}",
            UserState.Default.RandomizeAreaTaskAttachment,
            []
        )

    let rec randomizeCellAttachment =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof randomizeCellAttachment}",
            UserState.Default.RandomizeCellAttachment,
            []
        )

    let rec rightDock =
        Store.atomWithSync (State.collection, $"{nameof User}/{nameof rightDock}", UserState.Default.RightDock, [])

    let rec rightDockSize =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof rightDockSize}",
            UserState.Default.RightDockSize,
            []
        )

    let rec searchText =
        Store.atomWithSync (State.collection, $"{nameof User}/{nameof searchText}", UserState.Default.SearchText, [])

    let rec selectedDatabaseIdSet =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof selectedDatabaseIdSet}",
            UserState.Default.SelectedDatabaseIdSet,
            []
        )

    let rec sessionBreakDuration =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof sessionBreakDuration}",
            UserState.Default.SessionBreakDuration,
            []
        )

    let rec sessionDuration =
        Store.atomWithSync (
            State.collection,
            $"{nameof User}/{nameof sessionDuration}",
            UserState.Default.SessionDuration,
            []
        )

    let rec systemUiFont =
        Store.atomWithStorageSync (
            State.collection,
            $"{nameof User}/{nameof systemUiFont}",
            UserState.Default.SystemUiFont
        )

    let rec userColor =
        Store.atomWithSync (State.collection, $"{nameof User}/{nameof userColor}", (None: Color option), [])

    let rec view = Store.atomWithSync (State.collection, $"{nameof User}/{nameof view}", UserState.Default.View, [])

    let rec weekStart =
        Store.atomWithSync (State.collection, $"{nameof User}/{nameof weekStart}", UserState.Default.WeekStart, [])

    let rec uiFlag =
        Store.atomFamilyWithSync (
            State.collection,
            $"{nameof User}/{nameof uiFlag}",
            (fun (_uiFlagType: UIFlagType) -> uiFlagDefault),
            (string >> List.singleton)
        )

    let rec uiVisibleFlag =
        Store.atomFamilyWithSync (
            State.collection,
            $"{nameof User}/{nameof uiVisibleFlag}",
            (fun (_uiFlagType: UIFlagType) -> uiVisibleFlagDefault),
            (string >> List.singleton)
        )

    let rec accordionHiddenFlag =
        Store.atomFamilyWithSync (
            State.collection,
            $"{nameof User}/{nameof accordionHiddenFlag}",
            (fun (_accordionType: AccordionType) -> accordionHiddenFlagDefault),
            (string >> List.singleton)
        )
