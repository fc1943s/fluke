namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Feliz.Recoil
open Fluke.UI.Frontend.State


module TooltipPopup =

    open Domain.UserInteraction

    [<ReactComponent>]
    let TooltipPopup
        (input: {| Username: Username
                   Attachments: Attachment list |})
        =
        let cellSize = Recoil.useValue (Atoms.User.cellSize input.Username)

        let tooltipContainerRef = React.useElementRef ()

        let hovered = Listener.useElementHover tooltipContainerRef

        let comments =
            input.Attachments
            |> List.choose
                (fun x ->
                    match x with
                    | Attachment.Comment (user, comment) -> Some (user, comment)
                    | _ -> None)

        match comments with
        | [] -> nothing
        | _ ->
            Chakra.box
                (fun x ->
                    x.ref <- tooltipContainerRef
                    x.height <- $"{cellSize}px"
                    x.lineHeight <- $"{cellSize}px"
                    x.position <- "absolute"
                    x.top <- "0"
                    x.width <- "100%"

                    x._after <-
                        JS.newObj
                            (fun x ->
                                x.content <- "\"\""
                                x.borderTopWidth <- "8px"
                                x.borderTopColor <- "#000000"
                                x.borderLeftWidth <- "8px"
                                x.borderLeftColor <- "transparent"
                                x.position <- "absolute"
                                x.top <- "0"
                                x.right <- "0"))

                [
                    if hovered then
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
                                    comments
                                    |> List.map
                                        (fun ((Username username), (Comment.Comment comment)) ->
                                            Chakra.box
                                                (fun _ -> ())
                                                [
                                                    Chakra.box
                                                        (fun x -> x.fontWeight <- "normal")
                                                        [
                                                            str $"{username}:"
                                                        ]
                                                    Chakra.box
                                                        (fun _ -> ())
                                                        [
                                                            comment.Trim () |> Markdown.render
                                                        ]
                                                ])
                            ]
                ]
