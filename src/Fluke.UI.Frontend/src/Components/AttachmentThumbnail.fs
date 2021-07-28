namespace Fluke.UI.Frontend.Components

open Fable.React
open Fluke.Shared.Domain.UserInteraction
open FsStore
open Fluke.UI.Frontend.State
open Feliz


module AttachmentThumbnail =
    [<ReactComponent>]
    let AttachmentThumbnail onDelete onAdd attachmentId =
        let attachment = Store.useValue (Atoms.Attachment.attachment attachmentId)

        match attachment with
        | Some attachment ->
            match attachment with
            | Attachment.Image fileId -> TempFileThumbnail.TempFileThumbnail onDelete onAdd fileId
            | _ -> nothing
        | _ -> nothing
