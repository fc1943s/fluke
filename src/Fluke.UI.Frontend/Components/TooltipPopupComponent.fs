namespace Fluke.UI.Frontend.Components

open System
open FSharpPlus
open Fable.React
open Suigetsu.Core
open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.Shared.Model


module TooltipPopupComponent =
    let render =
        React.memo (fun (input: {| Attachments: Attachment list |}) ->
            let tooltipContainerRef = React.useElementRef ()

            let hovered =
                Temp.UseListener.onElementHover tooltipContainerRef

            let comments =
                input.Attachments
                |> List.choose ofAttachmentComment

            match comments with
            | [] -> nothing
            | _ ->
                let user = // TODO: 2+ different users in the same day. what to show? smooth transition between them?
                    fst comments.Head

                Html.div [
                    prop.ref tooltipContainerRef
                    prop.classes [
                        Css.tooltipContainer
                        match user with
                        | { Color = UserColor.Blue } -> Css.topRightBlueIndicator
                        | { Color = UserColor.Pink } -> Css.topRightPinkIndicator
                        | _ -> ()
                    ]
                    prop.children
                        [
                            Html.div [
                                prop.className Css.tooltipPopup
                                prop.children
                                    [
                                        comments
                                        |> List.map (fun (user, (Comment comment)) ->
                                            sprintf "%s:%s%s" user.Username Environment.NewLine (comment.Trim ()))
                                        |> List.map ((+) Environment.NewLine)
                                        |> String.concat (Environment.NewLine + Environment.NewLine)
                                        |> fun text ->
                                            match hovered with
                                            | false -> nothing
                                            | true ->
                                                ReactBindings.React.createElement
                                                    (Ext.reactMarkdown,
                                                     {|
                                                         source = text
                                                     |},
                                                     [])
                                    ]
                            ]
                        ]
                ])
