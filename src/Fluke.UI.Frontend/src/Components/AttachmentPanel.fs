namespace Fluke.UI.Frontend.Components

open Feliz
open FsUi.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.Components


module AttachmentPanel =
    [<ReactComponent>]
    let AttachmentPanel attachmentParent onAdd onDelete attachmentIdList =
        //        let onDragEnd = Store.useCallback ((fun _ _ x -> promise { printfn $"x={x}" }), [||])
//
//        DragDrop.dragDropContext
//            onDragEnd
//            [
        UI.stack
            (fun x ->
                x.spacing <- "15px"
                x.flex <- "1")
            [
                UI.stack
                    (fun x ->
                        x.flex <- "1"
                        x.display <- "contents"
                        x.overflowY <- "auto"
                        x.flexBasis <- 0)
                    [
                        if attachmentIdList |> List.isEmpty |> not then
                            UI.box
                                (fun _ -> ())
                                [
                                    yield!
                                        attachmentIdList
                                        |> List.map
                                            (fun attachmentId ->
                                                Attachment.Attachment
                                                    attachmentParent
                                                    (fun () -> onDelete attachmentId)
                                                    attachmentId)
                                ]
                        else
                            UI.str "No attachments found"
                    ]

                AddAttachmentInput.AddAttachmentInput attachmentParent onAdd
            ]
