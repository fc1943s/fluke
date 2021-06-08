namespace Fluke.UI.Frontend.Components

open Fable.Core
open System
open Fable.React
open Feliz
open Feliz.Recoil
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

        let cellSize = Recoil.useValue (Atoms.User.cellSize input.Username)

        let sessionStatus = Recoil.useValue (Selectors.Cell.sessionStatus (input.Username, input.TaskId, input.DateId))

        let cellSelectionMap =
            Recoil.useValueLoadableDefault (Selectors.Session.cellSelectionMap input.Username) Map.empty

        let dayStart = Recoil.useValue (Atoms.User.dayStart input.Username)

        let postponedUntil =
            match sessionStatus with
            | UserStatus (_, Postponed (Some until)) -> Some until
            | _ -> None

        let onClick =
            Recoil.useCallbackRef
                (fun setter (onClickStatus: CellStatus) ->
                    promise {
                        cellSelectionMap
                        |> Map.iter
                            (fun taskId dates ->
                                dates
                                |> Set.iter
                                    (fun date ->
                                        setter.set (
                                            Atoms.Cell.status (input.Username, taskId, DateId date),
                                            onClickStatus
                                        )))

                        setter.set (Atoms.Cell.status (input.Username, input.TaskId, input.DateId), onClickStatus)

                        input.OnClose ()
                    })

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
                                    | Some onClickStatus -> x.onClick <- fun _ -> onClick onClickStatus
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

                        match sessionStatus with
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
