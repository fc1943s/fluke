namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared.Domain


module TaskPriority =

    [<ReactComponent>]
    let TaskPriority (input: {| TaskId: Recoil.Atoms.Task.TaskId |}) =
        let priority = Recoil.useValue (Recoil.Atoms.Task.priority input.TaskId)

        let priorityText =
            priority
            |> Option.map (Priority.toTag >> (+) 1 >> string)
            |> Option.defaultValue ""

        Chakra.box
            {|
                position = "relative"
                height = "17px"
                lineHeight = "17px"
            |}
            [
                str priorityText
            ]
