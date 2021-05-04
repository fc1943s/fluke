namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared.Domain


module TaskPriority =

    [<ReactComponent>]
    let TaskPriority (input: {| TaskId: TaskId |}) =
        let priority = Recoil.useValue (Atoms.Task.priority (Some input.TaskId))

        let priorityText =
            priority
            |> Option.map (Priority.toTag >> (+) 1 >> string)
            |> Option.defaultValue ""

        Chakra.box
            (fun x ->
                x.position <- "relative"
                x.height <- "17px"
                x.lineHeight <- "17px")
            [
                str priorityText
            ]
