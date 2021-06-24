namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.State


module AttachmentIndicator =

    open Domain.UserInteraction

    [<ReactComponent>]
    let AttachmentIndicator (input: {| Attachments: (FlukeDateTime * Attachment) list |}) =
        let cellSize = Store.useValue Atoms.cellSize
        let color = Store.useValue Atoms.color

        let tooltipContainerRef = React.useElementRef ()

        match input.Attachments with
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
                    x.width <- "100%"

                    x._after <-
                        JS.newObj
                            (fun x ->
                                x.content <- "\"\""
                                x.borderTopWidth <- "8px"
                                x.borderTopColor <- color
                                x.borderLeftWidth <- "8px"
                                x.borderLeftColor <- "transparent"
                                x.position <- "absolute"
                                x.top <- "0"
                                x.right <- "0"))
                []
