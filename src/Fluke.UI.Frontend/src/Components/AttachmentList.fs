namespace Fluke.UI.Frontend.Components

open Fluke.UI.Frontend.Bindings
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
        UI.flex
            (fun x ->
                //                x.display <- "flex"
//                x.flexDirection <- "row"
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
