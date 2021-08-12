namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State
open FsStore
open FsUi.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.TempUI
open Fluke.UI.Frontend.State.State
open FsUi.Components


module TaskName =
    open Domain.Model

    [<ReactComponent>]
    let TaskName taskIdAtom =
        let taskId = Store.useValue taskIdAtom
        let navigate = Store.useCallbackRef Navigate.navigate
        let hasSelection = Store.useValue (Selectors.Task.hasSelection taskId)
        let name = Store.useValue (Atoms.Task.name taskId)
        let archived, setArchived = Store.useState (Atoms.Task.archived taskId)
        let attachmentIdSet = Store.useValue (Selectors.Task.attachmentIdSet taskId)
        let cellSize = Store.useValue Atoms.User.cellSize
        let databaseId = Store.useValue (Atoms.Task.databaseId taskId)
        let isReadWrite = Store.useValue (Selectors.Database.isReadWrite databaseId)
        let startSession = TaskForm.useStartSession ()
        let deleteTask = TaskForm.useDeleteTask ()

        Ui.flex
            (fun x ->
                x.flex <- "1"
                x.alignItems <- "center"
                //                x.ref <- ref
                x.position <- "relative"
                x.height <- $"{cellSize}px")
            [
                Ui.box
                    (fun x ->
                        //                        x.backgroundColor <- if hovered then "#292929" else null
                        x.color <- if hasSelection then "#ff5656" else null
                        //                        x.zIndex <- if hovered then 1 else 0
                        x.overflow <- "hidden"
                        x.paddingLeft <- "5px"
                        x.paddingRight <- "5px"
                        x.lineHeight <- $"{cellSize}px"
                        x.whiteSpace <- "nowrap"
                        x.textOverflow <- "ellipsis")
                    [
                        match name |> TaskName.Value with
                        | "" -> LoadingSpinner.InlineLoadingSpinner ()
                        | name ->
                            str name

                            if not isReadWrite then
                                nothing
                            else
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
                                                    x.display <- if isReadWrite then null else "none"
                                                    x.marginTop <- "-3px"
                                                    x.marginLeft <- "6px")
                                        Body =
                                            [
                                                MenuItem.MenuItem
                                                    Icons.bs.BsPen
                                                    "Edit Task"
                                                    (Some
                                                        (fun () ->
                                                            navigate (
                                                                Navigate.DockPosition.Right,
                                                                Some DockType.Task,
                                                                UIFlagType.Task,
                                                                UIFlag.Task (databaseId, taskId)
                                                            )))
                                                    (fun _ -> ())

                                                MenuItem.MenuItem
                                                    Icons.gi.GiHourglass
                                                    "Start Session"
                                                    (Some (fun () -> startSession taskId))
                                                    (fun _ -> ())

                                                MenuItem.MenuItem
                                                    Icons.ri.RiArchiveLine
                                                    $"""{if archived = Some true then "Unarchive" else "Archive"} Task"""
                                                    (Some
                                                        (fun () ->
                                                            promise {
                                                                setArchived (
                                                                    match archived with
                                                                    | Some archived -> Some (not archived)
                                                                    | None -> Some false
                                                                )
                                                            }))
                                                    (fun _ -> ())

                                                Popover.MenuItemConfirmPopover
                                                    Icons.bi.BiTrash
                                                    "Delete Task"
                                                    (fun () -> deleteTask taskId)
                                            ]
                                        MenuListProps = fun _ -> ()
                                    |}
                    ]

                if not attachmentIdSet.IsEmpty then
                    AttachmentIndicator.AttachmentIndicator ()
                else
                    nothing
            ]
