namespace Fluke.UI.Frontend.Components

open System
open Fable.React
open Feliz
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module TaskName =
    open Domain.Model

    [<ReactComponent>]
    let TaskName (input: {| TaskId: TaskId |}) =
        let ref = React.useElementRef ()
        let hovered = Listener.useElementHover ref
        let hasSelection = Store.useValue (Selectors.Task.hasSelection input.TaskId)
        let taskState = Store.useValue (Selectors.Task.taskState input.TaskId)
        let cellSize = Store.useValue Atoms.cellSize
        let isReadWrite = Store.useValue (Selectors.Task.isReadWrite input.TaskId)

        let editTask =
            Store.useCallback (
                (fun getter setter _ ->
                    promise {
                        let deviceInfo = Store.value getter Selectors.deviceInfo
                        if deviceInfo.IsMobile then Store.set setter Atoms.leftDock None
                        Store.set setter Atoms.rightDock (Some TempUI.DockType.Task)
                        let databaseId = Store.value getter (Selectors.Task.databaseId input.TaskId)

                        Store.set
                            setter
                            (Atoms.uiFlag Atoms.UIFlagType.Task)
                            (Atoms.UIFlag.Task (databaseId, input.TaskId))

                    }),
                [|
                    box input.TaskId
                |]
            )

        let startSession =
            Store.useCallback (
                (fun getter setter _ ->
                    promise {
                        let sessions = Store.value getter (Atoms.Task.sessions input.TaskId)

                        Store.set
                            setter
                            (Atoms.Task.sessions input.TaskId)
                            (Session (
                                (let now = DateTime.Now in if now.Second < 30 then now else now.AddMinutes 1.)
                                |> FlukeDateTime.FromDateTime
                             )
                             :: sessions)
                    }),
                [|
                    box input.TaskId
                |]
            )

        let deleteTask = Store.useCallback ((fun _ _ _ -> promise { () }), [||])

        Chakra.flex
            (fun x ->
                x.flex <- "1"
                x.alignItems <- "center"
                x.ref <- ref
                x.position <- "relative"
                x.height <- $"{cellSize}px")
            [
                Chakra.box
                    (fun x ->
                        x.backgroundColor <- if hovered then "#292929" else null
                        x.color <- if hasSelection then "#ff5656" else null
                        x.zIndex <- if hovered then 1 else 0
                        x.overflow <- "hidden"
                        x.paddingLeft <- "5px"
                        x.paddingRight <- "5px"
                        x.lineHeight <- $"{cellSize}px"
                        x.whiteSpace <- "nowrap"
                        x.textOverflow <- "ellipsis")
                    [
                        //                        match taskState with
//                        | Some taskState when taskState.Task.Name |> TaskName.Value <> "" ->
                        taskState.Task.Name |> TaskName.Value |> str
                    ]

                if not isReadWrite then
                    nothing
                else
                    Menu.Menu
                        {|
                            Tooltip = ""
                            Trigger =
                                InputLabelIconButton.InputLabelIconButton
                                    {|
                                        Props =
                                            fun x ->
                                                x.``as`` <- Chakra.react.MenuButton
                                                x.icon <- Icons.bs.BsThreeDots |> Icons.render
                                                x.fontSize <- "11px"
                                                x.height <- "15px"
                                                x.color <- "whiteAlpha.700"
                                                x.display <- if isReadWrite then null else "none"
                                                x.marginTop <- "-1px"
                                                x.marginLeft <- "6px"
                                    |}
                            Body =
                                [
                                    Chakra.menuItem
                                        (fun x ->
                                            x.closeOnSelect <- true

                                            x.icon <-
                                                Icons.bs.BsPen
                                                |> Icons.renderChakra (fun x -> x.fontSize <- "13px")

                                            x.onClick <- editTask)
                                        [
                                            str "Edit Task"
                                        ]

                                    Chakra.menuItem
                                        (fun x ->
                                            x.closeOnSelect <- true

                                            x.icon <-
                                                Icons.gi.GiHourglass
                                                |> Icons.renderChakra (fun x -> x.fontSize <- "13px")

                                            x.onClick <- startSession)
                                        [
                                            str "Start Session"
                                        ]

                                    Chakra.menuItem
                                        (fun x ->
                                            x.closeOnSelect <- true
                                            x.isDisabled <- true

                                            x.icon <-
                                                Icons.bs.BsTrash
                                                |> Icons.renderChakra (fun x -> x.fontSize <- "13px")

                                            x.onClick <- deleteTask)
                                        [
                                            str "Delete Task"
                                        ]
                                ]
                            MenuListProps = fun _ -> ()
                        |}


                //                match taskState.valueMaybe () with
//                | Some taskState ->
                AttachmentIndicator.AttachmentIndicator
                    {|
                        Attachments = taskState.Attachments
                    |}
            //                | None -> nothing
            ]
