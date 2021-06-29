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
        let cellSize = Store.useValue Atoms.cellSize

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

                                                    React.suspense (
                                                        [
                                                            TaskInformationName.TaskInformationName taskId
                                                        ],
                                                        nothing
                                                    ))
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

                                                    React.suspense (
                                                        [
                                                            TaskPriority.TaskPriority taskId
                                                        ],
                                                        nothing
                                                    ))
                                    ]
                                // Column: Task Name
                                Chakra.box
                                    (fun x -> x.flex <- "1")
                                    [
                                        yield!
                                            sortedTaskIdList
                                            |> List.map
                                                (fun taskId ->
                                                    Chakra.box
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

                Chakra.box
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
