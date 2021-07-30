namespace Fluke.UI.Frontend.State.Selectors

open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.State
open FsStore


module rec Attachment =
    let rec attachmentState =
        Store.readSelectorFamily
            $"{nameof Attachment}/{nameof attachmentState}"
            (fun (attachmentId: AttachmentId) getter ->
                let timestamp = Store.value getter (Atoms.Attachment.timestamp attachmentId)
                let archived = Store.value getter (Atoms.Attachment.archived attachmentId)
                let attachment = Store.value getter (Atoms.Attachment.attachment attachmentId)

                match timestamp, archived, attachment with
                | Some timestamp, Some archived, Some attachment ->
                    Some
                        {
                            Timestamp = timestamp
                            Archived = archived
                            Attachment = attachment
                        }
                | _ -> None)
