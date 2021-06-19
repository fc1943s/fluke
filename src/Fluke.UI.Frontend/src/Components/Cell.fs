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
        (input: {| Username: Username
                   TaskId: TaskId
                   DateId: DateId
                   SemiTransparent: bool |})
        =
        Profiling.addCount "- CellComponent.render"

        let cellSize = Store.useValue (Atoms.User.cellSize input.Username)
        let isTesting = Store.useValue Atoms.isTesting
        let showUser = Store.useValueLoadableDefault (Selectors.Task.showUser (input.Username, input.TaskId)) false

        let isReadWrite =
            Store.useValueLoadableDefault (Selectors.Task.isReadWrite (input.Username, input.TaskId)) false

        let sessionStatus =
            Store.useValueLoadableDefault
                (Selectors.Cell.sessionStatus (input.Username, input.TaskId, input.DateId))
                Disabled

        let sessions = Store.useValueLoadable (Selectors.Cell.sessions (input.Username, input.TaskId, input.DateId))

        let attachments =
            Store.useValueLoadable (Selectors.Cell.attachments (input.Username, input.TaskId, input.DateId))

        let isToday = Store.useValueLoadableDefault (Selectors.FlukeDate.isToday (input.DateId |> DateId.Value)) false

        let selected =
            Store.useValueLoadableDefault (Selectors.Cell.selected (input.Username, input.TaskId, input.DateId)) false

        let setSelected = Setters.useSetSelected ()

        let onCellClick =
            Store.useCallbackRef
                (fun setter _ ->
                    promise {
                        let! ctrlPressed = setter.snapshot.getPromise Atoms.ctrlPressed
                        let! shiftPressed = setter.snapshot.getPromise Atoms.shiftPressed

                        let newSelected =
                            if ctrlPressed || shiftPressed then
                                input.Username, input.TaskId, input.DateId, not selected
                            else
                                input.Username, input.TaskId, input.DateId, false

                        do! setSelected newSelected
                    })

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

                            match sessions.valueMaybe () with
                            | Some sessions ->
                                CellSessionIndicator.CellSessionIndicator
                                    {|
                                        Status = sessionStatus
                                        Sessions = sessions
                                    |}
                            | _ -> nothing

                            if selected then
                                nothing
                            else
                                CellBorder.CellBorder
                                    {|
                                        Username = input.Username
                                        Date = input.DateId |> DateId.Value
                                    |}

                            match showUser, sessionStatus with
                            | true, UserStatus (username, _manualCellStatus) ->
                                CellStatusUserIndicator.CellStatusUserIndicator {| Username = username |}
                            | _ -> nothing

                            match attachments.valueMaybe () with
                            | Some attachments ->
                                TooltipPopup.TooltipPopup
                                    {|
                                        Username = input.Username
                                        Attachments = attachments
                                    |}
                            | _ -> nothing
                        ]
                Body =
                    fun (disclosure, _initialFocusRef) ->
                        [
                            if isReadWrite && not isTesting then
                                CellMenu.CellMenu
                                    {|
                                        Username = input.Username
                                        TaskId = input.TaskId
                                        DateId = input.DateId
                                        OnClose = disclosure.onClose
                                    |}
                            else
                                nothing
                        ]
            |}
