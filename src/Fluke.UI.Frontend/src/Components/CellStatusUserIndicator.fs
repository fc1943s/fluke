namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open Fluke.Shared


module CellStatusUserIndicator =
    [<ReactComponent>]
    let CellStatusUserIndicator () =
        let color = Store.useValue Atoms.color
        let cellSize = Store.useValue Atoms.cellSize

        Chakra.box
            (fun x ->
                x.height <- $"{cellSize}px"
                x.lineHeight <- $"{cellSize}px"
                x.position <- "absolute"
                x.top <- "0"
                x.width <- "100%"

                x._after <-
                    (JS.newObj
                        (fun x ->
                            x.borderBottomWidth <- "8px"

                            x.borderBottomColor <-
                                match color with
                                | String.ValidString color -> color
                                | _ -> "#000000"

                            x.borderLeftWidth <- "8px"
                            x.borderLeftColor <- "transparent"
                            x.position <- "absolute"
                            x.content <- "\"\""
                            x.bottom <- "0"
                            x.right <- "0")))
            []
