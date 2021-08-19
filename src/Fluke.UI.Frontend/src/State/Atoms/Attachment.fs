namespace Fluke.UI.Frontend.State.Atoms

open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.State.State
open FsCore.BaseModel
open FsStore



module rec Attachment =
    let collection = Collection (nameof Attachment)

    let inline attachmentIdIdentifier (attachmentId: AttachmentId) =
        attachmentId
        |> AttachmentId.Value
        |> string
        |> List.singleton

    let inline atomFamilyWithSync name defaultValueFn =
        Store.atomFamilyWithSync Fluke.root collection name defaultValueFn attachmentIdIdentifier

    let rec parent = atomFamilyWithSync (nameof parent) (fun (_: AttachmentId) -> None: AttachmentParent option)
    let rec timestamp = atomFamilyWithSync (nameof timestamp) (fun (_: AttachmentId) -> None: FlukeDateTime option)
    let rec archived = atomFamilyWithSync (nameof archived) (fun (_: AttachmentId) -> None: bool option)
    let rec attachment = atomFamilyWithSync (nameof attachment) (fun (_: AttachmentId) -> None: Attachment option)
