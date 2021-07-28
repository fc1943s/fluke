namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.State
open FsJs
open FsStore
open FsUi.Bindings
open Fluke.Shared


module CellBorder =
    open Domain.UserInteraction

    [<ReactComponent>]
    let CellBorder taskIdAtom dateIdAtom =
        let taskId = Store.useValue taskIdAtom
        let dateId = Store.useValue dateIdAtom
        let weekStart = Store.useValue Atoms.User.weekStart
        let cellSize = Store.useValue Atoms.User.cellSize
        let databaseId = Store.useValue (Atoms.Task.databaseId taskId)
        let isReadWrite = Store.useValue (Selectors.Database.isReadWrite databaseId)

        match (weekStart, dateId) with
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
