namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.State
open FsJs
open FsStore
open FsStore.Hooks
open FsUi.Bindings
open Fluke.Shared


module CellBorder =
    open Domain.UserInteraction

    [<ReactComponent>]
    let CellBorder taskIdAtom dateAtom =
        let taskId, dateId = Store.useValueTuple taskIdAtom dateAtom
        let weekStart = Store.useValue Atoms.User.weekStart
        let cellWidth = Store.useValue Atoms.User.cellWidth
        let databaseId = Store.useValue (Atoms.Task.databaseId taskId)
        let isReadWrite = Store.useValue (Selectors.Database.isReadWrite databaseId)

        let borderLeftWidth, borderLeftColor =
            React.useMemo (
                (fun () ->
                    match (weekStart, dateId) with
                    | StartOfMonth -> Some "1px", Some "#ffffff3d"
                    | StartOfWeek -> Some "1px", Some "#222"
                    | _ -> None, None),
                [|
                    box weekStart
                    box dateId
                |]
            )

        match borderLeftWidth, borderLeftColor with
        | Some borderLeftWidth, Some borderLeftColor ->
            Ui.box
                (fun x ->
                    x.position <- "absolute"
                    x.top <- "-1px"
                    x.left <- "-1px"
                    x.bottom <- "-1px"
                    x.width <- $"{cellWidth}px"

                    if isReadWrite then
                        x._hover <- Js.newObj (fun x -> x.borderLeftWidth <- "0")

                    x.borderLeftWidth <- borderLeftWidth
                    x.borderLeftColor <- borderLeftColor)
                []
        | _ -> nothing
