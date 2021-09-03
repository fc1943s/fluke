namespace Fluke.UI.Frontend.State.Selectors

open FsCore
open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain.State
open FsCore.BaseModel
open FsStore
open FsStore.Model


#nowarn "40"


module rec Cell =
    let collection = Collection (nameof Cell)

    let selectorFamily name =
        Atom.selectorFamily
            (fun (taskId: TaskId, date: FlukeDate) ->
                StoreAtomPath.IndexedAtomPath (
                    Fluke.root,
                    Cell.collection,
                    Atoms.Task.formatTaskId taskId
                    @ FlukeDate.formatDate date,
                    AtomName name
                ))

    let readSelectorFamily name read =
        selectorFamily name read (fun _ -> failwith "readonly")

    let rec sessionStatus =
        selectorFamily
            (nameof sessionStatus)
            (fun (taskId: TaskId, date: FlukeDate) getter ->
                let hideSchedulingOverlay = Atom.get getter Atoms.User.hideSchedulingOverlay

                if hideSchedulingOverlay then
                    Atom.get getter (Atoms.Task.statusMap taskId)
                    |> Map.tryFind date
                    |> Option.map UserStatus
                    |> Option.defaultValue Disabled
                else
                    Atom.get getter (Task.cellStatusMap taskId)
                    |> Map.tryFind date
                    |> Option.defaultValue Disabled)
            (fun (taskId: TaskId, date: FlukeDate) _getter setter newValue ->
                Atom.change
                    setter
                    (Atoms.Task.statusMap taskId)
                    (fun statusMap ->
                        match newValue with
                        | UserStatus (username, status) -> statusMap |> Map.add date (username, status)
                        | _ -> statusMap |> Map.remove date))


    let rec selected =
        selectorFamily
            (nameof selected)
            (fun (taskId: TaskId, date: FlukeDate) getter ->
                let selectionSet = Atom.get getter (Atoms.Task.selectionSet taskId)
                selectionSet.Contains date)
            (fun (taskId: TaskId, date: FlukeDate) getter setter newValue ->
                let ctrlPressed = Atom.get getter Atoms.Session.ctrlPressed
                let shiftPressed = Atom.get getter Atoms.Session.shiftPressed

                let newCellSelectionMap =
                    match shiftPressed, ctrlPressed with
                    | false, false ->
                        let newTaskSelection = if newValue then Set.singleton date else Set.empty

                        [
                            taskId, newTaskSelection
                        ]
                        |> Map.ofSeq
                    | false, true ->
                        let swapSelection oldSelection taskId date =
                            let oldSet =
                                oldSelection
                                |> Map.tryFind taskId
                                |> Option.defaultValue Set.empty

                            let newSet =
                                let fn = if newValue then Set.add else Set.remove

                                fn date oldSet

                            oldSelection |> Map.add taskId newSet

                        let oldSelection = Atom.get getter Selectors.Session.visibleTaskSelectedDateMap

                        swapSelection oldSelection taskId date
                    | true, _ ->
                        let sortedTaskIdArray = Atom.get getter Selectors.Session.sortedTaskIdArray

                        let visibleTaskSelectedDateMap = Atom.get getter Selectors.Session.visibleTaskSelectedDateMap

                        let initialTaskIdSet =
                            visibleTaskSelectedDateMap
                            |> Map.toSeq
                            |> Seq.filter (fun (_, dates) -> Set.isEmpty dates |> not)
                            |> Seq.map fst
                            |> Set.ofSeq
                            |> Set.add taskId

                        let newTaskIdArray =
                            sortedTaskIdArray
                            |> Array.skipWhile (initialTaskIdSet.Contains >> not)
                            |> Array.rev
                            |> Array.skipWhile (initialTaskIdSet.Contains >> not)
                            |> Array.rev

                        let initialDateList =
                            visibleTaskSelectedDateMap
                            |> Map.values
                            |> Set.unionMany
                            |> Set.add date
                            |> Set.toList
                            |> List.sort

                        let dateSet =
                            match initialDateList with
                            | [] -> []
                            | dateList ->
                                [
                                    dateList.Head
                                    dateList |> List.last
                                ]
                                |> Rendering.getDateSequence (0, 0)
                            |> Set.ofSeq

                        newTaskIdArray
                        |> Array.map (fun taskId -> taskId, dateSet)
                        |> Map.ofSeq

                Atom.set setter Selectors.Session.visibleTaskSelectedDateMap newCellSelectionMap)

    let rec sessions =
        readSelectorFamily
            (nameof sessions)
            (fun (taskId: TaskId, date: FlukeDate) getter ->
                let sessions = Atom.get getter (Atoms.Task.sessions taskId)
                let dayStart = Atom.get getter Atoms.User.dayStart

                sessions
                |> List.filter (fun (Session start) -> isToday dayStart start date))

    let rec sessionCount =
        readSelectorFamily
            (nameof sessionCount)
            (fun (taskId: TaskId, date: FlukeDate) getter ->
                let sessions = Atom.get getter (sessions (taskId, date))
                sessions.Length)

    let rec attachmentIdSet =
        readSelectorFamily
            (nameof attachmentIdSet)
            (fun (taskId: TaskId, date: FlukeDate) getter ->
                let cellAttachmentIdMap = Atom.get getter (Task.cellAttachmentIdMap taskId)

                cellAttachmentIdMap
                |> Map.tryFind date
                |> Option.defaultValue Set.empty)
