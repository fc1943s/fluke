namespace Fluke.UI.Frontend.Components

open System
open FSharpPlus
open Fable.React
open Suigetsu.Core
open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.Shared


module TooltipPopupComponent =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun (input: {| Attachments: Attachment list |}) ->
            let tooltipContainerRef = React.useElementRef ()

            let hovered = Temp.UseListener.onElementHover tooltipContainerRef

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
                                        |> List.map (fun (({ Username = Username username } as user), (Comment.Comment comment)) ->
                                            sprintf "%s:%s%s" username Environment.NewLine (comment.Trim ()))
                                        |> List.map ((+) Environment.NewLine)
                                        |> String.concat (Environment.NewLine + Environment.NewLine)
                                        |> fun text ->
                                            match hovered with
                                            | false -> nothing
                                            | true ->
                                                ReactBindings.React.createElement
                                                    (Ext.reactMarkdown, {| source = text |}, [])
                                    ]
                            ]
                        ]
                ])
