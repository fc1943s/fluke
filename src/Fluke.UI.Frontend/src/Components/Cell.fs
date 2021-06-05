namespace Fluke.UI.Frontend.Components

open Fable.Core
open System
open Fable.React
open Fable.Core.JsInterop
open Feliz
open Feliz.Recoil
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared.Domain


module Cell =
    open UserInteraction
    open State

    [<ReactComponent>]
    let CellMenu
        (input: {| Username: Username
                   TaskId: TaskId
                   DateId: DateId
                   OnClose: unit -> unit |})
        =

        let cellSize = Recoil.useValue (Atoms.User.cellSize input.Username)
        let status, setStatus = Recoil.useState (Selectors.Cell.status (input.Username, input.TaskId, input.DateId))
        let dayStart = Recoil.useValue (Atoms.User.dayStart input.Username)

        let postponedUntil =
            match status with
            | UserStatus (_, Postponed (Some until)) -> Some until
            | _ -> None

        Chakra.stack
            (fun x ->
                x.spacing <- "0"
                x.borderWidth <- "1px"
                x.borderColor <- TempUI.cellStatusColor Disabled
                x.boxShadow <- "0px 0px 2px 1px #262626")
            [

                Chakra.simpleGrid
                    (fun x ->
                        x.columns <- 1
                        x.width <- $"{cellSize (* * 2*) }px")
                    [
                        let wrapButton icon color onClickStatus =
                            Chakra.iconButton
                                (fun x ->
                                    x.icon <- icon
                                    x._hover <- JS.newObj (fun x -> x.opacity <- 0.8)
                                    x.variant <- "outline"
                                    x.backgroundColor <- color
                                    x.border <- "0"
                                    x.minWidth <- $"{cellSize}px"
                                    x.height <- $"{cellSize}px"
                                    x.borderRadius <- "0"

                                    match onClickStatus with
                                    | Some onClickStatus ->
                                        x.onClick <-
                                            fun _ ->
                                                promise {
                                                    setStatus onClickStatus
                                                    input.OnClose ()
                                                }
                                    | None -> ())
                                []

                        let wrapButtonTooltip status tooltipLabel =
                            let color = TempUI.manualCellStatusColor status

                            Tooltip.wrap
                                tooltipLabel
                                [
                                    wrapButton None color (UserStatus (input.Username, status) |> Some)
                                ]

                        Chakra.box
                            (fun _ -> ())
                            [
                                str "Complete"
                            ]
                        |> wrapButtonTooltip Completed

                        Chakra.box
                            (fun _ -> ())
                            [
                                Chakra.box
                                    (fun _ -> ())
                                    [
                                        str "Dismiss"
                                    ]
                                Chakra.box
                                    (fun _ -> ())
                                    [
                                        str """???"""
                                    ]
                            ]
                        |> wrapButtonTooltip Dismissed

                        Chakra.box
                            (fun _ -> ())
                            [
                                str "Postpone until tomorrow"
                            ]
                        |> wrapButtonTooltip (Postponed None)

                        Popover.Popover
                            {|
                                Trigger =
                                    wrapButton
                                        None
                                        (postponedUntil
                                         |> Option.defaultValue dayStart
                                         |> Some
                                         |> Postponed
                                         |> TempUI.manualCellStatusColor)
                                        None
                                Body =
                                    fun (_disclosure, initialFocusRef) ->
                                        [
                                            Chakra.stack
                                                (fun x -> x.spacing <- "10px")
                                                [
                                                    Chakra.box
                                                        (fun x ->
                                                            x.paddingBottom <- "5px"
                                                            x.fontSize <- "15px")
                                                        [
                                                            str "Postpone until later"
                                                        ]

                                                    Input.Input
                                                        (fun x ->
                                                            x.label <- str "Time"
                                                            x.placeholder <- "00:00"
                                                            x.ref <- initialFocusRef
                                                            x.value <- postponedUntil
                                                            x.inputFormat <- Some Input.InputFormat.Time

                                                            x.onChange <-
                                                                fun (e: Browser.Types.KeyboardEvent) ->
                                                                    promise { printfn $"val={e.Value}" }

                                                            x.onFormat <- Some FlukeTime.Stringify

                                                            x.onValidate <-
                                                                Some (
                                                                    fst
                                                                    >> DateTime.TryParse
                                                                    >> function
                                                                    | true, date ->
                                                                        date |> FlukeTime.FromDateTime |> Some
                                                                    | _ -> None
                                                                ))

                                                    Chakra.box
                                                        (fun _ -> ())
                                                        [
                                                            Button.Button
                                                                {|
                                                                    Hint = None
                                                                    Icon =
                                                                        Some (
                                                                            Icons.fi.FiKey |> Icons.wrap,
                                                                            Button.IconPosition.Left
                                                                        )
                                                                    Props = fun x -> () //x.onClick <- signUpClick
                                                                    Children =
                                                                        [
                                                                            str "Confirm"
                                                                        ]
                                                                |}
                                                        ]
                                                ]
                                        ]
                            |}

                        Chakra.box
                            (fun x -> x.padding <- "4px")
                            [
                                Chakra.box
                                    (fun _ -> ())
                                    [
                                        str "Schedule"
                                    ]
                                Chakra.box
                                    (fun x -> x.marginTop <- "8px")
                                    [
                                        str
                                            """Manually schedule a task,
overriding any other behavior.
"""
                                    ]
                            ]
                        |> wrapButtonTooltip Scheduled

                        match status with
                        | UserStatus _ ->
                            Tooltip.wrap
                                (str "Clear")
                                [
                                    wrapButton
                                        (Icons.md.MdClear |> Icons.render |> Some)
                                        (TempUI.cellStatusColor Disabled)
                                        (Some Disabled)
                                ]
                        | _ -> nothing
                    ]
            ]

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
        let taskMetadata = Recoil.useValue (Selectors.Session.taskMetadata input.Username)
        let isReadWrite = Recoil.useValue (Selectors.Database.isReadWrite taskMetadata.[input.TaskId].DatabaseId)
        let status = Recoil.useValue (Selectors.Cell.status (input.Username, input.TaskId, input.DateId))
        let sessions = Recoil.useValue (Atoms.Cell.sessions (input.TaskId, input.DateId))
        let attachments = Recoil.useValue (Atoms.Cell.attachments (input.TaskId, input.DateId))
        let showUser = Recoil.useValue (Selectors.Task.showUser (input.Username, input.TaskId))
        let isToday = Recoil.useValue (Selectors.FlukeDate.isToday (input.DateId |> DateId.Value))

        let selected, setSelected =
            Recoil.useState (Selectors.Cell.selected (input.Username, input.TaskId, input.DateId))

        let onCellClick =
            Recoil.useCallbackRef
                (fun setter _ ->
                    promise {
                        let! ctrlPressed = setter.snapshot.getPromise Atoms.ctrlPressed
                        let! shiftPressed = setter.snapshot.getPromise Atoms.shiftPressed

                        if ctrlPressed || shiftPressed then
                            setSelected (not selected)
                        else
                            setSelected false
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

                            x.onClick <- onCellClick
                            x.width <- $"{cellSize}px"
                            x.height <- $"{cellSize}px"
                            x.lineHeight <- $"{cellSize}px"

                            x.backgroundColor <-
                                (TempUI.cellStatusColor status)
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
                                    Status = status
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

                            match showUser, status with
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
                            CellMenu
                                {|
                                    Username = input.Username
                                    TaskId = input.TaskId
                                    DateId = input.DateId
                                    OnClose = disclosure.onClose
                                |}
                        ]
            |}
