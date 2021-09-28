namespace Fluke.UI.Frontend.Components

open FsCore
open Fable.React
open Feliz
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend
open FsCore.BaseModel
open FsJs
open FsStore
open FsStore.Hooks
open FsStore.Model
open FsUi.Bindings
open Fluke.Shared.Domain
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State.State
open Fluke.UI.Frontend.State
open FsUi.Components


module Cell =
    open UserInteraction
    open State

    module Actions =
        let onCellClick =
            Atom.Primitives.setSelector
                (fun getter setter (taskId, date) ->
                    let selected = Atom.get getter (Selectors.Cell.selected (CellRef (taskId, date)))
                    //                        if deviceInfo.IsTesting then
                    let ctrlPressed = Atom.get getter Atoms.Session.ctrlPressed
                    let shiftPressed = Atom.get getter Atoms.Session.shiftPressed

                    let newSelected = if ctrlPressed || shiftPressed then not selected else false

                    Atom.set setter (Selectors.Cell.selected (CellRef (taskId, date))) newSelected
                    //                        else
//                            let newSelected = if e.ctrlKey || e.shiftKey then not selected else false
//
//                            if selected <> newSelected then
//                                do! setSelected (taskId, dateId, newSelected)
                    )

    [<ReactComponent>]
    let Cell
        (input: {| TaskIdAtom: AtomConfig<TaskId>
                   DateAtom: AtomConfig<FlukeDate>
                   SemiTransparent: bool |})
        =
        Profiling.addCount (fun () -> "- CellComponent.render") getLocals

        let taskId = Store.useValue input.TaskIdAtom
        let date = Store.useValue input.DateAtom
        let cellSize = Store.useValue Atoms.User.cellSize
        let databaseId = Store.useValue (Atoms.Task.databaseId taskId)
        let isReadWrite = Store.useValue (Selectors.Database.isReadWrite databaseId)
        let sessionStatus = Store.useValue (Selectors.Cell.sessionStatus (CellRef (taskId, date)))
        let attachmentIdSet = Store.useValue (Selectors.Cell.attachmentIdSet (CellRef (taskId, date)))
        let isToday = Store.useValue (Selectors.FlukeDate.isToday date)
        let selected = Store.useValue (Selectors.Cell.selected (CellRef (taskId, date)))
        let cellUIFlag = Store.useValue (Atoms.User.uiFlag UIFlagType.Cell)
        let rightDock = Store.useValue Atoms.User.rightDock
        //        let deviceInfo = Store.useValue Selectors.deviceInfo
        let cellColorDisabled = Store.useValue Atoms.User.cellColorDisabled
        let cellColorSuggested = Store.useValue Atoms.User.cellColorSuggested
        let cellColorPending = Store.useValue Atoms.User.cellColorPending
        let cellColorMissed = Store.useValue Atoms.User.cellColorMissed
        let cellColorMissedToday = Store.useValue Atoms.User.cellColorMissedToday
        let cellColorPostponedUntil = Store.useValue Atoms.User.cellColorPostponedUntil
        let cellColorPostponed = Store.useValue Atoms.User.cellColorPostponed
        let cellColorCompleted = Store.useValue Atoms.User.cellColorCompleted
        let cellColorDismissed = Store.useValue Atoms.User.cellColorDismissed
        let cellColorScheduled = Store.useValue Atoms.User.cellColorScheduled

        let onCellClick = Store.useSetState Actions.onCellClick

        Ui.center
            (fun x ->
                Ui.setTestId x $"cell-{taskId}-{(date |> FlukeDate.DateTime).ToShortDateString ()}"

                if isReadWrite then
                    x.onClick <- fun _ -> promise { onCellClick (taskId, date) }

                x.width <- $"{cellSize}px"
                x.height <- $"{cellSize}px"
                x.lineHeight <- $"{cellSize}px"
                x.position <- "relative"

                x.backgroundColor <-
                    (match sessionStatus with
                     | Disabled -> cellColorDisabled
                     | Suggested -> cellColorSuggested
                     | Pending -> cellColorPending
                     | Missed -> cellColorMissed
                     | MissedToday -> cellColorMissedToday
                     | UserStatus (_, status) ->
                         match status with
                         | Completed -> cellColorCompleted
                         | Postponed until -> if until.IsSome then cellColorPostponedUntil else cellColorPostponed
                         | Dismissed -> cellColorDismissed
                         | Scheduled -> cellColorScheduled
                     |> Color.Value)
                    + (if isToday then "aa"
                       elif input.SemiTransparent then "d9"
                       else "")

                x.textAlign <- "center"

                x.borderWidth <- "1px"

                x.borderColor <- if selected then "#ffffffAA" else "transparent"

                if isReadWrite then
                    x._hover <- Js.newObj (fun x -> x.borderColor <- "#ffffff55")
                    x.cursor <- "pointer")
            [
                match rightDock, cellUIFlag with
                | Some TempUI.DockType.Cell, UIFlag.Cell (taskId', dateId') when taskId' = taskId && dateId' = date ->
                    Ui.icon
                        (fun x ->
                            x.``as`` <- Icons.ti.TiPin
                            x.fontSize <- $"{cellSize - 4}px"
                            x.color <- "white")
                        []
                | _ -> nothing

                CellSessionIndicator.CellSessionIndicator input.TaskIdAtom input.DateAtom

                if selected then
                    nothing
                else
                    CellBorder.CellBorder input.TaskIdAtom input.DateAtom

                CellStatusUserIndicator.CellStatusUserIndicator input.TaskIdAtom input.DateAtom

                if not attachmentIdSet.IsEmpty then
                    AttachmentIndicator.AttachmentIndicator ()
                else
                    nothing
            ]

    [<ReactComponent>]
    let CellWrapper
        (input: {| TaskIdAtom: AtomConfig<TaskId>
                   DateAtom: AtomConfig<FlukeDate>
                   SemiTransparent: bool |})
        =
        let taskId = Store.useValue input.TaskIdAtom
        let date = Store.useValue input.DateAtom
        let enableCellPopover = Store.useValue Atoms.User.enableCellPopover
        let databaseId = Store.useValue (Atoms.Task.databaseId taskId)
        let isReadWrite = Store.useValue (Selectors.Database.isReadWrite databaseId) //
        let navigate = Store.useSetState Navigate.Actions.navigate

        if enableCellPopover then
            Popover.CustomPopover //
                {|
                    CloseButton = false //
                    Padding = Some "3px" //
                    Props = fun x -> x.placement <- "right-start" //
                    Trigger = Cell input
                    Body =
                        fun (disclosure, _fetchInitialFocusRef) ->
                            [ //
                                if isReadWrite then //
                                    CellMenu.CellMenu
                                        input.TaskIdAtom
                                        input.DateAtom
                                        (Some disclosure.onClose) // None
                                        true
                                else //
                                    nothing //
                            ] //
                |} //
        else //
            Ui.box
                (fun x ->
                    x.onClick <-
                        fun _ ->
                            promise {
                                navigate (
                                    Navigate.DockPosition.Right,
                                    Some TempUI.DockType.Cell,
                                    UIFlagType.Cell,
                                    UIFlag.Cell (taskId, date)
                                )
                            })
                [
                    Cell input
                ]
