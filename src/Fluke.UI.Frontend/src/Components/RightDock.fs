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
    open Domain.UserInteraction

    [<ReactComponent>]
    let RightDock (input: {| Username: Username |}) =
        let deviceInfo = Store.useValue Selectors.deviceInfo
        let setLeftDock = Store.useSetState (Atoms.User.leftDock input.Username)
        let rightDock, setRightDock = Store.useState (Atoms.User.rightDock input.Username)
        let databaseFormIdFlag = Store.useValue (Atoms.User.formIdFlag (input.Username, TextKey (nameof DatabaseForm)))
        let taskFormIdFlag = Store.useValue (Atoms.User.formIdFlag (input.Username, TextKey (nameof TaskForm)))
        let hydrateDatabase = Hydrate.useHydrateDatabase ()
        let hydrateTaskState = Hydrate.useHydrateTaskState ()

        let taskDatabaseId =
            Recoil.useValueLoadableDefault
                (Selectors.Task.databaseId (
                    input.Username,
                    taskFormIdFlag
                    |> Option.map TaskId
                    |> Option.defaultValue Task.Default.Id
                ))
                Database.Default.Id

        let setDatabaseIdSet = Store.useSetStatePrev (Atoms.Session.databaseIdSet input.Username)
        let setTaskIdSet = Store.useSetStatePrev (Atoms.Database.taskIdSet (input.Username, taskDatabaseId))

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
                                    Chakra.box
                                        (fun x ->
                                            x.flex <- "1"
                                            x.overflowY <- "auto"
                                            x.padding <- "14px"
                                            x.flexBasis <- 0)
                                        [
                                            let databaseId =
                                                databaseFormIdFlag
                                                |> Option.map DatabaseId
                                                |> Option.defaultValue Database.Default.Id

                                            DatabaseForm.DatabaseForm
                                                {|
                                                    Username = input.Username
                                                    DatabaseId = databaseId
                                                    OnSave =
                                                        fun database ->
                                                            promise {
                                                                do!
                                                                    hydrateDatabase (
                                                                        input.Username,
                                                                        Recoil.AtomScope.ReadOnly,
                                                                        database
                                                                    )

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
                                    Chakra.box
                                        (fun x ->
                                            x.flex <- "1"
                                            x.overflowY <- "auto"
                                            x.flexBasis <- 0)
                                        [
                                            str "Information Form"
                                        ]
                            RightIcons = []
                        |}

                        TempUI.DockType.Task,
                        {|
                            Name = "Task"
                            Icon = Icons.bs.BsListTask
                            Content =
                                fun () ->
                                    Chakra.box
                                        (fun x ->
                                            x.flex <- "1"
                                            x.overflowY <- "auto"
                                            x.padding <- "14px"
                                            x.flexBasis <- 0)
                                        [
                                            let taskId =
                                                taskFormIdFlag
                                                |> Option.map TaskId
                                                |> Option.defaultValue Task.Default.Id

                                            TaskForm.TaskForm
                                                {|
                                                    Username = input.Username
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
                                                                        input.Username,
                                                                        Recoil.AtomScope.ReadOnly,
                                                                        taskDatabaseId,
                                                                        taskState
                                                                    )

                                                                if task.Id <> taskId then
                                                                    JS.setTimeout
                                                                        (fun () ->
                                                                            setTaskIdSet (
                                                                                Set.remove Task.Default.Id
                                                                                >> Set.add task.Id
                                                                            ))
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
                                    Chakra.box
                                        (fun x ->
                                            x.flex <- "1"
                                            x.overflowY <- "auto"
                                            x.flexBasis <- 0)
                                        [

                                            str "Cell Form"
                                        ]
                            RightIcons = []
                        |}
                    ]),
                [|
                    box taskDatabaseId
                    box databaseFormIdFlag
                    box hydrateDatabase
                    box hydrateTaskState
                    box setTaskIdSet
                    box taskFormIdFlag
                    box input
                    box setRightDock
                    box setDatabaseIdSet
                |]
            )

        let itemsMap = items |> Map.ofList

        Chakra.flex
            (fun _ -> ())
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
                                        Atom = Atoms.User.rightDock input.Username
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
                                                    Atom = Atoms.User.rightDock input.Username
                                                |})
                            ]
                    ]
            ]
