namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared.Domain


module TaskPriority =

    [<ReactComponent>]
    let TaskPriority (input: {| TaskId: TaskId |}) =
        let priority = Store.useValue (Atoms.Task.priority input.TaskId)
        let cellSize = Store.useValue Atoms.cellSize

        let priorityText =
            priority
            |> Option.map (Priority.toTag >> (+) 1 >> string)
            |> Option.defaultValue ""

        Chakra.box
            (fun x ->
                x.position <- "relative"
                x.height <- $"{cellSize}px"
                x.lineHeight <- $"{cellSize}px")
            [
                str priorityText
            ]
