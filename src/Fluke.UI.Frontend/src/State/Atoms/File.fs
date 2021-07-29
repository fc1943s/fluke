namespace Fluke.UI.Frontend.State.Atoms

open Fluke.Shared
open Fluke.UI.Frontend.State.State
open FsStore
open Fluke.Shared.Domain.UserInteraction

#nowarn "40"


module rec File =
    let inline fileIdIdentifier (fileId: FileId) =
        fileId |> FileId.Value |> string |> List.singleton

    let inline atomFamilyWithSync atomPath defaultValueFn =
        Store.atomFamilyWithSync (State.collection, atomPath, defaultValueFn, fileIdIdentifier)

    let rec chunkCount = atomFamilyWithSync $"{nameof File}/{nameof chunkCount}" (fun (_fileId: FileId) -> 0)

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
