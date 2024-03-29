namespace Fluke.UI.Frontend.Components

open Fluke.Shared.Domain.Model
open Feliz
open Fluke.Shared.Domain.State
open FsStore
open FsStore.Hooks
open FsUi.Bindings
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.State.State
open Fluke.UI.Frontend.TempUI
open FsUi.Components
open FsStore.State


module DatabaseNodeMenu =
    [<ReactComponent>]
    let DatabaseNodeMenu databaseId disabled =
        let isReadWrite = Store.useValue (Selectors.Database.isReadWrite databaseId)
        let navigate = Store.useSetState Navigate.Actions.navigate
        let exportDatabase = Hydrate.useExportDatabase ()

        let deleteDatabase =
            Store.useCallbackRef
                (fun getter setter _ ->
                    promise {
                        Atom.change setter Atoms.User.selectedDatabaseIdSet (Set.remove databaseId)
                        do! Hydrate.deleteRecord getter Atoms.Database.collection (databaseId |> DatabaseId.Value)
                        return true
                    })

        Menu.Menu
            {|
                Tooltip = ""
                Trigger =
                    InputLabelIconButton.InputLabelIconButton
                        (fun x ->
                            x.``as`` <- Ui.react.MenuButton
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
                                (Some
                                    (fun () ->
                                        promise {
                                            navigate (
                                                Navigate.DockPosition.Right,
                                                Some DockType.Task,
                                                UIFlagType.Task,
                                                UIFlag.Task (databaseId, Task.Default.Id)
                                            )
                                        }))
                                (fun _ -> ())

                            MenuItem.MenuItem
                                Icons.bs.BsPen
                                "Edit Database"
                                (Some
                                    (fun () ->
                                        promise {
                                            navigate (
                                                Navigate.DockPosition.Right,
                                                Some DockType.Database,
                                                UIFlagType.Database,
                                                UIFlag.Database databaseId
                                            )
                                        }))
                                (fun _ -> ())

                        MenuItem.MenuItem Icons.fi.FiCopy "Clone Database" None (fun x -> x.isDisabled <- true)

                        MenuItem.MenuItem
                            Icons.bi.BiExport
                            "Export Database"
                            (Some (fun () -> exportDatabase databaseId))
                            (fun _ -> ())

                        Popover.MenuItemConfirmPopover Icons.bi.BiTrash "Delete Database" deleteDatabase
                    ]
                MenuListProps = fun _ -> ()
            |}
