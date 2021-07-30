namespace Fluke.UI.Frontend.State.Atoms

open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.State
open FsStore


module rec Database =
    let inline databaseIdIdentifier (databaseId: DatabaseId) =
        databaseId
        |> DatabaseId.Value
        |> string
        |> List.singleton

    let inline atomFamilyWithSync atomPath defaultValueFn =
        Store.atomFamilyWithSync Fluke.collection atomPath defaultValueFn databaseIdIdentifier

    let rec name =
        atomFamilyWithSync $"{nameof Database}/{nameof name}" (fun (_databaseId: DatabaseId) -> Database.Default.Name)

    let rec owner =
        atomFamilyWithSync $"{nameof Database}/{nameof owner}" (fun (_databaseId: DatabaseId) -> Database.Default.Owner)

    let rec sharedWith =
        atomFamilyWithSync
            $"{nameof Database}/{nameof sharedWith}"
            (fun (_databaseId: DatabaseId) -> Database.Default.SharedWith)

    let rec position =
        atomFamilyWithSync
            $"{nameof Database}/{nameof position}"
            (fun (_databaseId: DatabaseId) -> Database.Default.Position)

    let rec informationAttachmentIdMap =
        atomFamilyWithSync
            $"{nameof Database}/{nameof informationAttachmentIdMap}"
            (fun (_databaseId: DatabaseId) -> Map.empty: Map<Information, Set<AttachmentId>>)
