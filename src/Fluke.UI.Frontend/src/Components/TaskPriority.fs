namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared.Domain
open Fluke.Shared.Domain.State


module TaskPriority =

    [<ReactComponent>]
    let TaskPriority (taskId: TaskId) =
        let priority = Recoil.useValue (Recoil.Atoms.Task.priority (Some taskId))

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
