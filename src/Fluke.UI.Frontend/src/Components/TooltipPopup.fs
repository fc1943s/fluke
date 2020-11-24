namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module TooltipPopup =
    open Domain.UserInteraction

    let render =
        React.memo (fun (input: {| Attachments: Attachment list |}) ->
            let tooltipContainerRef = React.useElementRef ()

            let hovered = Listener.useElementHover tooltipContainerRef

            let comments =
                input.Attachments
                |> List.choose (fun x ->
                    match x with
                    | Attachment.Comment (user, comment) -> Some (user, comment)
                    | _ -> None)

            match comments with
            | [] -> nothing
            | _ ->
                let user = // TODO: 2+ different users in the same day. what to show? smooth transition between them?
                    fst comments.Head

                Chakra.box
                    {|
                        ref = tooltipContainerRef
                        height = "17px"
                        lineHeight = "17px"
                        position = "absolute"
                        top = 0
                        width = "100%"
                        _after =
                            {|
                                borderTopColor =
                                    match user with
                                    | { Color = UserColor.Blue } -> Some "#005688"
                                    | { Color = UserColor.Pink } -> Some "#a91c77"
                                    | _ -> None
                                borderTopWidth = "8px"
                                borderLeftColor = "transparent"
                                borderLeftWidth = "8px"
                                position = "absolute"
                                content = "\"\""
                                top = 0
                                right = 0
                            |}
                    |}

                    [
                        if hovered then
                            Chakra.stack
                                {|
                                    className = "markdown-container"
                                    spacing = "20px"
                                    padding = "20px 40px"
                                    minWidth = "200px"
                                    maxWidth = "600px"
                                    width = "600px"
                                    left = "100%"
                                    top = 0
                                    whiteSpace = "pre-wrap"
                                    position = "absolute"
                                    zIndex = 1
                                    backgroundColor = "#000c"
                                    textAlign = "left"
                                |}

                                [
                                    yield!
                                        comments
                                        |> List.map (fun ({ Username = Username username }, (Comment.Comment comment)) ->
                                            Chakra.box
                                                {|  |}
                                                [
                                                    Chakra.box
                                                        {| fontWeight = "normal" |}
                                                        [
                                                            str $"{username}:"
                                                        ]
                                                    Chakra.box
                                                        {|  |}
                                                        [
                                                            comment.Trim () |> Markdown.render
                                                        ]
                                                ])
                                ]
                    ])
