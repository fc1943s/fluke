namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module PriorityView =
    [<ReactComponent>]
    let PriorityView () =
        let sortedTaskIdList = Store.useValue Selectors.Session.sortedTaskIdList
        let cellSize = Store.useValue Atoms.User.cellSize

        Chakra.flex
            (fun x -> x.flex <- "1")
            [
                Chakra.flex
                    (fun x ->
                        x.direction <- "column"
                        x.flex <- "1"
                        x.paddingRight <- "10px"
                        x.paddingLeft <- "4px"
                        x.maxWidth <- "400px")
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
                                            |> List.map TaskInformationName.TaskInformationName
                                    ]
                                // Column: Priority
                                Chakra.box
                                    (fun x ->
                                        x.paddingRight <- "10px"
                                        x.textAlign <- "center")
                                    [
                                        yield!
                                            sortedTaskIdList
                                            |> List.map TaskPriority.TaskPriority
                                    ]
                                // Column: Task Name
                                Chakra.box
                                    (fun x -> x.flex <- "1")
                                    [
                                        yield! sortedTaskIdList |> List.map TaskName.TaskName
                                    ]
                            ]
                    ]

                Chakra.box
                    (fun _ -> ())
                    [
                        GridHeader.GridHeader ()
                        Cells.Cells sortedTaskIdList
                    ]
            ]
