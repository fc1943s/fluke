namespace Fluke.UI.Frontend.State.Atoms

open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.Shared.Domain.UserInteraction


module rec Attachment =
    let attachmentIdIdentifier (attachmentId: AttachmentId) =
        attachmentId
        |> AttachmentId.Value
        |> string
        |> List.singleton

    let rec timestamp =
        Store.atomFamilyWithSync (
            $"{nameof Attachment}/{nameof timestamp}",
            (fun (_attachmentId: AttachmentId) -> None: FlukeDateTime option),
            attachmentIdIdentifier
        )

    let rec archived =
        Store.atomFamilyWithSync (
            $"{nameof Attachment}/{nameof archived}",
            (fun (_attachmentId: AttachmentId) -> None: bool option),
            attachmentIdIdentifier
        )

    let rec attachment =
        Store.atomFamilyWithSync (
            $"{nameof Attachment}/{nameof attachment}",
            (fun (_attachmentId: AttachmentId) -> None: Attachment option),
            attachmentIdIdentifier
        )
