namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Components


module AttachmentHeader =
    [<ReactComponent>]
    let AttachmentHeader attachmentPanelType onDelete onEdit attachmentId =
        let attachmentState = Store.useValue (Selectors.Attachment.attachmentState attachmentId)
        let setArchived = Store.useSetState (Atoms.Attachment.archived attachmentId)

        UI.flex
            (fun x -> x.color <- "whiteAlpha.600")
            [
                UI.box
                    (fun x -> x.lineHeight <- "16px")
                    [
                        match attachmentState with
                        | Some attachmentState ->
                            UI.box
                                (fun x ->
                                    x.userSelect <- "text"
                                    x.display <- "inline")
                                [
                                    str (
                                        attachmentState.Timestamp
                                        |> FlukeDateTime.Stringify
                                    )
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
                                            match attachmentState.Attachment with
                                            | Attachment.Comment _ ->
                                                MenuItem.MenuItem
                                                    Icons.bs.BsPen
                                                    "Edit Attachment"
                                                    (Some onEdit)
                                                    (fun _ -> ())
                                            | _ -> nothing

                                            match attachmentPanelType with
                                            | AddAttachmentInput.AttachmentPanelType.Information ->
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
