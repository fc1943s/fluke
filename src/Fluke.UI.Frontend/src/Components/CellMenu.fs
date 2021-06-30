namespace Fluke.UI.Frontend.Components

open System
open Fable.Core.JsInterop
open Fable.React
open Feliz
open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared.Domain


module CellMenu =
    open UserInteraction
    open State

    [<ReactComponent>]
    let CellMenu (taskId: TaskId) (dateId: DateId) (onClose: unit -> unit) =
        let isTesting = Store.useValue Store.Atoms.isTesting
        let username = Store.useValue Store.Atoms.username
        let toast = Chakra.useToast ()
        let cellSize = Store.useValue Atoms.User.cellSize
        let sessionStatus, setSessionStatus = Store.useState (Selectors.Cell.sessionStatus (taskId, dateId))
        let cellSelectionMap = Store.useValue Selectors.Session.cellSelectionMap
        let dayStart = Store.useValue Atoms.User.dayStart
        let darkMode = Store.useValue Atoms.User.darkMode

        let postponedUntil, setPostponedUntil =
            React.useState (
                match sessionStatus with
                | UserStatus (_, Postponed (Some until)) -> Some until
                | _ -> None
            )

        let postponedUntilLabel =
            match sessionStatus with
            | UserStatus (_, Postponed (Some until)) -> until |> FlukeTime.Stringify
            | _ -> "later"

        let setRightDock = Store.useSetState Atoms.User.rightDock
        let setCellUIFlag = Store.useSetState (Atoms.User.uiFlag UIFlagType.Cell)

        let onClick =
            Store.useCallback (
                (fun _ setter (onClickStatus: CellStatus) ->
                    promise {
                        cellSelectionMap
                        |> Map.iter
                            (fun taskId dates ->
                                dates
                                |> Set.iter
                                    (fun date ->
                                        Store.set
                                            setter
                                            (Selectors.Cell.sessionStatus (taskId, DateId date))
                                            onClickStatus))

                        Store.set setter (Selectors.Cell.sessionStatus (taskId, dateId)) onClickStatus

                        cellSelectionMap
                        |> Map.keys
                        |> Seq.iter (fun taskId -> Store.set setter (Atoms.Task.selectionSet taskId) Set.empty)

                        onClose ()
                    }),
                [|
                    box onClose
                    box taskId
                    box dateId
                    box cellSelectionMap
                |]
            )

        let postponeUntilLater =
            Store.useCallback (
                (fun _ _ _ ->
                    promise {
                        match username, postponedUntil with
                        | Some username, Some postponedUntil ->
                            setSessionStatus (UserStatus (username, Postponed (Some postponedUntil)))
                            onClose ()
                        | _ -> toast (fun x -> x.description <- "Invalid time")
                    }),
                [|
                    box onClose
                    box username
                    box postponedUntil
                    box setSessionStatus
                    box toast
                |]
            )

        Chakra.stack
            (fun x ->
                x.spacing <- "0"
                x.borderWidth <- "1px"
                x.borderColor <- if darkMode then TempUI.cellStatusColor Disabled else "gray.45"
                x.boxShadow <- $"0px 0px 2px 1px #{if darkMode then 262626 else 777}")
            [
                Chakra.simpleGrid
                    (fun x ->
                        x.columns <- 1
                        x.borderColor <- "gray.77"
                        x.backgroundColor <- if darkMode then "#636363" else "gray.45"
                        x.spacing <- "1px"
                        x.width <- $"{cellSize (* * 2*) }px")
                    [
                        let wrapButton icon color onClick =
                            Chakra.iconButton
                                (fun x ->
                                    if isTesting then x?``data-testid`` <- $"cell-button-{color}"
                                    x.icon <- icon
                                    x.color <- "#dddddd"
                                    x._hover <- JS.newObj (fun x -> x.opacity <- 0.8)
                                    x.variant <- "outline"
                                    x.backgroundColor <- color
                                    x.border <- "0"
                                    x.minWidth <- $"{cellSize}px"
                                    x.height <- $"{cellSize}px"
                                    x.borderRadius <- "0"

                                    match onClick with
                                    | Some onClick -> x.onClick <- fun _ -> onClick ()
                                    | None -> ())
                                []

                        let wrapButtonStatus icon color status =
                            wrapButton icon color (Some (fun () -> onClick status))

                        let wrapButtonTooltip status tooltipLabel =
                            let color = TempUI.manualCellStatusColor status

                            Tooltip.wrap
                                tooltipLabel
                                [
                                    match username with
                                    | Some username -> wrapButtonStatus None color (UserStatus (username, status))
                                    | _ -> nothing
                                ]

                        Tooltip.wrap
                            (str "Details")
                            [
                                wrapButton
                                    (Icons.fi.FiArrowRight |> Icons.render |> Some)
                                    (TempUI.cellStatusColor Pending)
                                    (Some
                                        (fun () ->
                                            promise {
                                                setRightDock (Some TempUI.DockType.Cell)
                                                setCellUIFlag (UIFlag.Cell (taskId, dateId))
                                                onClose ()
                                            }))
                            ]

                        match sessionStatus with
                        | UserStatus _ ->
                            Tooltip.wrap
                                (str "Clear")
                                [
                                    wrapButtonStatus
                                        (Icons.md.MdClear |> Icons.render |> Some)
                                        (TempUI.cellStatusColor Disabled)
                                        Disabled
                                ]
                        | _ -> nothing

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
                                    Tooltip.wrap
                                        (str $"Postpone until {postponedUntilLabel}")
                                        [
                                            wrapButton
                                                None
                                                (postponedUntil
                                                 |> Option.defaultValue dayStart
                                                 |> Some
                                                 |> Postponed
                                                 |> TempUI.manualCellStatusColor)
                                                None
                                        ]
                                Body =
                                    fun (_disclosure, initialFocusRef) ->
                                        [
                                            Chakra.stack
                                                (fun x -> x.spacing <- "10px")
                                                [
                                                    Chakra.box
                                                        (fun x ->
                                                            x.paddingBottom <- "5px"
                                                            x.marginRight <- "24px"
                                                            x.fontSize <- "15px")
                                                        [
                                                            str $"Postpone until {postponedUntilLabel}"
                                                        ]

                                                    Input.Input
                                                        {|
                                                            CustomProps =
                                                                fun x ->
                                                                    x.fixedValue <- postponedUntil
                                                                    x.inputFormat <- Some Input.InputFormat.Time
                                                                    x.onFormat <- Some FlukeTime.Stringify

                                                                    x.onValidate <-
                                                                        Some (
                                                                            fst
                                                                            >> DateTime.TryParse
                                                                            >> function
                                                                            | true, date ->
                                                                                date |> FlukeTime.FromDateTime |> Some
                                                                            | _ -> None
                                                                        )
                                                            Props =
                                                                fun x ->
                                                                    x.label <- str "Time"
                                                                    x.placeholder <- "00:00"
                                                                    x.ref <- initialFocusRef

                                                                    x.onChange <-
                                                                        fun (e: Browser.Types.KeyboardEvent) ->
                                                                            promise {
                                                                                e.Value
                                                                                |> DateTime.TryParse
                                                                                |> function
                                                                                | true, date ->
                                                                                    date
                                                                                    |> FlukeTime.FromDateTime
                                                                                    |> Some
                                                                                | _ -> None
                                                                                |> setPostponedUntil
                                                                            }
                                                        |}

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
                                                                    Props = fun x -> x.onClick <- postponeUntilLater
                                                                    Children =
                                                                        [
                                                                            str "Confirm"
                                                                        ]
                                                                |}
                                                        ]
                                                ]
                                        ]
                                Props = fun _ -> ()
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
                    ]
            ]
