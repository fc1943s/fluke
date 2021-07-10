namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.TempUI


module LeftDock =
    [<ReactComponent>]
    let LeftDock () =
        let leftDock, setLeftDock = Store.useState Atoms.User.leftDock
        let setRightDock = Store.useSetState Atoms.User.rightDock
        let deviceInfo = Store.useValue Selectors.deviceInfo
        let setDatabaseUIFlag = Store.useSetState (Atoms.User.uiFlag UIFlagType.Database)

        let leftDockSize, setLeftDockSize = Store.useState Atoms.User.leftDockSize

        let deleteTemplates =
            Store.useCallback (
                (fun getter setter _ ->
                    promise {
                        let asyncDatabaseIdAtoms = Store.value getter Selectors.asyncDatabaseIdAtoms

                        let databaseIdArray =
                            asyncDatabaseIdAtoms
                            |> Store.waitForAll
                            |> Store.value getter

                        let owners =
                            databaseIdArray
                            |> Array.map Atoms.Database.owner
                            |> Store.waitForAll
                            |> Store.value getter

                        let templatesDatabaseIdArray =
                            owners
                            |> Array.indexed
                            |> Array.choose
                                (fun (i, owner) ->
                                    if owner = Templates.templatesUser.Username then
                                        Some databaseIdArray.[i]
                                    else
                                        None)

                        Store.change
                            setter
                            Atoms.User.selectedDatabaseIdSet
                            (fun selectedDatabaseIdSet ->
                                Set.difference selectedDatabaseIdSet (templatesDatabaseIdArray |> Set.ofArray))

                        Store.set setter Atoms.User.hideTemplates (Some (Flag true))

                        do!
                            templatesDatabaseIdArray
                            |> Array.map (fun databaseId -> Store.deleteRoot getter (Atoms.Database.name databaseId))
                            |> Promise.Parallel
                            |> Promise.ignore
                    }),
                [||]
            )

        let items, itemsMap =
            React.useMemo (
                (fun () ->
                    let items =
                        [
                            DockType.Settings,
                            {|
                                Name = "Settings"
                                Icon = Icons.md.MdSettings
                                Content =
                                    Settings.Settings
                                        (fun x ->
                                            x.flex <- "1"
                                            x.overflowY <- "auto"
                                            x.flexBasis <- 0)
                                RightIcons = []
                            |}

                            DockType.Databases,
                            {|
                                Name = "Databases"
                                Icon = Icons.fi.FiDatabase
                                Content =
                                    Databases.Databases
                                        (fun x ->
                                            x.flex <- "1"
                                            x.padding <- "10px"
                                            x.paddingLeft <- "0"
                                            x.paddingTop <- "3px"
                                            x.overflowY <- "auto"
                                            x.flexBasis <- 0)
                                RightIcons =
                                    [
                                        DockPanel.DockPanelIcon.Component (
                                            Tooltip.wrap
                                                (str "Add Database")
                                                [
                                                    TransparentIconButton.TransparentIconButton
                                                        {|
                                                            Props =
                                                                fun x ->
                                                                    UI.setTestId x "Add Database"
                                                                    x.icon <- Icons.fi.FiPlus |> Icons.render

                                                                    x.fontSize <- "17px"

                                                                    x.onClick <-
                                                                        fun _ ->
                                                                            promise {
                                                                                if deviceInfo.IsMobile then
                                                                                    setLeftDock None

                                                                                setRightDock (Some DockType.Database)

                                                                                setDatabaseUIFlag UIFlag.None
                                                                            }
                                                        |}
                                                ]
                                        )

                                        DockPanel.DockPanelIcon.Menu (
                                            "Options",
                                            Icons.bs.BsThreeDotsVertical |> Icons.render,
                                            [
                                                MenuItemToggle.MenuItemToggleFlagAtom
                                                    Atoms.User.hideTemplates
                                                    "Hide Templates"

                                                ConfirmPopover.ConfirmPopover
                                                    ConfirmPopover.ConfirmPopoverType.MenuItem
                                                    Icons.bi.BiTrash
                                                    "Delete Templates"
                                                    deleteTemplates
                                            ]
                                        )
                                    ]
                            |}
                        ]

                    let itemsMap = items |> Map.ofSeq
                    items, itemsMap),
                [|
                    box setLeftDock
                    box setRightDock
                    box deviceInfo
                    box setDatabaseUIFlag
                    box deleteTemplates
                |]
            )


        UI.flex
            (fun _ -> ())
            [
                UI.box
                    (fun x ->
                        x.width <- "24px"
                        x.position <- "relative"
                        x.margin <- "1px")
                    [
                        UI.stack
                            (fun x ->
                                x.spacing <- "1px"
                                x.direction <- "row"
                                x.right <- "0"
                                x.position <- "absolute"
                                x.transform <- "rotate(-90deg) translate(0, -100%)"
                                x.transformOrigin <- "100% 0"
                                x.height <- "24px")
                            [
                                yield!
                                    items
                                    |> List.mapi
                                        (fun i (dockType, item) ->
                                            DockButton.DockButton
                                                {|
                                                    DockType = dockType
                                                    Name = item.Name
                                                    Icon = item.Icon
                                                    OnClick =
                                                        fun _ ->
                                                            promise { if deviceInfo.IsMobile then setRightDock None }
                                                    Atom = Atoms.User.leftDock
                                                    Props = fun x -> x.tabIndex <- items.Length - i
                                                |})
                            ]
                    ]

                match leftDock with
                | None -> nothing
                | Some leftDock ->
                    match itemsMap |> Map.tryFind leftDock with
                    | None -> nothing
                    | Some item ->
                        Resizable.resizable
                            {|
                                size = {| width = $"{leftDockSize}px" |}
                                onResizeStop =
                                    fun _e _direction _ref (d: {| width: int |}) ->
                                        setLeftDockSize (leftDockSize + d.width)
                                minWidth = "200px"
                                enable =
                                    {|
                                        top = false
                                        right = true
                                        bottom = false
                                        left = false
                                        topRight = false
                                        bottomRight = false
                                        bottomLeft = false
                                        topLeft = false
                                    |}
                            |}
                            [
                                UI.flex
                                    (fun x ->
                                        x.width <-
                                            unbox (
                                                JS.newObj
                                                    (fun (x: UI.IBreakpoints<string>) ->
                                                        x.``base`` <- "calc(100vw - 50px)"
                                                        x.md <- "auto")
                                            )

                                        x.height <- "100%"
                                        x.borderRightWidth <- "1px"
                                        x.borderRightColor <- "gray.16"
                                        x.flex <- "1")
                                    [
                                        DockPanel.DockPanel
                                            {|
                                                Name = item.Name
                                                Icon = item.Icon
                                                RightIcons = item.RightIcons
                                                Atom = Atoms.User.leftDock
                                                children =
                                                    [
                                                        React.suspense (
                                                            [
                                                                item.Content
                                                            ],
                                                            LoadingSpinner.LoadingSpinner ()
                                                        )
                                                    ]
                                            |}
                                    ]
                            ]
            ]
