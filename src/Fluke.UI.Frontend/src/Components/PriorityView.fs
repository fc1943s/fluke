namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module PriorityView =

    open Domain.UserInteraction

    [<ReactComponent>]
    let PriorityView (username: Username) =
        let taskIdList = Recoil.useValue (Recoil.Atoms.Session.taskIdList username)

        Chakra.flex
            {|  |}
            [
                Chakra.box
                    {|  |}
                    [
                        yield!
                            Chakra.box {| height = "17px" |} []
                            |> List.replicate 3

                        Chakra.flex
                            {|  |}
                            [
                                Chakra.box
                                    {| paddingRight = "10px" |}
                                    [
                                        yield!
                                            taskIdList
                                            |> List.map TaskInformationName.TaskInformationName
                                    ]
                                // Column: Priority
                                Chakra.box
                                    {| paddingRight = "10px"; textAlign = "center" |}
                                    [
                                        yield! taskIdList |> List.map TaskPriority.TaskPriority
                                    ]
                                // Column: Task Name
                                Chakra.box
                                    {| width = "200px" |}
                                    [
                                        yield! taskIdList |> List.map TaskName.TaskName
                                    ]
                            ]
                    ]

                Chakra.box
                    {|  |}
                    [
                        GridHeader.GridHeader username
                        Cells.Cells {| Username = username; TaskIdList = taskIdList |}
                    ]
            ]
