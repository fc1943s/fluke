namespace Fluke.UI.Frontend.State.Selectors

open Fluke.UI.Frontend.State.State
open FsCore
open Fluke.Shared
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain.State
open FsCore.BaseModel
open FsJs
open FsStore
open FsStore.Model


#nowarn "40"


module Store =
    let inline selectorFamily collection name =
        Atom.selectorFamily
            (fun (CellRef (taskId, date)) ->
                StoreAtomPath.ValueAtomPath (
                    Fluke.root,
                    collection,
                    Atoms.Task.formatTaskId taskId
                    @ FlukeDate.formatDate date,
                    AtomName name
                ))

    let inline readSelectorFamily collection name read =
        selectorFamily collection name read (fun _ _ _ _ -> failwith "readSelectorFamily readonly")


[<AutoOpen>]
module CellMagic =
    module rec Cell =
        let collection = Collection (nameof Cell)

module Cell =
    let rec sessionStatus =
        Store.selectorFamily
            Cell.collection
            (nameof sessionStatus)
            (fun (CellRef (taskId, date)) getter ->
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
            (fun (CellRef (taskId, date)) _getter setter newValue ->
                let getLocals () =
                    $"newValue={newValue} date={date |> FlukeDate.Stringify} {getLocals ()}"

                Logger.logDebug (fun () -> "Cell.sessionStatus / write()") getLocals

                Atom.change
                    setter
                    (Atoms.Task.statusMap taskId)
                    (fun statusMap ->
                        let newMap =
                            match newValue with
                            | UserStatus (username, status) -> statusMap |> Map.add date (username, status)
                            | _ -> statusMap |> Map.remove date

                        let getLocals () =
                            $"newMap={newMap} statusMap={statusMap} {getLocals ()}"

                        Logger.logDebug (fun () -> "Cell.sessionStatus / write() / change") getLocals

                        newMap))


    let rec selected =
        Store.selectorFamily
            Cell.collection
            (nameof selected)
            (fun (CellRef (taskId, date)) getter ->
                let selectionSet = Atom.get getter (Atoms.Task.selectionSet taskId)
                selectionSet.Contains date)
            (fun (CellRef (taskId, date)) getter setter newValue ->
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

                let getLocals () =
                    $"ctrlPressed={ctrlPressed} shiftPressed={shiftPressed} taskId={taskId} date={date} newValue={newValue} newCellSelectionMap={newCellSelectionMap} {getLocals ()}"

                Logger.logDebug (fun () -> "Cell.selected / write()") getLocals

                Atom.set setter Selectors.Session.visibleTaskSelectedDateMap newCellSelectionMap)

    let rec sessions =
        Store.readSelectorFamily
            Cell.collection
            (nameof sessions)
            (fun (CellRef (taskId, date)) getter ->
                let sessions = Atom.get getter (Atoms.Task.sessions taskId)
                let dayStart = Atom.get getter Atoms.User.dayStart

                sessions
                |> List.filter (fun (Session start) -> isToday dayStart start date))

    let rec sessionCount =
        Store.readSelectorFamily
            Cell.collection
            (nameof sessionCount)
            (fun cellRef getter ->
                let sessions = Atom.get getter (sessions cellRef)
                sessions.Length)

    let rec attachmentIdSet =
        Store.readSelectorFamily
            Cell.collection
            (nameof attachmentIdSet)
            (fun (CellRef (taskId, date)) getter ->
                let cellAttachmentIdMap = Atom.get getter (Task.cellAttachmentIdMap taskId)

                cellAttachmentIdMap
                |> Map.tryFind date
                |> Option.defaultValue Set.empty)
