namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Fable.Core.JsInterop
open Fluke.Shared
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.TempUI


module LeftDock =
    open Domain.UserInteraction

    [<ReactComponent>]
    let LeftDock (input: {| Username: Username |}) =
        let isTesting = Store.useValue Atoms.isTesting
        let leftDock, setLeftDock = Store.useState (Atoms.User.leftDock input.Username)
        let setRightDock = Store.useSetState (Atoms.User.rightDock input.Username)
        let deviceInfo = Store.useValue Selectors.deviceInfo
        let hideTemplates, setHideTemplates = Store.useState (Atoms.User.hideTemplates input.Username)
        let setDatabaseUIFlag = Store.useSetState (Atoms.User.uiFlag (input.Username, Atoms.User.UIFlagType.Database))

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
                                    fun () ->
                                        Settings.Settings
                                            {|
                                                Username = input.Username
                                                Props =
                                                    fun x ->
                                                        x.flex <- "1"
                                                        x.overflowY <- "auto"
                                                        x.flexBasis <- 0
                                            |}
                                RightIcons = []
                            |}

                            DockType.Databases,
                            {|
                                Name = "Databases"
                                Icon = Icons.fi.FiDatabase
                                Content =
                                    fun () ->
                                        Databases.Databases
                                            {|
                                                Username = input.Username
                                                Props =
                                                    fun x ->
                                                        x.flex <- "1"
                                                        x.padding <- "10px"
                                                        x.overflowY <- "auto"
                                                        x.flexBasis <- 0
                                            |}
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
                                                                    if isTesting then
                                                                        x?``data-testid`` <- "Add Database"

                                                                    x.icon <- Icons.fi.FiPlus |> Icons.render

                                                                    x.fontSize <- "17px"

                                                                    x.onClick <-
                                                                        fun _ ->
                                                                            promise {
                                                                                if deviceInfo.IsMobile then
                                                                                    setLeftDock None

                                                                                setRightDock (Some DockType.Database)

                                                                                setDatabaseUIFlag None
                                                                            }
                                                        |}
                                                ]
                                        )

                                        DockPanel.DockPanelIcon.Menu (
                                            "Options",
                                            Icons.bs.BsThreeDotsVertical |> Icons.render,
                                            [
                                                Chakra.menuOptionGroup
                                                    (fun x ->
                                                        x.``type`` <- "checkbox"

                                                        x.value <-
                                                            [|
                                                                if hideTemplates then
                                                                    yield nameof Atoms.User.hideTemplates
                                                            |]

                                                        x.onChange <-
                                                            fun (checks: string []) ->
                                                                promise {
                                                                    setHideTemplates (
                                                                        checks
                                                                        |> Array.contains (
                                                                            nameof Atoms.User.hideTemplates
                                                                        )
                                                                    )
                                                                })
                                                    [
                                                        Chakra.menuItemOption
                                                            (fun x -> x.value <- nameof Atoms.User.hideTemplates)
                                                            [
                                                                str "Hide Templates"
                                                            ]
                                                    ]
                                            ]
                                        )
                                    ]
                            |}
                        ]

                    let itemsMap = items |> Map.ofList
                    items, itemsMap),
                [|
                    box setLeftDock
                    box setRightDock
                    box deviceInfo
                    box input.Username
                    box isTesting
                    box hideTemplates
                    box setDatabaseUIFlag
                    box setHideTemplates
                |]
            )


        Chakra.flex
            (fun _ -> ())
            [
                Chakra.box
                    (fun x ->
                        x.width <- "24px"
                        x.position <- "relative")
                    [
                        Chakra.flex
                            (fun x ->
                                x.right <- "0"
                                x.position <- "absolute"
                                x.transform <- "rotate(-90deg) translate(0, -100%)"
                                x.transformOrigin <- "100% 0"
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
                                                            promise { if deviceInfo.IsMobile then setRightDock None }
                                                    Atom = Atoms.User.leftDock input.Username
                                                |})
                            ]
                    ]

                match leftDock with
                | None -> nothing
                | Some leftDock ->
                    match itemsMap |> Map.tryFind leftDock with
                    | None -> nothing
                    | Some item ->
                        //                        Resizable.resizable
//                            {|
//                                defaultSize = {| width = "300px" |}
//                                minWidth = "300px"
//                                enable =
//                                    {|
//                                        top = false
//                                        right = true
//                                        bottom = false
//                                        left = false
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
                                x.borderRightWidth <- "1px"
                                x.borderRightColor <- "gray.16"
                                x.flex <- "1")
                            [
                                DockPanel.DockPanel
                                    {|
                                        Name = item.Name
                                        Icon = item.Icon
                                        RightIcons = item.RightIcons
                                        Atom = Atoms.User.leftDock input.Username
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
            ]
