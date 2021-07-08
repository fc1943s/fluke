namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module InformationView =
    [<ReactComponent>]
    let InformationTree information taskIdList =
        let cellSize = Store.useValue Atoms.User.cellSize

        UI.box
            (fun x -> x.paddingLeft <- $"{cellSize}px")
            [
                InformationName.InformationName information

                // Task Name
                UI.box
                    (fun x -> x.flex <- "1")
                    [
                        yield!
                            taskIdList
                            |> List.map
                                (fun taskId ->
                                    UI.stack
                                        (fun x ->
                                            x.position <- "relative"
                                            x.direction <- "row"
                                            x.spacing <- "10px"
                                            x.paddingLeft <- $"{cellSize}px")
                                        [
                                            UI.box
                                                (fun x ->
                                                    x.position <- "absolute"
                                                    x.left <- "10px"
                                                    x.top <- "0")
                                                [
                                                    TaskPriority.TaskPriority taskId
                                                ]
                                            TaskName.TaskName taskId
                                        ])
                    ]
            ]

    [<ReactComponent>]
    let InformationView () =
        let groupIndentationLength = 20

        let tasksByInformationKind = Store.useValue Selectors.Session.tasksByInformationKind
        let cellSize = Store.useValue Atoms.User.cellSize

        UI.flex
            (fun x -> x.flex <- "1")
            [
                UI.flex
                    (fun x ->
                        x.direction <- "column"
                        x.flex <- "1"
                        x.paddingRight <- "10px"
                        x.paddingLeft <- "4px"
                        x.maxWidth <- "400px")
                    [
                        yield!
                            UI.box (fun x -> x.minHeight <- $"{cellSize}px") []
                            |> List.replicate 3

                        UI.flex
                            (fun x -> x.direction <- "column")
                            [
                                yield!
                                    tasksByInformationKind
                                    |> List.map
                                        (fun (informationKindName, groups) ->
                                            UI.flex
                                                (fun x ->
                                                    x.direction <- "column"
                                                    x.flex <- "1")
                                                [
                                                    UI.box
                                                        (fun x ->
                                                            x.height <- $"{cellSize}px"
                                                            x.lineHeight <- $"{cellSize}px"
                                                            x.color <- "#444")
                                                        [
                                                            str informationKindName
                                                        ]

                                                    UI.box
                                                        (fun _ -> ())
                                                        [
                                                            yield!
                                                                groups
                                                                |> List.map
                                                                    (fun (information, taskIdList) ->
                                                                        InformationTree information taskIdList)
                                                        ]
                                                ])
                            ]
                    ]
                // Column: Grid
                UI.box
                    (fun _ -> ())
                    [
                        GridHeader.GridHeader ()

                        UI.box
                            (fun _ -> ())
                            [
                                yield!
                                    tasksByInformationKind
                                    |> List.map
                                        (fun (_, groups) ->
                                            UI.box
                                                (fun _ -> ())
                                                [
                                                    UI.box
                                                        (fun x ->
                                                            x.position <- "relative"
                                                            x.height <- $"{cellSize}px"
                                                            x.lineHeight <- $"{cellSize}px")
                                                        []
                                                    yield!
                                                        groups
                                                        |> List.map
                                                            (fun (_, taskIdList) ->
                                                                UI.box
                                                                    (fun _ -> ())
                                                                    [
                                                                        UI.box
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
