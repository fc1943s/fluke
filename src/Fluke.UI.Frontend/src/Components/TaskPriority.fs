namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared.Domain


module TaskPriority =

    [<ReactComponent>]
    let TaskPriority (input: {| Username: Username; TaskId: TaskId |}) =
        let priority = Recoil.useValue (Atoms.Task.priority input.TaskId)
        let cellSize = Recoil.useValue (Atoms.User.cellSize input.Username)

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
