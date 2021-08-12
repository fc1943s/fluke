namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.State
open FsStore
open FsUi.Bindings
open Fluke.Shared
open FsUi.Components


module HabitTrackerView =

    [<ReactComponent>]
    let InformationNameWrapper informationTaskIdAtom =
        let information, _taskIdAtoms = Store.useValue informationTaskIdAtom
        InformationName.InformationName information

    [<ReactComponent>]
    let HabitTrackerView () =
        let sortedTaskIdAtoms = Store.useValue Selectors.Session.sortedTaskIdAtoms
        let cellSize = Store.useValue Atoms.User.cellSize
        let informationTaskIdAtoms = Store.useValue Selectors.Session.informationTaskIdAtoms

        Ui.flex
            (fun x -> x.flex <- "1")
            [
                Ui.flex
                    (fun x ->
                        x.direction <- "column"
                        x.paddingRight <- "10px"
                        x.paddingLeft <- "4px")
                    [
                        yield!
                            Ui.box (fun x -> x.minHeight <- $"{cellSize}px") []
                            |> List.replicate 3

                        Ui.flex
                            (fun _ -> ())
                            [
                                Ui.box
                                    (fun x -> x.paddingRight <- "10px")
                                    [
                                        yield!
                                            sortedTaskIdAtoms
                                            |> Array.map
                                                (fun taskIdAtom ->
                                                    React.suspense (
                                                        [
                                                            TaskInformationName.TaskInformationName taskIdAtom
                                                        ],
                                                        nothing
                                                    ))
                                    ]
                                // Column: Priority
                                Ui.box
                                    (fun x ->
                                        x.paddingRight <- "10px"
                                        x.textAlign <- "center")
                                    [
                                        yield!
                                            sortedTaskIdAtoms
                                            |> Array.map
                                                (fun taskIdAtom ->
                                                    React.suspense (
                                                        [
                                                            TaskPriority.TaskPriority taskIdAtom
                                                        ],
                                                        nothing
                                                    ))
                                    ]
                                // Column: Task Name
                                Ui.box
                                    (fun x -> x.flex <- "1")
                                    [
                                        yield!
                                            sortedTaskIdAtoms
                                            |> Array.map
                                                (fun taskIdAtom ->
                                                    Ui.box
                                                        (fun x -> x.height <- $"{cellSize}px")
                                                        [
                                                            React.suspense (
                                                                [
                                                                    TaskName.TaskName taskIdAtom
                                                                ],
                                                                LoadingSpinner.InlineLoadingSpinner ()
                                                            )
                                                        ])
                                    ]
                            ]

                        yield!
                            informationTaskIdAtoms
                            |> Array.map
                                (fun informationTaskIdAtom ->
                                    Ui.flex
                                        (fun x -> x.direction <- "column")
                                        [
                                            InformationNameWrapper informationTaskIdAtom
                                        ])
                    ]

                Ui.box
                    (fun _ -> ())
                    [
                        GridHeader.GridHeader ()
                        React.suspense (
                            [
                                Cells.Cells sortedTaskIdAtoms
                            ],
                            nothing
                        )
                    ]
            ]
