namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.State


module TooltipPopup =

    open Domain.UserInteraction

    [<ReactComponent>]
    let TooltipPopup
        (input: {| Username: Username
                   Attachments: (FlukeDateTime * Attachment) list |})
        =
        let cellSize = Store.useValue (Atoms.User.cellSize input.Username)
        let color = Store.useValue (Atoms.User.color input.Username)

        let tooltipContainerRef = React.useElementRef ()

        let hovered = Listener.useElementHover tooltipContainerRef

        match input.Attachments with
        | [] -> nothing
        | _ ->
            Chakra.box
                (fun x ->
                    x.ref <- tooltipContainerRef
                    x.height <- $"{cellSize}px"
                    x.lineHeight <- $"{cellSize}px"
                    x.position <- "absolute"
                    x.top <- "-1px"
                    x.right <- "-1px"
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

                [
                    if not hovered then
                        nothing
                    else
                        Chakra.stack
                            (fun x ->
                                x.className <- "markdown-container"
                                x.spacing <- "20px"
                                x.padding <- "20px 40px"
                                x.minWidth <- "200px"
                                x.maxWidth <- "600px"
                                x.width <- "600px"
                                x.left <- "100%"
                                x.top <- "0"
                                x.whiteSpace <- "pre-wrap"
                                x.position <- "absolute"
                                x.zIndex <- 1
                                x.backgroundColor <- "#000c"
                                x.textAlign <- "left")

                            [
                                yield!
                                    input.Attachments
                                    |> List.choose
                                        (function
                                        | moment, Attachment.Comment comment -> Some (moment, comment)
                                        | _ -> None)
                                    |> List.map
                                        (fun (moment, (Comment.Comment comment)) ->
                                            Chakra.box
                                                (fun _ -> ())
                                                [
                                                    Chakra.box
                                                        (fun x -> x.fontWeight <- "normal")
                                                        [
                                                            str $"{moment |> FlukeDateTime.Stringify}:"
                                                        ]
                                                    Chakra.box
                                                        (fun _ -> ())
                                                        [
                                                            comment.Trim () |> Markdown.render
                                                        ]
                                                ])
                            ]
                ]
