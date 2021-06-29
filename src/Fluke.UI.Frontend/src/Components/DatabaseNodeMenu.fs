namespace Fluke.UI.Frontend.Components

open Fluke.Shared.Domain.Model
open Feliz
open Fable.React
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.TempUI

module MenuItem =
    [<ReactComponent>]
    let MenuItem icon label fn props =
        Chakra.menuItem
            (fun x ->
                x.closeOnSelect <- true
                x.paddingLeft <- "15px"
                x.paddingRight <- "10px"
                x.paddingTop <- "5px"
                x.paddingBottom <- "5px"
                x._hover <- JS.newObj (fun x -> x.backgroundColor <- "gray.10")

                x.icon <-
                    icon
                    |> Icons.renderChakra
                        (fun x ->
                            x.fontSize <- "13px"
                            x.marginBottom <- "-1px")

                x.onClick <-
                    fun e ->
                        promise {
                            do! fn ()
                            e.preventDefault ()
                        }

                props x)
            [
                str label
            ]

type DatabaseNodeMenu () =
    [<ReactComponent>]
    static member DatabaseNodeMenu (databaseId, disabled) =
        let deviceInfo = Store.useValue Selectors.deviceInfo
        let isReadWrite = Store.useValue (Selectors.Database.isReadWrite databaseId)
        let setLeftDock = Store.useSetState Atoms.leftDock
        let setRightDock = Store.useSetState Atoms.rightDock
        let setDatabaseUIFlag = Store.useSetState (Atoms.uiFlag Atoms.UIFlagType.Database)
        let setTaskUIFlag = Store.useSetState (Atoms.uiFlag Atoms.UIFlagType.Task)

        let exportDatabase = Hydrate.useExportDatabase ()
        let setDatabaseIdSet = Store.useSetStatePrev Atoms.databaseIdSet

        Menu.Menu
            {|
                Tooltip = ""
                Trigger =
                    InputLabelIconButton.InputLabelIconButton
                        (fun x ->
                            x.``as`` <- Chakra.react.MenuButton
                            x.icon <- Icons.bs.BsThreeDots |> Icons.render
                            x.fontSize <- "11px"
                            x.disabled <- disabled
                            x.marginLeft <- "6px")
                Body =
                    [
                        if isReadWrite then
                            MenuItem.MenuItem
                                Icons.bs.BsPlus
                                "Add Task"
                                (fun () ->
                                    promise {
                                        if deviceInfo.IsMobile then setLeftDock None
                                        setRightDock (Some DockType.Task)
                                        setTaskUIFlag ((databaseId, Task.Default.Id) |> Atoms.UIFlag.Task)
                                    })
                                (fun _ -> ())

                            MenuItem.MenuItem
                                Icons.bs.BsPen
                                "Edit Database"
                                (fun () ->
                                    promise {
                                        if deviceInfo.IsMobile then setLeftDock None
                                        setRightDock (Some DockType.Database)
                                        setDatabaseUIFlag (databaseId |> Atoms.UIFlag.Database)
                                    })
                                (fun _ -> ())

                            MenuItem.MenuItem
                                Icons.fi.FiCopy
                                "Clone Database"
                                (fun () -> promise { () })
                                (fun x -> x.isDisabled <- true)

                            MenuItem.MenuItem
                                Icons.bi.BiExport
                                "Export Database"
                                (fun () -> exportDatabase databaseId)
                                (fun _ -> ())

                        Popover.CustomPopover
                            {|
                                CloseButton = true
                                RenderOnHover = true
                                Props = fun x -> x.closeOnBlur <- false
                                Padding = "10px"
                                Trigger =
                                    MenuItem.MenuItem
                                        Icons.bi.BiTrash
                                        "Delete Database"
                                        (fun () -> promise { () })
                                        (fun x -> x.closeOnSelect <- false)
                                Body =
                                    fun (disclosure, initialFocusRef) ->
                                        [
                                            Chakra.box
                                                (fun _ -> ())
                                                [
                                                    Chakra.stack
                                                        (fun x -> x.spacing <- "10px")
                                                        [
                                                            Chakra.box
                                                                (fun x ->
                                                                    x.paddingBottom <- "5px"
                                                                    x.marginRight <- "24px"
                                                                    x.fontSize <- "15px")
                                                                [
                                                                    str "Delete Database"
                                                                ]

                                                            Chakra.box
                                                                (fun _ -> ())
                                                                [
                                                                    Button.Button
                                                                        {|
                                                                            Hint = None
                                                                            Icon =
                                                                                Some (
                                                                                    Icons.bi.BiTrash |> Icons.wrap,
                                                                                    Button.IconPosition.Left
                                                                                )
                                                                            Props =
                                                                                fun x ->
                                                                                    x.ref <- initialFocusRef

                                                                                    x.onClick <-
                                                                                        fun e ->
                                                                                            promise {
                                                                                                setDatabaseIdSet (
                                                                                                    Set.remove
                                                                                                        databaseId
                                                                                                )

                                                                                                disclosure.onClose ()

                                                                                                e.preventDefault ()
                                                                                            }
                                                                            Children =
                                                                                [
                                                                                    str "Confirm"
                                                                                ]
                                                                        |}
                                                                ]
                                                        ]
                                                ]
                                        ]
                            |}
                    ]
                MenuListProps = fun _ -> ()
            |}
