namespace Fluke.UI.Frontend.Components

open Fluke.Shared.Domain.Model
open Feliz
open Fable.React
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.TempUI


module DatabaseNodeMenu =
    [<ReactComponent>]
    let DatabaseNodeMenu databaseId disabled =
        let deviceInfo = Store.useValue Selectors.deviceInfo
        let isReadWrite = Store.useValue (Selectors.Database.isReadWrite databaseId)
        let setLeftDock = Store.useSetState Atoms.User.leftDock
        let setRightDock = Store.useSetState Atoms.User.rightDock
        let setDatabaseUIFlag = Store.useSetState (Atoms.User.uiFlag UIFlagType.Database)
        let setTaskUIFlag = Store.useSetState (Atoms.User.uiFlag UIFlagType.Task)

        let exportDatabase = Hydrate.useExportDatabase ()

        let deleteDatabase =
            Store.useCallback (
                (fun getter setter _ ->
                    Store.change setter Atoms.User.selectedDatabaseIdSet (Set.remove databaseId)
                    Store.deleteRoot getter (Atoms.Database.name databaseId)),
                [|
                    box databaseId
                |]
            )

        Menu.Menu
            {|
                Tooltip = ""
                Trigger =
                    InputLabelIconButton.InputLabelIconButton
                        (fun x ->
                            x.``as`` <- UI.react.MenuButton
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
                                        setTaskUIFlag ((databaseId, Task.Default.Id) |> UIFlag.Task)
                                    })
                                (fun _ -> ())

                            MenuItem.MenuItem
                                Icons.bs.BsPen
                                "Edit Database"
                                (fun () ->
                                    promise {
                                        if deviceInfo.IsMobile then setLeftDock None
                                        setRightDock (Some DockType.Database)
                                        setDatabaseUIFlag (databaseId |> UIFlag.Database)
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

                        ConfirmPopover.ConfirmPopover
                            ConfirmPopover.ConfirmPopoverType.MenuItem
                            Icons.bi.BiTrash
                            "Delete Database"
                            deleteDatabase
                    ]
                MenuListProps = fun _ -> ()
            |}
