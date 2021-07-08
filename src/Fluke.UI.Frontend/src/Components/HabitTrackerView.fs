namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module HabitTrackerView =
    [<ReactComponent>]
    let HabitTrackerView () =
        let sortedTaskIdList = Store.useValue Selectors.Session.sortedTaskIdList
        let cellSize = Store.useValue Atoms.User.cellSize

        UI.flex
            (fun x -> x.flex <- "1")
            [
                UI.flex
                    (fun x ->
                        x.direction <- "column"
                        x.paddingRight <- "10px"
                        x.paddingLeft <- "4px")
                    [
                        yield!
                            UI.box (fun x -> x.minHeight <- $"{cellSize}px") []
                            |> List.replicate 3

                        UI.flex
                            (fun _ -> ())
                            [
                                UI.box
                                    (fun x -> x.paddingRight <- "10px")
                                    [
                                        yield!
                                            sortedTaskIdList
                                            |> List.map
                                                (fun taskId ->

                                                    React.suspense (
                                                        [
                                                            TaskInformationName.TaskInformationName taskId
                                                        ],
                                                        nothing
                                                    ))
                                    ]
                                // Column: Priority
                                UI.box
                                    (fun x ->
                                        x.paddingRight <- "10px"
                                        x.textAlign <- "center")
                                    [
                                        yield!
                                            sortedTaskIdList
                                            |> List.map
                                                (fun taskId ->

                                                    React.suspense (
                                                        [
                                                            TaskPriority.TaskPriority taskId
                                                        ],
                                                        nothing
                                                    ))
                                    ]
                                // Column: Task Name
                                UI.box
                                    (fun x -> x.flex <- "1")
                                    [
                                        yield!
                                            sortedTaskIdList
                                            |> List.map
                                                (fun taskId ->
                                                    UI.box
                                                        (fun x -> x.height <- $"{cellSize}px")
                                                        [

                                                            React.suspense (
                                                                [
                                                                    TaskName.TaskName taskId
                                                                ],
                                                                LoadingSpinner.InlineLoadingSpinner ()
                                                            )
                                                        ])
                                    ]
                            ]
                    ]

                UI.box
                    (fun _ -> ())
                    [
                        GridHeader.GridHeader ()
                        React.suspense (
                            [
                                Cells.Cells sortedTaskIdList
                            ],
                            nothing
                        )
                    ]
            ]
