namespace Fluke.UI.Frontend.Components

open FSharpPlus
open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module InformationView =
    open Domain.UserInteraction

    let render =
        React.memo (fun (input: {| Username: Username |}) ->
            let groupIndentationLength = 20

            let tasksByInformationKind =
                Recoil.useValue (Recoil.Selectors.Session.tasksByInformationKind input.Username)

            Chakra.flex
                ()
                [
                    Chakra.box
                        ()
                        [
                            yield!
                                Chakra.box {| height = "17px" |} []
                                |> List.replicate 3

                            Chakra.box
                                ()
                                [
                                    yield!
                                        tasksByInformationKind
                                        |> List.map (fun (informationKindName, groups) ->
                                            Chakra.box
                                                ()
                                                [
                                                    Chakra.box
                                                        {|
                                                            height = "17px"
                                                            lineHeight = "17px"
                                                            color = "#444"
                                                        |}
                                                        [
                                                            str informationKindName
                                                        ]

                                                    Chakra.box
                                                        ()
                                                        [
                                                            yield!
                                                                groups
                                                                |> List.map (fun (informationId, taskIdList) ->
                                                                    Chakra.box
                                                                        {| paddingLeft = "17px" |}
                                                                        [
                                                                            InformationName.render
                                                                                {| InformationId = informationId |}

                                                                            // Task Name
                                                                            Chakra.box
                                                                                {| width = "400px" |}
                                                                                [
                                                                                    yield!
                                                                                        taskIdList
                                                                                        |> List.map (fun taskId ->
                                                                                            Chakra.stack
                                                                                                {|
                                                                                                    direction = "row"
                                                                                                    spacing = "10px"
                                                                                                    paddingLeft = "17px"
                                                                                                |}
                                                                                                [
                                                                                                    TaskPriority.render
                                                                                                        {|
                                                                                                            TaskId =
                                                                                                                taskId
                                                                                                        |}
                                                                                                    TaskName.render
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
                        ()
                        [
                            GridHeader.render {| Username = input.Username |}

                            Chakra.box
                                ()
                                [
                                    yield!
                                        tasksByInformationKind
                                        |> List.map (fun (_, groups) ->
                                            Chakra.box
                                                ()
                                                [
                                                    Chakra.box
                                                        {|
                                                            position = "relative"
                                                            height = "17px"
                                                            lineHeight = "17px"
                                                        |}
                                                        []
                                                    yield!
                                                        groups
                                                        |> List.map (fun (_, taskIdList) ->
                                                            Chakra.box
                                                                ()
                                                                [
                                                                    Chakra.box
                                                                        {|
                                                                            position = "relative"
                                                                            height = "17px"
                                                                            lineHeight = "17px"
                                                                        |}
                                                                        []
                                                                    Cells.render
                                                                        {|
                                                                            Username = input.Username
                                                                            TaskIdList = taskIdList
                                                                        |}
                                                                ])

                                                ])
                                ]

                        ]

                ])
