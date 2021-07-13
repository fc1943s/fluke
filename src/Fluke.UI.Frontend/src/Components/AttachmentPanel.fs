namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.Components


module AttachmentPanel =
    [<ReactComponent>]
    let AttachmentPanel attachmentPanelType onAdd onDelete attachmentIdList =
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
                                                    attachmentPanelType
                                                    (fun () -> onDelete attachmentId)
                                                    attachmentId)
                                ]
                        else
                            UI.box
                                (fun _ -> ())
                                [
                                    str "No attachments found"
                                ]
                    ]

                AddAttachmentInput.AddAttachmentInput attachmentPanelType onAdd
            ]
