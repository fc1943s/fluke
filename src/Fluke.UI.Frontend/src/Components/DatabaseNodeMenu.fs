namespace Fluke.UI.Frontend.Components

open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Fluke.Shared.Domain.UserInteraction
open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.TempUI


module DatabaseNodeMenu =
    [<ReactComponent>]
    let DatabaseNodeMenu
        (input: {| Username: Username
                   DatabaseId: DatabaseId
                   Disabled: bool |})
        =
        let deviceInfo = Recoil.useValue Selectors.deviceInfo
        let isReadWrite = Recoil.useValueLoadableDefault (Selectors.Database.isReadWrite input.DatabaseId) false

        let setLeftDock = Recoil.useSetState (Atoms.User.leftDock input.Username)
        let setRightDock = Recoil.useSetState (Atoms.User.rightDock input.Username)

        let setDatabaseFormIdFlag =
            Recoil.useSetState (Atoms.User.formIdFlag (input.Username, TextKey (nameof DatabaseForm)))

        let setTaskFormIdFlag = Recoil.useSetState (Atoms.User.formIdFlag (input.Username, TextKey (nameof TaskForm)))
        let setNewTaskDatabaseId = Recoil.useSetState (Selectors.Task.databaseId (input.Username, Task.Default.Id))

        let exportDatabase = Hydrate.useExportDatabase ()

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
                                    x.disabled <- input.Disabled
                                    x.marginLeft <- "6px"
                        |}
                Body =
                    [
                        if isReadWrite then
                            Chakra.menuItem
                                (fun x ->
                                    x.closeOnSelect <- true

                                    x.icon <-
                                        Icons.bs.BsPlus
                                        |> Icons.renderChakra
                                            (fun x ->
                                                x.fontSize <- "13px"
                                                x.marginTop <- "-1px")

                                    x.onClick <-
                                        fun _ ->
                                            promise {
                                                if deviceInfo.IsMobile then setLeftDock None

                                                setRightDock (Some DockType.Task)

                                                setNewTaskDatabaseId input.DatabaseId
                                                setTaskFormIdFlag None
                                            })
                                [
                                    str "Add Task"
                                ]

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

                                                setRightDock (Some DockType.Database)

                                                setDatabaseFormIdFlag (input.DatabaseId |> DatabaseId.Value |> Some)
                                            })
                                [
                                    str "Edit Database"
                                ]

                        Chakra.menuItem
                            (fun x ->
                                x.icon <-
                                    Icons.fi.FiCopy
                                    |> Icons.renderChakra
                                        (fun x ->
                                            x.fontSize <- "13px"
                                            x.marginTop <- "-1px")

                                x.isDisabled <- true
                                x.onClick <- fun e -> promise { e.preventDefault () })
                            [
                                str "Clone Database"
                            ]

                        Chakra.menuItem
                            (fun x ->
                                x.icon <-
                                    Icons.bi.BiExport
                                    |> Icons.renderChakra
                                        (fun x ->
                                            x.fontSize <- "13px"
                                            x.marginTop <- "-1px")

                                x.onClick <- fun _ -> exportDatabase (input.Username, input.DatabaseId))
                            [
                                str "Export Database"
                            ]

                        Chakra.menuItem
                            (fun x ->
                                x.icon <-
                                    Icons.bs.BsTrash
                                    |> Icons.renderChakra
                                        (fun x ->
                                            x.fontSize <- "13px"
                                            x.marginTop <- "-1px")

                                x.onClick <- fun e -> promise { e.preventDefault () })
                            [
                                str "Delete Database"
                            ]
                    ]
                MenuListProps = fun _ -> ()
            |}
