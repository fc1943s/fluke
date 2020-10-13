namespace Fluke.UI.Frontend.Components

open Feliz.MaterialUI
open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module TaskNameComponent =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    let useStyles =
        Styles.makeStyles (fun (styles: StyleCreator<{| hovered: bool |}>) _theme ->
            {|
                root =
                    styles.create (fun props ->
                        [
                            if props.hovered then
                                style.zIndex 1
                        ])
                name =
                    styles.create (fun props ->
                        [
                            style.overflow.hidden
                            if props.hovered then
                                style.backgroundColor "#333"
                            else
                                style.whitespace.nowrap
                                style.textOverflow.ellipsis
                        ])
            |})

    let render =
        React.memo (fun (input: {| TaskId: Recoil.Atoms.Task.TaskId |}) ->
            let ref = React.useElementRef ()
            let hovered = Listener.useElementHover ref
            let classes = useStyles {| hovered = hovered |}
            let hasSelection = Recoil.useValue (Recoil.Selectors.Task.hasSelection input.TaskId)
            let (TaskName taskName) = Recoil.useValue (Recoil.Atoms.Task.name input.TaskId)
            let attachments = Recoil.useValue (Recoil.Atoms.Task.attachments input.TaskId)

            Chakra.box
                {|
                    ref = ref
                    position = "relative"
                    height = "17px"
                    lineHeight = "17px"
                    className = classes.root
                |}
                [
                    Chakra.box
                        {|
                            color =
                                if hasSelection then
                                    "#ff5656"
                                else
                                    ""
                            className =
                                [
                                    classes.name
                                ]
                                |> String.concat " "
                        |}
                        [

                            str taskName
                        ]
                    TooltipPopupComponent.render {| Attachments = attachments |}
                ])
