namespace Fluke.UI.Frontend.Components

open System
open Fable.Core
open Fluke.Shared
open Fluke.Shared.Domain.State
open Fluke.Shared.Domain.UserInteraction
open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State


module DatabaseNodeMenu =
    [<ReactComponent>]
    let DatabaseNodeMenu
        (input: {| Username: Username
                   DatabaseId: DatabaseId
                   Disabled: bool |})
        =
        let isReadWrite = Recoil.useValueLoadableDefault (Selectors.Database.isReadWrite input.DatabaseId) false

        let exportDatabase =
            Recoil.useCallbackRef
                (fun setter ->
                    promise {
                        let! database =
                            setter.snapshot.getPromise (Selectors.Database.database (input.Username, input.DatabaseId))

                        let! taskIdSet =
                            setter.snapshot.getPromise (Atoms.Database.taskIdSet (input.Username, input.DatabaseId))

                        let! taskStateArray =
                            taskIdSet
                            |> Set.toList
                            |> List.map (fun taskId -> Selectors.Task.taskState (input.Username, taskId))
                            |> List.map setter.snapshot.getPromise
                            |> Promise.Parallel

                        let! informationStateList =
                            setter.snapshot.getPromise (Selectors.Session.informationStateList input.Username)

                        let databaseState =
                            {
                                Database = database
                                InformationStateMap =
                                    informationStateList
                                    |> List.map (fun informationState -> informationState.Information, informationState)
                                    |> Map.ofList
                                TaskStateMap =
                                    taskStateArray
                                    |> Array.map (fun taskState -> taskState.Task.Id, taskState)
                                    |> Map.ofArray
                            }

                        let json = databaseState |> Gun.jsonEncode

                        let timestamp =
                            (FlukeDateTime.FromDateTime DateTime.Now)
                            |> FlukeDateTime.Stringify

                        JS.download json $"{database.Name |> DatabaseName.Value}-{timestamp}.json" "application/json"
                    })

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
                Menu =
                    [
                        if isReadWrite then
                            TaskFormTrigger.TaskFormTrigger
                                {|
                                    Username = input.Username
                                    DatabaseId = input.DatabaseId
                                    TaskId = None
                                    Trigger =
                                        fun trigger _setter ->
                                            Chakra.menuItem
                                                (fun x ->
                                                    x.icon <-
                                                        Icons.bs.BsPlus
                                                        |> Icons.renderChakra
                                                            (fun x ->
                                                                x.fontSize <- "13px"
                                                                x.marginTop <- "-1px")

                                                    x.onClick <- fun _ -> promise { trigger () })
                                                [
                                                    str "Add Task"
                                                ]
                                |}

                            DatabaseFormTrigger.DatabaseFormTrigger
                                {|
                                    Username = input.Username
                                    DatabaseId = Some input.DatabaseId
                                    Trigger =
                                        fun trigger _setter ->
                                            Chakra.menuItem
                                                (fun x ->
                                                    x.icon <-
                                                        Icons.bs.BsPen
                                                        |> Icons.renderChakra
                                                            (fun x ->
                                                                x.fontSize <- "13px"
                                                                x.marginTop <- "-1px")

                                                    x.onClick <-
                                                        fun _ ->
                                                            promise {
                                                                trigger ()
                                                                ()
                                                            })
                                                [
                                                    str "Edit Database"
                                                ]
                                |}

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

                                x.onClick <- fun _ -> exportDatabase ())
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
