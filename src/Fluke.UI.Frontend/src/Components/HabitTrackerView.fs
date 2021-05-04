namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module HabitTrackerView =
    open Domain.UserInteraction

    [<ReactComponent>]
    let HabitTrackerView (input: {| Username: Username |}) =
        let taskIdList = Recoil.useValue (Selectors.Session.taskIdList input.Username)

        Chakra.flex
            (fun _ -> ())
            [
                Chakra.box
                    (fun _ -> ())
                    [
                        yield!
                            Chakra.box (fun x -> x.height <- "17px") []
                            |> List.replicate 3

                        Chakra.flex
                            (fun _ -> ())
                            [
                                Chakra.box
                                    (fun x -> x.paddingRight <- "10px")
                                    [
                                        yield!
                                            taskIdList
                                            |> List.map
                                                (fun taskId ->
                                                    TaskInformationName.TaskInformationName {| TaskId = taskId |})
                                    ]
                                // Column: Priority
                                Chakra.box
                                    (fun x ->
                                        x.paddingRight <- "10px"
                                        x.textAlign <- "center")
                                    [
                                        yield!
                                            taskIdList
                                            |> List.map (fun taskId -> TaskPriority.TaskPriority {| TaskId = taskId |})
                                    ]
                                // Column: Task Name
                                Chakra.box
                                    (fun x -> x.width <- "200px")
                                    [
                                        yield!
                                            taskIdList
                                            |> List.map (fun taskId -> TaskName.TaskName {| TaskId = taskId |})
                                    ]
                            ]
                    ]
                Chakra.box
                    (fun _ -> ())
                    [
                        GridHeader.GridHeader {| Username = input.Username |}
                        Cells.Cells
                            {|
                                Username = input.Username
                                TaskIdList = taskIdList
                            |}
                    ]
            ]
