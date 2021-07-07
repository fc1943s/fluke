namespace Fluke.UI.Frontend.Components

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
    let CellMenu (taskId: TaskId) (dateId: DateId) (onClose: (unit -> unit) option) (floating: bool) =
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
        let cellUIFlag, setCellUIFlag = Store.useState (Atoms.User.uiFlag UIFlagType.Cell)

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

                        match onClose with
                        | Some onClose -> onClose ()
                        | None -> ()
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

                            match onClose with
                            | Some onClose -> onClose ()
                            | None -> ()
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

        let random =
            Store.useCallback (
                (fun _ setter _ ->
                    promise {
                        let newMap =
                            if cellSelectionMap.Count = 1 then
                                cellSelectionMap
                                |> Map.mapValues (JS.randomSeq >> Set.singleton)
                            else
                                let key = cellSelectionMap |> Map.keys |> JS.randomSeq

                                cellSelectionMap
                                |> Map.map (fun key' value -> if key' = key then value else Set.empty)

                        match cellUIFlag with
                        | UIFlag.Cell (taskId, dateId) when
                            cellSelectionMap
                            |> Map.keys
                            |> Seq.contains taskId
                            && cellSelectionMap.[taskId]
                               |> Set.contains (dateId |> DateId.Value)
                            && (newMap |> Map.keys |> Seq.contains taskId |> not
                                || newMap.[taskId]
                                   |> Set.contains (dateId |> DateId.Value)
                                   |> not) ->
                            let newTaskId =
                                newMap
                                |> Map.pick (fun k v -> if v.IsEmpty then None else Some k)

                            setCellUIFlag (UIFlag.Cell (newTaskId, newMap.[newTaskId] |> JS.randomSeq |> DateId))
                        | _ -> ()

                        newMap
                        |> Map.iter
                            (fun taskId dates ->
                                Store.set setter (Atoms.Task.selectionSet taskId) (dates |> Set.map DateId))

                        match onClose with
                        | Some onClose when
                            newMap.Count = 1
                            && newMap |> Map.values |> Seq.head |> Set.count = 1 -> onClose ()
                        | _ -> ()
                    }),
                [|
                    box onClose
                    box cellUIFlag
                    box cellSelectionMap
                    box setCellUIFlag
                |]
            )

        Chakra.stack
            (fun x ->
                x.spacing <- "0"

                if floating then
                    x.borderWidth <- "1px"
                    x.borderColor <- if darkMode then TempUI.cellStatusColor Disabled else "gray.45"
                    x.boxShadow <- $"0px 0px 2px 1px #{if darkMode then 262626 else 777}")
            [
                Chakra.stack
                    (fun x ->
                        x.direction <- if floating then "column" else "row"
                        x.borderColor <- "gray.77"
                        x.backgroundColor <- if darkMode then "#636363" else "gray.45"
                        x.spacing <- "1px"
                        x.width <- $"{cellSize (* * 2*) }px")
                    [
                        let wrapButton icon color onClick =
                            Chakra.iconButton
                                (fun x ->
                                    Chakra.setTestId x $"cell-button-{color}"
                                    x.icon <- icon
                                    x.display <- "flex"
                                    x.color <- "#dddddd"
                                    x._hover <- JS.newObj (fun x -> x.opacity <- 0.8)
                                    x._active <- JS.newObj (fun x -> x.opacity <- 0.5)
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
                                    | Some username ->
                                        wrapButtonStatus
                                            (match sessionStatus with
                                             | UserStatus (_, sessionStatus) when sessionStatus = status ->
                                                 Icons.hi.HiOutlineCheck |> Icons.render |> Some
                                             | _ -> None)
                                            color
                                            (UserStatus (username, status))
                                    | _ -> nothing
                                ]

                        if not floating then
                            nothing
                        else
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

                                                    match onClose with
                                                    | Some onClose -> onClose ()
                                                    | None -> ()
                                                }))
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
                                                                            Icons.fi.FiKey |> Icons.render,
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

                        if cellSelectionMap.IsEmpty
                           || (cellSelectionMap.Count = 1
                               && cellSelectionMap.[cellSelectionMap |> Map.keys |> Seq.head]
                                   .Count = 1)
                           || (not floating
                               && cellSelectionMap
                                  |> Map.tryFind taskId
                                  |> Option.defaultValue Set.empty
                                  |> Set.contains (dateId |> DateId.Value)
                                  |> not) then
                            nothing
                        else
                            Tooltip.wrap
                                (str "Randomize Selection")
                                [
                                    wrapButton
                                        (Icons.bi.BiShuffle |> Icons.render |> Some)
                                        (TempUI.cellStatusColor Suggested)
                                        (Some random)
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
                    ]
            ]
