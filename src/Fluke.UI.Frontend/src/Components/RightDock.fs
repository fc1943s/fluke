namespace Fluke.UI.Frontend.Components

open FsJs
open Feliz
open Fable.React
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open FsStore
open FsStore.Hooks
open FsUi.Bindings
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.State.State
open FsUi.Components


module RightDock =

    [<ReactComponent>]
    let RightDock () =
        let deviceInfo = Store.useValue Selectors.deviceInfo
        let setLeftDock = Store.useSetState Atoms.User.leftDock
        let rightDock = Store.useValue Atoms.User.rightDock

        let rightDockSize, setRightDockSize = Store.useState Atoms.User.rightDockSize
        let informationUIFlag = Store.useValue (Atoms.User.uiFlag UIFlagType.Information)
        let taskUIFlag = Store.useValue (Atoms.User.uiFlag UIFlagType.Task)

        let information =
            match informationUIFlag with
            | UIFlag.Information information -> Some information
            | _ -> None

        let items =
            React.useMemo (
                (fun () ->
                    [
                        TempUI.DockType.Database,
                        {|
                            Name = "Database"
                            Icon = Icons.fi.FiDatabase
                            Content = DatabaseForm.DatabaseFormWrapper
                            RightIcons = []
                        |}

                        TempUI.DockType.Information,
                        {|
                            Name = "Information"
                            Icon = Icons.bs.BsListNested
                            Content = InformationForm.InformationFormWrapper
                            RightIcons =
                                [
                                    DockPanel.DockPanelIcon.Component (TaskForm.AddTaskButton information)
                                ]
                        |}

                        TempUI.DockType.Task,
                        {|
                            Name = "Task"
                            Icon = Icons.bs.BsListTask
                            Content = TaskForm.TaskFormWrapper
                            RightIcons =
                                [
                                    match taskUIFlag with
                                    | UIFlag.Task (_databaseId, taskId) when taskId <> Task.Default.Id ->
                                        yield DockPanel.DockPanelIcon.Component (TaskForm.AddTaskButton None)
                                    | _ -> ()
                                ]
                        |}

                        TempUI.DockType.Cell,
                        {|
                            Name = "Cell"
                            Icon = Icons.fa.FaCalendarCheck
                            Content = CellForm.CellFormWrapper
                            RightIcons = []
                        |}

                        TempUI.DockType.Search,
                        {|
                            Name = "Search"
                            Icon = Icons.bs.BsSearch
                            Content = SearchForm.SearchForm
                            RightIcons = []
                        |}

                        TempUI.DockType.Filter,
                        {|
                            Name = "Filter"
                            Icon = Icons.fi.FiFilter
                            Content = FilterForm.FilterForm
                            RightIcons = []
                        |}
                    ]),
                [|
                    box taskUIFlag
                    box information
                |]
            )

        let itemsMap = items |> Map.ofSeq

        Ui.flex
            (fun x ->
                x.overflowY <- "auto"
                x.overflowX <- "hidden")
            [
                match rightDock with
                | None -> nothing
                | Some rightDock ->
                    match itemsMap |> Map.tryFind rightDock with
                    | None -> nothing
                    | Some item ->
                        Resizable.resizable
                            {|
                                size = {| width = $"{rightDockSize}px" |}
                                onResizeStop =
                                    fun _e _direction _ref (d: {| width: int |}) ->
                                        setRightDockSize (rightDockSize + d.width)
                                minWidth = "200px"
                                enable =
                                    {|
                                        top = false
                                        right = false
                                        bottom = false
                                        left = true
                                        topRight = false
                                        bottomRight = false
                                        bottomLeft = false
                                        topLeft = false
                                    |}
                            |}
                            [
                                Ui.flex
                                    (fun x ->
                                        x.width <-
                                            unbox (
                                                Js.newObj
                                                    (fun (x: Ui.IBreakpoints<string>) ->
                                                        x.``base`` <- "calc(100vw - 52px)"
                                                        x.md <- "auto")
                                            )

                                        x.height <- "100%"
                                        x.borderLeftWidth <- "1px"
                                        x.borderLeftColor <- "gray.16"
                                        x.flex <- "1")
                                    [
                                        DockPanel.DockPanel
                                            {|
                                                Name = item.Name
                                                Icon = item.Icon
                                                RightIcons = item.RightIcons
                                                Atom = Atoms.User.rightDock
                                                children =
                                                    [
                                                        React.suspense (
                                                            [
                                                                item.Content ()
                                                            ],
                                                            LoadingSpinner.LoadingSpinner ()
                                                        )
                                                    ]
                                            |}
                                    ]
                            ]

                Ui.box
                    (fun x ->
                        x.width <- "24px"
                        x.position <- "relative"
                        x.margin <- "1px")
                    [
                        Ui.stack
                            (fun x ->
                                x.spacing <- "1px"
                                x.direction <- "row"
                                x.left <- "0"
                                x.position <- "absolute"
                                x.transform <- "rotate(90deg) translate(-24px, 0%)"
                                x.transformOrigin <- "0 100%"
                                x.height <- "24px")
                            [
                                yield!
                                    items
                                    |> List.map
                                        (fun (dockType, item) ->
                                            DockButton.DockButton
                                                {|
                                                    DockType = dockType
                                                    Name = item.Name
                                                    Icon = item.Icon
                                                    OnClick =
                                                        fun _ ->
                                                            promise { if deviceInfo.IsMobile then setLeftDock None }
                                                    Atom = Atoms.User.rightDock
                                                    Props = fun _ -> ()
                                                |})
                            ]
                    ]
            ]
