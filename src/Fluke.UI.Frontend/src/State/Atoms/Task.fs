namespace Fluke.UI.Frontend.State.Atoms

open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.State
open FsCore.Model
open FsStore


module rec Task =
    let inline taskIdIdentifier (taskId: TaskId) =
        taskId |> TaskId.Value |> string |> List.singleton

    let inline atomFamilyWithSync atomPath defaultValueFn =
        Store.atomFamilyWithSync Fluke.collection atomPath defaultValueFn taskIdIdentifier

    let rec statusMap =
        atomFamilyWithSync
            $"{nameof Task}/{nameof statusMap}"
            (fun (_taskId: TaskId) -> Map.empty: Map<DateId, Username * ManualCellStatus>)

    let rec databaseId =
        atomFamilyWithSync $"{nameof Task}/{nameof databaseId}" (fun (_taskId: TaskId) -> Database.Default.Id)

    let rec sessions = atomFamilyWithSync $"{nameof Task}/{nameof sessions}" (fun (_taskId: TaskId) -> []: Session list)

    let rec selectionSet =
        atomFamilyWithSync $"{nameof Task}/{nameof selectionSet}" (fun (_taskId: TaskId) -> Set.empty: Set<DateId>)

    let rec information =
        atomFamilyWithSync $"{nameof Task}/{nameof information}" (fun (_taskId: TaskId) -> Task.Default.Information)

    let rec name = atomFamilyWithSync $"{nameof Task}/{nameof name}" (fun (_taskId: TaskId) -> Task.Default.Name)

    let rec scheduling =
        Store.atomFamilyWithSync
            Fluke.collection
            $"{nameof Task}/{nameof scheduling}"
            (fun (_taskId: TaskId) -> Task.Default.Scheduling)
            taskIdIdentifier

    let rec pendingAfter =
        atomFamilyWithSync $"{nameof Task}/{nameof pendingAfter}" (fun (_taskId: TaskId) -> Task.Default.PendingAfter)

    let rec missedAfter =
        atomFamilyWithSync $"{nameof Task}/{nameof missedAfter}" (fun (_taskId: TaskId) -> Task.Default.MissedAfter)

    let rec priority =
        atomFamilyWithSync $"{nameof Task}/{nameof priority}" (fun (_taskId: TaskId) -> Task.Default.Priority)

    let rec duration =
        atomFamilyWithSync $"{nameof Task}/{nameof duration}" (fun (_taskId: TaskId) -> Task.Default.Duration)

    let rec archived =
        atomFamilyWithSync $"{nameof Task}/{nameof archived}" (fun (_taskId: TaskId) -> None: bool option)
