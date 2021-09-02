namespace Fluke.UI.Frontend.Components

open FsCore
open FsCore.BaseModel
open FsStore.Bindings
open FsUi.State
open System
open Fable.React
open Feliz
open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend
open FsJs
open FsStore
open FsStore.Hooks
open FsUi.Bindings
open Fluke.Shared.Domain
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State.State
open Fluke.UI.Frontend.State
open FsUi.Components


module CellMenu =

    [<ReactComponent>]
    let PostponeTooltipText dateIdAtom =
        let position = Store.useValue Atoms.Session.position
        let dateId = Store.useValue dateIdAtom
        let visibleTaskSelectedDateIdMap = Store.useValue Selectors.Session.visibleTaskSelectedDateIdMap

        str
            $"""Postpone{match position, dateId with
                         | Some position, dateId when
                             position.Date = (dateId |> DateId.ValueOrDefault)
                             && visibleTaskSelectedDateIdMap
                                |> Map.values
                                |> Seq.forall ((=) (Set.singleton dateId))
                             ->
                             " until tomorrow"
                         | _ -> ""}"""

    [<ReactComponent>]
    let CellMenu taskIdAtom dateIdAtom (onClose: (unit -> unit) option) (floating: bool) =
        let taskId, dateId = Store.useValueTuple taskIdAtom dateIdAtom
        let alias = Store.useValue Selectors.Gun.alias
        let toast = Ui.useToast ()
        let cellSize = Store.useValue Atoms.User.cellSize
        let sessionStatus, setSessionStatus = Store.useState (Selectors.Cell.sessionStatus (taskId, dateId))
        let darkMode = Store.useValue Atoms.Ui.darkMode

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

        let navigate = Store.useCallbackRef Navigate.navigate
        let cellUIFlag, setCellUIFlag = Store.useState (Atoms.User.uiFlag UIFlagType.Cell)
        let cellColorPostponedUntil = Store.useValue Atoms.User.cellColorPostponedUntil
        let cellColorPostponed = Store.useValue Atoms.User.cellColorPostponed
        let cellColorCompleted = Store.useValue Atoms.User.cellColorCompleted
        let cellColorDismissed = Store.useValue Atoms.User.cellColorDismissed
        let cellColorScheduled = Store.useValue Atoms.User.cellColorScheduled
        let visibleTaskSelectedDateIdMap = Store.useValue Selectors.Session.visibleTaskSelectedDateIdMap

        let onClick =
            Store.useCallbackRef
                (fun _ setter (onClickStatus: CellStatus) ->
                    promise {
                        visibleTaskSelectedDateIdMap
                        |> Map.iter
                            (fun taskId dateIdSet ->
                                dateIdSet
                                |> Set.iter
                                    (fun dateId ->
                                        Store.set setter (Selectors.Cell.sessionStatus (taskId, dateId)) onClickStatus))

                        Store.set setter (Selectors.Cell.sessionStatus (taskId, dateId)) onClickStatus

                        visibleTaskSelectedDateIdMap
                        |> Map.keys
                        |> Seq.iter (fun taskId -> Store.set setter (Atoms.Task.selectionSet taskId) Set.empty)

                        match onClose with
                        | Some onClose -> onClose ()
                        | None -> ()
                    })

        let postponeUntilLater =
            Store.useCallbackRef
                (fun _ _ _ ->
                    promise {
                        match alias, postponedUntil with
                        | Some (Gun.Alias alias), Some postponedUntil ->
                            setSessionStatus (UserStatus (Username alias, Postponed (Some postponedUntil)))

                            match onClose with
                            | Some onClose -> onClose ()
                            | None -> ()

                            return true
                        | _ ->
                            toast (fun x -> x.description <- "Invalid time")
                            return false
                    })

        let random =
            Store.useCallbackRef
                (fun _ setter _ ->
                    promise {
                        let newMap =
                            if visibleTaskSelectedDateIdMap.Count = 1 then
                                visibleTaskSelectedDateIdMap
                                |> Map.mapValues (Seq.random >> Set.singleton)
                            else
                                let key =
                                    visibleTaskSelectedDateIdMap
                                    |> Map.keys
                                    |> Seq.random

                                visibleTaskSelectedDateIdMap
                                |> Map.map (fun key' value -> if key' = key then value else Set.empty)

                        match cellUIFlag with
                        | UIFlag.Cell (taskId, dateId) when
                            visibleTaskSelectedDateIdMap
                            |> Map.keys
                            |> Seq.contains taskId
                            && visibleTaskSelectedDateIdMap.[taskId]
                               |> Set.contains dateId
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
                    })

        Ui.stack
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
                Ui.stack
                    (fun x ->
                        x.direction <- if floating then "column" else "row"
                        x.borderColor <- "gray.77"
                        x.backgroundColor <- if darkMode then "#636363" else "gray.45"
                        x.spacing <- "1px"
                        x.width <- $"{cellSize (* * 2*) }px")
                    [
                        let wrapButton icon color onClick =
                            Ui.iconButton
                                (fun x ->
                                    Ui.setTestId x $"cell-button-{color}"
                                    x.icon <- icon
                                    x.display <- "flex"
                                    x.color <- "#dddddd"
                                    x._hover <- Js.newObj (fun x -> x.opacity <- 0.8)
                                    x._active <- Js.newObj (fun x -> x.opacity <- 0.5)
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
                                    match alias with
                                    | Some (Gun.Alias alias) ->
                                        wrapButtonStatus
                                            (match sessionStatus with
                                             | UserStatus (_, sessionStatus) when sessionStatus = status ->
                                                 Icons.hi.HiOutlineCheck |> Icons.render |> Some
                                             | _ -> None)
                                            color
                                            (UserStatus (Username alias, status))
                                    | _ -> nothing
                                ]


                        if floating
                           && visibleTaskSelectedDateIdMap.Count <= 1
                           && visibleTaskSelectedDateIdMap
                              |> Map.values
                              |> Seq.fold Set.union Set.empty
                              |> Set.count
                              <= 1 then
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
                        else
                            nothing

                        Ui.str "Complete" |> wrapButtonTooltip Completed

                        Ui.box
                            (fun _ -> ())
                            [
                                Ui.str "Dismiss"
                                Ui.box
                                    (fun _ -> ())
                                    [
                                    //                                        str """???"""
                                    ]
                            ]
                        |> wrapButtonTooltip Dismissed

                        Ui.box
                            (fun _ -> ())
                            [
                                PostponeTooltipText dateIdAtom
                            ]
                        |> wrapButtonTooltip (Postponed None)

                        Popover.ConfirmPopover
                            (Tooltip.wrap
                                (str $"Postpone until {postponedUntilLabel}")
                                [
                                    wrapButton None (cellColorPostponedUntil |> Color.Value) None
                                ])
                            postponeUntilLater
                            (fun (_disclosure, fetchInitialFocusRef) ->
                                [
                                    Ui.stack
                                        (fun x -> x.spacing <- "10px")
                                        [
                                            Ui.box
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

                        Ui.stack
                            (fun x ->
                                x.padding <- "4px"
                                x.spacing <- "8px")
                            [
                                Ui.str "Schedule"
                                Ui.str
                                    """Manually schedule a task,
overriding any other behavior.
"""
                            ]
                        |> wrapButtonTooltip Scheduled

                        if visibleTaskSelectedDateIdMap.IsEmpty
                           || (visibleTaskSelectedDateIdMap.Count = 1
                               && visibleTaskSelectedDateIdMap.[visibleTaskSelectedDateIdMap
                                                                |> Map.keys
                                                                |> Seq.head]
                                   .Count = 1)
                           || (not floating
                               && visibleTaskSelectedDateIdMap
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
