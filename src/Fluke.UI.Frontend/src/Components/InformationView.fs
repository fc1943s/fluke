namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module InformationView =
    [<ReactComponent>]
    let InformationView () =
        let groupIndentationLength = 20

        let tasksByInformationKind = Store.useValue Selectors.Session.tasksByInformationKind
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
                            (fun x -> x.direction <- "column")
                            [
                                yield!
                                    tasksByInformationKind
                                    |> List.map
                                        (fun (informationKindName, groups) ->
                                            Chakra.flex
                                                (fun x ->
                                                    x.direction <- "column"
                                                    x.flex <- "1")
                                                [
                                                    Chakra.box
                                                        (fun x ->
                                                            x.height <- $"{cellSize}px"
                                                            x.lineHeight <- $"{cellSize}px"
                                                            x.color <- "#444")
                                                        [
                                                            str informationKindName
                                                        ]

                                                    Chakra.box
                                                        (fun _ -> ())
                                                        [
                                                            yield!
                                                                groups
                                                                |> List.map
                                                                    (fun (information, taskIdList) ->
                                                                        Chakra.box
                                                                            (fun x -> x.paddingLeft <- $"{cellSize}px")
                                                                            [
                                                                                InformationName.InformationName
                                                                                    information

                                                                                // Task Name
                                                                                Chakra.box
                                                                                    (fun x -> x.flex <- "1")
                                                                                    [
                                                                                        yield!
                                                                                            taskIdList
                                                                                            |> List.map
                                                                                                (fun taskId ->
                                                                                                    Chakra.stack
                                                                                                        (fun x ->
                                                                                                            x.direction <-
                                                                                                                "row"

                                                                                                            x.spacing <-
                                                                                                                "10px"

                                                                                                            x.paddingLeft <-
                                                                                                                $"{
                                                                                                                    cellSize
                                                                                                                }px")
                                                                                                        [
                                                                                                            TaskPriority.TaskPriority
                                                                                                                taskId
                                                                                                            TaskName.TaskName
                                                                                                                taskId
                                                                                                        ])
                                                                                    ]
                                                                            ])
                                                        ]
                                                ])
                            ]
                    ]
                // Column: Grid
                Chakra.box
                    (fun _ -> ())
                    [
                        GridHeader.GridHeader ()

                        Chakra.box
                            (fun _ -> ())
                            [
                                yield!
                                    tasksByInformationKind
                                    |> List.map
                                        (fun (_, groups) ->
                                            Chakra.box
                                                (fun _ -> ())
                                                [
                                                    Chakra.box
                                                        (fun x ->
                                                            x.position <- "relative"
                                                            x.height <- $"{cellSize}px"
                                                            x.lineHeight <- $"{cellSize}px")
                                                        []
                                                    yield!
                                                        groups
                                                        |> List.map
                                                            (fun (_, taskIdList) ->
                                                                Chakra.box
                                                                    (fun _ -> ())
                                                                    [
                                                                        Chakra.box
                                                                            (fun x ->
                                                                                x.position <- "relative"
                                                                                x.height <- $"{cellSize}px"
                                                                                x.lineHeight <- $"{cellSize}px")
                                                                            []
                                                                        Cells.Cells taskIdList
                                                                    ])
                                                ])
                            ]
                    ]
            ]
