namespace Fluke.UI.Frontend.State.Atoms

open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.State.State
open FsStore


module rec Database =
    let databaseIdIdentifier (databaseId: DatabaseId) =
        databaseId
        |> DatabaseId.Value
        |> string
        |> List.singleton

    let rec name =
        Store.atomFamilyWithSync (
            State.collection,
            $"{nameof Database}/{nameof name}",
            (fun (_databaseId: DatabaseId) -> Database.Default.Name),
            databaseIdIdentifier
        )

    let rec owner =
        Store.atomFamilyWithSync (
            State.collection,
            $"{nameof Database}/{nameof owner}",
            (fun (_databaseId: DatabaseId) -> Database.Default.Owner),
            databaseIdIdentifier
        )

    let rec sharedWith =
        Store.atomFamilyWithSync (
            State.collection,
            $"{nameof Database}/{nameof sharedWith}",
            (fun (_databaseId: DatabaseId) -> Database.Default.SharedWith),
            databaseIdIdentifier
        )

    let rec position =
        Store.atomFamilyWithSync (
            State.collection,
            $"{nameof Database}/{nameof position}",
            (fun (_databaseId: DatabaseId) -> Database.Default.Position),
            databaseIdIdentifier
        )

    let rec informationAttachmentIdMap =
        Store.atomFamilyWithSync (
            State.collection,
            $"{nameof Database}/{nameof informationAttachmentIdMap}",
            (fun (_databaseId: DatabaseId) -> Map.empty: Map<Information, Set<AttachmentId>>),
            databaseIdIdentifier
        )
