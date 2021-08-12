namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.State
open FsStore
open FsUi.Bindings
open Fluke.Shared.Domain


module TaskPriority =
    [<ReactComponent>]
    let TaskPriority taskIdAtom =
        let taskId = Store.useValue taskIdAtom
        let priority = Store.useValue (Atoms.Task.priority taskId)
        let cellSize = Store.useValue Atoms.User.cellSize

        let priorityText =
            priority
            |> Option.map (Priority.toTag >> (+) 1 >> string)
            |> Option.defaultValue ""

        Ui.box
            (fun x ->
                x.position <- "relative"
                x.height <- $"{cellSize}px"
                x.lineHeight <- $"{cellSize}px")
            [
                str priorityText
            ]
