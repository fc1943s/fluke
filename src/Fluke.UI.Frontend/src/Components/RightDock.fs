namespace Fluke.UI.Frontend.Components

open Fable.Core
open Feliz
open Fable.React
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State


module RightDock =

    [<ReactComponent>]
    let RightDock () =
        let deviceInfo = Store.useValue Selectors.deviceInfo
        let setLeftDock = Store.useSetState Atoms.leftDock
        let rightDock, setRightDock = Store.useState Atoms.rightDock
        let databaseUIFlag = Store.useValue (Atoms.uiFlag Atoms.UIFlagType.Database)
        let taskUIFlag = Store.useValue (Atoms.uiFlag Atoms.UIFlagType.Task)
        let hydrateDatabase = Hydrate.useHydrateDatabase ()
        let hydrateTaskState = Hydrate.useHydrateTaskState ()

        let taskDatabaseId =
            match taskUIFlag with
            | Atoms.UIFlag.Task (databaseId, _) -> databaseId
            | _ -> Database.Default.Id

        let setDatabaseIdSet = Store.useSetStatePrev Atoms.databaseIdSet
        let setTaskIdSet = Store.useSetStatePrev (Atoms.Database.taskIdSet taskDatabaseId)

        let items =
            React.useMemo (
                (fun () ->
                    [
                        TempUI.DockType.Database,
                        {|
                            Name = "Database"
                            Icon = Icons.fi.FiDatabase
                            Content =
                                fun () ->
                                    Chakra.flex
                                        (fun x ->
                                            x.direction <- "column"
                                            x.flex <- "1"
                                            x.overflowY <- "auto"
                                            x.padding <- "15px"
                                            x.flexBasis <- 0)
                                        [
                                            let databaseId =
                                                match databaseUIFlag with
                                                | Atoms.UIFlag.Database databaseId -> databaseId
                                                | _ -> Database.Default.Id

                                            DatabaseForm.DatabaseForm
                                                {|
                                                    DatabaseId = databaseId
                                                    OnSave =
                                                        fun database ->
                                                            promise {
                                                                do! hydrateDatabase (Store.AtomScope.ReadOnly, database)

                                                                if database.Id <> databaseId then
                                                                    JS.setTimeout
                                                                        (fun () ->
                                                                            setDatabaseIdSet (Set.add database.Id))
                                                                        0
                                                                    |> ignore

                                                                setRightDock None
                                                            }
                                                |}
                                        ]
                            RightIcons = []
                        |}

                        TempUI.DockType.Information,
                        {|
                            Name = "Information"
                            Icon = Icons.bs.BsListNested
                            Content =
                                fun () ->
                                    Chakra.flex
                                        (fun x ->
                                            x.direction <- "column"
                                            x.flex <- "1"
                                            x.overflowY <- "auto"
                                            x.flexBasis <- 0)
                                        [
                                            InformationForm.InformationForm ()
                                        ]
                            RightIcons = []
                        |}

                        TempUI.DockType.Task,
                        {|
                            Name = "Task"
                            Icon = Icons.bs.BsListTask
                            Content =
                                fun () ->
                                    Chakra.flex
                                        (fun x ->
                                            x.direction <- "column"
                                            x.flex <- "1"
                                            x.overflowY <- "auto"
                                            x.padding <- "15px"
                                            x.flexBasis <- 0)
                                        [
                                            let taskId =
                                                match taskUIFlag with
                                                | Atoms.UIFlag.Task (_, taskId) -> taskId
                                                | _ -> Task.Default.Id

                                            TaskForm.TaskForm
                                                {|
                                                    TaskId = taskId
                                                    OnSave =
                                                        fun task ->
                                                            promise {
                                                                let taskState =
                                                                    {
                                                                        Task = task
                                                                        SortList = []
                                                                        Sessions = []
                                                                        Attachments = []
                                                                        CellStateMap = Map.empty
                                                                    }

                                                                do!
                                                                    hydrateTaskState (
                                                                        Store.AtomScope.ReadOnly,
                                                                        taskDatabaseId,
                                                                        taskState
                                                                    )

                                                                if task.Id <> taskId then
                                                                    JS.setTimeout
                                                                        (fun () -> setTaskIdSet (Set.add task.Id))
                                                                        0
                                                                    |> ignore

                                                                setRightDock None
                                                            }
                                                |}
                                        ]
                            RightIcons = []
                        |}

                        TempUI.DockType.Cell,
                        {|
                            Name = "Cell"
                            Icon = Icons.fa.FaCalendarCheck
                            Content =
                                fun () ->
                                    Chakra.flex
                                        (fun x ->
                                            x.direction <- "column"
                                            x.flex <- "1"
                                            x.overflowY <- "auto"
                                            x.flexBasis <- 0)
                                        [
                                            CellForm.CellFormWrapper ()
                                        ]
                            RightIcons = []
                        |}
                    ]),
                [|
                    box databaseUIFlag
                    box hydrateDatabase
                    box hydrateTaskState
                    box setDatabaseIdSet
                    box setRightDock
                    box setTaskIdSet
                    box taskDatabaseId
                    box taskUIFlag
                |]
            )

        let itemsMap = items |> Map.ofSeq

        Chakra.flex
            (fun x -> x.overflow <- "auto")
            [
                match rightDock with
                | None -> nothing
                | Some rightDock ->
                    match itemsMap |> Map.tryFind rightDock with
                    | None -> nothing
                    | Some item ->
                        //                        Resizable.resizable
//                            {|
//                                defaultSize = {| width = "300px" |}
//                                minWidth = "300px"
//                                enable =
//                                    {|
//                                        top = false
//                                        right = false
//                                        bottom = false
//                                        left = true
//                                        topRight = false
//                                        bottomRight = false
//                                        bottomLeft = false
//                                        topLeft = false
//                                    |}
//                            |}
//                            [
                        Chakra.flex
                            (fun x ->
                                x.width <-
                                    unbox (
                                        JS.newObj
                                            (fun (x: Chakra.IBreakpoints<string>) ->
                                                x.``base`` <- "calc(100vw - 50px)"
                                                x.md <- "300px")
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
                                        Atom = Atoms.rightDock
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
                //                            ]

                Chakra.box
                    (fun x ->
                        x.width <- "24px"
                        x.position <- "relative")
                    [
                        Chakra.flex
                            (fun x ->
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
                                                    Atom = Atoms.rightDock
                                                    Props = fun _ -> ()
                                                |})
                            ]
                    ]
            ]
