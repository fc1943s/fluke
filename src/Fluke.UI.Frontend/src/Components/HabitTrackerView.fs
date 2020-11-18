namespace Fluke.UI.Frontend.Components

open FSharpPlus
open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module HabitTrackerView =
    open Domain.Model
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun (input: {| Username: Username |}) ->
            let taskIdList = Recoil.useValue (Recoil.Atoms.Session.taskIdList input.Username)

            Chakra.flex
                ()
                [
                    Chakra.box
                        ()
                        [
                            yield! Chakra.box {| height = "17px" |} []
                                   |> List.replicate 3

                            Chakra.flex
                                ()
                                [
                                    Chakra.box
                                        {| paddingRight = "10px" |}
                                        [
                                            yield! taskIdList
                                                   |> List.map (fun taskId ->
                                                       TaskInformationName.render {| TaskId = taskId |})
                                        ]
                                    // Column: Priority
                                    Chakra.box
                                        {| paddingRight = "10px"; textAlign = "center" |}
                                        [
                                            yield! taskIdList
                                                   |> List.map (fun taskId -> TaskPriority.render {| TaskId = taskId |})
                                        ]
                                    // Column: Task Name
                                    Chakra.box
                                        {| width = "200px" |}
                                        [
                                            yield! taskIdList
                                                   |> List.map (fun taskId -> TaskName.render {| TaskId = taskId |})
                                        ]
                                ]
                        ]
                    Chakra.box
                        ()
                        [
                            GridHeader.render {| Username = input.Username |}
                            Cells.render
                                {|
                                    Username = input.Username
                                    TaskIdList = taskIdList
                                |}
                        ]
                ])
