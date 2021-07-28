namespace Fluke.UI.Frontend.Components

open FsUi.Bindings
open Fluke.Shared
open Feliz


module AttachmentList =
    [<ReactComponent>]
    let rec AttachmentList onDelete onAdd attachmentIdList =

        //        DragDrop.droppable
//            {|
//                droppableId = nameof AttachmentList
//                direction = "horizontal"
//            |}
        UI.stack
            (fun x ->
                //                x.display <- "flex"
                x.direction <- "row"
                x.marginTop <- "5px"

                x.overflow <- "auto")
            [
                yield!
                    attachmentIdList
                    |> List.map
                        (fun attachmentId ->
                            //                            DragDrop.draggable
//                                {|
//                                    draggableId = string attachmentId
//                                    index = i
//                                |}
//                                (fun x -> ())
                            UI.box
                                (fun _ -> ())
                                [
                                    AttachmentThumbnail.AttachmentThumbnail
                                        (fun () -> onDelete attachmentId)
                                        (fun () -> onAdd attachmentId)
                                        attachmentId
                                ])
            ]
