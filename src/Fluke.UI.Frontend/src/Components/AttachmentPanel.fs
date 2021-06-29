namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module AttachmentPanel =
    [<ReactComponent>]
    let AttachmentPanel attachments onAdd =

        Chakra.stack
            (fun x ->
                x.spacing <- "15px"
                x.flex <- "1")
            [
                Chakra.stack
                    (fun x ->
                        x.flex <- "1"
                        x.display <- "contents"
                        x.overflowY <- "auto"
                        x.flexBasis <- 0)
                    [
                        match attachments with
                        //                                                | None -> LoadingSpinner.LoadingSpinner ()
                        | [] ->
                            Chakra.box
                                (fun _ -> ())
                                [
                                    str "No attachments found"
                                ]
                        | attachments ->
                            Chakra.stack
                                (fun x -> x.spacing <- "10px")
                                [
                                    yield!
                                        attachments
                                        |> List.map
                                            (fun (moment, attachment) ->
                                                Chakra.stack
                                                    (fun x -> x.flex <- "1")
                                                    [
                                                        match attachment with
                                                        | Attachment.Comment (Comment.Comment comment) ->

                                                            Chakra.flex
                                                                (fun x ->
                                                                    x.justifyContent <- "space-between"

                                                                    x.color <- "whiteAlpha.600")
                                                                [
                                                                    Chakra.box
                                                                        (fun _ -> ())
                                                                        [
                                                                            str "Comment"
                                                                        ]

                                                                    Chakra.box
                                                                        (fun x -> x.lineHeight <- "16px")
                                                                        [
                                                                            str (moment |> FlukeDateTime.Stringify)
                                                                        ]
                                                                ]

                                                            Chakra.box
                                                                (fun _ -> ())
                                                                [
                                                                    str comment
                                                                ]
                                                        | _ -> str "???"
                                                    ])
                                ]
                    ]

                AddAttachmentInput.AddAttachmentInput onAdd
            ]
