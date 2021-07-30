namespace Fluke.UI.Frontend.State.Atoms

open Fluke.Shared
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.State
open FsStore


module rec Attachment =
    let inline attachmentIdIdentifier (attachmentId: AttachmentId) =
        attachmentId
        |> AttachmentId.Value
        |> string
        |> List.singleton

    let inline atomFamilyWithSync atomPath defaultValueFn =
        Store.atomFamilyWithSync Fluke.collection atomPath defaultValueFn attachmentIdIdentifier

    let rec timestamp =
        atomFamilyWithSync
            $"{nameof Attachment}/{nameof timestamp}"
            (fun (_attachmentId: AttachmentId) -> None: FlukeDateTime option)

    let rec archived =
        atomFamilyWithSync
            $"{nameof Attachment}/{nameof archived}"
            (fun (_attachmentId: AttachmentId) -> None: bool option)

    let rec attachment =
        atomFamilyWithSync
            $"{nameof Attachment}/{nameof attachment}"
            (fun (_attachmentId: AttachmentId) -> None: Attachment option)
