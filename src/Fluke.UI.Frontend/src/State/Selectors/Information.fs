namespace Fluke.UI.Frontend.State.Selectors

open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.State
open FsStore


module rec Information =
    let rec attachmentIdMap =
        Atom.Primitives.readSelectorFamily
            (fun (information: Information) getter ->
                let selectedDatabaseIdArray =
                    Atom.get getter Atoms.User.selectedDatabaseIdSet
                    |> Set.toArray

                let informationAttachmentIdMapByArchiveArray =
                    selectedDatabaseIdArray
                    |> Array.map Database.informationAttachmentIdMapByArchive
                    |> Atom.waitForAll
                    |> Atom.get getter

                informationAttachmentIdMapByArchiveArray
                |> Array.mapi
                    (fun i informationAttachmentIdMapByArchive ->
                        selectedDatabaseIdArray.[i],
                        informationAttachmentIdMapByArchive
                        |> Map.tryFind information
                        |> Option.defaultValue Set.empty)
                |> Map.ofSeq)
