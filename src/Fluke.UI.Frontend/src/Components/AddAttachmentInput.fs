namespace Fluke.UI.Frontend.Components

open System
open Fable.Core.JsInterop
open Browser.Types
open Fable.React
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State
open Fable.Core
open Feliz


module AddAttachmentInput =

    [<ReactComponent>]
    let FileThumbnail onDelete fileId =
        let objectUrl = Store.useValue (Selectors.File.objectUrl fileId)

        Chakra.box
            (fun x -> x.position <- "relative")
            [
                Chakra.flex
                    (fun x ->
                        x.width <- "75px"
                        x.height <- "75px"
                        x.justifyContent <- "center"
                        x.borderWidth <- "1px"
                        x.borderColor <- "gray.16"
                        x.alignItems <- "center")
                    [
                        match objectUrl with
                        | Some url ->
                            ModalFlag.ModalFlagBundle
                                {|
                                    UIFlagType = UIFlagType.File
                                    UIFlagValue = UIFlag.File fileId
                                    Trigger =
                                        fun trigger _ ->
                                            Chakra.box
                                                (fun x ->
                                                    x.``as`` <- "img"
                                                    x.cursor <- "pointer"
                                                    x.onClick <- fun _ -> promise { do! trigger () }
                                                    x.src <- url)
                                                []
                                    Content =
                                        fun onHide _ ->
                                            Chakra.box
                                                (fun x ->
                                                    x.``as`` <- "img"
                                                    x.cursor <- "pointer"
                                                    x.onClick <- fun _ -> promise { do! onHide () }
                                                    x.src <- url)
                                                []
                                |}
                        | None -> LoadingSpinner.InlineLoadingSpinner ()
                    ]
                Chakra.box
                    (fun x ->
                        x.position <- "absolute"
                        x.top <- "-1px"
                        x.right <- "1px")
                    [
                        Menu.Menu
                            {|
                                Tooltip = ""
                                Trigger =
                                    InputLabelIconButton.InputLabelIconButton
                                        (fun x ->
                                            x.``as`` <- Chakra.react.MenuButton

                                            x.icon <- Icons.bs.BsThreeDots |> Icons.render

                                            x.margin <- "0"
                                            x.fontSize <- "11px"
                                            x.height <- "15px"
                                            x.color <- "whiteAlpha.700")
                                Body =
                                    [
                                        Chakra.menuItem
                                            (fun x ->
                                                x.closeOnSelect <- true

                                                x.icon <-
                                                    Icons.bi.BiTrash
                                                    |> Icons.renderChakra (fun x -> x.fontSize <- "13px")

                                                x.onClick <- fun _ -> promise { do! onDelete () })
                                            [
                                                str "Delete Attachment"
                                            ]
                                    ]
                                MenuListProps = fun _ -> ()
                            |}
                    ]
            ]

    [<ReactComponent>]
    let AttachmentThumbnail onDelete attachmentId =
        let attachment = Store.useValue (Selectors.Attachment.attachment attachmentId)

        match attachment with
        | Some (_moment, attachment) ->
            match attachment with
            | Attachment.Image fileId -> FileThumbnail onDelete fileId
            | _ -> nothing
        | _ -> nothing

    [<ReactComponent>]
    let rec AddAttachmentInput onAdd =
        let isTesting = Store.useValue Store.Atoms.isTesting
        let ctrlPressed = Store.useValue Atoms.ctrlPressed
        let addAttachmentText, setAddAttachmentText = React.useState ""

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

                            do! onAdd attachmentId

                            setAddAttachmentText ""
                        | _ -> ()
                    }),
                [|
                    box onAdd
                    box addAttachmentText
                    box setAddAttachmentText
                |]
            )

        let clipboardVisible, setClipboardVisible = Store.useState Atoms.User.clipboardVisible
        let clipboardAttachmentSet = Store.useValue Atoms.User.clipboardAttachmentSet


        let deleteAttachment =
            Store.useCallback (
                (fun _getter setter attachmentId ->
                    promise {
                        Store.change setter Atoms.User.clipboardAttachmentSet (Set.remove attachmentId)
                        ()
                    }
                    //                Store.deleteRoot getter (Atoms.Task.databaseId taskId)
                    ),
                [||]
            )


        if true then
            React.fragment [
                Chakra.stack
                    (fun x -> x.spacing <- "0")
                    [
                        if not clipboardVisible then
                            nothing
                        else
                            Chakra.box
                                (fun _ -> ())
                                [
                                    if clipboardAttachmentSet.Count = 0 then
                                        Chakra.box
                                            (fun x -> x.padding <- "10px")
                                            [
                                                str "Empty clipboard"
                                            ]
                                    else
                                        Chakra.stack
                                            (fun x ->
                                                x.direction <- "row"
                                                x.marginBottom <- "5px")
                                            [
                                                yield!
                                                    clipboardAttachmentSet
                                                    |> Set.toList
                                                    |> List.map
                                                        (fun attachmentId ->
                                                            AttachmentThumbnail
                                                                (fun () -> deleteAttachment attachmentId)
                                                                attachmentId)
                                            ]
                                ]

                        Chakra.flex
                            (fun _ -> ())
                            [
                                Input.LeftIconInput
                                    {|
                                        Icon = Icons.fi.FiPaperclip |> Icons.render
                                        CustomProps =
                                            fun x ->
                                                x.textarea <- true
                                                x.fixedValue <- Some addAttachmentText
                                                x.autoFocusOnAllMounts <- true

                                                x.onEnterPress <-
                                                    Some (fun _ -> promise { if ctrlPressed then do! addAttachment () })
                                        Props =
                                            fun x ->
                                                x.placeholder <- "Add Attachment"
                                                x.autoFocus <- true
                                                x.maxHeight <- "200px"
                                                x.borderBottomRightRadius <- "0"
                                                x.borderTopRightRadius <- "0"

                                                x.onChange <-
                                                    (fun (e: KeyboardEvent) -> promise { setAddAttachmentText e.Value })
                                    |}

                                Chakra.stack
                                    (fun x -> x.spacing <- "0")
                                    [
                                        Tooltip.wrap
                                            (if clipboardAttachmentSet.Count = 0 then str "Clipboard" else nothing)
                                            [
                                                Chakra.box
                                                    (fun _ -> ())
                                                    [
                                                        Button.Button
                                                            {|
                                                                Hint = None
                                                                Icon =
                                                                    Some (
                                                                        Icons.io5.IoDocumentAttachOutline |> Icons.wrap,
                                                                        Button.IconPosition.Left
                                                                    )
                                                                Props =
                                                                    fun x ->
                                                                        if isTesting then
                                                                            x?``data-testid`` <- "Clipboard"
                                                                        //                                                x.borderBottomLeftRadius <- "0"
                                                                        //                                                x.borderTopLeftRadius <- "0"
                                                                        x.onClick <-
                                                                            fun _ ->
                                                                                promise {
                                                                                    setClipboardVisible (
                                                                                        not clipboardVisible
                                                                                    )
                                                                                }
                                                                Children =
                                                                    [
                                                                        Chakra.box
                                                                            (fun _ -> ())
                                                                            [
                                                                                str (
                                                                                    string clipboardAttachmentSet.Count
                                                                                )
                                                                            ]
                                                                    ]
                                                            |}
                                                    ]
                                            ]

                                        Chakra.spacer (fun _ -> ()) []

                                        Button.Button
                                            {|
                                                Hint = None
                                                Icon = Some (Icons.fa.FaPlus |> Icons.wrap, Button.IconPosition.Left)
                                                Props =
                                                    fun x ->
                                                        if isTesting then x?``data-testid`` <- "Add Attachment"
                                                        //                                                x.borderBottomLeftRadius <- "0"
//                                                x.borderTopLeftRadius <- "0"
                                                        x.onClick <- fun _ -> addAttachment ()
                                                Children = []
                                            |}
                                    ]
                            ]
                    ]
            ]
        else
            Vim.render
                {|
                    OnVimCreated = fun vim -> printfn $"vim {vim}"
                    Props = fun x -> x.height <- "150px"
                    Fallback = fun () -> str "wasm error"
                |}
