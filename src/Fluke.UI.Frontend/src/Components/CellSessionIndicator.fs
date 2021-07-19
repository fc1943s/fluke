namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.State


module CellSessionIndicator =
    open Domain.State

    [<ReactComponent>]
    let CellSessionIndicator taskIdAtom dateIdAtom =
        let taskId = Store.useValue taskIdAtom
        let dateId = Store.useValue dateIdAtom
        let sessionStatus = Store.useValue (Selectors.Cell.sessionStatus (taskId, dateId))
        let sessionCount = Store.useValue (Selectors.Cell.sessionCount (taskId, dateId))

        UI.box
            (fun x ->
                x.fontSize <- "11px"

                x.color <-
                    match sessionStatus with
                    | UserStatus (_, Completed) -> "#ccc"
                    | _ -> "#999"

                x.textShadow <- "0 0 2px #000")
            [
                if sessionCount > 0 then str (string sessionCount) else nothing
            ]
