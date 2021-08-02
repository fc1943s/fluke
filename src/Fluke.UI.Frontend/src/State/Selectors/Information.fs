namespace Fluke.UI.Frontend.State.Selectors

open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.State
open FsCore.Model
open FsStore


module rec Information =
    let collection = Collection (nameof Information)

    let rec attachmentIdMap =
        Store.readSelectorFamily
            Fluke.root
            collection
            (nameof attachmentIdMap)
            (fun (information: Information) getter ->
                let selectedDatabaseIdArray =
                    Store.value getter Atoms.User.selectedDatabaseIdSet
                    |> Set.toArray

                let informationAttachmentIdMapByArchiveArray =
                    selectedDatabaseIdArray
                    |> Array.map Database.informationAttachmentIdMapByArchive
                    |> Store.waitForAll
                    |> Store.value getter

                informationAttachmentIdMapByArchiveArray
                |> Array.mapi
                    (fun i informationAttachmentIdMapByArchive ->
                        selectedDatabaseIdArray.[i],
                        informationAttachmentIdMapByArchive
                        |> Map.tryFind information
                        |> Option.defaultValue Set.empty)
                |> Map.ofSeq)
