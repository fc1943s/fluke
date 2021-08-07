namespace Fluke.UI.Frontend.Components

open Fable.React
open Fluke.UI.Frontend.State.State
open FsStore
open FsUi.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.State
open Feliz


module AttachmentsClipboard =
    [<ReactComponent>]
    let rec AttachmentsClipboard attachmentParent onAdd =
        let archive = Store.useValue Atoms.User.archive
        let clipboardVisible = Store.useValue Atoms.User.clipboardVisible
        let clipboardAttachmentIdMap = Store.useValue Atoms.User.clipboardAttachmentIdMap

        let deleteImageAttachment =
            Store.useCallbackRef
                (fun getter setter attachmentId ->
                    promise {
                        Store.change setter Atoms.User.clipboardAttachmentIdMap (Map.remove attachmentId)
                        do! Store.deleteRoot getter (Atoms.Attachment.attachment attachmentId)
                    })

        let addImageAttachment =
            Store.useCallbackRef
                (fun _ setter attachmentId ->
                    promise {
                        Store.change setter Atoms.User.clipboardAttachmentIdMap (Map.remove attachmentId)

                        match attachmentParent with
                        | AttachmentParent.Information _ ->
                            Store.set setter (Atoms.Attachment.archived attachmentId) archive
                        | _ -> ()

                        do! onAdd attachmentId
                    })

        if not clipboardVisible then
            nothing
        else
            UI.box
                (fun _ -> ())
                [
                    if clipboardAttachmentIdMap.Count = 0 then
                        UI.box
                            (fun x -> x.padding <- "10px")
                            [
                                str "Empty clipboard"
                            ]
                    else
                        AttachmentList.AttachmentList
                            deleteImageAttachment
                            addImageAttachment
                            (clipboardAttachmentIdMap |> Map.keys |> Seq.toList)
                ]
