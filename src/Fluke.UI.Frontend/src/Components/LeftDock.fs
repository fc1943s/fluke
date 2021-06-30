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
    [<ReactComponent>]
    let LeftDock () =
        let isTesting = Store.useValue Store.Atoms.isTesting
        let leftDock, setLeftDock = Store.useState Atoms.leftDock
        let setRightDock = Store.useSetState Atoms.rightDock
        let deviceInfo = Store.useValue Selectors.deviceInfo
        let setDatabaseUIFlag = Store.useSetState (Atoms.uiFlag Atoms.UIFlagType.Database)

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

                                                                                setDatabaseUIFlag Atoms.UIFlag.None
                                                                            }
                                                        |}
                                                ]
                                        )

                                        DockPanel.DockPanelIcon.Menu (
                                            "Options",
                                            Icons.bs.BsThreeDotsVertical |> Icons.render,
                                            [
                                                MenuItemToggle.MenuItemToggle Atoms.hideTemplates "Hide Templates"
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
                    box isTesting
                    box setDatabaseUIFlag
                |]
            )


        Chakra.flex
            (fun _ -> ())
            [
                Chakra.box
                    (fun x ->
                        x.width <- "24px"
                        x.position <- "relative"
                        x.margin <- "1px")
                    [
                        Chakra.stack
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
                                                    Atom = Atoms.leftDock
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
                                        Atom = Atoms.leftDock
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
            //                            ]
            ]
