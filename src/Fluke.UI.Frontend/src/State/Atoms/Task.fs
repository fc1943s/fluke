namespace Fluke.UI.Frontend.State.Atoms

open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain.State


module rec Task =
    let taskIdIdentifier (taskId: TaskId) =
        taskId |> TaskId.Value |> string |> List.singleton

    let rec statusMap =
        Store.atomFamilyWithSync (
            $"{nameof Task}/{nameof statusMap}",
            (fun (_taskId: TaskId) -> Map.empty: Map<DateId, Username * ManualCellStatus>),
            taskIdIdentifier
        )

    let rec databaseId =
        Store.atomFamilyWithSync (
            $"{nameof Task}/{nameof databaseId}",
            (fun (_taskId: TaskId) -> Database.Default.Id),
            taskIdIdentifier
        )

    let rec sessions =
        Store.atomFamilyWithSync (
            $"{nameof Task}/{nameof sessions}",
            (fun (_taskId: TaskId) -> []: Session list),
            taskIdIdentifier
        )

    let rec attachmentIdSet =
        Store.atomFamilyWithSync (
            $"{nameof Task}/{nameof attachmentIdSet}",
            (fun (_taskId: TaskId) -> Set.empty: Set<AttachmentId>),
            taskIdIdentifier
        )

    let rec cellAttachmentIdMap =
        Store.atomFamilyWithSync (
            $"{nameof Task}/{nameof cellAttachmentIdMap}",
            (fun (_taskId: TaskId) -> Map.empty: Map<DateId, Set<AttachmentId>>),
            taskIdIdentifier
        )

    let rec selectionSet =
        Store.atomFamilyWithSync (
            $"{nameof Task}/{nameof selectionSet}",
            (fun (_taskId: TaskId) -> Set.empty: Set<DateId>),
            taskIdIdentifier
        )

    let rec information =
        Store.atomFamilyWithSync (
            $"{nameof Task}/{nameof information}",
            (fun (_taskId: TaskId) -> Task.Default.Information),
            taskIdIdentifier
        )

    let rec name =
        Store.atomFamilyWithSync (
            $"{nameof Task}/{nameof name}",
            (fun (_taskId: TaskId) -> Task.Default.Name),
            taskIdIdentifier
        )

    let rec scheduling =
        Store.atomFamilyWithSync (
            $"{nameof Task}/{nameof scheduling}",
            (fun (_taskId: TaskId) -> Task.Default.Scheduling),
            taskIdIdentifier
        )

    let rec pendingAfter =
        Store.atomFamilyWithSync (
            $"{nameof Task}/{nameof pendingAfter}",
            (fun (_taskId: TaskId) -> Task.Default.PendingAfter),
            taskIdIdentifier
        )

    let rec missedAfter =
        Store.atomFamilyWithSync (
            $"{nameof Task}/{nameof missedAfter}",
            (fun (_taskId: TaskId) -> Task.Default.MissedAfter),
            taskIdIdentifier
        )

    let rec priority =
        Store.atomFamilyWithSync (
            $"{nameof Task}/{nameof priority}",
            (fun (_taskId: TaskId) -> Task.Default.Priority),
            taskIdIdentifier
        )

    let rec duration =
        Store.atomFamilyWithSync (
            $"{nameof Task}/{nameof duration}",
            (fun (_taskId: TaskId) -> Task.Default.Duration),
            taskIdIdentifier
        )

    let rec archived =
        Store.atomFamilyWithSync (
            $"{nameof Task}/{nameof archived}",
            (fun (_taskId: TaskId) -> None: bool option),
            taskIdIdentifier
        )
