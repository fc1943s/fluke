namespace Fluke.UI.Frontend.Components

open Fable.Core
open Fable.React
open Fable.Core.JsInterop
open Feliz
open Feliz.Recoil
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
        Profiling.addCount "CellComponent.render"

        let cellSize = Recoil.useValue (Atoms.User.cellSize input.Username)
        let isTesting = Recoil.useValue Atoms.isTesting
        let showUser = Recoil.useValueLoadableDefault (Selectors.Task.showUser (input.Username, input.TaskId)) false

        let isReadWrite =
            Recoil.useValueLoadableDefault (Selectors.Task.isReadWrite (input.Username, input.TaskId)) false

        let sessionStatus = Recoil.useValue (Selectors.Cell.sessionStatus (input.Username, input.TaskId, input.DateId))
        let sessions = Recoil.useValue (Atoms.Cell.sessions (input.TaskId, input.DateId))
        let attachments = Recoil.useValue (Atoms.Cell.attachments (input.TaskId, input.DateId))
        let isToday = Recoil.useValueLoadableDefault (Selectors.FlukeDate.isToday (input.DateId |> DateId.Value)) false
        let selected = Recoil.useValue (Atoms.Cell.selected (input.Username, input.TaskId, input.DateId))

        let setSelected = Setters.useSetSelected ()

        let onCellClick =
            Recoil.useCallbackRef
                (fun setter _ ->
                    promise {
                        let! ctrlPressed = setter.snapshot.getPromise Atoms.ctrlPressed
                        let! shiftPressed = setter.snapshot.getPromise Atoms.shiftPressed

                        if ctrlPressed || shiftPressed then
                            do! setSelected (input.Username, input.TaskId, input.DateId, not selected)
                        else
                            do! setSelected (input.Username, input.TaskId, input.DateId, false)
                    })

        Popover.CustomPopover
            {|
                CloseButton = false
                Padding = "3px"
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

                            CellSessionIndicator.CellSessionIndicator
                                {|
                                    Status = sessionStatus
                                    Sessions = sessions
                                |}

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

                            TooltipPopup.TooltipPopup
                                {|
                                    Username = input.Username
                                    Attachments = attachments
                                |}
                        ]
                Body =
                    fun (disclosure, _initialFocusRef) ->
                        [
                            if isReadWrite then
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
