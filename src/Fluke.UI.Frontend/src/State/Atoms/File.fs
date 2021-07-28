namespace Fluke.UI.Frontend.State.Atoms

open Fluke.Shared
open Fluke.UI.Frontend.State.State
open FsStore
open Fluke.Shared.Domain.UserInteraction

#nowarn "40"


module rec File =
    let fileIdIdentifier (fileId: FileId) =
        fileId |> FileId.Value |> string |> List.singleton

    let rec chunkCount =
        Store.atomFamilyWithSync (
            State.collection,
            $"{nameof File}/{nameof chunkCount}",
            (fun (_fileId: FileId) -> 0),
            fileIdIdentifier
        )

    let rec chunk =
        Store.atomFamilyWithSync (
            State.collection,
            $"{nameof File}/{nameof chunk}",
            (fun (_fileId: FileId, _index: int) -> ""),
            (fun (fileId: FileId, index: int) ->
                fileIdIdentifier fileId
                @ [
                    string index
                ])
        )
