namespace Fluke.UI.Frontend.State.Atoms

#nowarn "40"

open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.Shared.Domain.UserInteraction


module rec File =
    let fileIdIdentifier (fileId: FileId) =
        fileId |> FileId.Value |> string |> List.singleton

    let rec chunkCount =
        Store.atomFamilyWithSync ($"{nameof File}/{nameof chunkCount}", (fun (_fileId: FileId) -> 0), fileIdIdentifier)

    let rec chunk =
        Store.atomFamilyWithSync (
            $"{nameof File}/{nameof chunk}",
            (fun (_fileId: FileId, _index: int) -> ""),
            (fun (fileId: FileId, index: int) ->
                fileIdIdentifier fileId
                @ [
                    string index
                ])
        )
