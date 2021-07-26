namespace Fluke.UI.Frontend.Components

open System
open Fable.React
open Feliz
open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared.Domain
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State.State
open Fluke.UI.Frontend.State


module CellMenu =
    open UserInteraction
    open State

    [<ReactComponent>]
    let PostponeTooltipText dateIdAtom =
        let position = Store.useValue Atoms.Session.position
        let dateId = Store.useValue dateIdAtom
        let cellSelectionMap = Store.useValue Selectors.Session.cellSelectionMap

        str
            $"""Postpone{match position, dateId with
                         | Some position, dateId when
                             position.Date = (dateId |> DateId.Value)
                             && cellSelectionMap
                                |> Map.values
                                |> Seq.forall ((=) (Set.singleton dateId))
                             ->
                             " until tomorrow"
                         | _ -> ""}"""

    [<ReactComponent>]
    let CellMenu taskIdAtom dateIdAtom (onClose: (unit -> unit) option) (floating: bool) =
        let taskId = Store.useValue taskIdAtom
        let dateId = Store.useValue dateIdAtom
        let username = Store.useValue Store.Atoms.username
        let toast = UI.useToast ()
        let cellSize = Store.useValue Atoms.User.cellSize
        let sessionStatus, setSessionStatus = Store.useState (Selectors.Cell.sessionStatus (taskId, dateId))
        let cellSelectionMap = Store.useValue Selectors.Session.cellSelectionMap
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

        let navigate = Navigate.useNavigate ()
        let cellUIFlag, setCellUIFlag = Store.useState (Atoms.User.uiFlag UIFlagType.Cell)
        let cellColorPostponedUntil = Store.useValue Atoms.User.cellColorPostponedUntil
        let cellColorPostponed = Store.useValue Atoms.User.cellColorPostponed
        let cellColorCompleted = Store.useValue Atoms.User.cellColorCompleted
        let cellColorDismissed = Store.useValue Atoms.User.cellColorDismissed
        let cellColorScheduled = Store.useValue Atoms.User.cellColorScheduled

        let onClick =
            Store.useCallback (
                (fun _ setter (onClickStatus: CellStatus) ->
                    promise {
                        cellSelectionMap
                        |> Map.iter
                            (fun taskId dateIdSet ->
                                dateIdSet
                                |> Set.iter
                                    (fun dateId ->
                                        Store.set setter (Selectors.Cell.sessionStatus (taskId, dateId)) onClickStatus))

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

                            return true
                        | _ ->
                            toast (fun x -> x.description <- "Invalid time")
                            return false
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
                                |> Map.mapValues (Seq.random >> Set.singleton)
                            else
                                let key = cellSelectionMap |> Map.keys |> Seq.random

                                cellSelectionMap
                                |> Map.map (fun key' value -> if key' = key then value else Set.empty)

                        match cellUIFlag with
                        | UIFlag.Cell (taskId, dateId) when
                            cellSelectionMap
                            |> Map.keys
                            |> Seq.contains taskId
                            && cellSelectionMap.[taskId] |> Set.contains dateId
                            && (newMap |> Map.keys |> Seq.contains taskId |> not
                                || newMap.[taskId] |> Set.contains dateId |> not)
                            ->
                            let newTaskId =
                                newMap
                                |> Map.pick (fun k v -> if v.IsEmpty then None else Some k)

                            setCellUIFlag (UIFlag.Cell (newTaskId, newMap.[newTaskId] |> Seq.random))
                        | _ -> ()

                        newMap
                        |> Map.iter (Atoms.Task.selectionSet >> Store.set setter)

                        match onClose with
                        | Some onClose when
                            newMap
                            |> Map.values
                            |> Seq.map Set.count
                            |> Seq.sum = 1
                            ->
                            onClose ()
                        | _ -> ()
                    }),
                [|
                    box onClose
                    box cellUIFlag
                    box cellSelectionMap
                    box setCellUIFlag
                |]
            )

        UI.stack
            (fun x ->
                x.spacing <- "0"

                if floating then
                    x.borderWidth <- "1px"

                    x.borderColor <-
                        if darkMode then
                            Color.Value UserState.Default.CellColorDisabled
                        else
                            "gray.45"

                    x.boxShadow <- $"0px 0px 2px 1px #{if darkMode then 262626 else 777}")
            [
                UI.stack
                    (fun x ->
                        x.direction <- if floating then "column" else "row"
                        x.borderColor <- "gray.77"
                        x.backgroundColor <- if darkMode then "#636363" else "gray.45"
                        x.spacing <- "1px"
                        x.width <- $"{cellSize (* * 2*) }px")
                    [
                        let wrapButton icon color onClick =
                            UI.iconButton
                                (fun x ->
                                    UI.setTestId x $"cell-button-{color}"
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
                            let color =
                                match status with
                                | Completed -> cellColorCompleted
                                | Postponed until ->
                                    if until.IsSome then cellColorPostponedUntil else cellColorPostponed
                                | Dismissed -> cellColorDismissed
                                | Scheduled -> cellColorScheduled
                                |> Color.Value

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
                                        (Color.Value UserState.Default.CellColorPending)
                                        (Some
                                            (fun () ->
                                                promise {
                                                    do!
                                                        navigate (
                                                            Navigate.DockPosition.Right,
                                                            Some TempUI.DockType.Cell,
                                                            UIFlagType.Cell,
                                                            UIFlag.Cell (taskId, dateId)
                                                        )

                                                    match onClose with
                                                    | Some onClose -> onClose ()
                                                    | None -> ()
                                                }))
                                ]

                        UI.box
                            (fun _ -> ())
                            [
                                str "Complete"
                            ]
                        |> wrapButtonTooltip Completed

                        UI.box
                            (fun _ -> ())
                            [
                                UI.box
                                    (fun _ -> ())
                                    [
                                        str "Dismiss"
                                    ]
                                UI.box
                                    (fun _ -> ())
                                    [
                                    //                                        str """???"""
                                    ]
                            ]
                        |> wrapButtonTooltip Dismissed

                        UI.box
                            (fun _ -> ())
                            [
                                PostponeTooltipText dateIdAtom
                            ]
                        |> wrapButtonTooltip (Postponed None)

                        Popover.ConfirmPopover
                            (Tooltip.wrap
                                (str $"Postpone until {postponedUntilLabel}")
                                [
                                    wrapButton None (Color.Value cellColorPostponedUntil) None
                                ])
                            postponeUntilLater
                            (fun (_disclosure, fetchInitialFocusRef) ->
                                [
                                    UI.stack
                                        (fun x -> x.spacing <- "10px")
                                        [
                                            UI.box
                                                (fun x ->
                                                    x.paddingBottom <- "5px"
                                                    x.marginRight <- "24px"
                                                    x.fontSize <- "1.3rem")
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
                                                            x.ref <- fetchInitialFocusRef ()

                                                            x.onChange <-
                                                                fun (e: Browser.Types.KeyboardEvent) ->
                                                                    promise {
                                                                        e.Value
                                                                        |> DateTime.TryParse
                                                                        |> function
                                                                            | true, date ->
                                                                                date |> FlukeTime.FromDateTime |> Some
                                                                            | _ -> None
                                                                        |> setPostponedUntil
                                                                    }
                                                |}
                                        ]
                                ])

                        UI.box
                            (fun x -> x.padding <- "4px")
                            [
                                UI.box
                                    (fun _ -> ())
                                    [
                                        str "Schedule"
                                    ]
                                UI.box
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
                                  |> Set.contains dateId
                                  |> not) then
                            nothing
                        else
                            Tooltip.wrap
                                (str "Randomize Selection")
                                [
                                    wrapButton
                                        (Icons.bi.BiShuffle |> Icons.render |> Some)
                                        (Color.Value UserState.Default.CellColorSuggested)
                                        (Some random)
                                ]

                        match sessionStatus with
                        | UserStatus _ ->
                            Tooltip.wrap
                                (str "Clear")
                                [
                                    wrapButtonStatus
                                        (Icons.md.MdClear |> Icons.render |> Some)
                                        (Color.Value UserState.Default.CellColorDisabled)
                                        Disabled
                                ]
                        | _ -> nothing
                    ]
            ]
