namespace Fluke.UI.Frontend.State.Atoms

open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.State
open FsCore.BaseModel
open FsStore
open FsStore.Bindings.Gun


module rec Database =
    let collection = Collection (nameof Database)

    let formatDatabaseId =
        Engine.getKeysFormatter
            (fun databaseId ->
                databaseId
                |> DatabaseId.Value
                |> string
                |> AtomKeyFragment
                |> List.singleton)

    let inline createFamilyWithSubscription name defaultValueFn =
        Engine.createFamilyWithSubscription Fluke.root collection name defaultValueFn formatDatabaseId

    let rec name = createFamilyWithSubscription (nameof name) (fun (_: DatabaseId) -> Database.Default.Name)
    let rec owner = createFamilyWithSubscription (nameof owner) (fun (_: DatabaseId) -> Database.Default.Owner)

    let rec sharedWith =
        createFamilyWithSubscription (nameof sharedWith) (fun (_: DatabaseId) -> Database.Default.SharedWith)

    let rec position = createFamilyWithSubscription (nameof position) (fun (_: DatabaseId) -> Database.Default.Position)
