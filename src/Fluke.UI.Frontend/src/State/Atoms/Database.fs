namespace Fluke.UI.Frontend.State.Atoms

open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain.State


module rec Database =
    let databaseIdIdentifier (databaseId: DatabaseId) =
        databaseId
        |> DatabaseId.Value
        |> string
        |> List.singleton

    let rec name =
        Store.atomFamilyWithSync (
            $"{nameof Database}/{nameof name}",
            (fun (_databaseId: DatabaseId) -> Database.Default.Name),
            databaseIdIdentifier
        )

    let rec owner =
        Store.atomFamilyWithSync (
            $"{nameof Database}/{nameof owner}",
            (fun (_databaseId: DatabaseId) -> Database.Default.Owner),
            databaseIdIdentifier
        )

    let rec sharedWith =
        Store.atomFamilyWithSync (
            $"{nameof Database}/{nameof sharedWith}",
            (fun (_databaseId: DatabaseId) -> Database.Default.SharedWith),
            databaseIdIdentifier
        )

    let rec position =
        Store.atomFamilyWithSync (
            $"{nameof Database}/{nameof position}",
            (fun (_databaseId: DatabaseId) -> Database.Default.Position),
            databaseIdIdentifier
        )

    let rec informationAttachmentIdMap =
        Store.atomFamilyWithSync (
            $"{nameof Database}/{nameof informationAttachmentIdMap}",
            (fun (_databaseId: DatabaseId) -> Map.empty: Map<Information, Set<AttachmentId>>),
            databaseIdIdentifier
        )
