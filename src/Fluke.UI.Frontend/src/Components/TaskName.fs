namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings
open Fluke.Shared.Domain.State
open Fluke.Shared


module TaskName =
    open Domain.Model

    [<ReactComponent>]
    let TaskName (taskId: TaskId) =
        let ref = React.useElementRef ()
        let hovered = Listener.useElementHover ref
        let hasSelection = Recoil.useValue (Recoil.Selectors.Task.hasSelection taskId)
        let (TaskName taskName) = Recoil.useValue (Recoil.Atoms.Task.name (Some taskId))
        let attachments = Recoil.useValue (Recoil.Atoms.Task.attachments (Some taskId))

        Chakra.box
            {|
                flex = 1
                ref = ref
                position = "relative"
                height = "17px"
                lineHeight = "17px"
                zIndex = if hovered then Some 1 else None
            |}
            [
                Chakra.box
                    {|
                        color = if hasSelection then Some "#ff5656" else None
                        overflow = "hidden"
                        backgroundColor = if hovered then Some "#333" else None
                        whiteSpace = if not hovered then Some "nowrap" else None
                        textOverflow = if not hovered then Some "ellipsis" else None
                    |}
                    [
                        str taskName
                    ]

                TooltipPopup.TooltipPopup attachments
            ]
