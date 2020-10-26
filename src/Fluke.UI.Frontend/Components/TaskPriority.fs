namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.Shared.Domain


module TaskPriority =
    open Domain.Model
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun (input: {| TaskId: Recoil.Atoms.Task.TaskId |}) ->
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
                ])