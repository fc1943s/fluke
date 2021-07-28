namespace Fluke.UI.Frontend.Components

open FsCore.Model
open FsJs
open Feliz
open FsStore
open FsUi.Bindings
open Fluke.UI.Frontend.State
open Fable.React
open Fable.Core
open Fluke.UI.Frontend
open Fluke.Shared.Domain


module CellStatusUserIndicator =
    open State

    [<ReactComponent>]
    let CellStatusUserIndicator taskIdAtom dateIdAtom =
        let taskId = Store.useValue taskIdAtom
        let dateId = Store.useValue dateIdAtom
        let userColor = Store.useValue Atoms.User.userColor
        let cellSize = Store.useValue Atoms.User.cellSize
        let showUser = Store.useValue (Selectors.Task.showUser taskId)
        let sessionStatus = Store.useValue (Selectors.Cell.sessionStatus (taskId, dateId))

        match showUser, sessionStatus with
        | true, UserStatus _ ->
            UI.box
                (fun x ->
                    x.height <- $"{cellSize}px"
                    x.lineHeight <- $"{cellSize}px"
                    x.position <- "absolute"
                    x.top <- "0"
                    //                x.width <- "100%"

                    x._after <-
                        (JS.newObj
                            (fun x ->

                                x.borderBottomColor <-
                                    userColor
                                    |> Option.map Color.ValueOrDefault
                                    |> Option.get

                                x.borderBottomWidth <- $"{min (cellSize / 2) 10}px"
                                x.borderLeftColor <- "transparent"
                                x.borderLeftWidth <- $"{min (cellSize / 2) 10}px"
                                x.position <- "absolute"
                                x.content <- "\"\""
                                x.bottom <- "0"
                                x.right <- "0")))
                []
        | _ -> nothing
