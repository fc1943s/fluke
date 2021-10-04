namespace Fluke.UI.Frontend.Components

open System
open Fable.React
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.State.State
open FsStore
open FsStore.Hooks
open FsStore.Bindings
open FsStore.Model
open FsUi.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State
open FsCore
open Feliz
open Fluke.Shared.Domain
open FsUi.Components


module AddAttachmentInput =
    let emptyAttachmentAtomReference = AtomReference.Atom (Atoms.Attachment.attachment (AttachmentId Guid.Empty))

    [<ReactComponent>]
    let rec AddAttachmentInput attachmentParent onAdd =
        let archive = Store.useValue Atoms.User.archive
        let ctrlPressed = Store.useValue Atoms.Session.ctrlPressed

        let tempAttachment = Store.useTempState emptyAttachmentAtomReference

        let clipboardVisible, setClipboardVisible = Store.useState Atoms.User.clipboardVisible
        let clipboardAttachmentIdMap = Store.useValue Atoms.User.clipboardAttachmentIdMap

        let addAttachment =
            Store.useCallbackRef
                (fun getter setter _ ->
                    promise {
                        match onAdd, tempAttachment.Value with
                        | Some onAdd, Some (Attachment.Comment (Comment.Comment (String.Valid _)) as attachment) ->

                            let attachmentId =
                                Hydrate.hydrateAttachmentState
                                    getter
                                    setter
                                    (AtomScope.Current,
                                     attachmentParent,
                                     {
                                         Timestamp = DateTime.Now |> FlukeDateTime.FromDateTime
                                         Archived =
                                             match attachmentParent with
                                             | AttachmentParent.Information _ -> archive |> Option.defaultValue false
                                             | _ -> false
                                         Attachment = attachment
                                     })

                            do! onAdd attachmentId

                            // TempValue.reset setter (Atoms.Attachment.attachment (AttachmentId Guid.Empty))
                            tempAttachment.SetValue (Some (Attachment.Comment (Comment.Comment "")))
                        | _ -> ()
                    })

        Ui.stack
            (fun x ->
                x.spacing <- "0"
                x.flex <- "1")
            [
                Ui.flex
                    (fun _ -> ())
                    [

                        Tooltip.wrap
                            (if onAdd.IsNone then str "No database selected" else nothing)
                            [
                                Input.LeftIconInput
                                    {|
                                        Icon = Icons.fi.FiPaperclip |> Icons.render
                                        CustomProps =
                                            fun x ->
                                                x.textarea <- true

                                                x.atom <-
                                                    Some (
                                                        InputAtom (
                                                            AtomReference.Atom (
                                                                Atoms.Attachment.attachment (AttachmentId Guid.Empty)
                                                            )
                                                        )
                                                    )

                                                x.inputScope <- Some (InputScope.Temp Gun.defaultSerializer)
                                                x.autoFocusOnAllMounts <- true
                                                x.variableHeight <- true

                                                x.onEnterPress <-
                                                    Some (fun _ -> promise { if ctrlPressed then do! addAttachment () })

                                                x.onFormat <-
                                                    Some
                                                        (fun attachment ->
                                                            match attachment with
                                                            | Some (Attachment.Comment (Comment.Comment comment)) ->
                                                                comment
                                                            | attachment -> $"{attachment}")

                                                x.onValidate <-
                                                    Some (
                                                        fst
                                                        >> Comment.Comment
                                                        >> Attachment.Comment
                                                        >> Some
                                                        >> Some
                                                    )
                                        Props =
                                            fun x ->
                                                x.placeholder <- "Add Attachment"
                                                x.autoFocus <- true
                                                x.maxHeight <- "200px"
                                                x.borderBottomRightRadius <- "0"
                                                x.borderTopRightRadius <- "0"

                                                if onAdd.IsNone then x.disabled <- true
                                    |}
                            ]

                        Ui.stack
                            (fun x ->
                                x.spacing <- "0"
                                x.paddingTop <- "1px"
                                x.paddingBottom <- "1px")
                            [
                                Button.Button
                                    {|
                                        Tooltip = None
                                        Icon = Some (Icons.fa.FaPlus |> Icons.render, Button.IconPosition.Left)
                                        Props =
                                            fun x ->
                                                Ui.setTestId x "Add Attachment"
                                                x.borderBottomLeftRadius <- "0"
                                                x.borderTopLeftRadius <- "0"
                                                if onAdd.IsNone then x.disabled <- true

                                                x.onClick <- fun _ -> addAttachment ()
                                        Children = []
                                    |}

                                Ui.spacer (fun _ -> ()) []

                                Tooltip.wrap
                                    (str "Clipboard")
                                    [
                                        Ui.box
                                            (fun _ -> ())
                                            [
                                                Button.Button
                                                    {|
                                                        Tooltip = None
                                                        Icon =
                                                            Some (
                                                                Icons.io5.IoDocumentAttachOutline |> Icons.render,
                                                                Button.IconPosition.Left
                                                            )
                                                        Props =
                                                            fun x ->
                                                                Ui.setTestId x "Clipboard"

                                                                if onAdd.IsNone then x.disabled <- true
                                                                x.borderBottomLeftRadius <- "0"
                                                                x.borderTopLeftRadius <- "0"

                                                                x.onClick <-
                                                                    fun _ ->
                                                                        promise {
                                                                            setClipboardVisible (not clipboardVisible) }
                                                        Children =
                                                            [
                                                                Ui.str (string clipboardAttachmentIdMap.Count)
                                                            ]
                                                    |}
                                            ]
                                    ]
                            ]
                    ]
                match onAdd with
                | Some onAdd -> AttachmentsClipboard.AttachmentsClipboard attachmentParent onAdd
                | None -> nothing
            ]
