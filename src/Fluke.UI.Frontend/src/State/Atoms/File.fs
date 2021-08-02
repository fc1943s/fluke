namespace Fluke.UI.Frontend.State.Atoms

open Fluke.Shared
open Fluke.UI.Frontend.State
open FsCore.Model
open FsStore
open Fluke.Shared.Domain.UserInteraction

#nowarn "40"


module rec File =
    let collection = Collection (nameof File)

    let fileIdIdentifier (fileId: FileId) =
        fileId |> FileId.Value |> string |> List.singleton

    let rec chunkCount =
        Store.atomFamilyWithSync Fluke.root collection (nameof chunkCount) (fun (_: FileId) -> 0) fileIdIdentifier

    let rec chunk =
        Store.atomFamilyWithSync
            Fluke.root
            collection
            (nameof chunk)
            (fun (_: FileId, _: int) -> "")
            (fun (fileId: FileId, index: int) ->
                fileIdIdentifier fileId
                @ [
                    string index
                ])
