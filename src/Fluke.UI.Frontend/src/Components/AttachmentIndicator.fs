namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.State


module AttachmentIndicator =
    [<ReactComponent>]
    let AttachmentIndicator () =
        let cellSize = Store.useValue Atoms.User.cellSize
        let userColor = Store.useValue Atoms.User.userColor

        UI.box
            (fun x ->
                x.height <- $"{cellSize}px"
                x.lineHeight <- $"{cellSize}px"
                x.position <- "absolute"
                x.top <- "0px"
                x.right <- "0px"

                x._after <-
                    JS.newObj
                        (fun x ->
                            x.content <- "\"\""
                            x.borderTopWidth <- $"{min (cellSize / 2) 10}px"

                            x.borderTopColor <-
                                userColor
                                |> Option.defaultValue Color.Default
                                |> Color.ValueOrDefault

                            x.borderLeftWidth <- $"{min (cellSize / 2) 10}px"
                            x.borderLeftColor <- "transparent"
                            x.position <- "absolute"
                            x.top <- "0"
                            x.right <- "0"))
            []
