namespace Fluke.UI.Frontend.State.Selectors

open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.State
open FsStore
open FsStore.Bindings.Gun
open FsStore.Model


module Attachment =
    let formatAttachmentId =
        Engine.getKeysFormatter
            (fun attachmentId ->
                attachmentId
                |> AttachmentId.Value
                |> string
                |> AtomKeyFragment
                |> List.singleton)

    let rec attachmentState =
        Atom.readSelectorFamily
            (fun attachmentId ->
                StoreAtomPath.ValueAtomPath (
                    Fluke.root,
                    Atoms.Attachment.collection,
                    formatAttachmentId attachmentId,
                    AtomName (nameof attachmentState)
                ))
            (fun (attachmentId: AttachmentId) ->
                (fun getter ->
                    let timestamp = Atom.get getter (Atoms.Attachment.timestamp attachmentId)
                    let archived = Atom.get getter (Atoms.Attachment.archived attachmentId)
                    let attachment = Atom.get getter (Atoms.Attachment.attachment attachmentId)

                    match timestamp, archived, attachment with
                    | Some timestamp, Some archived, Some attachment ->
                        Some
                            {
                                Timestamp = timestamp
                                Archived = archived
                                Attachment = attachment
                            }
                    | _ -> None))
