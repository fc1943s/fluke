namespace Fluke.UI.Frontend.Components

open Feliz.MaterialUI
open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.Shared


module TaskPriorityComponent =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun (input: {| TaskId: Recoil.Atoms.Task.TaskId |}) ->
            let priority = Recoil.useValue (Recoil.Atoms.Task.priority input.TaskId)

            let priorityText =
                priority
                |> Option.map (fun x -> x.Value)
                |> Option.defaultValue 0
                |> string


            Html.div [
                prop.className Css.cellRectangle
                prop.children
                    [
                        str priorityText
                    ]
            ])
