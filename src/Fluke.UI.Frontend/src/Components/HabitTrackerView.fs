namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module HabitTrackerView =
    open Domain.UserInteraction

    [<ReactComponent>]
    let HabitTrackerView (input: {| Username: Username |}) =
        let sortedTaskIdList = Store.useValue (Selectors.Session.sortedTaskIdList input.Username)
        let cellSize = Store.useValue (Atoms.User.cellSize input.Username)

        Chakra.flex
            (fun x -> x.flex <- "1")
            [
                Chakra.flex
                    (fun x ->
                        x.direction <- "column"
                        x.paddingRight <- "10px"
                        x.paddingLeft <- "4px")
                    [
                        yield!
                            Chakra.box (fun x -> x.minHeight <- $"{cellSize}px") []
                            |> List.replicate 3

                        Chakra.flex
                            (fun _ -> ())
                            [
                                Chakra.box
                                    (fun x -> x.paddingRight <- "10px")
                                    [
                                        yield!
                                            sortedTaskIdList
                                            |> List.map
                                                (fun taskId ->
                                                    TaskInformationName.TaskInformationName
                                                        {|
                                                            Username = input.Username
                                                            TaskId = taskId
                                                        |})
                                    ]
                                // Column: Priority
                                Chakra.box
                                    (fun x ->
                                        x.paddingRight <- "10px"
                                        x.textAlign <- "center")
                                    [
                                        yield!
                                            sortedTaskIdList
                                            |> List.map
                                                (fun taskId ->
                                                    TaskPriority.TaskPriority
                                                        {|
                                                            Username = input.Username
                                                            TaskId = taskId
                                                        |})
                                    ]
                                // Column: Task Name
                                Chakra.box
                                    (fun x -> x.flex <- "1")
                                    [
                                        yield!
                                            sortedTaskIdList
                                            |> List.map
                                                (fun taskId ->
                                                    TaskName.TaskName
                                                        {|
                                                            Username = input.Username
                                                            TaskId = taskId
                                                        |})
                                    ]
                            ]
                    ]

                Chakra.box
                    (fun x -> x.flex <- "1")
                    [
                        GridHeader.GridHeader {| Username = input.Username |}
                        Cells.Cells
                            {|
                                Username = input.Username
                                TaskIdList = sortedTaskIdList
                            |}
                    ]
            ]
