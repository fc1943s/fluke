namespace Fluke.UI.Frontend.Components

open Fable.Core
open Fable.React
open Fable.Core.JsInterop
open Feliz
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared.Domain
open Fluke.UI.Frontend.Hooks


module Cell =
    open UserInteraction
    open State

    [<ReactComponent>]
    let Cell
        (input: {| TaskId: TaskId
                   DateId: DateId
                   SemiTransparent: bool |})
        =
        Profiling.addCount "- CellComponent.render"

        let cellSize = Store.useValue Atoms.cellSize
        let isTesting = Store.useValue Store.Atoms.isTesting
        let showUser = Store.useValue (Selectors.Task.showUser input.TaskId)
        let isReadWrite = Store.useValue (Selectors.Task.isReadWrite input.TaskId)
        let sessionStatus = Store.useValue (Selectors.Cell.sessionStatus (input.TaskId, input.DateId))
        let sessions = Store.useValue (Selectors.Cell.sessions (input.TaskId, input.DateId))
        let attachments = Store.useValue (Selectors.Cell.attachments (input.TaskId, input.DateId))
        let isToday = Store.useValue (Selectors.FlukeDate.isToday (input.DateId |> DateId.Value))
        let selected = Store.useValue (Selectors.Cell.selected (input.TaskId, input.DateId))
        let setSelected = Setters.useSetSelected ()

        let onCellClick =
            Store.useCallback (
                (fun getter _ _ ->
                    promise {
                        let ctrlPressed = Store.value getter Atoms.ctrlPressed
                        let shiftPressed = Store.value getter Atoms.shiftPressed

                        let newSelected =
                            if ctrlPressed || shiftPressed then
                                input.TaskId, input.DateId, not selected
                            else
                                input.TaskId, input.DateId, false

                        do! setSelected newSelected
                    }),
                [|
                    box input.TaskId
                    box input.DateId
                    box selected
                    box setSelected
                |]
            )

        Popover.CustomPopover
            {|
                CloseButton = false
                Padding = "3px"
                Placement = Some "right-start"
                Trigger =
                    Chakra.center
                        (fun x ->
                            if isTesting then
                                x?``data-testid`` <- $"cell-{input.TaskId}-{
                                                                                (input.DateId
                                                                                 |> DateId.Value
                                                                                 |> FlukeDate.DateTime)
                                                                                    .ToShortDateString ()
                                }"

                            if isReadWrite then x.onClick <- onCellClick
                            x.width <- $"{cellSize}px"
                            x.height <- $"{cellSize}px"
                            x.lineHeight <- $"{cellSize}px"
                            x.position <- "relative"

                            x.backgroundColor <-
                                (TempUI.cellStatusColor sessionStatus)
                                + (if isToday then "aa"
                                   elif input.SemiTransparent then "d9"
                                   else "")

                            x.textAlign <- "center"

                            x.borderColor <- if selected then "#ffffffAA" else "transparent"

                            x.borderWidth <- "1px"

                            if isReadWrite then
                                x.cursor <- "pointer"
                                x._hover <- JS.newObj (fun x -> x.borderColor <- "#ffffff55"))
                        [

                            CellSessionIndicator.CellSessionIndicator sessionStatus sessions

                            if selected then
                                nothing
                            else
                                CellBorder.CellBorder input.TaskId (input.DateId |> DateId.Value)

                            match showUser, sessionStatus with
                            | true, UserStatus (_username, _manualCellStatus) ->
                                CellStatusUserIndicator.CellStatusUserIndicator ()
                            | _ -> nothing

                            AttachmentIndicator.AttachmentIndicator attachments
                        ]
                Body =
                    fun (disclosure, _initialFocusRef) ->
                        [
                            if isReadWrite then
                                CellMenu.CellMenu input.TaskId input.DateId disclosure.onClose
                            else
                                nothing
                        ]
            |}
