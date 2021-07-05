namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.Shared.Domain
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.State


module AttachmentPanel =
    [<ReactComponent>]
    let AttachmentHeader onDelete attachmentId =
        let timestamp = Store.useValue (Atoms.Attachment.timestamp attachmentId)

        Chakra.flex
            (fun x -> x.color <- "whiteAlpha.600")
            [
                Chakra.box
                    (fun x -> x.lineHeight <- "16px")
                    [
                        match timestamp with
                        | Some timestamp ->
                            Chakra.box
                                (fun x ->
                                    x.userSelect <- "text"
                                    x.display <- "inline")
                                [
                                    str (timestamp |> FlukeDateTime.Stringify)
                                ]

                            Menu.Menu
                                {|
                                    Tooltip = ""
                                    Trigger =
                                        InputLabelIconButton.InputLabelIconButton
                                            (fun x ->
                                                x.``as`` <- Chakra.react.MenuButton
                                                x.icon <- Icons.bs.BsThreeDots |> Icons.render
                                                x.fontSize <- "11px"
                                                x.height <- "15px"
                                                x.color <- "whiteAlpha.700"
                                                x.marginTop <- "-5px"
                                                x.marginLeft <- "6px")
                                    Body =
                                        [
                                            ConfirmPopover.ConfirmPopover
                                                ConfirmPopover.ConfirmPopoverType.MenuItem
                                                Icons.bi.BiTrash
                                                "Delete Attachment"
                                                onDelete
                                        ]
                                    MenuListProps = fun _ -> ()
                                |}
                        | None -> LoadingSpinner.InlineLoadingSpinner ()
                    ]
            ]

    [<ReactComponent>]
    let AttachmentComment text =
        Chakra.box
            (fun x ->
                x.userSelect <- "text"
                x.overflow <- "auto"
                x.paddingTop <- "2px"
                x.paddingBottom <- "2px")
            [
                Markdown.render text
            ]

    [<ReactComponent>]
    let Attachment onDelete attachmentId =
        let attachment = Store.useValue (Atoms.Attachment.attachment attachmentId)

        Chakra.stack
            (fun x ->
                x.flex <- "1"
                x.spacing <- "6px")
            [
                AttachmentHeader onDelete attachmentId

                match attachment with
                | Some (Attachment.Comment (Comment.Comment comment)) -> AttachmentComment comment
                | Some (Attachment.List list) ->
                    let comments, list = list |> List.partition Attachment.isComment
                    let _images, _list = list |> List.partition Attachment.isImage

                    React.fragment [
                        yield!
                            comments
                            |> List.map
                                (fun attachment ->
                                    match attachment with
                                    | Attachment.Comment (Comment.Comment comment) -> AttachmentComment comment
                                    | _ -> nothing)

                    ]

                    nothing
                | _ -> str "???"
            ]

    [<ReactComponent>]
    let AttachmentPanel onAdd onDelete attachmentIdList =
        //        let onDragEnd = Store.useCallback ((fun _ _ x -> promise { printfn $"x={x}" }), [||])
//
//        DragDrop.dragDropContext
//            onDragEnd
//            [
        Chakra.stack
            (fun x ->
                x.spacing <- "15px"
                x.flex <- "1")
            [
                Chakra.stack
                    (fun x ->
                        x.flex <- "1"
                        x.display <- "contents"
                        x.overflowY <- "auto"
                        x.flexBasis <- 0)
                    [
                        match attachmentIdList with
                        //                                                | None -> LoadingSpinner.LoadingSpinner ()
                        | [] ->
                            Chakra.box
                                (fun _ -> ())
                                [
                                    str "No attachments found"
                                ]
                        | attachmentIdList ->
                            Chakra.stack
                                (fun x -> x.spacing <- "10px")
                                [
                                    yield!
                                        attachmentIdList
                                        |> List.map
                                            (fun attachmentId ->
                                                Attachment (fun () -> onDelete attachmentId) attachmentId)
                                ]
                    ]

                AddAttachmentInput.AddAttachmentInput onAdd
            ]
