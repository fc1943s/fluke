namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open Fluke.Shared


module CellStatusUserIndicator =
    [<ReactComponent>]
    let CellStatusUserIndicator () =
        let color = Store.useValue Atoms.User.color
        let cellSize = Store.useValue Atoms.User.cellSize

        Chakra.box
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
                                match color with
                                | Some (String.ValidString color) -> color
                                | _ -> "#000000"

                            x.borderBottomWidth <- $"{min (cellSize / 2) 10}px"
                            x.borderLeftColor <- "transparent"
                            x.borderLeftWidth <- $"{min (cellSize / 2) 10}px"
                            x.position <- "absolute"
                            x.content <- "\"\""
                            x.bottom <- "0"
                            x.right <- "0")))
            []
