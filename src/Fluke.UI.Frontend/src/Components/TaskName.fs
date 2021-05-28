namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module TaskName =
    open Domain.Model

    [<ReactComponent>]
    let TaskName (input: {| Username: Username; TaskId: TaskId |}) =
        let ref = React.useElementRef ()
        let hovered = Listener.useElementHover ref
        let hasSelection = Recoil.useValue (Selectors.Task.hasSelection input.TaskId)
        let (TaskName taskName) = Recoil.useValue (Atoms.Task.name (input.Username, input.TaskId))
        let attachments = Recoil.useValue (Atoms.Task.attachments input.TaskId)
        let cellSize = Recoil.useValue (Atoms.User.cellSize input.Username)

        Chakra.box
            (fun x ->
                x.flex <- "1"
                x.ref <- ref
                x.position <- "relative"
                x.height <- $"{cellSize}px"
                x.lineHeight <- $"{cellSize}px"
                x.zIndex <- if hovered then 1 else 0)
            [
                Chakra.box
                    (fun x ->
                        x.color <- if hasSelection then "#ff5656" else null
                        x.overflow <- "hidden"
                        x.backgroundColor <- if hovered then "#333" else null
                        x.whiteSpace <- if not hovered then "nowrap" else null
                        x.textOverflow <- if not hovered then "ellipsis" else null)
                    [
                        str taskName
                    ]

                TooltipPopup.TooltipPopup
                    {|
                        Username = input.Username
                        Attachments = attachments
                    |}
            ]
