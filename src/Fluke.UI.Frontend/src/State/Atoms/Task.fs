namespace Fluke.UI.Frontend.State.Atoms

open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.State
open FsCore.BaseModel
open FsStore



module rec Task =
    let collection = Collection (nameof Task)

    let inline taskIdIdentifier (taskId: TaskId) =
        taskId |> TaskId.Value |> string |> List.singleton

    let inline atomFamilyWithSync name defaultValueFn =
        Store.atomFamilyWithSync Fluke.root collection name defaultValueFn taskIdIdentifier

    let rec statusMap =
        atomFamilyWithSync
            (nameof statusMap)
            (fun (_taskId: TaskId) -> Map.empty: Map<DateId, Username * ManualCellStatus>)

    let rec databaseId = atomFamilyWithSync (nameof databaseId) (fun (_: TaskId) -> Database.Default.Id)
    let rec sessions = atomFamilyWithSync (nameof sessions) (fun (_: TaskId) -> []: Session list)
    let rec selectionSet = atomFamilyWithSync (nameof selectionSet) (fun (_: TaskId) -> Set.empty: Set<DateId>)
    let rec information = atomFamilyWithSync (nameof information) (fun (_: TaskId) -> Task.Default.Information)
    let rec name = atomFamilyWithSync (nameof name) (fun (_: TaskId) -> Task.Default.Name)
    let rec scheduling = atomFamilyWithSync (nameof scheduling) (fun (_: TaskId) -> Task.Default.Scheduling)
    let rec pendingAfter = atomFamilyWithSync (nameof pendingAfter) (fun (_: TaskId) -> Task.Default.PendingAfter)
    let rec missedAfter = atomFamilyWithSync (nameof missedAfter) (fun (_: TaskId) -> Task.Default.MissedAfter)
    let rec priority = atomFamilyWithSync (nameof priority) (fun (_: TaskId) -> Task.Default.Priority)
    let rec duration = atomFamilyWithSync (nameof duration) (fun (_: TaskId) -> Task.Default.Duration)
    let rec archived = atomFamilyWithSync (nameof archived) (fun (_: TaskId) -> None: bool option)
