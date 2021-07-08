namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module CellBorder =
    open Domain.UserInteraction

    [<ReactComponent>]
    let CellBorder (taskId: TaskId) (date: FlukeDate) =
        let weekStart = Store.useValue Atoms.User.weekStart
        let cellSize = Store.useValue Atoms.User.cellSize
        let isReadWrite = Store.useValue (Selectors.Task.isReadWrite taskId)

        match (weekStart, date) with
        | StartOfMonth -> Some ("1px", "#ffffff3d")
        | StartOfWeek -> Some ("1px", "#222")
        | _ -> None
        |> Option.map
            (fun (borderLeftWidth, borderLeftColor) ->
                UI.box
                    (fun x ->
                        x.position <- "absolute"
                        x.top <- "-1px"
                        x.left <- "-1px"
                        x.bottom <- "-1px"
                        x.width <- $"{cellSize}px"

                        if isReadWrite then
                            x._hover <- JS.newObj (fun x -> x.borderLeftWidth <- "0")

                        x.borderLeftWidth <- borderLeftWidth
                        x.borderLeftColor <- borderLeftColor)
                    [])
        |> Option.defaultValue nothing
