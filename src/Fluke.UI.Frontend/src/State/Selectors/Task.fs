namespace Fluke.UI.Frontend.State.Selectors

open FsCore
open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.State.State
open FsStore


#nowarn "40"


module rec Task =
    open Rendering

    let rec task =
        Store.readSelectorFamily
            Fluke.root
            (nameof task)
            (fun (taskId: TaskId) getter ->
                {
                    Id = taskId
                    Name = Store.value getter (Atoms.Task.name taskId)
                    Information = Store.value getter (Atoms.Task.information taskId)
                    PendingAfter = Store.value getter (Atoms.Task.pendingAfter taskId)
                    MissedAfter = Store.value getter (Atoms.Task.missedAfter taskId)
                    Scheduling = Store.value getter (Atoms.Task.scheduling taskId)
                    Priority = Store.value getter (Atoms.Task.priority taskId)
                    Duration = Store.value getter (Atoms.Task.duration taskId)
                })

    [<RequireQualifiedAccess>]
    type TaskAttachment =
        | Task of TaskId
        | Cell of TaskId * DateId

    let rec taskAttachmentArray =
        Store.readSelectorFamilyInterval
            Fluke.root
            (nameof taskAttachmentArray)
            Selectors.interval
            [||]
            (fun (taskId: TaskId) getter ->
                Selectors.asyncAttachmentIdAtoms
                |> Store.value getter
                |> Array.choose
                    (fun attachmentIdAtom ->
                        let attachmentId = Store.value getter attachmentIdAtom
                        let parent = Store.value getter (Atoms.Attachment.parent attachmentId)

                        match parent with
                        | Some (AttachmentParent.Cell (taskId', dateId)) when taskId' = taskId ->
                            Some (attachmentId, TaskAttachment.Cell (taskId, dateId))
                        | Some (AttachmentParent.Task taskId') when taskId' = taskId ->
                            Some (attachmentId, TaskAttachment.Task taskId)
                        | _ -> None))

    let rec cellAttachmentIdMap =
        Store.readSelectorFamily
            Fluke.root
            (nameof cellAttachmentIdMap)
            (fun (taskId: TaskId) getter ->
                let taskAttachmentArray = taskAttachmentArray taskId |> Store.value getter

                taskAttachmentArray
                |> Array.choose
                    (fun taskAttachment ->
                        match taskAttachment with
                        | attachmentId, TaskAttachment.Cell (_, dateId) -> Some (dateId, attachmentId)
                        | _ -> None)
                |> Array.groupBy fst
                |> Array.map (fun (dateId, items) -> dateId, items |> Array.map snd |> Set.ofArray)
                |> Map.ofSeq)

    let rec attachmentIdSet =
        Store.readSelectorFamily
            Fluke.root
            (nameof attachmentIdSet)
            (fun (taskId: TaskId) getter ->
                let taskAttachmentArray = taskAttachmentArray taskId |> Store.value getter

                taskAttachmentArray
                |> Array.choose
                    (fun taskAttachment ->
                        match taskAttachment with
                        | attachmentId, TaskAttachment.Task _ -> Some attachmentId
                        | _ -> None)
                |> Set.ofArray)

    let rec cellStateMap =
        Store.readSelectorFamily
            Fluke.root
            (nameof cellStateMap)
            (fun (taskId: TaskId) getter ->
                let statusMap = Store.value getter (Atoms.Task.statusMap taskId)
                let cellAttachmentIdMap = Store.value getter (cellAttachmentIdMap taskId)

                let sessions = Store.value getter (Atoms.Task.sessions taskId)
                let dayStart = Store.value getter Atoms.User.dayStart

                let sessionMap =
                    sessions
                    |> List.map (fun (Session start as session) -> dateId dayStart start, session)
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
                                |> Store.waitForAll
                                |> Store.value getter
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
        Store.readSelectorFamily
            Fluke.root
            (nameof filteredCellStateMap)
            (fun (taskId: TaskId) getter ->
                let dateIdArray = Store.value getter Selectors.dateIdArray
                let cellStateMap = Store.value getter (cellStateMap taskId)

                dateIdArray
                |> Array.map
                    (fun dateId ->
                        let cellState =
                            cellStateMap
                            |> Map.tryFind dateId
                            |> Option.defaultValue CellState.Default

                        dateId, cellState)
                |> Map.ofSeq
                |> Map.filter
                    (fun _ cellState ->
                        match cellState with
                        | { Status = UserStatus _ } -> true
                        | { SessionList = _ :: _ } -> true
                        | { AttachmentStateList = _ :: _ } -> true
                        | _ -> false))


    let rec taskState =
        Store.readSelectorFamily
            Fluke.root
            (nameof taskState)
            (fun (taskId: TaskId) getter ->

                let task = Store.value getter (task taskId)
                let sessions = Store.value getter (Atoms.Task.sessions taskId)
                let archived = Store.value getter (Atoms.Task.archived taskId)
                let cellStateMap = Store.value getter (cellStateMap taskId)
                let attachmentIdSet = Store.value getter (attachmentIdSet taskId)

                let attachmentStateList =
                    attachmentIdSet
                    |> Set.toArray
                    |> Array.map Attachment.attachmentState
                    |> Store.waitForAll
                    |> Store.value getter
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
        Store.readSelectorFamilyInterval
            Fluke.root
            (nameof cellStatusMap)
            Selectors.interval
            Map.empty
            (fun (taskId: TaskId) getter ->
                let taskState = Store.value getter (taskState taskId)
                let dateIdArray = Store.value getter Selectors.dateIdArray
                let dayStart = Store.value getter Atoms.User.dayStart

                match dateIdArray with
                | [||] -> Map.empty
                | _ ->
                    let dateSequence =
                        dateIdArray
                        |> Array.choose DateId.Value
                        |> Array.toList

                    let firstDateRange, lastDateRange, taskStateDateSequence =
                        taskStateDateSequence dayStart dateSequence taskState

                    let position = Store.value getter Atoms.Session.position

                    let rec loop renderState =
                        function
                        | moment :: tail ->
                            //                                    let result =
//                                        Store.value
//                                            getter
//                                            (Cell.internalSessionStatus (taskId, dateId dayStart moment, renderState))

                            let result =
                                match position with
                                | Some position ->
                                    Some (
                                        internalSessionStatus
                                            dayStart
                                            position
                                            taskState
                                            (dateId dayStart moment)
                                            renderState
                                    )
                                | None -> None

                            match result with
                            | Some (status, renderState) -> (moment, status) :: loop renderState tail
                            | None -> (moment, Disabled) :: loop renderState tail
                        | [] -> []

                    loop WaitingFirstEvent taskStateDateSequence
                    |> List.filter (fun (moment, _) -> moment >==< (firstDateRange, lastDateRange))
                    |> List.map (fun (moment, cellStatus) -> dateId dayStart moment, cellStatus)
                    |> Map.ofSeq)


    let rec lastSession =
        Store.readSelectorFamily
            Fluke.root
            (nameof lastSession)
            (fun (taskId: TaskId) getter ->
                let dateIdArray = Store.value getter Selectors.dateIdArray
                let cellStateMap = Store.value getter (cellStateMap taskId)

                dateIdArray
                |> Seq.rev
                |> Seq.tryPick
                    (fun dateId ->
                        cellStateMap
                        |> Map.tryFind dateId
                        |> Option.map (fun cellState -> cellState.SessionList)
                        |> Option.defaultValue []
                        |> List.sortByDescending (fun (Session start) -> start |> FlukeDateTime.DateTime)
                        |> List.tryHead))


    let rec activeSession =
        Store.readSelectorFamily
            Fluke.root
            (nameof activeSession)
            (fun (taskId: TaskId) getter ->
                let position = Store.value getter Atoms.Session.position
                let lastSession = Store.value getter (lastSession taskId)

                match position, lastSession with
                | Some position, Some lastSession ->
                    let sessionDuration = Store.value getter Atoms.User.sessionDuration
                    let sessionBreakDuration = Store.value getter Atoms.User.sessionBreakDuration

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
        Store.readSelectorFamily
            Fluke.root
            (nameof showUser)
            (fun (taskId: TaskId) getter ->
                let cellStateMap = Store.value getter (cellStateMap taskId)

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
        Store.readSelectorFamily
            Fluke.root
            (nameof hasSelection)
            (fun (taskId: TaskId) getter ->
                let dateIdArray = Store.value getter Selectors.dateIdArray
                let selectionSet = Store.value getter (Atoms.Task.selectionSet taskId)
                dateIdArray |> Array.exists selectionSet.Contains)
