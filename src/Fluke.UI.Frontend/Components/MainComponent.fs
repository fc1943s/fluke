namespace Fluke.UI.Frontend.Components

open System
open Browser
open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend
open Browser.Types
open Browser.Types
open FSharpPlus
open Fable.Core
open Feliz
open Feliz.Recoil
open Fluke.Shared
open Fluke.UI.Frontend
open Fable.React
open Fable.React.Props
open Fable.DateFunctions
open Fulma
open System
open Fulma.Extensions.Wikiki
open Suigetsu.UI.Frontend.ElmishBridge
open Suigetsu.UI.Frontend.React
open Suigetsu.Core


module SuigetsuTemp =
    module CustomHooks =
        let useWindowSize () =
            let getWindowSize () =
                {| Width = window.innerWidth
                   Height = window.innerHeight |}
            let size, setSize = React.useState (getWindowSize ())

            React.useLayoutEffect (fun () ->
                let updateSize (_event: Event) =
                    setSize (getWindowSize ())

                window.addEventListener ("resize", updateSize)

                { new IDisposable with
                    member _.Dispose () =
                        window.removeEventListener ("resize", updateSize)
                }
            )
            size

module PageLoaderComponent =
    let render = React.memo (fun () ->
        PageLoader.pageLoader [ PageLoader.Color IsDark
                                PageLoader.IsActive true ][]
    )

