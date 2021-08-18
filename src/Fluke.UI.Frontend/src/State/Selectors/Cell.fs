namespace Fluke.UI.Frontend.State.Selectors

open FsCore
open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain.State
open FsStore
open FsStore.Hooks

#nowarn "40"


module rec Cell =
    let rec sessionStatus =
        Store.selectorFamily
            Fluke.root
            (Some Atoms.Cell.collection)
            (nameof sessionStatus)
            (fun (taskId: TaskId, dateId: DateId) getter ->
                let hideSchedulingOverlay = Store.value getter Atoms.User.hideSchedulingOverlay

                if hideSchedulingOverlay then
                    Store.value getter (Atoms.Task.statusMap taskId)
                    |> Map.tryFind dateId
                    |> Option.map UserStatus
                    |> Option.defaultValue Disabled
                else
                    Store.value getter (Task.cellStatusMap taskId)
                    |> Map.tryFind dateId
                    |> Option.defaultValue Disabled)
            (fun (taskId: TaskId, dateId: DateId) _getter setter newValue ->
                Store.change
                    setter
                    (Atoms.Task.statusMap taskId)
                    (fun statusMap ->
                        match newValue with
                        | UserStatus (username, status) -> statusMap |> Map.add dateId (username, status)
                        | _ -> statusMap |> Map.remove dateId))


    let rec selected =
        Store.selectorFamily
            Fluke.root
            (Some Atoms.Cell.collection)
            (nameof selected)
            (fun (taskId: TaskId, dateId: DateId) getter ->
                let selectionSet = Store.value getter (Atoms.Task.selectionSet taskId)
                selectionSet.Contains dateId)
            (fun (taskId: TaskId, dateId: DateId) getter setter newValue ->
                let ctrlPressed = Store.value getter Atoms.Session.ctrlPressed
                let shiftPressed = Store.value getter Atoms.Session.shiftPressed

                let newCellSelectionMap =
                    match shiftPressed, ctrlPressed with
                    | false, false ->
                        let newTaskSelection = if newValue then Set.singleton dateId else Set.empty

                        [
                            taskId, newTaskSelection
                        ]
                        |> Map.ofSeq
                    | false, true ->
                        let swapSelection oldSelection taskId dateId =
                            let oldSet =
                                oldSelection
                                |> Map.tryFind taskId
                                |> Option.defaultValue Set.empty

                            let newSet =
                                let fn = if newValue then Set.add else Set.remove

                                fn dateId oldSet

                            oldSelection |> Map.add taskId newSet

                        let oldSelection = Store.value getter Selectors.Session.visibleTaskSelectedDateIdMap

                        swapSelection oldSelection taskId dateId
                    | true, _ ->
                        let sortedTaskIdArray = Store.value getter Selectors.Session.sortedTaskIdArray

                        let visibleTaskSelectedDateIdMap =
                            Store.value getter Selectors.Session.visibleTaskSelectedDateIdMap

                        let initialTaskIdSet =
                            visibleTaskSelectedDateIdMap
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
                            visibleTaskSelectedDateIdMap
                            |> Map.values
                            |> Set.unionMany
                            |> Set.add dateId
                            |> Set.toList
                            |> List.choose DateId.Value
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
                                |> List.map DateId
                            |> Set.ofSeq

                        newTaskIdArray
                        |> Array.map (fun taskId -> taskId, dateSet)
                        |> Map.ofSeq

                Store.set setter Selectors.Session.visibleTaskSelectedDateIdMap newCellSelectionMap)

    let rec sessions =
        Store.readSelectorFamily
            Fluke.root
            (nameof sessions)
            (fun (taskId: TaskId, dateId: DateId) getter ->
                let sessions = Store.value getter (Atoms.Task.sessions taskId)
                let dayStart = Store.value getter Atoms.User.dayStart

                sessions
                |> List.filter (fun (Session start) -> isToday dayStart start dateId))

    let rec sessionCount =
        Store.readSelectorFamily
            Fluke.root
            (nameof sessionCount)
            (fun (taskId: TaskId, dateId: DateId) getter ->
                let sessions = Store.value getter (sessions (taskId, dateId))
                sessions.Length)

    let rec attachmentIdSet =
        Store.readSelectorFamily
            Fluke.root
            (nameof attachmentIdSet)
            (fun (taskId: TaskId, dateId: DateId) getter ->
                let cellAttachmentIdMap = Store.value getter (Task.cellAttachmentIdMap taskId)

                cellAttachmentIdMap
                |> Map.tryFind dateId
                |> Option.defaultValue Set.empty)
