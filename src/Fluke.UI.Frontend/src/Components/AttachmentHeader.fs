namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.State.State
open FsStore
open FsUi.Bindings
open Fluke.UI.Frontend.State
open FsUi.Components


module AttachmentHeader =
    [<ReactComponent>]
    let AttachmentHeader attachmentParent onDelete onEdit attachmentId =
        let attachmentState = Store.useValue (Selectors.Attachment.attachmentState attachmentId)
        let setArchived = Store.useSetState (Atoms.Attachment.archived attachmentId)

        let copyAttachmentClipboard =
            Store.useCallbackRef
                (fun _ _ _ ->
                    promise {
                        match Browser.Navigator.navigator.clipboard with
                        | Some clipboard ->
                            match attachmentState with
                            | Some attachmentState ->
                                match attachmentState.Attachment with
                                | Attachment.Comment (Comment.Comment comment) -> do! clipboard.writeText comment
                                | Attachment.Image _fileId ->
                                    printfn "TODO: implement download image then copy to clipboard"
                                | _ -> ()
                            | None -> ()
                        | None -> ()
                    })

        Ui.flex
            (fun x -> x.color <- "whiteAlpha.600")
            [
                Ui.box
                    (fun x -> x.lineHeight <- "16px")
                    [
                        match attachmentState with
                        | Some attachmentState ->
                            Ui.box
                                (fun x ->
                                    x.userSelect <- "text"
                                    x.display <- "inline")
                                [
                                    str (
                                        attachmentState.Timestamp
                                        |> FlukeDateTime.Stringify
                                    )
                                ]

                            InputLabelIconButton.InputLabelIconButton
                                (fun x ->
                                    x.icon <- Icons.hi.HiOutlineClipboard |> Icons.render
                                    x.onClick <- copyAttachmentClipboard
                                    x.fontSize <- "11px"
                                    x.height <- "15px"
                                    x.color <- "whiteAlpha.700"
                                    x.marginTop <- "-5px"
                                    x.marginLeft <- "6px")

                            Menu.Menu
                                {|
                                    Tooltip = ""
                                    Trigger =
                                        InputLabelIconButton.InputLabelIconButton
                                            (fun x ->
                                                x.``as`` <- Ui.react.MenuButton
                                                x.icon <- Icons.bs.BsThreeDots |> Icons.render
                                                x.fontSize <- "11px"
                                                x.height <- "15px"
                                                x.color <- "whiteAlpha.700"
                                                x.marginTop <- "-5px"
                                                x.marginLeft <- "6px")
                                    Body =
                                        [
                                            match attachmentState.Attachment with
                                            | Attachment.Comment _ ->
                                                MenuItem.MenuItem
                                                    Icons.bs.BsPen
                                                    "Edit Attachment"
                                                    (Some onEdit)
                                                    (fun _ -> ())
                                            | _ -> nothing

                                            match attachmentParent with
                                            | AttachmentParent.Information _ ->
                                                MenuItem.MenuItem
                                                    Icons.ri.RiArchiveLine
                                                    $"""{if attachmentState.Archived = true then "Unarchive" else "Archive"} Attachment"""
                                                    (Some
                                                        (fun () ->
                                                            promise {
                                                                setArchived (Some (not attachmentState.Archived)) }))
                                                    (fun _ -> ())
                                            | _ -> nothing

                                            Popover.MenuItemConfirmPopover Icons.bi.BiTrash "Delete Attachment" onDelete
                                        ]
                                    MenuListProps = fun _ -> ()
                                |}
                        | None -> LoadingSpinner.InlineLoadingSpinner ()
                    ]
            ]
