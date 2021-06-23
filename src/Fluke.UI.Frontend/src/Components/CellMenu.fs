namespace Fluke.UI.Frontend.Components

open Fable.Core
open System
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
    let CellMenu
        (input: {| Username: Username
                   TaskId: TaskId
                   DateId: DateId
                   OnClose: unit -> unit |})
        =

        let toast = Chakra.useToast ()
        let cellSize = Store.useValue (Atoms.User.cellSize input.Username)

        let sessionStatus, setSessionStatus =
            Store.useState (Selectors.Cell.sessionStatus (input.Username, input.TaskId, input.DateId))

        let cellSelectionMap = Store.useValue (Selectors.Session.cellSelectionMap input.Username)

        let dayStart = Store.useValue (Atoms.User.dayStart input.Username)

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

        let setRightDock = Store.useSetState (Atoms.User.rightDock input.Username)
        let setCellUIFlag = Store.useSetState (Atoms.User.uiFlag (input.Username, Atoms.User.UIFlagType.Cell))

        let onClick =
            Store.useCallback (
                (fun get set (onClickStatus: CellStatus) ->
                    promise {
                        cellSelectionMap
                        |> Map.iter
                            (fun taskId dates ->
                                dates
                                |> Set.iter
                                    (fun date ->
                                        Atoms.setAtomValue
                                            set
                                            (Selectors.Cell.sessionStatus (input.Username, taskId, DateId date))
                                            onClickStatus))

                        Atoms.setAtomValue
                            set
                            (Selectors.Cell.sessionStatus (input.Username, input.TaskId, input.DateId))
                            onClickStatus

                        input.OnClose ()
                    }),
                [|
                    box cellSelectionMap
                    box input
                |]
            )

        let postponeUntilLater =
            Store.useCallback (
                (fun _ _ _ ->
                    promise {
                        match postponedUntil with
                        | Some postponedUntil ->
                            setSessionStatus (UserStatus (input.Username, Postponed (Some postponedUntil)))
                            input.OnClose ()
                        | _ -> toast (fun x -> x.description <- "Invalid time")
                    }),
                [|
                    box postponedUntil
                    box setSessionStatus
                    box input
                    box toast
                |]
            )

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
                        x.backgroundColor <- "#636363"
                        x.spacing <- "1px"
                        x.width <- $"{cellSize (* * 2*) }px")
                    [
                        let wrapButton icon color onClick =
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
                                    wrapButtonStatus None color (UserStatus (input.Username, status))
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

                                                setCellUIFlag (Atoms.User.UIFlag.Cell (input.TaskId, input.DateId))
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
