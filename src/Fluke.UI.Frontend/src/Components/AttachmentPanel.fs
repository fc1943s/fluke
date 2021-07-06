namespace Fluke.UI.Frontend.Components

open Browser.Types
open Feliz
open Fable.React
open Fluke.Shared.Domain
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.State


module AttachmentPanel =
    [<ReactComponent>]
    let AttachmentHeader onDelete onEdit attachmentId =
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

                                            MenuItem.MenuItem Icons.bs.BsPen "Edit Attachment" onEdit (fun _ -> ())

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

        //        let tempAttachment =
//            Store.Hooks.useTempAtom
//                (Some (Store.InputAtom (Store.AtomReference.Atom (Atoms.Attachment.attachment attachmentId))))
//                (Some (Store.InputScope.ReadWrite Gun.defaultSerializer))
//
        let editing, setEditing = React.useState false

        let onEdit =
            Store.useCallback (
                (fun _ _ _ -> promise { setEditing true }),
                [|
                    box setEditing
                |]
            )

        let reset =
            Store.useCallback (
                (fun _ setter _ ->
                    promise {
                        Store.readWriteReset setter (Atoms.Attachment.attachment attachmentId)
                        setEditing false
                    }),
                [|
                    box setEditing
                    box attachmentId
                |]
            )

        let onSave =
            Store.useCallback (
                (fun getter setter () ->
                    promise {
                        let attachment = Store.getReadWrite getter (Atoms.Attachment.attachment attachmentId)

                        match attachment with
                        | Some (Attachment.Comment (Comment.Comment (String.ValidString _))) ->
                            Store.set setter (Atoms.Attachment.attachment attachmentId) attachment
                            do! reset ()
                        | _ -> ()
                    }),
                [|
                    box attachmentId
                    box reset
                |]
            )

        Chakra.stack
            (fun x ->
                x.flex <- "1"
                x.spacing <- "6px")
            [
                AttachmentHeader onDelete onEdit attachmentId

                match attachment with
                | Some (Attachment.Comment (Comment.Comment comment)) ->
                    Chakra.box
                        (fun _ -> ())
                        [
                            if not editing then
                                AttachmentComment comment
                            else
                                Chakra.flex
                                    (fun x -> x.position <- "relative")
                                    [
                                        Input.Input
                                            {|
                                                CustomProps =
                                                    fun x ->
                                                        x.textarea <- true
                                                        x.variableHeight <- true

                                                        x.atom <-
                                                            Some (
                                                                Store.InputAtom (
                                                                    Store.AtomReference.Atom (
                                                                        Atoms.Attachment.attachment attachmentId
                                                                    )
                                                                )
                                                            )

                                                        x.inputScope <-
                                                            Some (Store.InputScope.ReadWrite Gun.defaultSerializer)

                                                        x.onFormat <-
                                                            Some
                                                                (function
                                                                | Some (Attachment.Comment (Comment.Comment comment)) ->
                                                                    comment
                                                                | _ -> "")

                                                        x.onValidate <-
                                                            Some (
                                                                fst
                                                                >> Comment.Comment
                                                                >> Attachment.Comment
                                                                >> Some
                                                                >> Some
                                                            )

                                                        x.onEnterPress <-
                                                            Some
                                                                (fun (x: KeyboardEvent) ->
                                                                    promise { if x.ctrlKey then do! onSave () })
                                                Props = fun x -> x.autoFocus <- true
                                            |}

                                        Chakra.stack
                                            (fun x ->
                                                x.direction <- "row"
                                                x.spacing <- "2px"
                                                x.position <- "absolute"
                                                x.bottom <- "5px"
                                                x.right <- "7px")
                                            [
                                                InputLabelIconButton.InputLabelIconButton
                                                    (fun x ->
                                                        x.icon <- Icons.md.MdClear |> Icons.render
                                                        x.margin <- "0"
                                                        x.fontSize <- "11px"
                                                        x.height <- "15px"
                                                        x.color <- "whiteAlpha.700"
                                                        x.onClick <- fun _ -> reset ())

                                                InputLabelIconButton.InputLabelIconButton
                                                    (fun x ->
                                                        x.icon <- Icons.hi.HiOutlineCheck |> Icons.render
                                                        x.margin <- "0"
                                                        x.fontSize <- "11px"
                                                        x.height <- "15px"
                                                        x.color <- "whiteAlpha.700"
                                                        x.onClick <- fun _ -> onSave ())
                                            ]
                                    ]
                        ]

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
