namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.State


module AttachmentIndicator =
    [<ReactComponent>]
    let AttachmentIndicator attachmentIdList =
        let cellSize = Store.useValue Atoms.User.cellSize
        let color = Store.useValue Atoms.User.color

        let tooltipContainerRef = React.useElementRef ()

        match attachmentIdList with
        | [] -> nothing
        | _ ->
            Chakra.box
                (fun x ->
                    x.ref <- tooltipContainerRef
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
                                x.borderTopColor <- color |> Option.defaultValue "#000"
                                x.borderLeftWidth <- $"{min (cellSize / 2) 10}px"
                                x.borderLeftColor <- "transparent"
                                x.position <- "absolute"
                                x.top <- "0"
                                x.right <- "0"))
                []
