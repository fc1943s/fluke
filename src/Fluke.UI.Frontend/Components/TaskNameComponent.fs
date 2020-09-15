namespace Fluke.UI.Frontend.Components

open Feliz.MaterialUI
open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.Shared.Model


module TaskNameComponent =
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
        React.memo (fun (input: {| Css: IStyleAttribute list
                                   TaskId: Recoil.Atoms.RecoilTask.TaskId |}) ->
            let ref = React.useElementRef ()
            let hovered = Temp.UseListener.onElementHover ref
            let classes = useStyles {| hovered = hovered |}

            // TODO: put inside RecoilTask object?
            let hasSelection = Recoil.useValue (Recoil.Selectors.RecoilTask.hasSelectionFamily input.TaskId)
            let task = Recoil.useValue (Recoil.Atoms.RecoilTask.taskFamily input.TaskId)

            let (TaskName taskName) = Recoil.useValue task.Name
            let attachments = Recoil.useValue task.Attachments

            Html.div [
                prop.ref ref
                prop.classes [
                    classes.root
                    Css.cellRectangle
                ]
                prop.children [
                    Html.div [
                        prop.classes [
                            classes.name
                            if hasSelection then
                                Css.selectionHighlight
                        ]
                        prop.style
                            [
                                yield! input.Css
                            ]
                        prop.children
                            [
                                str taskName
                            ]
                    ]
                    TooltipPopupComponent.render {| Attachments = attachments |}
                ]
            ])
