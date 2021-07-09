namespace Fluke.UI.Frontend.Components

open Browser.Types
open Feliz
open Fable.React
open Fluke.Shared.Domain
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.State
open Fable.Extras
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Hooks
open Fable.Core


module AttachmentPanel =
    [<ReactComponent>]
    let AttachmentHeader onDelete onEdit attachmentId =
        let attachment = Store.useValue (Atoms.Attachment.attachment attachmentId)
        let timestamp = Store.useValue (Atoms.Attachment.timestamp attachmentId)

        UI.flex
            (fun x -> x.color <- "whiteAlpha.600")
            [
                UI.box
                    (fun x -> x.lineHeight <- "16px")
                    [
                        match timestamp with
                        | Some timestamp ->
                            UI.box
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
                                                x.``as`` <- UI.react.MenuButton
                                                x.icon <- Icons.bs.BsThreeDots |> Icons.render
                                                x.fontSize <- "11px"
                                                x.height <- "15px"
                                                x.color <- "whiteAlpha.700"
                                                x.marginTop <- "-5px"
                                                x.marginLeft <- "6px")
                                    Body =
                                        [
                                            match attachment with
                                            | Some (Attachment.Comment _) ->
                                                MenuItem.MenuItem Icons.bs.BsPen "Edit Attachment" onEdit (fun _ -> ())
                                            | _ -> nothing

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
        let youtubeIdList =
            React.useMemo (
                (fun () ->
                    (JSe.RegExp (
                        @"((youtu.be\/)|(v\/)|(\/u\/\w\/)|(embed\/)|(watch\?))\??v?=?([^#&?\n ]*)",
                        JSe.RegExpFlag().i.g
                    ))
                        .MatchAll text
                    |> Seq.choose
                        (fun matches ->
                            let matches = matches |> Seq.toList
                            if matches.Length = 8 then Some matches.[7] else None)
                    |> Seq.toList),
                [|
                    box text
                |]
            )

        let youtubeImgList =
            React.useMemo (
                (fun () ->
                    youtubeIdList
                    |> List.map (fun youtubeVideoId -> $"https://img.youtube.com/vi/{youtubeVideoId}/maxresdefault.jpg")),
                [|
                    box youtubeIdList
                |]
            )

        let youtubeMetadataMap, setYoutubeMetadataMap = React.useState Map.empty

        React.useEffect (
            (fun () ->
                async {
                    let! newMetadataList =
                        youtubeIdList
                        |> List.map
                            (fun youtubeId ->
                                $"https://www.youtube.com/oembed?url=https://www.youtube.com/watch?v={youtubeId}")
                        |> List.map Fable.SimpleHttp.Http.get
                        |> Async.Parallel

                    if newMetadataList.Length > 0 then
                        newMetadataList
                        |> Array.map
                            (fun (code, content) ->
                                if code <> 200 then
                                    None
                                else
                                    Some (content |> Json.decode<{| title: string |}>))
                        |> Array.mapi
                            (fun i metadata ->
                                match metadata with
                                | Some metadata -> Some (youtubeImgList.[i], metadata)
                                | None -> None)
                        |> Array.choose id
                        |> Map.ofArray
                        |> setYoutubeMetadataMap
                }
                |> Async.StartAsPromise
                |> Promise.start),
            [|
                box youtubeIdList
                box setYoutubeMetadataMap
                box youtubeImgList
            |]
        )

        UI.box
            (fun x ->
                x.userSelect <- "text"
                x.overflow <- "auto"
                x.paddingTop <- "2px"
                x.paddingBottom <- "2px"
                x.maxHeight <- "50vh")
            [
                Markdown.render text

                match youtubeImgList with
                | [] -> nothing
                | youtubeImgList ->
                    UI.flex
                        (fun x ->
                            x.marginTop <- "10px"
                            x.overflow <- "auto")
                        [
                            yield!
                                youtubeImgList
                                |> List.map
                                    (fun url ->
                                        UI.flex
                                            (fun x ->
                                                x.width <- "75px"
                                                x.height <- "75px"
                                                x.justifyContent <- "center"
                                                x.borderWidth <- "1px"
                                                x.borderColor <- "gray.16"
                                                x.alignItems <- "center")
                                            [
                                                AddAttachmentInput.ImageModal
                                                    UIFlagType.RawImage
                                                    (UIFlag.RawImage url)
                                                    (youtubeMetadataMap
                                                     |> Map.tryFind url
                                                     |> Option.map (fun metadata -> metadata.title)
                                                     |> Option.defaultValue "")
                                                    url
                                            ])
                        ]
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
                        Store.resetTemp setter (Atoms.Attachment.attachment attachmentId)
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
                        let attachment = Store.getTemp getter (Atoms.Attachment.attachment attachmentId)

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
                x.spacing <- "6px")
            [
                AttachmentHeader onDelete onEdit attachmentId

                match attachment with
                | Some (Attachment.Image fileId) -> AddAttachmentInput.FileThumbnail fileId
                | Some (Attachment.Comment (Comment.Comment comment)) ->
                    UI.box
                        (fun _ -> ())
                        [
                            if not editing then
                                AttachmentComment comment
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
                        match attachmentIdList with
                        //                                                | None -> LoadingSpinner.LoadingSpinner ()
                        | [] ->
                            UI.box
                                (fun _ -> ())
                                [
                                    str "No attachments found"
                                ]
                        | attachmentIdList ->
                            UI.box
                                (fun _ -> ())
                                [
                                    yield!
                                        attachmentIdList
                                        |> List.map
                                            (fun attachmentId ->
                                                UI.box
                                                    (fun x ->
                                                        x.paddingTop <- "12px"
                                                        x.paddingBottom <- "12px"
                                                        x.borderBottomWidth <- "1px"
                                                        x.borderBottomColor <- "gray.16")
                                                    [
                                                        Attachment (fun () -> onDelete attachmentId) attachmentId
                                                    ])
                                ]
                    ]

                AddAttachmentInput.AddAttachmentInput onAdd
            ]
