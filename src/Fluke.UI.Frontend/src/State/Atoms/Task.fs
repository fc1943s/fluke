namespace Fluke.UI.Frontend.State.Atoms

open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.State
open FsCore.BaseModel
open FsStore
open FsStore.Bindings.Gun


module rec Task =
    let collection = Collection (nameof Task)

    let formatTaskId =
        Engine.getKeysFormatter
            (fun taskId ->
                taskId
                |> TaskId.Value
                |> string
                |> AtomKeyFragment
                |> List.singleton)

    let inline createFamilyWithSubscription name defaultValueFn =
        Engine.createFamilyWithSubscription Fluke.root collection name defaultValueFn formatTaskId

    let rec statusMap =
        createFamilyWithSubscription
            (nameof statusMap)
            (fun (_taskId: TaskId) -> Map.empty: Map<FlukeDate, Username * ManualCellStatus>)

    let rec databaseId = createFamilyWithSubscription (nameof databaseId) (fun (_: TaskId) -> Database.Default.Id)
    let rec sessions = createFamilyWithSubscription (nameof sessions) (fun (_: TaskId) -> []: Session list)

    let rec selectionSet =
        createFamilyWithSubscription (nameof selectionSet) (fun (_: TaskId) -> Set.empty: Set<FlukeDate>)

    let rec information =
        createFamilyWithSubscription (nameof information) (fun (_: TaskId) -> Task.Default.Information)

    let rec name = createFamilyWithSubscription (nameof name) (fun (_: TaskId) -> Task.Default.Name)
    let rec scheduling = createFamilyWithSubscription (nameof scheduling) (fun (_: TaskId) -> Task.Default.Scheduling)

    let rec pendingAfter =
        createFamilyWithSubscription (nameof pendingAfter) (fun (_: TaskId) -> Task.Default.PendingAfter)

    let rec missedAfter =
        createFamilyWithSubscription (nameof missedAfter) (fun (_: TaskId) -> Task.Default.MissedAfter)

    let rec priority = createFamilyWithSubscription (nameof priority) (fun (_: TaskId) -> Task.Default.Priority)
    let rec duration = createFamilyWithSubscription (nameof duration) (fun (_: TaskId) -> Task.Default.Duration)
    let rec archived = createFamilyWithSubscription (nameof archived) (fun (_: TaskId) -> None: bool option)
