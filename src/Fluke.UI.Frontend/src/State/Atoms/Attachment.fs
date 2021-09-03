namespace Fluke.UI.Frontend.State.Atoms

open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.State.State
open FsCore.BaseModel
open FsStore
open FsStore.Bindings.Gun


module rec Attachment =
    let collection = Collection (nameof Attachment)

    let formatAttachmentId =
        Engine.getKeysFormatter
            (fun attachmentId ->
                attachmentId
                |> AttachmentId.Value
                |> string
                |> AtomKeyFragment
                |> List.singleton)

    let inline createFamilyWithSubscription name defaultValueFn =
        Engine.createFamilyWithSubscription Fluke.root collection name defaultValueFn formatAttachmentId

    let rec parent =
        createFamilyWithSubscription (nameof parent) (fun (_: AttachmentId) -> None: AttachmentParent option)

    let rec timestamp =
        createFamilyWithSubscription (nameof timestamp) (fun (_: AttachmentId) -> None: FlukeDateTime option)

    let rec archived = createFamilyWithSubscription (nameof archived) (fun (_: AttachmentId) -> None: bool option)

    let rec attachment =
        createFamilyWithSubscription (nameof attachment) (fun (_: AttachmentId) -> None: Attachment option)
