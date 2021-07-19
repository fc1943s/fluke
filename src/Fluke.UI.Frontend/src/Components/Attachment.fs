namespace Fluke.UI.Frontend.Components

open Browser.Types
open Feliz
open Fable.React
open Fluke.Shared.Domain
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Hooks


module Attachment =
    [<ReactComponent>]
    let Attachment attachmentPanelType onDelete attachmentId =
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
                        Store.resetTempValue setter (Atoms.Attachment.attachment attachmentId)
                        setEditing false
                    }),
                [|
                    box setEditing
                    box attachmentId
                |]
            )

        Listener.useKeyPress
            [|
                "Escape"
            |]
            (fun _ _ e -> promise { if e.key = "Escape" && e.``type`` = "keydown" then do! reset () })

        let onSave =
            Store.useCallback (
                (fun getter setter () ->
                    promise {
                        let attachment = Store.getTempValue getter (Atoms.Attachment.attachment attachmentId)

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


        UI.stack
            (fun x ->
                x.flex <- "1"
                x.spacing <- "6px"
                x.paddingTop <- "12px"
                x.paddingBottom <- "12px"
                x.borderBottomWidth <- "1px"
                x.borderBottomColor <- "gray.16")
            [
                AttachmentHeader.AttachmentHeader attachmentPanelType onDelete onEdit attachmentId

                match attachment with
                | Some (Attachment.Image fileId) -> FileThumbnail.FileThumbnail fileId
                | Some (Attachment.Comment (Comment.Comment comment)) ->
                    UI.box
                        (fun _ -> ())
                        [
                            if not editing then
                                AttachmentComment.AttachmentComment comment
                            else
                                UI.flex
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
                                                            Some (Store.InputScope.Temp Gun.defaultSerializer)

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

                                        UI.stack
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
                                                        x.icon <- Icons.fi.FiSave |> Icons.render
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
                                    | Attachment.Comment (Comment.Comment comment) ->
                                        AttachmentComment.AttachmentComment comment
                                    | _ -> nothing)
                    ]

                    nothing
                | _ -> str "???"
            ]
