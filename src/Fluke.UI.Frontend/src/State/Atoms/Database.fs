namespace Fluke.UI.Frontend.State.Atoms

open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.State
open FsCore.BaseModel
open FsStore


module rec Database =
    let collection = Collection (nameof Database)

    let inline databaseIdIdentifier (databaseId: DatabaseId) =
        databaseId
        |> DatabaseId.Value
        |> string
        |> List.singleton

    let inline atomFamilyWithSync name defaultValueFn =
        Store.atomFamilyWithSync Fluke.root collection name defaultValueFn databaseIdIdentifier

    let rec name = atomFamilyWithSync (nameof name) (fun (_: DatabaseId) -> Database.Default.Name)
    let rec owner = atomFamilyWithSync (nameof owner) (fun (_: DatabaseId) -> Database.Default.Owner)
    let rec sharedWith = atomFamilyWithSync (nameof sharedWith) (fun (_: DatabaseId) -> Database.Default.SharedWith)
    let rec position = atomFamilyWithSync (nameof position) (fun (_: DatabaseId) -> Database.Default.Position)
