namespace Fluke.UI.Frontend.Components

open System
open Browser
open Fable.Core
open Fluke.UI.Frontend
open Browser.Types
open FSharpPlus
open Fluke.Shared
open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open Fable.DateFunctions
open Fulma
open Fulma.Extensions.Wikiki
open Suigetsu.UI.Frontend.React
open Suigetsu.Core
open Feliz
open Feliz.Recoil
open Feliz.Bulma
open Feliz.UseListener


module Temp =
    module UseListener =
        let onElementHover (elemRef: IRefValue<#HTMLElement option>) =
            let isHovered, setIsHovered = React.useState false

            React.useElementListener.onMouseEnter(elemRef, (fun _ -> setIsHovered true), passive = true)
            React.useElementListener.onMouseLeave(elemRef, (fun _ -> setIsHovered false), passive = true)

            React.useMemo((fun () ->
                isHovered
            ), [| isHovered :> obj |])

    module Sound =
        let playDing () =
             [ 0; 1400 ]
             |> List.map (JS.setTimeout (fun () -> Ext.playSound "../sounds/ding.wav"))
             |> ignore

        let playTick () =
            Ext.playSound "../sounds/tick.wav"

module PageLoaderComponent =
    let render = React.memo (fun () ->
        PageLoader.pageLoader [ PageLoader.Color IsDark
                                PageLoader.IsActive true ][]
    )

module SpinnerComponent =
    let render = React.memo (fun () ->
        str "(S)"
    )

module NavBarComponent =
    open Model
    let render = React.memo (fun () ->
        let debug, setDebug = Recoil.useState Recoil.Atoms.debug
        let view, setView = Recoil.useState Recoil.Atoms.view
        let activeSessions = Recoil.useValue Recoil.Selectors.activeSessions

        React.useListener.onKeyDown (fun (e: KeyboardEvent) ->
            match e.ctrlKey, e.shiftKey, e.key with
            | _, true, "C" -> setView View.Calendar
            | _, true, "G" -> setView View.Groups
            | _, true, "T" -> setView View.Tasks
            | _, true, "W" -> setView View.Week
            | _            -> ()
        )

//        Bulma.navbar [
//            prop.children [
//            ]
//
//        ]
        Navbar.navbar [ Navbar.Color IsBlack ][

            let checkbox isChecked text onClick =
                Bulma.navbarItem.div [
                    prop.className "field"
                    prop.onClick (fun _ -> onClick ())
                    prop.style [
                        style.marginBottom 0
                        style.alignSelf.center
                    ]
                    prop.children [
                        Checkbox.input [ CustomClass "switch is-small is-dark"
                                         Props [ Checked isChecked
                                                 OnChange (fun _ -> ()) ]]

                        Checkbox.checkbox [][
                            str text
                        ]
                    ]
                ]

            let viewCheckbox newView text =
                checkbox (view = newView) text (fun () -> setView newView)

            viewCheckbox View.Calendar "calendar view"
            viewCheckbox View.Groups "groups view"
            viewCheckbox View.Tasks "tasks view"
            viewCheckbox View.Week "week view"
            checkbox debug "debug" (fun () -> setDebug (not debug))

            Bulma.navbarItem.div [
                activeSessions
                |> List.map (fun (ActiveSession (taskName, duration)) ->
                    let sessionType, color, duration, left =
                        let left = TempData.Consts.sessionLength - duration
                        match duration < TempData.Consts.sessionLength with
                        | true  -> "Session", "#7cca7c", duration, left
                        | false -> "Break",   "#ca7c7c", -left,    TempData.Consts.sessionBreakLength + left

                    Html.span [
                        prop.style [
                            style.color color
                        ]
                        prop.children [
                            sprintf "%s: Task[ %s ]; Duration[ %.1f ]; Left[ %.1f ]" sessionType taskName duration left
                            |> str
                        ]
                    ]
                )
                |> List.intersperse (br [])
                |> function
                    | [] -> str "No active session"
                    | list -> ofList list
            ]

        ]
    )


module TooltipPopupComponent =
    open Model

    let render = React.memo (fun (input: {| Comments: UserComment list |}) ->
        let tooltipContainerRef = React.useElementRef ()
        let hovered = Temp.UseListener.onElementHover tooltipContainerRef

        match input.Comments with
        | [] -> nothing
        | _ ->
            let user = // TODO: 2+ different users in the same day. what to show? smooth transition between them?
                input.Comments.Head
                |> (ofUserComment >> fst)

            Html.div [
                prop.ref tooltipContainerRef
                prop.classes [
                    Css.tooltipContainer
                    match user with
                    | { Color = UserColor.Blue } -> Css.topRightBlueIndicator
                    | { Color = UserColor.Pink } -> Css.topRightPinkIndicator
                ]
                prop.children [
                    Html.div [
                        prop.className Css.tooltipPopup
                        prop.children [
                            input.Comments
                            |> List.map (fun (UserComment (user, comment)) ->
                                sprintf "%s:%s%s" user.Username Environment.NewLine (comment.Trim ())
                            )
                            |> List.map ((+) Environment.NewLine)
                            |> String.concat (Environment.NewLine + Environment.NewLine)
                            |> fun text ->
                                match hovered with
                                | false -> nothing
                                | true ->
                                    ReactBindings.React.createElement
                                        (Ext.reactMarkdown, {| source = text |}, [])
                        ]
                    ]
                ]
            ]
    )

module PanelsComponent =
    open Model

    module LanesPanel =
        let paddingLeftLevel level =
            match level with
            | 0 -> []
            | level -> [ style.paddingLeft (20 * level) ]
        let cellSize = 17

        module HeaderComponent =
            let render = React.memo (fun () ->
                let dateSequence = Recoil.useValue Recoil.Selectors.dateSequence
                let dateMap = Recoil.useValue Recoil.Selectors.dateMap

                Html.div [
                    // Month row
                    Html.div [
                        prop.style [
                            style.display.flex
                        ]
                        prop.children [
                            yield! dateSequence
                            |> List.groupBy (fun date -> date.Month)
                            |> List.map (fun (_, dates) -> dates.Head, dates.Length)
                            |> List.map (fun (firstDay, days) ->
                                Html.span [
                                    prop.style [
                                        style.textAlign.center
                                        style.width (cellSize * days)
                                    ]
                                    prop.children [
                                        str (firstDay.DateTime.Format "MMM")
                                    ]
                                ]
                            )
                        ]
                    ]

                    // Day of Week row
                    Html.div [
                        prop.style [
                            style.display.flex
                        ]
                        prop.children [
                            yield! dateSequence
                            |> List.map (fun date ->
                                Html.span [
                                    prop.classes [
                                        Css.cellSquare
                                        if dateMap.[date].IsToday then Css.todayHeader
                                        if dateMap.[date].IsSelected then Css.selectionHighlight
                                        match date with
                                        | StartOfMonth -> Css.cellStartMonth
                                        | StartOfWeek -> Css.cellStartWeek
                                        | _ -> ()
                                    ]
                                    prop.children [
                                        date.DateTime.Format "EEEEEE"
                                        |> String.toLower
                                        |> str
                                    ]
                                ]
                            )
                        ]
                    ]

                    // Day row
                    Html.div [
                        prop.style [
                            style.display.flex
                        ]
                        prop.children [
                            yield! dateSequence
                            |> List.map (fun date ->
                                Html.span [
                                    prop.classes [
                                        Css.cellSquare
                                        if dateMap.[date].IsToday then Css.todayHeader
                                        if dateMap.[date].IsSelected then Css.selectionHighlight
                                        match date with
                                        | StartOfMonth -> Css.cellStartMonth
                                        | StartOfWeek -> Css.cellStartWeek
                                        | _ -> ()
                                    ]
                                    prop.children [
                                        str (date.Day.ToString "D2")
                                    ]
                                ]
                            )
                        ]
                    ]
                ]
            )

        module TaskNameComponent =
            let render = React.memo (fun (input: {| Level: int
                                                    TaskId: TaskId |}) ->
                let selection = Recoil.useValue Recoil.Selectors.selection

                let task = Recoil.useValue (Recoil.Atoms.RecoilTask.taskFamily input.TaskId)

                let taskName = Recoil.useValue task.Name
                let taskComments = Recoil.useValue task.Comments


                let isSelected =
                    selection
                    |> Map.tryFind input.TaskId
                    |> Option.defaultValue Set.empty
                    |> Set.isEmpty
                    |> not

                Html.div [
                    prop.className Css.cellRectangle
                    prop.children [
                        Html.div [
                            prop.classes [
                                if isSelected then Css.selectionHighlight
                            ]
                            prop.style [
                                style.overflow.hidden
                                style.whitespace.nowrap
                                style.textOverflow.ellipsis
                                yield! paddingLeftLevel input.Level
                            ]
                            prop.children [
                                str taskName
                            ]
                        ]
                        TooltipPopupComponent.render
                            {| Comments = taskComments |}
                    ]
                ]
            )

        module UserStatusIndicatorComponent =
            let render = React.memo (fun (input: {| User: User |}) ->
                Html.div [
                    prop.classes [
                        Css.userIndicator
                        match input.User with
                        | { Color = UserColor.Blue } -> Css.bottomRightBlueIndicator
                        | { Color = UserColor.Pink } -> Css.bottomRightPinkIndicator
                    ]
                ]
            )

        module CellBorderComponent =
            let render = React.memo (fun (input: {| Date: FlukeDate |}) ->
                match input.Date with
                | StartOfMonth -> Some Css.cellStartMonth
                | StartOfWeek -> Some Css.cellStartWeek
                | _ -> None
                |> Option.map (fun className ->
                    Html.div [
                        prop.classes [
                            Css.cellSquare
                            className
                        ]
                    ]
                )
                |> Option.defaultValue nothing
            )

        module CellSessionIndicatorComponent =
            let render = React.memo (fun (input: {| Sessions: TaskSession list |}) ->
                Html.div [
                    prop.classes [
                        Css.cellSquare
                        Css.sessionLengthIndicator
                    ]
                    prop.children [
                        match input.Sessions.Length with
                        | x when x > 0 -> str (string x)
                        | _ -> ()
                    ]
                ]
            )

        module CellComponent =
            let render = React.memo (fun (input: {| TaskId: TaskId
                                                    Date: FlukeDate |}) ->
                let isToday = Recoil.useValue (Recoil.Selectors.isTodayFamily input.Date)

                let cellId = Recoil.Atoms.RecoilCell.cellId input.TaskId input.Date
                let cell = Recoil.useValue (Recoil.Atoms.RecoilCell.cellFamily cellId)

                let showUser = Recoil.useValue (Recoil.Selectors.showUserFamily input.TaskId)

                let comments = Recoil.useValue cell.Comments
                let sessions = Recoil.useValue cell.Sessions
                let status = Recoil.useValue cell.Status
                let selected, setSelected = Recoil.useState (Recoil.Selectors.RecoilCell.selectedFamily cellId)

                let onCellClick = React.useCallbackRef (fun () ->
                    setSelected (not selected)
                )

                Html.div [
                    prop.classes [
                        status.CellClass
                        if selected then Css.cellSelected
                        if isToday then Css.cellToday
                    ]
                    prop.onClick (fun (_event: MouseEvent) ->
                        onCellClick ()
                    )
                    prop.children [
                        CellBorderComponent.render
                            {| Date = input.Date |}
                        CellSessionIndicatorComponent.render
                            {| Sessions = sessions |}
                        if showUser then
                            match status with
                            | UserStatus (user, manualCellStatus) ->
                                UserStatusIndicatorComponent.render
                                    {| User = user |}
                            | _ -> ()
                        TooltipPopupComponent.render
                            {| Comments = comments |}
                    ]
                ]
            )

        module CellsComponent =
            let render = React.memo (fun (input: {| TaskIdList: TaskId list |}) ->
                Recoil.Profiling.addTimestamp "cells.render"
                let dateSequence = Recoil.useValue Recoil.Selectors.dateSequence

                Html.div [
                    prop.className Css.laneContainer
                    prop.children [
                        yield! input.TaskIdList
                        |> List.map (fun taskId ->
                            Html.div [
                                yield! dateSequence
                                |> List.map (fun date ->
                                    CellComponent.render
                                        {| TaskId = taskId
                                           Date = date |}
                                )
                            ]
                        )
                    ]
                ]
            )

        module CalendarViewComponent =
            let render = React.memo (fun () ->
                let currentTaskList = Recoil.useValue Recoil.Selectors.currentTaskList
                let taskIdList = currentTaskList |> List.map (fun x -> x.Id)

                Html.div [
                    prop.className Css.lanesPanel
                    prop.children [
                        // Column: Left
                        Html.div [
                            // Top Padding
                            yield! Html.div [
                                prop.className Css.cellRectangle
                            ]
                            |> List.replicate 3

                            Html.div [
                                prop.style [
                                    style.display.flex
                                ]
                                prop.children [
                                    // Column: Information Type
                                    Html.div [
                                        prop.style [
                                            style.paddingRight 10
                                        ]
                                        prop.children [
                                            yield! currentTaskList
                                            |> List.map (fun task ->
                                                Html.div [
                                                    prop.className Css.cellRectangle
                                                    prop.children [
                                                        Html.div [
                                                            prop.style [
                                                                style.color task.Information.Color
                                                                style.whitespace.nowrap
                                                            ]
                                                            prop.children [
                                                                str task.Information.Name
                                                            ]
                                                        ]
                                                        TooltipPopupComponent.render
                                                            {| Comments = task.InformationComments |}
                                                    ]
                                                ]
                                            )
                                        ]
                                    ]
                                    // Column: Task Name
                                    Html.div [
                                        prop.style [
                                            style.width 200
                                        ]
                                        prop.children [
                                            yield! currentTaskList
                                            |> List.map (fun task ->
                                                TaskNameComponent.render
                                                    {| Level = 0
                                                       TaskId = task.Id |}
                                            )
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            HeaderComponent.render ()
                            CellsComponent.render
                                {| TaskIdList = taskIdList |}
                        ]
                    ]
                ]
            )

        module GroupsViewComponent =
            let render = React.memo (fun () ->
                let currentTaskList = Recoil.useValue Recoil.Selectors.currentTaskList

                let groupMap =
                    currentTaskList
                    |> List.map (fun x -> x.Information, x)
                    |> Map.ofList

                let groups =
                    currentTaskList
                    |> List.groupBy (fun group -> group.Information)
                    |> List.sortBy (fun (information, _) -> information.Name)
                    |> List.groupBy (fun (information, _) -> information.KindName)
                    |> List.sortBy (snd >> List.head >> fst >> fun information -> information.Order)

                Html.div [
                    prop.className Css.lanesPanel
                    prop.children [
                        // Column: Left
                        Html.div [
                            // Top Padding
                            yield! Html.div [
                                prop.className Css.cellRectangle
                            ]
                            |> List.replicate 3

                            Html.div [
                                yield! groups
                                |> List.map (fun (informationKindName, taskGroups) ->
                                    Html.div [
                                        // Information Type
                                        Html.div [
                                            prop.style [
                                                style.color "#444"
                                            ]
                                            prop.children [
                                                str informationKindName
                                            ]
                                        ]
                                        Html.div [
                                            yield! taskGroups
                                            |> List.map (fun (information, group) ->
                                                let informationComments = groupMap.[information].InformationComments
                                                Html.div [
                                                    // Information
                                                    Html.div [
                                                        prop.className Css.cellRectangle
                                                        prop.children [
                                                            Html.div [
                                                                prop.style [
                                                                    style.color "#444"
                                                                    yield! paddingLeftLevel 1
                                                                ]
                                                                prop.children [
                                                                    str information.Name
                                                                ]
                                                            ]
                                                            TooltipPopupComponent.render
                                                                {| Comments = informationComments |}
                                                        ]
                                                    ]
                                                    // Task Name
                                                    Html.div [
                                                        prop.style [
                                                            style.width 400
                                                        ]
                                                        prop.children [
                                                            yield! group
                                                            |> List.map (fun groupTask ->
                                                                TaskNameComponent.render
                                                                    {| Level = 2
                                                                       TaskId = groupTask.Id |}
                                                            )
                                                        ]
                                                    ]
                                                ]
                                            )
                                        ]
                                    ]
                                )
                            ]
                        ]
                        // Column: Grid
                        Html.div [
                            HeaderComponent.render ()
                            Html.div [
                                yield! groups
                                |> List.map (fun (_, taskGroups) ->
                                    Html.div [
                                        Html.div [
                                            prop.className Css.cellRectangle
                                        ]
                                        Html.div [
                                            yield! taskGroups
                                            |> List.map (fun (_, groupTask) ->
                                                Html.div [
                                                    Html.div [
                                                        prop.className Css.cellRectangle
                                                    ]
                                                    CellsComponent.render
                                                        {| TaskIdList = groupTask |> List.map (fun x -> x.Id) |}
                                                ]
                                            )
                                        ]
                                    ]
                                )
                            ]
                        ]
                    ]
                ]
            )

        module TasksViewComponent =
            let render = React.memo (fun () ->
                let currentTaskList = Recoil.useValue Recoil.Selectors.currentTaskList
                let taskIdList = currentTaskList |> List.map (fun x -> x.Id)

                Html.div [
                    prop.className Css.lanesPanel
                    prop.children [
                        // Column: Left
                        Html.div [
                            // Top Padding
                            yield! Html.div [
                                prop.className Css.cellRectangle
                            ]
                            |> List.replicate 3

                            Html.div [
                                prop.style [
                                    style.display.flex
                                ]
                                prop.children [
                                    // Column: Information Type
                                    Html.div [
                                        prop.style [
                                            style.paddingRight 10
                                        ]
                                        prop.children [
                                            yield! currentTaskList
                                            |> List.map (fun task ->
                                                Html.div [
                                                    prop.className Css.cellRectangle
                                                    prop.children [
                                                        Html.div [
                                                            prop.style [
                                                                style.color task.Information.Color
                                                                style.whitespace.nowrap
                                                            ]
                                                            prop.children [
                                                                str task.Information.Name
                                                            ]
                                                        ]

                                                        TooltipPopupComponent.render
                                                            {| Comments = task.InformationComments |}
                                                    ]
                                                ]
                                            )
                                        ]
                                    ]
                                    // Column: Priority
                                    Html.div [
                                        prop.style [
                                            style.paddingRight 10
                                            style.textAlign.center
                                        ]
                                        prop.children [
                                            yield! currentTaskList
                                            |> List.map (fun task ->
                                                Html.div [
                                                    prop.className Css.cellRectangle
                                                    prop.children [
                                                        task.Priority
                                                        |> ofTaskPriorityValue
                                                        |> string
                                                        |> str
                                                    ]
                                                ]
                                            )
                                        ]
                                    ]
                                    // Column: Task Name
                                    Html.div [
                                        prop.style [
                                            style.width 200
                                        ]
                                        prop.children [
                                            yield! currentTaskList
                                            |> List.map (fun task ->
                                                TaskNameComponent.render
                                                    {| Level = 0
                                                       TaskId = task.Id |}
                                            )
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        Html.div [
                            HeaderComponent.render ()
                            CellsComponent.render
                                {| TaskIdList = taskIdList |}
                        ]
                    ]
                ]
            )

        module WeekViewComponent =
            let render = React.memo (fun () ->
                nothing
            )

    module DetailsPanel =

        module DetailsComponent =
            let render = React.memo (fun () ->
    //            let selectedCells = Recoil.useValue Recoil.Selectors.selectedCells


                Html.div [
                    prop.className Css.detailsPanel
                    prop.children [
                        str "Details"
                    ]
                ]
            )

    let render = React.memo (fun () ->

        let view = Recoil.useValue Recoil.Atoms.view

        Html.div [
            prop.className Css.panels
            prop.children [
                match view with
                | View.Calendar -> LanesPanel.CalendarViewComponent.render ()
                | View.Groups   -> LanesPanel.GroupsViewComponent.render ()
                | View.Tasks    -> LanesPanel.TasksViewComponent.render ()
                | View.Week     -> LanesPanel.WeekViewComponent.render ()

                DetailsPanel.DetailsComponent.render ()
            ]
        ]
    )

module MainComponent =
    let globalShortcutHandler = React.memo (fun () ->
        let selection, setSelection = Recoil.useState Recoil.Selectors.selection
        let ctrlPressed, setCtrlPressed = Recoil.useState Recoil.Atoms.ctrlPressed

        let keyEvent (e: KeyboardEvent) =
            if e.ctrlKey <> ctrlPressed then
                setCtrlPressed e.ctrlKey

            if e.key = "Escape" && not selection.IsEmpty then
                setSelection Map.empty

        React.useListener.onKeyDown keyEvent
        React.useListener.onKeyUp keyEvent

        nothing
    )
    let positionUpdater = React.memo (fun () ->
        let resetPosition = Recoil.useResetState Recoil.Selectors.position

        Scheduling.useScheduling Scheduling.Interval resetPosition (60 * 1000)
//        Scheduling.useScheduling Scheduling.Interval resetPosition (10 * 1000)

        nothing
    )
    let dataLoader = React.memo (fun () ->
        let view = Recoil.useValue Recoil.Atoms.view

        let loadTree = Recoil.useCallbackRef (fun setter ->
            async {
                Recoil.Profiling.addTimestamp "dataLoader.loadTreeCallback[0]"
                let! treeAsync = setter.snapshot.getAsync (Recoil.Selectors.treeAsync view)
                //Uncaught (in promise) Promise {<fulfilled>: List}
                Recoil.Profiling.addTimestamp "dataLoader.loadTreeCallback[1]"
                setter.set (Recoil.Selectors.currentTree, Some treeAsync)
                Recoil.Profiling.addTimestamp "dataLoader.loadTreeCallback[2]"
            }
            |> Async.StartImmediate
        )

        Recoil.Profiling.addTimestamp "dataLoader render"
        React.useEffect (fun () ->
            Recoil.Profiling.addTimestamp "dataLoader effect"
            loadTree ()

            // TODO: return a cleanup?
        , [| view :> obj |])

        nothing
    )
    let soundPlayer = React.memo (fun () ->
        let oldActiveSessions = React.useRef []

        let activeSessions = Recoil.useValue Recoil.Selectors.activeSessions

        React.useEffect (fun () ->
            oldActiveSessions.current
            |> List.map (fun (Model.ActiveSession (oldTaskName, oldDuration)) ->
                let newSession =
                    activeSessions
                    |> List.tryFind (fun (Model.ActiveSession (taskName, duration)) ->
                        taskName = oldTaskName && duration = oldDuration + 1.
                    )

                match newSession with
                | Some (Model.ActiveSession (_, newDuration)) when oldDuration = -1. && newDuration = 0. -> Temp.Sound.playTick
                | Some (Model.ActiveSession (_, newDuration)) when newDuration = TempData.Consts.sessionLength -> Temp.Sound.playDing
                | None when oldDuration = TempData.Consts.sessionLength + TempData.Consts.sessionBreakLength - 1. -> Temp.Sound.playDing
                | _ -> fun () -> ()
            )
            |> List.iter (fun x -> x ())

            oldActiveSessions.current <- activeSessions
        , [| activeSessions :> obj |])

        nothing
    )
    let autoReload_TEMP = React.memo (fun () ->
        let reload = React.useCallback (fun () ->
            Dom.window.location.reload true
        )

        Scheduling.useScheduling Scheduling.Timeout reload (60 * 60 * 1000)

        nothing
    )

    let render = React.memo (fun () ->
        React.fragment [
            globalShortcutHandler ()
            positionUpdater ()
            autoReload_TEMP ()

            React.suspense ([
                dataLoader ()
                soundPlayer ()

                NavBarComponent.render ()
                PanelsComponent.render ()
            ], PageLoaderComponent.render ())
        ]
    )

