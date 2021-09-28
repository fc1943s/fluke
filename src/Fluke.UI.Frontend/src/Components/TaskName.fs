namespace Fluke.UI.Frontend.Components

open FsCore
open Fable.React
open Feliz
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State
open FsJs
open FsStore
open FsStore.Hooks
open FsUi.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.TempUI
open Fluke.UI.Frontend.State.State
open FsUi.Components


module TaskName =
    open Domain.Model

    module Actions =
        let details =
            Atom.Primitives.setSelector
                (fun _getter setter (databaseId, taskId) ->
                    Profiling.addTimestamp (fun () -> $"{nameof Fluke} | TaskName.Actions.details") getLocals

                    Atom.set
                        setter
                        Navigate.Actions.navigate
                        (Navigate.DockPosition.Right,
                         Some DockType.Task,
                         UIFlagType.Task,
                         UIFlag.Task (databaseId, taskId)))

    [<ReactComponent>]
    let TaskName taskIdAtom =
        let taskId = Store.useValue taskIdAtom
        let details = Store.useSetState Actions.details
        let hasSelection = Store.useValue (Selectors.Task.hasSelection taskId)
        let name = Store.useValue (Atoms.Task.name taskId)
        let attachmentIdSet = Store.useValue (Selectors.Task.attachmentIdSet taskId)
        let cellSize = Store.useValue Atoms.User.cellSize
        let databaseId = Store.useValue (Atoms.Task.databaseId taskId)
        let isReadWrite = Store.useValue (Selectors.Database.isReadWrite databaseId)


        Ui.flex
            (fun x ->
                x.flex <- "1"
                x.alignItems <- "center"
                //                x.ref <- ref
                x.position <- "relative"
                x.height <- $"{cellSize}px")
            [
                Ui.box
                    (fun x ->
                        //                        x.backgroundColor <- if hovered then "#292929" else null
                        x.color <- if hasSelection then "#ff5656" else null
                        //                        x.zIndex <- if hovered then 1 else 0
                        x.overflow <- "hidden"
                        x.paddingLeft <- "5px"
                        x.paddingRight <- "5px"
                        x.lineHeight <- $"{cellSize}px"
                        x.whiteSpace <- "nowrap"
                        x.textOverflow <- "ellipsis")
                    [
                        match name |> TaskName.Value with
                        | "" -> LoadingSpinner.InlineLoadingSpinner ()
                        | name ->
                            str name

                            if not isReadWrite then
                                nothing
                            else
                                InputLabelIconButton.InputLabelIconButton
                                    (fun x ->
                                        x.icon <- Icons.fi.FiArrowRight |> Icons.render
                                        x.fontSize <- "11px"
                                        x.height <- "15px"
                                        x.color <- "whiteAlpha.700"
                                        x.marginTop <- "-1px"
                                        x.marginLeft <- "6px"
                                        x.onClick <- fun _ -> promise { details (databaseId, taskId) })
                    ]

                if not attachmentIdSet.IsEmpty then
                    AttachmentIndicator.AttachmentIndicator ()
                else
                    nothing
            ]
