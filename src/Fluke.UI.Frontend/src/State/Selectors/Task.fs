namespace Fluke.UI.Frontend.State.Selectors

open FsCore
open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.State.State
open FsJs
open FsStore
open FsStore.Model


#nowarn "40"


module rec Task =
    open Rendering

    let readSelectorFamily name (defaultValue: 'T option) read =
        Atom.Primitives.atomFamily
            (fun taskId ->
                let storeAtomPath =
                    StoreAtomPath.ValueAtomPath (
                        Fluke.root,
                        Atoms.Task.collection,
                        Atoms.Task.formatTaskId taskId,
                        AtomName name
                    )

                let wrapper = Atom.selector storeAtomPath (read taskId) Atom.Primitives.throwReadOnly

                match defaultValue with
                | None -> wrapper
                | Some _defaultValue ->
                    wrapper
//                    |> Engine.wrapAtomWithInterval defaultValue Selectors.interval
                    )


    let rec task =
        readSelectorFamily
            (nameof task)
            None
            (fun (taskId: TaskId) getter ->
                {
                    Id = taskId
                    Name = Atom.get getter (Atoms.Task.name taskId)
                    Information = Atom.get getter (Atoms.Task.information taskId)
                    PendingAfter = Atom.get getter (Atoms.Task.pendingAfter taskId)
                    MissedAfter = Atom.get getter (Atoms.Task.missedAfter taskId)
                    Scheduling = Atom.get getter (Atoms.Task.scheduling taskId)
                    Priority = Atom.get getter (Atoms.Task.priority taskId)
                    Duration = Atom.get getter (Atoms.Task.duration taskId)
                })

    [<RequireQualifiedAccess>]
    type TaskAttachment =
        | Task of TaskId
        | Cell of TaskId * FlukeDate

    let rec taskAttachmentArray =
        readSelectorFamily
            (nameof taskAttachmentArray)
            (Some [||])
            (fun (taskId: TaskId) getter ->
                Selectors.asyncAttachmentIdAtoms
                |> Atom.get getter
                |> Array.choose
                    (fun attachmentIdAtom ->
                        let attachmentId = Atom.get getter attachmentIdAtom
                        let parent = Atom.get getter (Atoms.Attachment.parent attachmentId)

                        match parent with
                        | Some (AttachmentParent.Cell (taskId', date)) when taskId' = taskId ->
                            Some (attachmentId, TaskAttachment.Cell (taskId, date))
                        | Some (AttachmentParent.Task taskId') when taskId' = taskId ->
                            Some (attachmentId, TaskAttachment.Task taskId)
                        | _ -> None))


    let rec cellAttachmentIdMap =
        readSelectorFamily
            (nameof cellAttachmentIdMap)
            None
            (fun (taskId: TaskId) getter ->
                let taskAttachmentArray = taskAttachmentArray taskId |> Atom.get getter

                taskAttachmentArray
                |> Array.choose
                    (fun taskAttachment ->
                        match taskAttachment with
                        | attachmentId, TaskAttachment.Cell (_, date) -> Some (date, attachmentId)
                        | _ -> None)
                |> Array.groupBy fst
                |> Array.map (fun (date, items) -> date, items |> Array.map snd |> Set.ofArray)
                |> Map.ofSeq)

    let rec attachmentIdSet =
        readSelectorFamily
            (nameof attachmentIdSet)
            None
            (fun (taskId: TaskId) getter ->
                let taskAttachmentArray = taskAttachmentArray taskId |> Atom.get getter

                taskAttachmentArray
                |> Array.choose
                    (fun taskAttachment ->
                        match taskAttachment with
                        | attachmentId, TaskAttachment.Task _ -> Some attachmentId
                        | _ -> None)
                |> Set.ofArray)

    let rec cellStateMap =
        readSelectorFamily
            (nameof cellStateMap)
            None
            (fun (taskId: TaskId) getter ->
                let statusMap = Atom.get getter (Atoms.Task.statusMap taskId)
                let cellAttachmentIdMap = Atom.get getter (cellAttachmentIdMap taskId)

                let sessions = Atom.get getter (Atoms.Task.sessions taskId)
                let dayStart = Atom.get getter Atoms.User.dayStart

                let sessionMap =
                    sessions
                    |> List.map (fun (Session start as session) -> getReferenceDay dayStart start, session)
                    |> List.groupBy fst
                    |> Map.ofList
                    |> Map.mapValues (List.map snd)

                let cellStateAttachmentMap =
                    cellAttachmentIdMap
                    |> Map.mapValues
                        (fun attachmentIdSet ->
                            let attachmentStateList =
                                attachmentIdSet
                                |> Set.toArray
                                |> Array.map Attachment.attachmentState
                                |> Atom.waitForAll
                                |> Atom.get getter
                                |> Array.toList
                                |> List.choose id
                                |> List.sortByDescending
                                    (fun attachmentState ->
                                        attachmentState.Timestamp
                                        |> FlukeDateTime.DateTime)

                            {
                                Status = Disabled
                                SessionList = []
                                AttachmentStateList = attachmentStateList
                            })

                let cellStateSessionMap =
                    sessionMap
                    |> Map.mapValues
                        (fun sessions ->
                            { CellState.Default with
                                SessionList = sessions
                            })

                let newStatusMap =
                    statusMap
                    |> Map.mapValues
                        (fun status ->
                            { CellState.Default with
                                Status = UserStatus status
                            })

                let getLocals () =
                    $"statusMap={statusMap} {getLocals ()}"

                Profiling.addTimestamp (fun () -> $"{nameof Fluke} | Task.cellStateMap") getLocals

                newStatusMap
                |> mergeCellStateMap cellStateSessionMap
                |> mergeCellStateMap cellStateAttachmentMap
                |> Map.filter
                    (fun _ cellState ->
                        match cellState with
                        | { Status = UserStatus _ } -> true
                        | { SessionList = _ :: _ } -> true
                        | { AttachmentStateList = _ :: _ } -> true
                        | _ -> false))


    let rec filteredCellStateMap =
        readSelectorFamily
            (nameof filteredCellStateMap)
            None
            (fun (taskId: TaskId) getter ->
                let dateArray = Atom.get getter Selectors.dateArray
                let cellStateMap = Atom.get getter (cellStateMap taskId)

                dateArray
                |> Array.map
                    (fun date ->
                        let cellState =
                            cellStateMap
                            |> Map.tryFind date
                            |> Option.defaultValue CellState.Default

                        date, cellState)
                |> Map.ofSeq
                |> Map.filter
                    (fun _ cellState ->
                        match cellState with
                        | { Status = UserStatus _ } -> true
                        | { SessionList = _ :: _ } -> true
                        | { AttachmentStateList = _ :: _ } -> true
                        | _ -> false))


    let rec taskState =
        readSelectorFamily
            (nameof taskState)
            None
            (fun (taskId: TaskId) getter ->

                let task = Atom.get getter (task taskId)
                let sessions = Atom.get getter (Atoms.Task.sessions taskId)
                let archived = Atom.get getter (Atoms.Task.archived taskId)
                let cellStateMap = Atom.get getter (cellStateMap taskId)
                let attachmentIdSet = Atom.get getter (attachmentIdSet taskId)

                let attachmentStateList =
                    attachmentIdSet
                    |> Set.toArray
                    |> Array.map Attachment.attachmentState
                    |> Atom.waitForAll
                    |> Atom.get getter
                    |> Array.toList
                    |> List.choose id
                    |> List.sortByDescending
                        (fun attachmentState ->
                            attachmentState.Timestamp
                            |> FlukeDateTime.DateTime)

                {
                    Task = task
                    Archived = archived |> Option.defaultValue false
                    SessionList = sessions
                    AttachmentStateList = attachmentStateList
                    SortList = []
                    CellStateMap = cellStateMap
                })


    let rec cellStatusMap =
        readSelectorFamily
            (nameof cellStatusMap)
            (Some Map.empty)
            (fun (taskId: TaskId) getter ->
                let taskState = Atom.get getter (taskState taskId)
                let dateArray = Atom.get getter Selectors.dateArray
                let dayStart = Atom.get getter Atoms.User.dayStart

                match dateArray with
                | [||] -> Map.empty
                | _ ->
                    let dateSequence = dateArray |> Array.toList

                    let firstDateRange, lastDateRange, taskStateDateSequence =
                        taskStateDateSequence dayStart dateSequence taskState

                    let position = Atom.get getter Atoms.Session.position

                    let rec loop renderState =
                        function
                        | moment :: tail ->
                            //                                    let result =
//                                        Store.value
//                                            getter
//                                            (Cell.internalSessionStatus (taskId, date dayStart moment, renderState))

                            let result =
                                match position with
                                | Some position ->
                                    Some (
                                        internalSessionStatus
                                            dayStart
                                            position
                                            taskState
                                            (getReferenceDay dayStart moment)
                                            renderState
                                    )
                                | None -> None

                            match result with
                            | Some (status, renderState) -> (moment, status) :: loop renderState tail
                            | None -> (moment, Disabled) :: loop renderState tail
                        | [] -> []

                    loop WaitingFirstEvent taskStateDateSequence
                    |> List.filter (fun (moment, _) -> moment >==< (firstDateRange, lastDateRange))
                    |> List.map (fun (moment, cellStatus) -> getReferenceDay dayStart moment, cellStatus)
                    |> Map.ofSeq)


    let rec lastSession =
        readSelectorFamily
            (nameof lastSession)
            None
            (fun (taskId: TaskId) getter ->
                let dateArray = Atom.get getter Selectors.dateArray
                let cellStateMap = Atom.get getter (cellStateMap taskId)

                dateArray
                |> Seq.rev
                |> Seq.tryPick
                    (fun date ->
                        cellStateMap
                        |> Map.tryFind date
                        |> Option.map (fun cellState -> cellState.SessionList)
                        |> Option.defaultValue []
                        |> List.sortByDescending (fun (Session start) -> start |> FlukeDateTime.DateTime)
                        |> List.tryHead))


    let rec activeSession =
        readSelectorFamily
            (nameof activeSession)
            None
            (fun (taskId: TaskId) getter ->
                let position = Atom.get getter Atoms.Session.position
                let lastSession = Atom.get getter (lastSession taskId)

                match position, lastSession with
                | Some position, Some lastSession ->
                    let sessionDuration = Atom.get getter Atoms.User.sessionDuration
                    let sessionBreakDuration = Atom.get getter Atoms.User.sessionBreakDuration

                    let (Session start) = lastSession

                    let currentDuration =
                        ((position |> FlukeDateTime.DateTime)
                         - (start |> FlukeDateTime.DateTime))
                            .TotalMinutes
                        |> int

                    let active =
                        currentDuration < (sessionDuration |> Minute.Value)
                                          + (sessionBreakDuration |> Minute.Value)

                    match active with
                    | true -> Some currentDuration
                    | false -> None
                | _ -> None)


    let rec showUser =
        readSelectorFamily
            (nameof showUser)
            None
            (fun (taskId: TaskId) getter ->
                let cellStateMap = Atom.get getter (cellStateMap taskId)

                let usersCount =
                    cellStateMap
                    |> Map.values
                    |> Seq.choose
                        (function
                        | { Status = UserStatus (user, _) } -> Some user
                        | _ -> None)
                    |> Seq.distinct
                    |> Seq.length

                usersCount > 1)


    let rec hasSelection =
        readSelectorFamily
            (nameof hasSelection)
            None
            (fun (taskId: TaskId) getter ->
                let dateArray = Atom.get getter Selectors.dateArray
                let selectionSet = Atom.get getter (Atoms.Task.selectionSet taskId)
                dateArray |> Array.exists selectionSet.Contains)
