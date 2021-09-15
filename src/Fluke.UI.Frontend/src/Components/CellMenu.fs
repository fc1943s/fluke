namespace Fluke.UI.Frontend.Components

open FsStore.State
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
    let PostponeTooltipText dateAtom =
        let position = Store.useValue Atoms.Session.position
        let date = Store.useValue dateAtom
        let visibleTaskSelectedDateMap = Store.useValue Selectors.Session.visibleTaskSelectedDateMap

        str
            $"""Postpone{match position, date with
                         | Some position, date when
                             position.Date = date
                             && visibleTaskSelectedDateMap
                                |> Map.values
                                |> Seq.forall ((=) (Set.singleton date))
                             ->
                             " until tomorrow"
                         | _ -> ""}"""

    module Actions =
        let postponeUntilLater =
            Atom.Primitives.setSelector
                (fun getter setter (taskId, date, postponedUntil) ->
                    Profiling.addTimestamp (fun () -> $"{nameof Fluke} | Actions.postponeUntilLater") getLocals
                    let alias = Atom.get getter Selectors.Gun.alias

                    match alias with
                    | Some (Gun.Alias alias) ->
                        Atom.set
                            setter
                            (Selectors.Cell.sessionStatus (CellRef (taskId, date)))
                            (UserStatus (Username alias, Postponed (Some postponedUntil)))
                    | _ -> ())

        let setCellStatus =
            Atom.Primitives.setSelector
                (fun getter setter (taskId, date, newStatus: CellStatus) ->
                    let visibleTaskSelectedDateMap = Atom.get getter Selectors.Session.visibleTaskSelectedDateMap

                    let newMap =
                        visibleTaskSelectedDateMap
                        |> Map.add
                            taskId
                            (visibleTaskSelectedDateMap
                             |> Map.tryFind taskId
                             |> Option.defaultValue Set.empty
                             |> Set.add date)

                    let newStatuses =
                        newMap
                        |> Map.toList
                        |> List.collect
                            (fun (taskId, dateSet) ->
                                dateSet
                                |> Set.toList
                                |> List.map (fun date -> taskId, date, newStatus))

                    newStatuses
                    |> List.iter
                        (fun (taskId, date, newStatus) ->
                            let getLocals () =
                                $"date={date |> FlukeDate.Stringify} taskId={taskId} newStatus={newStatus} {getLocals ()}"

                            Profiling.addTimestamp
                                (fun () ->
                                    $"{nameof Fluke} | Actions.setCellStatus / write () / setting Selectores.Cell.sessionStatus")
                                getLocals

                            Atom.set setter (Selectors.Cell.sessionStatus (CellRef (taskId, date))) newStatus)

                    visibleTaskSelectedDateMap
                    |> Map.toSeq
                    |> Seq.iter
                        (fun (taskId, selectionSet) ->
                            if selectionSet <> Set.empty then
                                let getLocals () =
                                    $"date={date |> FlukeDate.Stringify} taskId={taskId} selectionSet={selectionSet} {getLocals ()}"

                                Profiling.addTimestamp
                                    (fun () ->
                                        $"{nameof Fluke} | Actions.setCellStatus / write () / setting Atoms.Task.selectionSet=Set.empty")
                                    getLocals

                                Atom.set setter (Atoms.Task.selectionSet taskId) Set.empty))

        let random =
            Atom.Primitives.setSelector
                (fun getter setter () ->
                    Profiling.addTimestamp (fun () -> $"{nameof Fluke} | Actions.random") getLocals
                    let visibleTaskSelectedDateMap = Atom.get getter Selectors.Session.visibleTaskSelectedDateMap
                    let cellUIFlag = Atom.get getter (Atoms.User.uiFlag UIFlagType.Cell)

                    let newMap =
                        if visibleTaskSelectedDateMap.Count = 1 then
                            visibleTaskSelectedDateMap
                            |> Map.mapValues (Seq.random >> Set.singleton)
                        else
                            let key =
                                visibleTaskSelectedDateMap
                                |> Map.keys
                                |> Seq.random

                            visibleTaskSelectedDateMap
                            |> Map.map (fun key' value -> if key' = key then value else Set.empty)

                    match cellUIFlag with
                    | UIFlag.Cell (taskId, date) when
                        visibleTaskSelectedDateMap
                        |> Map.keys
                        |> Seq.contains taskId
                        && visibleTaskSelectedDateMap.[taskId]
                           |> Set.contains date
                        && (newMap |> Map.keys |> Seq.contains taskId |> not
                            || newMap.[taskId] |> Set.contains date |> not)
                        ->
                        let newTaskId =
                            newMap
                            |> Map.pick (fun k v -> if v.IsEmpty then None else Some k)

                        Atom.set
                            setter
                            (Atoms.User.uiFlag UIFlagType.Cell)
                            (UIFlag.Cell (newTaskId, newMap.[newTaskId] |> Seq.random))
                    | _ -> ()

                    newMap
                    |> Map.iter (Atoms.Task.selectionSet >> Atom.set setter))

    [<ReactComponent>]
    let CellMenu taskIdAtom dateAtom (onClose: (unit -> unit) option) (floating: bool) =
        let taskId, date = Store.useValueTuple taskIdAtom dateAtom
        let alias = Store.useValue Selectors.Gun.alias
        let cellSize = Store.useValue Atoms.User.cellSize
        let sessionStatus = Store.useValue (Selectors.Cell.sessionStatus (CellRef (taskId, date)))
        let darkMode = Store.useValue Atoms.Ui.darkMode
        let setCellStatus = Store.useSetState Actions.setCellStatus
        let random = Store.useSetState Actions.random
        let postponeUntilLater = Store.useSetState Actions.postponeUntilLater

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
        let cellColorPostponedUntil = Store.useValue Atoms.User.cellColorPostponedUntil
        let cellColorPostponed = Store.useValue Atoms.User.cellColorPostponed
        let cellColorCompleted = Store.useValue Atoms.User.cellColorCompleted
        let cellColorDismissed = Store.useValue Atoms.User.cellColorDismissed
        let cellColorScheduled = Store.useValue Atoms.User.cellColorScheduled
        let visibleTaskSelectedDateMap = Store.useValue Selectors.Session.visibleTaskSelectedDateMap


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
                        let inline wrapButton icon color onClick =
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

                        let inline wrapButtonStatus icon color status =
                            wrapButton
                                icon
                                color
                                (Some
                                    (fun () ->
                                        promise {
                                            setCellStatus (taskId, date, status)

                                            match onClose with
                                            | Some onClose -> onClose ()
                                            | None -> ()
                                        }))

                        let inline wrapButtonTooltip status tooltipLabel =
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
                           && visibleTaskSelectedDateMap.Count <= 1
                           && visibleTaskSelectedDateMap
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
                                                            UIFlag.Cell (taskId, date)
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
                                PostponeTooltipText dateAtom
                            ]
                        |> wrapButtonTooltip (Postponed None)

                        Popover.ConfirmPopover
                            (Tooltip.wrap
                                (str $"Postpone until {postponedUntilLabel}")
                                [
                                    wrapButton None (cellColorPostponedUntil |> Color.Value) None
                                ])
                            (fun () ->
                                promise {
                                    match postponedUntil with
                                    | Some postponedUntil ->
                                        postponeUntilLater (taskId, date, postponedUntil)
                                        return true
                                    | None -> return false
                                })
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

                        if visibleTaskSelectedDateMap.IsEmpty
                           || (visibleTaskSelectedDateMap.Count = 1
                               && visibleTaskSelectedDateMap.[visibleTaskSelectedDateMap |> Map.keys |> Seq.head]
                                   .Count = 1)
                           || (not floating
                               && visibleTaskSelectedDateMap
                                  |> Map.tryFind taskId
                                  |> Option.defaultValue Set.empty
                                  |> Set.contains date
                                  |> not) then
                            nothing
                        else
                            Tooltip.wrap
                                (str "Randomize Selection")
                                [
                                    wrapButton
                                        (Icons.bi.BiShuffle |> Icons.render |> Some)
                                        (Color.Value UserState.Default.CellColorSuggested)
                                        (Some (fun () -> promise { random () }))
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
