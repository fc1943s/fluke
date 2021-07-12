namespace Fluke.UI.Frontend.Components

open System
open Fable.React
open Feliz
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module TaskName =
    open Domain.Model

    [<ReactComponent>]
    let TaskName taskId =
        //        let ref = React.useElementRef ()
//        let hovered = Listener.useElementHover ref
        let hasSelection = Store.useValue (Selectors.Task.hasSelection taskId)

        let name = Store.useValue (Atoms.Task.name taskId)
        let archived, setArchived = Store.useState (Atoms.Task.archived taskId)
        let attachmentIdSet = Store.useValue (Atoms.Task.attachmentIdSet taskId)
        let cellSize = Store.useValue Atoms.User.cellSize
        let isReadWrite = Store.useValue (Selectors.Task.isReadWrite taskId)

        let editTask =
            Store.useCallback (
                (fun getter setter _ ->
                    promise {
                        let deviceInfo = Store.value getter Selectors.deviceInfo
                        if deviceInfo.IsMobile then Store.set setter Atoms.User.leftDock None
                        Store.set setter Atoms.User.rightDock (Some TempUI.DockType.Task)
                        let databaseId = Store.value getter (Atoms.Task.databaseId taskId)
                        Store.set setter (Atoms.User.uiFlag UIFlagType.Task) (UIFlag.Task (databaseId, taskId))
                    }),
                [|
                    box taskId
                |]
            )

        let startSession =
            Store.useCallback (
                (fun getter setter _ ->
                    promise {
                        let sessions = Store.value getter (Atoms.Task.sessions taskId)

                        Store.set
                            setter
                            (Atoms.Task.sessions taskId)
                            (Session (
                                (let now = DateTime.Now in if now.Second < 30 then now else now.AddMinutes 1.)
                                |> FlukeDateTime.FromDateTime
                             )
                             :: sessions)
                    }),
                [|
                    box taskId
                |]
            )

        let deleteTask =
            Store.useCallback (
                (fun getter _ _ -> Store.deleteRoot getter (Atoms.Task.databaseId taskId)),
                [|
                    box taskId
                |]
            )

        UI.flex
            (fun x ->
                x.flex <- "1"
                x.alignItems <- "center"
                //                x.ref <- ref
                x.position <- "relative"
                x.height <- $"{cellSize}px")
            [
                UI.box
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
                                                    x.``as`` <- UI.react.MenuButton
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
                                                    (Some editTask)
                                                    (fun _ -> ())

                                                MenuItem.MenuItem
                                                    Icons.ri.RiArchiveLine
                                                    $"""{if archived = Some true then "Unarchive" else "Archive"} Task"""
                                                    (Some
                                                        (fun () -> promise { setArchived (archived |> Option.map not) }))
                                                    (fun _ -> ())

                                                MenuItem.MenuItem
                                                    Icons.gi.GiHourglass
                                                    "Start Session"
                                                    (Some startSession)
                                                    (fun _ -> ())

                                                ConfirmPopover.ConfirmPopover
                                                    ConfirmPopover.ConfirmPopoverType.MenuItem
                                                    Icons.bi.BiTrash
                                                    "Delete Task"
                                                    deleteTask
                                            ]
                                        MenuListProps = fun _ -> ()
                                    |}
                    ]

                if not attachmentIdSet.IsEmpty then
                    AttachmentIndicator.AttachmentIndicator ()
                else
                    nothing
            ]
