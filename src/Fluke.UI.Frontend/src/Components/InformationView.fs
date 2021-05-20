namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module InformationView =
    open Domain.UserInteraction

    [<ReactComponent>]
    let InformationView (input: {| Username: Username |}) =
        let groupIndentationLength = 20

        let tasksByInformationKind = Recoil.useValue (Selectors.Session.tasksByInformationKind input.Username)

        Chakra.flex
            (fun x -> x.flex <- "1")
            [
                Chakra.flex
                    (fun x ->
                        x.direction <- "column"
                        x.flex <- "1"
                        x.paddingRight <- "10px"
                        x.maxWidth <- "400px")
                    [
                        yield!
                            Chakra.box (fun x -> x.minHeight <- "17px") []
                            |> List.replicate 3

                        Chakra.flex
                            (fun _ -> ())
                            [
                                yield!
                                    tasksByInformationKind
                                    |> List.map
                                        (fun (informationKindName, groups) ->
                                            Chakra.flex
                                                (fun x -> x.direction <- "column")
                                                [
                                                    Chakra.box
                                                        (fun x ->
                                                            x.height <- "17px"
                                                            x.lineHeight <- "17px"
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
                                                                            (fun x -> x.paddingLeft <- "17px")
                                                                            [
                                                                                InformationName.InformationName
                                                                                    {| Information = information |}

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
                                                                                                                "17px")
                                                                                                        [
                                                                                                            TaskPriority.TaskPriority
                                                                                                                {|
                                                                                                                    TaskId =
                                                                                                                        taskId
                                                                                                                |}
                                                                                                            TaskName.TaskName
                                                                                                                {|
                                                                                                                    TaskId =
                                                                                                                        taskId
                                                                                                                |}
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
                        GridHeader.GridHeader {| Username = input.Username |}

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
                                                            x.height <- "17px"
                                                            x.lineHeight <- "17px")
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
                                                                                x.height <- "17px"
                                                                                x.lineHeight <- "17px")
                                                                            []
                                                                        Cells.Cells
                                                                            {|
                                                                                Username = input.Username
                                                                                TaskIdList = taskIdList
                                                                            |}
                                                                    ])
                                                ])
                            ]
                    ]
            ]
