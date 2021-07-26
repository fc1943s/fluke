namespace Fluke.UI.Frontend.State.Selectors

#nowarn "40"

open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain.State


module rec Cell =
    open Rendering

    let rec sessionStatus =
        Store.selectorFamily (
            $"{nameof Cell}/{nameof sessionStatus}",
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
                    |> Option.defaultValue Disabled),
            (fun (taskId: TaskId, dateId: DateId) _getter setter newValue ->
                Store.change
                    setter
                    (Atoms.Task.statusMap taskId)
                    (fun statusMap ->
                        match newValue with
                        | UserStatus (username, status) -> statusMap |> Map.add dateId (username, status)
                        | _ -> statusMap |> Map.remove dateId))
        )

    let rec selected =
        Store.selectorFamily (
            $"{nameof Cell}/{nameof selected}",
            (fun (taskId: TaskId, dateId: DateId) getter ->
                let selectionSet = Store.value getter (Atoms.Task.selectionSet taskId)
                selectionSet.Contains dateId),
            (fun (taskId: TaskId, dateId: DateId) _ setter newValue ->
                Store.change setter (Atoms.Task.selectionSet taskId) ((if newValue then Set.add else Set.remove) dateId))
        )

    let rec sessions =
        Store.readSelectorFamily (
            $"{nameof Cell}/{nameof sessions}",
            (fun (taskId: TaskId, dateId: DateId) getter ->
                let sessions = Store.value getter (Atoms.Task.sessions taskId)
                let dayStart = Store.value getter Atoms.User.dayStart

                sessions
                |> List.filter (fun (Session start) -> isToday dayStart start dateId))
        )

    let rec sessionCount =
        Store.readSelectorFamily (
            $"{nameof Cell}/{nameof sessionCount}",
            (fun (taskId: TaskId, dateId: DateId) getter ->
                let sessions = Store.value getter (sessions (taskId, dateId))
                sessions.Length)
        )

    let rec attachmentIdSet =
        Store.readSelectorFamily (
            $"{nameof Cell}/{nameof attachmentIdSet}",
            (fun (taskId: TaskId, dateId: DateId) getter ->
                let cellAttachmentIdMap = Store.value getter (Atoms.Task.cellAttachmentIdMap taskId)

                cellAttachmentIdMap
                |> Map.tryFind dateId
                |> Option.defaultValue Set.empty)
        )