module NavBarComponent =
    open Model
    let render = React.memo (fun () ->
        let view, setView = Recoil.useState Recoil.Atoms.view
        let activeSessions = Recoil.useValue Recoil.Atoms.activeSessions

        Ext.useEventListener "keydown" (fun (e: KeyboardEvent) ->
            match e.ctrlKey, e.shiftKey, e.key with
            | _, true, "C" -> setView View.Calendar
            | _, true, "G" -> setView View.Groups
            | _, true, "T" -> setView View.Tasks
            | _, true, "W" -> setView View.Week
            | _            -> ()
        )

        Navbar.navbar [ Navbar.Color IsBlack
                        Navbar.Props [ Style [ Height 36
                                               MinHeight 36
                                               Display DisplayOptions.Flex
                                               JustifyContent "space-around" ]]][

            let checkbox newView text =
                Navbar.Item.div [ Navbar.Item.Props [ Class "field"
                                                      OnClick (fun _ -> setView newView)
                                                      Style [ MarginBottom 0
                                                              AlignSelf AlignSelfOptions.Center ] ] ][

                    Checkbox.input [ CustomClass "switch is-small is-dark"
                                     Props [ Checked (view = newView)
                                             OnChange (fun _ -> ()) ]]

                    Checkbox.checkbox [][
                        str text
                    ]
                ]

            checkbox View.Calendar "calendar view"
            checkbox View.Groups "groups view"
            checkbox View.Tasks "tasks view"
            checkbox View.Week "week view"

            Navbar.Item.div [][
                activeSessions
                |> List.map (fun (ActiveSession (task, duration)) ->
                    let sessionType, color, duration, left =
                        let left = TempData.sessionLength - duration
                        match duration < TempData.sessionLength with
                        | true  -> "Session", "#7cca7c", duration, left
                        | false -> "Break",   "#ca7c7c", -left,    TempData.sessionBreakLength + left

                    span [ Style [ Color color ] ][
                        sprintf "%s: Task[ %s ]; Duration[ %.1f ]; Left[ %.1f ]" sessionType task.Name duration left
                        |> str
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

    let render = React.memo (fun (input: {| Comments: Comment list |}) ->
        Html.div [
            prop.className Css.tooltipPopup
            prop.children [
                input.Comments
                |> List.map (fun (Comment (_user, comment)) -> comment.Trim ())
                |> List.map ((+) Environment.NewLine)
                |> String.concat (Environment.NewLine + Environment.NewLine)
                |> fun text ->
                    ReactBindings.React.createElement
                        (Ext.reactMarkdown, {| source = text |}, [])
            ]
        ]
    )

module ApplicationComponent =
    open Model

    module Grid =
        let emptyDiv =
            div [ DangerouslySetInnerHTML { __html = "&nbsp;" } ][]

        let paddingLeftLevel level =
            PaddingLeft (20 * level)

        let header = React.memo (fun () ->
            let dateSequence = Recoil.useValue Recoil.Selectors.dateSequence
            let selection = Recoil.useValue Recoil.Selectors.selectionTracker

            let selectionSet =
                selection
                |> Map.values
                |> Set.unionMany

            let datesInfo =
                dateSequence
                |> List.map (fun date ->
                    let isToday = Recoil.useValue (Recoil.Selectors.isTodayFamily date)
                    let info =
                        {| IsSelected = selectionSet.Contains date
                           IsToday = isToday |}
                    date, info
                )
                |> Map.ofList

            div [][
                // Month row
                div [ Style [ Display DisplayOptions.Flex ] ][
                    yield! dateSequence
                    |> List.groupBy (fun date -> date.Month)
                    |> List.map (fun (_, dates) -> dates.Head, dates.Length)
                    |> List.map (fun (firstDay, days) ->
                        span [ Style [ TextAlign TextAlignOptions.Center
                                       Width (17 * days) ] ][
                            str (firstDay.DateTime.Format "MMM")
                        ]
                    )
                ]

                // Day of Week row
                div [ Style [ Display DisplayOptions.Flex ] ][
                    yield! dateSequence
                    |> List.map (fun date ->
                        span [ classList [ Css.todayHeader, datesInfo.[date].IsToday
                                           Css.selectionHighlight, datesInfo.[date].IsSelected ]
                               Style [ Width 17
                                       Functions.getCellSeparatorBorderLeft date
                                       TextAlign TextAlignOptions.Center ] ][

                            date.DateTime.Format "dd"
                            |> String.toLower
                            |> str
                        ]
                    )
                ]

                // Day row
                div [ Style [ Display DisplayOptions.Flex ] ][

                    yield! dateSequence
                    |> List.map (fun date ->
                        span [ classList [ Css.todayHeader, datesInfo.[date].IsToday
                                           Css.selectionHighlight, datesInfo.[date].IsSelected ]
                               Style [ Width 17
                                       Functions.getCellSeparatorBorderLeft date
                                       TextAlign TextAlignOptions.Center ] ][
                            str (date.Day.ToString "D2")
                        ]
                    )
                ]
            ]
        )

        let taskName = React.memo (fun (input: {| Level: int
                                                  Task: Task |}) ->
            let taskId = Recoil.Atoms.RecoilTask.taskId input.Task
            let comments = Recoil.useValue (Recoil.Selectors.RecoilTask.comments taskId)
            let selection = Recoil.useValue Recoil.Selectors.selectionTracker

            let isSelected =
                selection
                |> Map.tryFind taskId
                |> Option.defaultValue Set.empty
                |> Set.isEmpty
                |> not

            div [ classList [ Css.tooltipContainer, not comments.IsEmpty ]
                  Style [ Height 17 ] ][

                div [ classList [ Css.selectionHighlight, isSelected ]
                      Style [ CSSProp.Overflow OverflowOptions.Hidden
                              WhiteSpace WhiteSpaceOptions.Nowrap
                              paddingLeftLevel input.Level
                              TextOverflow "ellipsis" ] ][

                    str input.Task.Name
                ]

                if not comments.IsEmpty then
                    TooltipPopupComponent.render {| Comments = comments |}
            ]
        )

        let cell = React.memo (fun (input: {| CellAddress: CellAddress |}) ->
            let isToday = Recoil.useValue (Recoil.Selectors.isTodayFamily input.CellAddress.Date)
            let taskId = Recoil.Atoms.RecoilTask.taskId input.CellAddress.Task
            let cellId = Recoil.Atoms.RecoilCell.cellId taskId input.CellAddress.Date
            let cell = Recoil.useValue (Recoil.Atoms.RecoilCell.cellFamily cellId)
            let status = Recoil.useValue cell.Status
            let comments = Recoil.useValue cell.Comments
            let sessions = Recoil.useValue cell.Sessions
            let selected, setSelected = Recoil.useState cell.Selected
    //        let selected, setSelected = false, fun _ -> ()

            let events = {|
                OnCellClick = fun () ->
                    setSelected (not selected)
            |}


            Html.div [
                prop.classes [
                    status.CellClass
                    if not comments.IsEmpty then
                        Css.tooltipContainer
                    if selected then
                        Css.cellSelected
                    if isToday then
                        Css.cellToday
                ]
                prop.children [
                    Html.div [
                        prop.style [
                            match Functions.getCellSeparatorBorderLeft2 input.CellAddress.Date with
                            | Some borderLeft -> borderLeft
                            | None -> ()
                        ]
                        prop.onClick (fun (_event: MouseEvent) ->
                            events.OnCellClick ()
                        )
                        prop.children [
                            match sessions.Length with
            //                | x -> str (string x)
                            | x when x > 0 -> str (string x)
                            | _ -> ()
                        ]
                    ]

                    if not comments.IsEmpty then
                        TooltipPopupComponent.render {| Comments = comments |}
                ]
            ]
        )

        let cells = React.memo (fun () ->
            let dateSequence = Recoil.useValue Recoil.Selectors.dateSequence
            let sortedLaneList = Recoil.useValue Recoil.Selectors.sortedLaneList
            let tasks = sortedLaneList |> List.map (ofLane >> fst)

            div [ Class Css.laneContainer ][

                yield! tasks
                |> List.map (fun task ->

                    div [][
                        yield! dateSequence
                        |> List.map (fun date ->
                            React.suspense ([
                                cell {| CellAddress = { Task = task; Date = date } |}
                            ], str "C")
                        )
                    ]
                )
            ]
        )

    module CalendarViewComponent =
        let render = React.memo (fun () ->
            let sortedLaneList = Recoil.useValue Recoil.Selectors.sortedLaneList
            let tasks = sortedLaneList |> List.map (ofLane >> fst)

            div [ Style [ Display DisplayOptions.Flex ] ][

                // Column: Left
                div [][

                    // Top Padding
                    div [][
                        yield! Grid.emptyDiv |> List.replicate 3
                    ]

                    div [ Style [ Display DisplayOptions.Flex ] ][

                        // Column: Information Type
                        div [ Style [ PaddingRight 10 ] ] [

                            yield! tasks
                            |> List.map (fun task ->
                                let comments = Recoil.useValue (Recoil.Selectors.RecoilInformation.comments task.Information)
//                                let comments = []

                                div [ classList [ Css.blueIndicator, not comments.IsEmpty
                                                  Css.tooltipContainer, not comments.IsEmpty ]
                                      Style [ Padding 0
                                              Height 17
                                              Color task.Information.Color
                                              WhiteSpace WhiteSpaceOptions.Nowrap ] ][

                                    str task.Information.Name

                                    if not comments.IsEmpty then
                                        TooltipPopupComponent.render {| Comments = comments |}
                                ]
                            )
                        ]

                        // Column: Task Name
                        div [ Style [ Width 200 ] ] [
                            yield! tasks
                            |> List.map (fun task ->
                                Grid.taskName {| Level = 0; Task = task |}
                            )
                        ]
                    ]
                ]

                div [][
                    Grid.header ()

                    Grid.cells ()
                ]
            ]
        )

    module GroupsViewComponent =
        let render = React.memo (fun () ->
            printfn "GROUPS VIEW"
            let sortedLaneList = Recoil.useValue Recoil.Selectors.sortedLaneList

            let groups =
                sortedLaneList
                |> List.map (ofLane >> fst)
                |> List.groupBy (fun task -> task.Information)
                |> List.groupBy (fun (info, _) ->
                    match info with
                    | Project _  -> "projects"
                    | Area _     -> "areas"
                    | Resource _ -> "resources"
                    | Archive _  -> "archives"
                )

            div [ Style [ Display DisplayOptions.Flex ] ][

                // Column: Left
                div [][

                    // Top Padding
                    div [][
                        yield! Grid.emptyDiv |> List.replicate 3
                    ]

                    div [][

                        yield! groups
                        |> List.map (fun (informationType, taskGroups) ->
                            div [][
                                // Information Type
                                div [ Style [ Color "#444" ] ][
                                    str informationType
                                ]

                                div [][

                                    yield! taskGroups
                                    |> List.map (fun (information, tasks) ->
                                        let comments = Recoil.useValue (Recoil.Selectors.RecoilInformation.comments information)

                                        div [][
                                            // Information
                                            div [ classList [ Css.blueIndicator, comments.IsEmpty
                                                              Css.tooltipContainer, comments.IsEmpty ]
                                                  Style [ Grid.paddingLeftLevel 1
                                                          Color "#444" ] ][
                                                str information.Name

                                                if not comments.IsEmpty then
                                                    TooltipPopupComponent.render {| Comments = comments |}
                                            ]


                                            // Task Name
                                            div [ Style [ Width 500 ] ][

                                                yield! tasks
                                                |> List.map (fun task ->
                                                    Grid.taskName {| Level = 2; Task = task |}
                                                )
                                            ]
                                        ]
                                    )
                                ]
                            ]
                        )
                    ]
                ]

                // Column: Grid
                div [][
                    Grid.header ()

                    div [][

                        yield! groups
                        |> List.map (fun (_, taskGroups) ->

                            div [][

                                Grid.emptyDiv
                                div [][

                                    yield! taskGroups
                                    |> List.map (fun (_, tasks) ->

                                        div [][
                                            Grid.emptyDiv
                                            Grid.cells ()
                                        ]
                                    )
                                ]
                            ]
                        )
                    ]
                ]
            ]
        )

    module TasksViewComponent =
        let render = React.memo (fun () ->
            let sortedLaneList = Recoil.useValue Recoil.Selectors.sortedLaneList
            let tasks = sortedLaneList |> List.map (ofLane >> fst)

            div [ Style [ Display DisplayOptions.Flex ] ][

                // Column: Left
                div [][
                    // Top Padding
                    div [][
                        yield! Grid.emptyDiv |> List.replicate 3
                    ]

                    div [ Style [ Display DisplayOptions.Flex ] ][
                        // Column: Information Type
                        div [ Style [ PaddingRight 10 ] ] [
                            yield! tasks
                            |> List.map (fun task ->
                                let comments = Recoil.useValue (Recoil.Selectors.RecoilInformation.comments task.Information)

                                div [ classList [ Css.blueIndicator, comments.IsEmpty
                                                  Css.tooltipContainer, comments.IsEmpty ]
                                      Style [ Padding 0
                                              Height 17
                                              Color task.Information.Color
                                              WhiteSpace WhiteSpaceOptions.Nowrap ] ][

                                    str task.Information.Name

                                    if not comments.IsEmpty then
                                        TooltipPopupComponent.render {| Comments = comments |}
                                ]
                            )
                        ]

                        // Column: Priority
                        div [ Style [ PaddingRight 10
                                      TextAlign TextAlignOptions.Center ] ] [
                            yield! tasks
                            |> List.map (fun task ->
                                let taskId = Recoil.Atoms.RecoilTask.taskId task
                                let priorityValue = Recoil.useValue (Recoil.Selectors.RecoilTask.priorityValue taskId)
                                div [ Style [ Height 17 ] ][
                                    priorityValue
                                    |> ofTaskPriorityValue
                                    |> string
                                    |> str
                                ]
                            )
                        ]

                        // Column: Task Name
                        div [ Style [ Width 200 ] ] [
                            yield! tasks
                            |> List.map (fun task ->
                                Grid.taskName {| Level = 0; Task = task |}
                            )
                        ]
                    ]
                ]

                div [][
                    Grid.header ()

                    Grid.cells ()
                ]
            ]
        )

    module WeekViewComponent =
        let render = React.memo (fun () ->
            nothing
        )

    let render = React.memo (fun () ->

        let view = Recoil.useValue Recoil.Atoms.view
//        let now = Recoil.useValue Recoil.Selectors.now
//        let activeSessions, setActiveSessions = Recoil.useState Recoil.Selectors.activeSessions
//        let selection, setSelection = Recoil.useState Recoil.Selectors.selection
//        let dayStart = Recoil.useValue Recoil.Selectors.dayStart
//        let dateSequence = Recoil.useValue Recoil.Selectors.dateSequence
//        let dings, setDings = Recoil.useState (Recoil.Selectors.dingsFamily now)
//        let ctrlPressed = Recoil.useValue Recoil.Selectors.ctrlPressed
//        let taskStateList = Recoil.useValue Recoil.Selectors.taskStateList
//        let informationList = Recoil.useValue Recoil.Selectors.informationList
//        let taskOrderList = Recoil.useValue Recoil.Selectors.taskOrderList

//        taskStateList
//        |> List.iter (fun taskState ->
//            let setTaskCells = Recoil.useSetState (Recoil.Selectors.taskCellsFamily taskState.Task)
//            setTaskCells cells
//        )

//        let newActiveSessions =
//            lastSessions
//            |> List.map (Tuple2.mapSnd (fun (TaskSession start) -> (now.DateTime - start.DateTime).TotalMinutes))
//            |> List.filter (fun (_, length) -> length < TempData.sessionLength + TempData.sessionBreakLength)
//            |> List.map ActiveSession
//
//        if activeSessions <> newActiveSessions then
//            setActiveSessions newActiveSessions
//
//        newActiveSessions
//        |> List.map (fun (ActiveSession (oldTask, oldDuration)) ->
//            let newSession =
//                activeSessions
//                |> List.tryFind (fun (ActiveSession (task, duration)) ->
//                    task = oldTask && duration = oldDuration + 1.
//                )
//
//            match newSession with
//            | Some (ActiveSession (_, newDuration)) when oldDuration = -1. && newDuration = 0. -> playTick
//            | Some (ActiveSession (_, newDuration)) when newDuration = TempData.sessionLength -> playDing
//            | None when oldDuration = TempData.sessionLength + TempData.sessionBreakLength - 1. -> playDing
//            | _ -> fun () -> ()
//        )
//        |> List.iter (fun x -> x ())

        printfn "HomePageComponent.render. VIEW: %A" view

        Html.div [
            match view with
            | View.Calendar -> CalendarViewComponent.render ()
            | View.Groups   -> GroupsViewComponent.render ()
            | View.Tasks    -> TasksViewComponent.render ()
            | View.Week     -> WeekViewComponent.render ()
        ]
    )



    ()

module MainComponent =
    let nowUpdater = React.memo (fun () ->
        let getNow = Recoil.useValue Recoil.Atoms.getNow
        let setNow = Recoil.useSetState Recoil.Atoms.now

        let updateNow () =
            getNow
            |> fun x -> x ()
            |> setNow

        CustomHooks.useInterval updateNow (60 * 1000)

        nothing
    )

    let globalShortcutHandler = React.memo (fun () ->
        let selection, setSelection = Recoil.useState Recoil.Selectors.selectionTracker
        let ctrlPressed, setCtrlPressed = Recoil.useState Recoil.Atoms.ctrlPressed

        let keyEvent (e: KeyboardEvent) =
            if e.ctrlKey <> ctrlPressed then
                setCtrlPressed e.ctrlKey

            if e.key = "Escape" && not selection.IsEmpty then
                setSelection Map.empty

        Ext.useEventListener "keydown" keyEvent
        Ext.useEventListener "keyup" keyEvent

        nothing
    )

    let render = React.memo (fun () ->
        React.suspense ([
            nowUpdater ()
            globalShortcutHandler ()

            NavBarComponent.render ()

            ApplicationComponent.render ()
        ], PageLoaderComponent.render ())
    )

