namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.TempUI


module TaskName =
    open Domain.Model

    [<ReactComponent>]
    let TaskName (input: {| Username: Username; TaskId: TaskId |}) =
        let ref = React.useElementRef ()
        let hovered = Listener.useElementHover ref
        let hasSelection = Recoil.useValueLoadableDefault (Selectors.Task.hasSelection input.TaskId) false
        let (TaskName taskName) = Recoil.useValue (Atoms.Task.name (input.Username, input.TaskId))
        let taskState = Recoil.useValue (Selectors.Task.taskState (input.Username, input.TaskId))
        let cellSize = Recoil.useValue (Atoms.User.cellSize input.Username)

        let isReadWrite =
            Recoil.useValueLoadableDefault (Selectors.Task.isReadWrite (input.Username, input.TaskId)) false

        let deviceInfo = Recoil.useValue Selectors.deviceInfo
        let setLeftDock = Recoil.useSetState (Atoms.User.leftDock input.Username)
        let setRightDock = Recoil.useSetState (Atoms.User.rightDock input.Username)
        let setTaskFormIdFlag = Recoil.useSetState (Atoms.User.formIdFlag (input.Username, TextKey (nameof TaskForm)))

        Chakra.flex
            (fun x ->
                x.flex <- "1"
                x.alignItems <- "center"
                x.ref <- ref
                x.position <- "relative"
                x.height <- $"{cellSize}px"
                x.zIndex <- if hovered then 1 else 0)
            [
                Chakra.box
                    (fun x ->
                        x.color <- if hasSelection then "#ff5656" else null
                        x.overflow <- "hidden"
                        x.paddingLeft <- "5px"
                        x.paddingRight <- "5px"
                        x.lineHeight <- $"{cellSize}px"
                        x.backgroundColor <- if hovered then "#333" else null
                        x.whiteSpace <- if not hovered then "nowrap" else null
                        x.textOverflow <- if not hovered then "ellipsis" else null)
                    [
                        str taskName
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
                                                x.marginLeft <- "6px"
                                    |}
                            Body =
                                [
                                    Chakra.menuItem
                                        (fun x ->
                                            x.closeOnSelect <- true

                                            x.icon <-
                                                Icons.bs.BsPen
                                                |> Icons.renderChakra
                                                    (fun x ->
                                                        x.fontSize <- "13px"
                                                        x.marginTop <- "-1px")

                                            x.onClick <-
                                                fun _ ->
                                                    promise {
                                                        if deviceInfo.IsMobile then setLeftDock None

                                                        setRightDock (Some DockType.Task)
                                                        setTaskFormIdFlag (input.TaskId |> TaskId.Value |> Some)
                                                    })
                                        [
                                            str "Edit Task"
                                        ]

                                    Chakra.menuItem
                                        (fun x ->
                                            x.closeOnSelect <- true

                                            x.icon <-
                                                Icons.bs.BsTrash
                                                |> Icons.renderChakra
                                                    (fun x ->
                                                        x.fontSize <- "13px"
                                                        x.marginTop <- "-1px")

                                            x.onClick <- fun e -> promise { e.preventDefault () })
                                        [
                                            str "Delete Task"
                                        ]
                                ]
                            MenuListProps = fun _ -> ()
                        |}

                TooltipPopup.TooltipPopup
                    {|
                        Username = input.Username
                        Attachments = taskState.Attachments
                    |}
            ]
