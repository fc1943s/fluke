namespace Fluke.UI.Frontend.State.Atoms

open System
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.State.State
open FsCore.BaseModel
open FsStore
open FsStore.Bindings.Gun
open FsStore.Model

#nowarn "40"

module rec User =
    let collection = Collection (nameof User)

    let inline createAtomWithSubscription<'A8 when 'A8: equality and 'A8 :> IComparable> name (defaultValue: 'A8) =
        Engine.createAtomWithSubscription
            (StoreAtomPath.ValueAtomPath (Fluke.root, collection, [], AtomName name))
            defaultValue

    let rec archive = createAtomWithSubscription (nameof archive) UserState.Default.Archive

    let rec cellColorDisabled =
        createAtomWithSubscription (nameof cellColorDisabled) UserState.Default.CellColorDisabled

    let rec cellColorSuggested =
        createAtomWithSubscription (nameof cellColorSuggested) UserState.Default.CellColorSuggested

    let rec cellColorPending = createAtomWithSubscription (nameof cellColorPending) UserState.Default.CellColorPending
    let rec cellColorMissed = createAtomWithSubscription (nameof cellColorMissed) UserState.Default.CellColorMissed

    let rec cellColorMissedToday =
        createAtomWithSubscription (nameof cellColorMissedToday) UserState.Default.CellColorMissedToday

    let rec cellColorPostponedUntil =
        createAtomWithSubscription (nameof cellColorPostponedUntil) UserState.Default.CellColorPostponedUntil

    let rec cellColorPostponed =
        createAtomWithSubscription (nameof cellColorPostponed) UserState.Default.CellColorPostponed

    let rec cellColorCompleted =
        createAtomWithSubscription (nameof cellColorCompleted) UserState.Default.CellColorCompleted

    let rec cellColorDismissed =
        createAtomWithSubscription (nameof cellColorDismissed) UserState.Default.CellColorDismissed

    let rec cellColorScheduled =
        createAtomWithSubscription (nameof cellColorScheduled) UserState.Default.CellColorScheduled

    let rec cellSize = createAtomWithSubscription (nameof cellSize) UserState.Default.CellSize

    let rec clipboardAttachmentIdMap =
        createAtomWithSubscription (nameof clipboardAttachmentIdMap) UserState.Default.ClipboardAttachmentIdMap

    let rec clipboardVisible = createAtomWithSubscription (nameof clipboardVisible) UserState.Default.ClipboardVisible
    let rec daysAfter = createAtomWithSubscription (nameof daysAfter) UserState.Default.DaysAfter
    let rec daysBefore = createAtomWithSubscription (nameof daysBefore) UserState.Default.DaysBefore
    let rec dayStart = createAtomWithSubscription (nameof dayStart) UserState.Default.DayStart

    let rec enableCellPopover =
        createAtomWithSubscription (nameof enableCellPopover) UserState.Default.EnableCellPopover

    let rec expandedDatabaseIdSet =
        createAtomWithSubscription (nameof expandedDatabaseIdSet) UserState.Default.ExpandedDatabaseIdSet

    let rec filter = createAtomWithSubscription (nameof filter) UserState.Default.Filter

    let rec hideSchedulingOverlay =
        createAtomWithSubscription (nameof hideSchedulingOverlay) UserState.Default.HideSchedulingOverlay

    let rec hideTemplates = createAtomWithSubscription (nameof hideTemplates) UserState.Default.HideTemplates
    let rec language = createAtomWithSubscription (nameof language) UserState.Default.Language

    let rec lastDatabaseSelected =
        createAtomWithSubscription (nameof lastDatabaseSelected) UserState.Default.LastDatabaseSelected

    let rec leftDock = createAtomWithSubscription (nameof leftDock) UserState.Default.LeftDock
    let rec leftDockSize = createAtomWithSubscription (nameof leftDockSize) UserState.Default.LeftDockSize
    let rec randomizeProject = createAtomWithSubscription (nameof randomizeProject) UserState.Default.RandomizeProject

    let rec randomizeProjectAttachment =
        createAtomWithSubscription (nameof randomizeProjectAttachment) UserState.Default.RandomizeProjectAttachment

    let rec randomizeArea = createAtomWithSubscription (nameof randomizeArea) UserState.Default.RandomizeArea

    let rec randomizeAreaAttachment =
        createAtomWithSubscription (nameof randomizeAreaAttachment) UserState.Default.RandomizeAreaAttachment

    let rec randomizeResource =
        createAtomWithSubscription (nameof randomizeResource) UserState.Default.RandomizeResource

    let rec randomizeResourceAttachment =
        createAtomWithSubscription (nameof randomizeResourceAttachment) UserState.Default.RandomizeResourceAttachment

    let rec randomizeProjectTask =
        createAtomWithSubscription (nameof randomizeProjectTask) UserState.Default.RandomizeProjectTask

    let rec randomizeAreaTask =
        createAtomWithSubscription (nameof randomizeAreaTask) UserState.Default.RandomizeAreaTask

    let rec randomizeProjectTaskAttachment =
        createAtomWithSubscription
            (nameof randomizeProjectTaskAttachment)
            UserState.Default.RandomizeProjectTaskAttachment

    let rec randomizeAreaTaskAttachment =
        createAtomWithSubscription (nameof randomizeAreaTaskAttachment) UserState.Default.RandomizeAreaTaskAttachment

    let rec randomizeCellAttachment =
        createAtomWithSubscription (nameof randomizeCellAttachment) UserState.Default.RandomizeCellAttachment

    let rec rightDock = createAtomWithSubscription (nameof rightDock) UserState.Default.RightDock
    let rec rightDockSize = createAtomWithSubscription (nameof rightDockSize) UserState.Default.RightDockSize
    let rec searchText = createAtomWithSubscription (nameof searchText) UserState.Default.SearchText

    let rec selectedDatabaseIdSet =
        createAtomWithSubscription (nameof selectedDatabaseIdSet) UserState.Default.SelectedDatabaseIdSet

    let rec sessionBreakDuration =
        createAtomWithSubscription (nameof sessionBreakDuration) UserState.Default.SessionBreakDuration

    let rec sessionDuration = createAtomWithSubscription (nameof sessionDuration) UserState.Default.SessionDuration
    let rec userColor = createAtomWithSubscription (nameof userColor) (None: Color option)
    let rec view = createAtomWithSubscription (nameof view) UserState.Default.View
    let rec weekStart = createAtomWithSubscription (nameof weekStart) UserState.Default.WeekStart

    let rec uiFlag =
        Engine.createFamilyWithSubscription
            Fluke.root
            collection
            (nameof uiFlag)
            (fun _ -> uiFlagDefault)
            (fun (uiFlagType: UIFlagType) ->
                uiFlagType
                |> string
                |> AtomKeyFragment
                |> List.singleton)

    let rec uiVisibleFlag =
        Engine.createFamilyWithSubscription
            Fluke.root
            collection
            (nameof uiVisibleFlag)
            (fun _ -> uiVisibleFlagDefault)
            (fun (uiFlagType: UIFlagType) ->
                uiFlagType
                |> string
                |> AtomKeyFragment
                |> List.singleton)

    let rec accordionHiddenFlag =
        Engine.createFamilyWithSubscription
            Fluke.root
            collection
            (nameof accordionHiddenFlag)
            (fun _ -> accordionHiddenFlagDefault)
            (fun (accordionType: AccordionType) ->
                accordionType
                |> string
                |> AtomKeyFragment
                |> List.singleton)
