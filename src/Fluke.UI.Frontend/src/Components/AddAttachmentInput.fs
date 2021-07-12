namespace Fluke.UI.Frontend.Components

open System
open Browser.Types
open Fable.React
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State
open Feliz
open Fluke.Shared.Domain


module AddAttachmentInput =
    [<ReactComponent>]
    let ImageModal uiFlagType uiFlagValue title url =
        ModalFlag.ModalFlagBundle
            {|
                UIFlagType = uiFlagType
                UIFlagValue = uiFlagValue
                Trigger =
                    fun trigger _ ->
                        UI.box
                            (fun x ->
                                x.``as`` <- "img"
                                x.cursor <- "pointer"
                                x.title <- title
                                x.onClick <- fun _ -> promise { do! trigger () }
                                x.src <- url)
                            []
                Content =
                    fun onHide _ ->
                        UI.box
                            (fun _ -> ())
                            [
                                UI.box
                                    (fun x ->
                                        x.``as`` <- "img"
                                        x.cursor <- "pointer"
                                        x.title <- title
                                        x.onClick <- fun _ -> promise { do! onHide () }
                                        x.src <- url)
                                    []
                                str title
                            ]
            |}

    [<ReactComponent>]
    let FileThumbnail fileId =
        let objectUrl = Store.useValue (Selectors.File.objectUrl fileId)

        UI.flex
            (fun x ->
                x.width <- "75px"
                x.height <- "75px"
                x.justifyContent <- "center"
                x.borderWidth <- "1px"
                x.borderColor <- "gray.16"
                x.alignItems <- "center")
            [
                match objectUrl with
                | Some url -> ImageModal UIFlagType.File (UIFlag.File fileId) $"File ID: {fileId |> FileId.Value}" url
                | None -> LoadingSpinner.InlineLoadingSpinner ()
            ]

    [<ReactComponent>]
    let TempFileThumbnail onDelete onAdd fileId =

        UI.box
            (fun x -> x.position <- "relative")
            [
                FileThumbnail fileId

                UI.stack
                    (fun x ->
                        x.direction <- "row"
                        x.spacing <- "2px"
                        x.position <- "absolute"
                        x.bottom <- "1px"
                        x.right <- "1px")
                    [
                        InputLabelIconButton.InputLabelIconButton
                            (fun x ->
                                x.icon <- Icons.bs.BsTrash |> Icons.render
                                x.margin <- "0"
                                x.fontSize <- "11px"
                                x.height <- "15px"
                                x.color <- "whiteAlpha.700"
                                x.onClick <- fun _ -> onDelete ())

                        InputLabelIconButton.InputLabelIconButton
                            (fun x ->
                                x.icon <- Icons.fi.FiSave |> Icons.render
                                x.margin <- "0"
                                x.fontSize <- "11px"
                                x.height <- "15px"
                                x.color <- "whiteAlpha.700"
                                x.onClick <- fun _ -> onAdd ())
                    ]
            ]

    [<ReactComponent>]
    let AttachmentThumbnail onDelete onAdd attachmentId =
        let attachment = Store.useValue (Selectors.Attachment.attachment attachmentId)

        match attachment with
        | Some (_moment, attachment) ->
            match attachment with
            | Attachment.Image fileId -> TempFileThumbnail onDelete onAdd fileId
            | _ -> nothing
        | _ -> nothing


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
                                    AttachmentThumbnail
                                        (fun () -> onDelete attachmentId)
                                        (fun () -> onAdd attachmentId)
                                        attachmentId
                                ])
            ]


    [<ReactComponent>]
    let rec AttachmentsClipboard onAdd =
        let clipboardVisible = Store.useValue Atoms.User.clipboardVisible
        let clipboardAttachmentIdMap = Store.useValue Atoms.User.clipboardAttachmentIdMap

        let deleteImageAttachment =
            Store.useCallback (
                (fun getter setter attachmentId ->
                    promise {
                        Store.change setter Atoms.User.clipboardAttachmentIdMap (Map.remove attachmentId)
                        do! Store.deleteRoot getter (Atoms.Attachment.attachment attachmentId)
                    }),
                [||]
            )

        let addImageAttachment =
            Store.useCallback (
                (fun _ setter attachmentId ->
                    promise {
                        Store.change setter Atoms.User.clipboardAttachmentIdMap (Map.remove attachmentId)
                        do! onAdd attachmentId
                    }),
                [|
                    box onAdd
                |]
            )

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
                        AttachmentList
                            deleteImageAttachment
                            addImageAttachment
                            (clipboardAttachmentIdMap |> Map.keys |> Seq.toList)
                ]

    [<ReactComponent>]
    let rec AddAttachmentInput onAdd =
        let ctrlPressed = Store.useValue Atoms.ctrlPressed
        let addAttachmentText, setAddAttachmentText = React.useState ""

        let clipboardVisible, setClipboardVisible = Store.useState Atoms.User.clipboardVisible
        let clipboardAttachmentIdMap = Store.useValue Atoms.User.clipboardAttachmentIdMap

        let addAttachment =
            Store.useCallback (
                (fun _ setter _ ->
                    promise {
                        match addAttachmentText with
                        | String.ValidString _ ->
                            let attachmentId = AttachmentId.NewId ()

                            Store.set
                                setter
                                (Atoms.Attachment.timestamp attachmentId)
                                (DateTime.Now |> FlukeDateTime.FromDateTime |> Some)

                            Store.set
                                setter
                                (Atoms.Attachment.attachment attachmentId)
                                (addAttachmentText
                                 |> Comment.Comment
                                 |> Attachment.Comment
                                 |> Some)


                            match onAdd with
                            | Some onAdd -> do! onAdd attachmentId
                            | None -> ()

                            setAddAttachmentText ""
                        | _ -> ()
                    }),
                [|
                    box onAdd
                    box addAttachmentText
                    box setAddAttachmentText
                |]
            )

        if true then
            UI.stack
                (fun x -> x.spacing <- "0")
                [
                    UI.flex
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
                                                    x.fixedValue <- Some addAttachmentText
                                                    x.autoFocusOnAllMounts <- true
                                                    x.variableHeight <- true

                                                    x.onEnterPress <-
                                                        Some
                                                            (fun _ ->
                                                                promise { if ctrlPressed then do! addAttachment () })
                                            Props =
                                                fun x ->
                                                    x.placeholder <- "Add Attachment"
                                                    x.autoFocus <- true
                                                    x.maxHeight <- "200px"
                                                    x.borderBottomRightRadius <- "0"
                                                    x.borderTopRightRadius <- "0"

                                                    if onAdd.IsNone then x.disabled <- true

                                                    x.onChange <-
                                                        (fun (e: KeyboardEvent) ->
                                                            promise { setAddAttachmentText e.Value })
                                        |}
                                ]

                            UI.stack
                                (fun x ->
                                    x.spacing <- "0"
                                    x.paddingTop <- "1px"
                                    x.paddingBottom <- "1px")
                                [
                                    Button.Button
                                        {|
                                            Hint = None
                                            Icon = Some (Icons.fa.FaPlus |> Icons.render, Button.IconPosition.Left)
                                            Props =
                                                fun x ->
                                                    UI.setTestId x "Add Attachment"
                                                    //                                                x.borderBottomLeftRadius <- "0"
//                                                x.borderTopLeftRadius <- "0"
                                                    if onAdd.IsNone then x.disabled <- true

                                                    x.onClick <- fun _ -> addAttachment ()
                                            Children = []
                                        |}

                                    UI.spacer (fun _ -> ()) []

                                    Tooltip.wrap
                                        (str "Clipboard")
                                        [
                                            UI.box
                                                (fun _ -> ())
                                                [
                                                    Button.Button
                                                        {|
                                                            Hint = None
                                                            Icon =
                                                                Some (
                                                                    Icons.io5.IoDocumentAttachOutline |> Icons.render,
                                                                    Button.IconPosition.Left
                                                                )
                                                            Props =
                                                                fun x ->
                                                                    UI.setTestId x "Clipboard"

                                                                    if onAdd.IsNone then x.disabled <- true

                                                                    x.onClick <-
                                                                        fun _ ->
                                                                            promise {
                                                                                setClipboardVisible (
                                                                                    not clipboardVisible
                                                                                )
                                                                            }
                                                            Children =
                                                                [
                                                                    UI.box
                                                                        (fun _ -> ())
                                                                        [
                                                                            str (string clipboardAttachmentIdMap.Count)
                                                                        ]
                                                                ]
                                                        |}
                                                ]
                                        ]
                                ]
                        ]
                    match onAdd with
                    | Some onAdd -> AttachmentsClipboard onAdd
                    | None -> nothing
                ]
        else
            Vim.render
                {|
                    OnVimCreated = fun vim -> printfn $"vim {vim}"
                    Props = fun x -> x.height <- "150px"
                    Fallback = fun () -> str "wasm error"
                |}
