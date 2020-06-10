namespace Fluke.UI.Frontend

open Browser.Types
open FSharpPlus
open Fable.Core
open Feliz
open Fluke.Shared
open Fluke.UI.Frontend
open Fable.React
open Fable.React.Props
open Fable.DateFunctions
open Fulma
open System
open Suigetsu.UI.Frontend.ElmishBridge
open Suigetsu.UI.Frontend.React
open Suigetsu.Core


module Temp =
    type View =
        | CalendarView
        | GroupsView
        | TasksView

    type TempDataType =
        | TempPrivate
        | TempPublic
        | Test

    let view = CalendarView
//    let view = GroupsView
//    let view = TasksView

    let tempDataType = TempPrivate
//    let tempDataType = Test
//    let tempDataType = TempPublic

    let testData = TempData.tempData.RenderLaneTests
//    let testData = TempData.tempData.SortLanesTests


    let cellComments =
        PrivateData.Journal.journalComments
        |> List.append PrivateData.CellComments.cellComments
        |> List.append SharedPrivateData.Data.cellComments

    let taskStateList, getNow, informationComments, taskOrderList, dayStart, informationList =
        match tempDataType with
        | TempPrivate ->
            let taskData = PrivateData.Tasks.tempManualTasks
            let sharedTaskData = SharedPrivateData.SharedTasks.tempManualTasks

            let taskStateList =
                taskData.TaskStateList
                |> List.map (fun taskState ->
                    { taskState with
                        StatusEntries =
                            PrivateData.CellStatusEntries.cellStatusEntries
                            |> Model.createTaskStatusEntries taskState.Task
                            |> List.prepend taskState.StatusEntries
                        Comments =
                            PrivateData.TaskComments.taskComments
                            |> List.filter (fun (Model.TaskComment (task, _)) -> task = taskState.Task)
                            |> List.map (Model.ofTaskComment >> snd)
                            |> List.prepend taskState.Comments })

            let sharedTaskStateList =
                sharedTaskData.TaskStateList
                |> List.map (fun taskState ->
                    { taskState with
                        StatusEntries =
                            SharedPrivateData.Data.cellStatusEntries
                            |> Model.createTaskStatusEntries taskState.Task
                            |> List.prepend taskState.StatusEntries
                        Comments =
                            SharedPrivateData.Data.taskComments
                            |> List.filter (fun (Model.TaskComment (task, _)) -> task = taskState.Task)
                            |> List.map (Model.ofTaskComment >> snd)
                            |> List.prepend taskState.Comments })

            let informationComments =
                PrivateData.InformationComments.informationComments
                |> List.append SharedPrivateData.Data.informationComments
                |> List.groupBy (fun x -> x.Information)
                |> List.map (Tuple2.mapSnd (List.map (fun x -> x.Comment)))
                |> Map.ofList

            taskStateList |> List.append sharedTaskStateList,
            TempData.getNow,
            informationComments,
            taskData.TaskOrderList @ PrivateData.Tasks.taskOrderList,
            PrivateData.PrivateData.dayStart,
            taskData.InformationList
        | TempPublic ->
            let taskData = TempData.tempData.ManualTasks

            taskData.TaskStateList,
            TempData.getNow,
            Map.empty,
            taskData.TaskOrderList,
            TempData.dayStart,
            taskData.InformationList
        | Test ->
            testData.TaskStateList,
            testData.GetNow,
            Map.empty,
            testData.TaskOrderList,
            TempData.testDayStart,
            [] // informationList


module HomePageComponent =
    open Model

    type Props =
        { Dispatch: SharedState.SharedServerMessage -> unit
          UIState: UIState.State
          PrivateState: Client.PrivateState<UIState.State> }

    type ActiveSession = ActiveSession of task:Task * duration:float

    type State =
        { Now: FlukeDateTime
          Selection: CellAddress list
          Lanes: Lane list
          ActiveSessions: ActiveSession list
          View: Temp.View }
        static member inline Default =
            { Now = { Date = flukeDate 0000 Month.January 01; Time = flukeTime 00 00 }
              Selection = []
              Lanes = []
              ActiveSessions = []
              View = Temp.CalendarView }

    let playDing () =
         [ 0; 1400 ]
         |> List.map (JS.setTimeout (fun () -> Ext.playAudio "./sounds/ding.wav"))
         |> ignore

    let playTick () =
        Ext.playAudio "./sounds/tick.wav"

    let navBar (props: {| View: Temp.View
                          SetView: Temp.View -> unit
                          Now: FlukeDateTime
                          ActiveSessions: ActiveSession list |}) =

        let events = {|
            OnViewChange = fun view ->
                props.SetView view
        |}

        Ext.useEventListener "keydown" (fun (e: KeyboardEvent) ->
            match e.ctrlKey, e.shiftKey, e.key with
            | _, true, "C" -> events.OnViewChange Temp.CalendarView
            | _, true, "G" -> events.OnViewChange Temp.GroupsView
            | _, true, "T" -> events.OnViewChange Temp.TasksView
            | _            -> ()
        )

        Navbar.navbar [ Navbar.Color IsBlack
                        Navbar.Props [ Style [ Height 36
                                               MinHeight 36
                                               Display DisplayOptions.Flex
                                               JustifyContent "space-around" ]]][

            let checkbox view text =
                Navbar.Item.div [ Navbar.Item.Props [ Class "field"
                                                      OnClick (fun _ -> events.OnViewChange view)
                                                      Style [ MarginBottom 0
                                                              AlignSelf AlignSelfOptions.Center ] ] ][

                    Checkbox.input [ CustomClass "switch is-small is-dark"
                                     Props [ Checked (props.View = view)
                                             OnChange (fun _ -> ()) ]]

                    Checkbox.checkbox [][
                        str text
                    ]
                ]

            checkbox Temp.CalendarView "calendar view"
            checkbox Temp.GroupsView "groups view"
            checkbox Temp.TasksView "tasks view"

            Navbar.Item.div [][
                props.ActiveSessions
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


    module Grid =
        let paddingLeftLevel level =
            PaddingLeft (20 * level)

        let emptyDiv =
            div [ DangerouslySetInnerHTML { __html = "&nbsp;" } ][]

        let taskNameList level (taskStateMap: Map<Task,TaskState>) tasks =
            tasks
            |> List.map (fun task ->
                let comments =
                    taskStateMap
                    |> Map.tryFind task
                    |> Option.map (fun taskState -> taskState.Comments)

                div [ classList [ Css.tooltipContainer, match comments with Some (_ :: _) -> true | _ -> false ]
                      Style [ Height 17 ] ][

                    div [ Style [ CSSProp.Overflow OverflowOptions.Hidden
                                  WhiteSpace WhiteSpaceOptions.Nowrap
                                  paddingLeftLevel level
                                  TextOverflow "ellipsis" ] ][

                        str task.Name
                    ]

                    comments
                    |> Option.map CellComponent.tooltipPopup
                    |> Option.defaultValue nothing
                ]
            )

        let gridCells dayStart now selection cellComments taskStateMap lanes =
            div [ Class Css.laneContainer ][

                yield! Rendering.getLanesState dayStart now selection cellComments taskStateMap lanes
                |> List.map (fun (_, laneState) ->

                    div [][
                        yield! laneState
                        |> List.map (fun laneCell ->

                            CellComponent.``default``
                                { CellAddress = laneCell.CellAddress
                                  Comments = laneCell.Comments
                                  Sessions = laneCell.Sessions
                                  IsSelected = laneCell.IsSelected
                                  IsToday = laneCell.IsToday
                                  Status = laneCell.Status }
                        )
                    ]
                )
            ]

        let gridHeader dayStart dateSequence (now: FlukeDateTime) =
            div [][

                // Month row
                div [ Style [ Display DisplayOptions.Flex ] ][
                    yield! dateSequence
                    |> List.groupBy (fun date -> date.Month)
                    |> List.map (fun (_, dates) -> dates.Head, dates.Length)
                    |> List.map (fun (firstDay, days) ->
                        span [ Style [ Functions.getCellSeparatorBorderLeft firstDay
                                       TextAlign TextAlignOptions.Center
                                       Width (17 * days) ] ][
                            str (firstDay.DateTime.Format "MMM")
                        ]
                    )
                ]

                // Day of Week row
                div [ Style [ Display DisplayOptions.Flex ] ][
                    yield! dateSequence
                    |> List.map (fun date ->
                        span [ Style [ Width 17
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
                        span [ Style [ Width 17
                                       Functions.getCellSeparatorBorderLeft date
                                       TextAlign TextAlignOptions.Center
                                       Color (if isToday dayStart now date then "#f22" else "") ] ][
                            str (date.Day.ToString "D2")
                        ]
                    )
                ]
            ]

        let calendarView dayStart dateSequence now selection informationComments cellComments taskStateMap lanes =
            let tasks = lanes |> List.map (fun (Lane (task, _)) -> task)

            div [ Style [ Display DisplayOptions.Flex ] ][

                // Column: Left
                div [][

                    // Top Padding
                    div [][
                        yield! emptyDiv |> List.replicate 3
                    ]

                    div [ Style [ Display DisplayOptions.Flex ] ][

                        // Column: Information Type
                        div [ Style [ PaddingRight 10 ] ] [

                            yield! tasks
                            |> List.map (fun task ->
                                let comments = informationComments |> Map.tryFind task.Information

                                div [ classList [ Css.blueIndicator, comments.IsSome
                                                  Css.tooltipContainer, comments.IsSome ]
                                      Style [ Padding 0
                                              Height 17
                                              Color task.Information.Color
                                              WhiteSpace WhiteSpaceOptions.Nowrap ] ][

                                    str task.Information.Name

                                    match comments with
                                    | None -> ()
                                    | Some comments ->
                                        comments
                                        |> CellComponent.tooltipPopup
                                ]
                            )
                        ]

                        // Column: Task Name
                        div [ Style [ Width 200 ] ] [
                            yield! taskNameList 0 taskStateMap tasks
                        ]
                    ]
                ]

                div [][
                    gridHeader dayStart dateSequence now

                    gridCells dayStart now selection cellComments taskStateMap lanes
                ]
            ]

        let groupsView dayStart dateSequence now selection informationComments cellComments taskStateMap lanes =
            let tasks = lanes |> List.map (fun (Lane (task, _)) -> task)

            let groups =
                lanes
                |> List.groupBy (fun (Lane (task, _)) -> task.Information)
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
                        yield! emptyDiv |> List.replicate 3
                    ]

                    div [][

                        yield! groups
                        |> List.map (fun (informationType, lanesGroups) ->
                            div [][
                                // Information Type
                                div [ Style [ Color "#444" ] ][
                                    str informationType
                                ]

                                div [][

                                    yield! lanesGroups
                                    |> List.map (fun (information, _lanes) ->
                                        let comments = informationComments |> Map.tryFind information

                                        div [][
                                            // Information
                                            div [ classList [ Css.blueIndicator, comments.IsSome
                                                              Css.tooltipContainer, comments.IsSome ]
                                                  Style [ paddingLeftLevel 1
                                                          Color "#444" ] ][
                                                str information.Name

                                                match comments with
                                                | None -> ()
                                                | Some comments ->
                                                    comments
                                                    |> CellComponent.tooltipPopup
                                            ]


                                            // Task Name
                                            div [ Style [ Width 500 ] ][

                                                yield! taskNameList 2 taskStateMap tasks
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
                    gridHeader dayStart dateSequence now

                    div [][

                        yield! groups
                        |> List.map (fun (_, groupLanes) ->

                            div [][

                                emptyDiv
                                div [][

                                    yield! groupLanes
                                    |> List.map (fun (_, lanes) ->

                                        div [][
                                            emptyDiv
                                            gridCells dayStart now selection cellComments taskStateMap lanes
                                        ]
                                    )
                                ]
                            ]
                        )
                    ]
                ]
            ]

        let tasksView dayStart dateSequence now selection informationComments cellComments taskStateMap lanes =
            let lanes =
                lanes
                |> List.sortByDescending (fun (Lane (task, _)) ->
                    taskStateMap
                    |> Map.find task
                    |> fun x -> x.PriorityValue
                    |> Option.map ofTaskPriorityValue
                    |> Option.defaultValue 0
                )

            let tasks = lanes |> List.map (fun (Lane (task, _)) -> task)

            div [ Style [ Display DisplayOptions.Flex ] ][

                // Column: Left
                div [][
                    // Top Padding
                    div [][
                        yield! emptyDiv |> List.replicate 3
                    ]

                    div [ Style [ Display DisplayOptions.Flex ] ][
                        // Column: Information Type
                        div [ Style [ PaddingRight 10 ] ] [
                            yield! tasks
                            |> List.map (fun task ->
                                let comments = informationComments |> Map.tryFind task.Information

                                div [ classList [ Css.blueIndicator, comments.IsSome
                                                  Css.tooltipContainer, comments.IsSome ]
                                      Style [ Padding 0
                                              Height 17
                                              Color task.Information.Color
                                              WhiteSpace WhiteSpaceOptions.Nowrap ] ][

                                    str task.Information.Name

                                    match comments with
                                    | None -> ()
                                    | Some comments ->
                                        comments
                                        |> CellComponent.tooltipPopup
                                ]
                            )
                        ]

                        // Column: Priority
                        div [ Style [ PaddingRight 10
                                      TextAlign TextAlignOptions.Center ] ] [
                            yield! tasks
                            |> List.map (fun task ->
                                let taskState = taskStateMap.[task]
                                div [ Style [ Height 17 ] ][
                                    taskState.PriorityValue
                                    |> Option.map ofTaskPriorityValue
                                    |> Option.defaultValue 0
                                    |> string
                                    |> str
                                ]
                            )
                        ]

                        // Column: Task Name
                        div [ Style [ Width 200 ] ] [
                            yield! taskNameList 0 taskStateMap tasks
                        ]
                    ]
                ]

                div [][
                    gridHeader dayStart dateSequence now

                    gridCells dayStart now selection cellComments taskStateMap lanes
                ]
            ]

    let getLanes dayStart (dateSequence: FlukeDate list) (now: FlukeDateTime) informationList taskStateList taskOrderList view =
        match dateSequence with
        | [] -> []
        | dateSequence ->
            let dateRange =
                let head = dateSequence |> List.head |> fun x -> x.DateTime
                let last = dateSequence |> List.last |> fun x -> x.DateTime
                head, last

            match view with
            | Temp.CalendarView ->
                taskStateList
                |> List.filter (function
                    | { Task = { Task.Scheduling = Manual WithoutSuggestion }
                        StatusEntries = statusEntries
                        Sessions = sessions }
                        when
                            statusEntries
                            |> List.exists (fun (TaskStatusEntry (date, _)) -> date.DateTime >==< dateRange)
                            |> not
                        &&
                            sessions
                            |> List.exists (fun (TaskSession start) -> start.Date.DateTime >==< dateRange)
                            |> not
                        -> false
                    | _ -> true
                )
                |> List.map (fun taskState ->
//                    printfn "Task2: %A. LEN: %A" taskState.Task.Name taskState.Sessions.Length
                    Rendering.renderLane dayStart now dateSequence taskState.Task taskState.StatusEntries
                )
                |> Sorting.sortLanesByFrequency
                |> Sorting.sortLanesByIncomingRecurrency dayStart now
                |> Sorting.sortLanesByTimeOfDay dayStart now taskOrderList
            | Temp.GroupsView ->
                let lanes =
                    taskStateList
                    |> List.filter (function
                        | { Task = { Task.Scheduling = Manual WithoutSuggestion }
                            StatusEntries = []
                            Sessions = [] } -> true
                        | _ -> false
                    )
    //                    |> List.filter (fun (_, statusEntries) ->
    //                        statusEntries
    //                        |> List.filter (function
    //                            | { Cell = { Date = date } } when date.DateTime <= now.Date.DateTime -> true
    //                            | _ -> false
    //                        )
    //                        |> List.tryLast
    //                        |> function Some { Status = Dismissed } -> false | _ -> true
    //                    )
                    |> List.map (fun taskState ->
                        Rendering.renderLane dayStart now dateSequence taskState.Task taskState.StatusEntries
                    )
                    |> Sorting.applyManualOrder taskOrderList

                informationList
                |> List.map (fun information ->
                    let lanes =
                        lanes
                        |> List.filter (fun (Lane (task, _)) -> task.Information = information)

                    information, lanes
                )
                |> List.collect snd
            | Temp.TasksView ->
                taskStateList
                |> List.filter (function { Task = { Task.Scheduling = Manual _ }} -> true | _ -> false)
                |> List.map (fun taskState ->
                    Rendering.renderLane dayStart now dateSequence taskState.Task taskState.StatusEntries
                )
                |> Sorting.applyManualOrder taskOrderList


    let createState (getNow: unit -> FlukeDateTime) lastSessions dayStart informationList taskStateList taskOrderList oldState =
        let now = getNow ()

        let dateSequence =
            [ now.Date ]
            |> Rendering.getDateSequence (35, 35)

        let lanes = getLanes dayStart dateSequence now informationList taskStateList taskOrderList oldState.View

        let selection =
            match oldState.Selection with
            | [] ->
                lanes
                |> List.tryHead
                |> Option.map (fun (Lane (_, cells)) ->
                    cells
                    |> List.tryFind (fun (Cell (address, _)) -> isToday dayStart now address.Date)
                    |> Option.map (fun (Cell (address, _)) -> [ address ])
                    |> Option.defaultValue []
                )
                |> Option.defaultValue []
            | x -> x

        let activeSessions =
            lastSessions
            |> List.map (Tuple2.mapSnd (fun (TaskSession start) -> (now.DateTime - start.DateTime).TotalMinutes))
            |> List.filter (fun (_, length) -> length < TempData.sessionLength + TempData.sessionBreakLength)
            |> List.map ActiveSession

        oldState.ActiveSessions
        |> List.map (fun (ActiveSession (oldTask, oldDuration)) ->
            let newSession =
                activeSessions
                |> List.tryFind (fun (ActiveSession (task, duration)) ->
                    task = oldTask && duration = oldDuration + 1.
                )

            match newSession with
            | Some (ActiveSession (_, newDuration)) when oldDuration = -1. && newDuration = 0. -> playTick
            | Some (ActiveSession (_, newDuration)) when newDuration = TempData.sessionLength -> playDing
            | None when oldDuration = TempData.sessionLength + TempData.sessionBreakLength - 1. -> playDing
            | _ -> fun () -> ()
        )
        |> List.iter (fun x -> x ())

        { oldState with
              Now = now
              Lanes = lanes
              Selection = selection
              ActiveSessions = activeSessions }

    let ``default`` = React.memo (fun () ->

        let dayStart = Temp.dayStart
        let getNow = Temp.getNow
        let taskStateList = Temp.taskStateList
        let taskOrderList = Temp.taskOrderList
        let cellComments = Temp.cellComments
        let informationComments = Temp.informationComments
        let informationList = Temp.informationList

        let taskStateMap =
            taskStateList
            |> List.map (fun taskState -> taskState.Task, taskState)
            |> Map.ofList

        let lastSessions =
            taskStateList
            |> Seq.filter (fun taskState -> not taskState.Sessions.IsEmpty)
            |> Seq.map (fun taskState -> taskState.Task, taskState.Sessions)
            |> Seq.map (Tuple2.mapSnd (fun sessions ->
                sessions
                |> Seq.sortByDescending (fun (TaskSession start) -> start.DateTime)
                |> Seq.head
            ))
            |> Seq.toList

        let createState =
            createState getNow lastSessions dayStart informationList taskStateList taskOrderList

        let state = Hooks.useState (createState State.Default)

        CustomHooks.useInterval (fun () ->
            state.update createState
        ) (60 * 1000)

        let dateSequence =
            match state.current.Lanes with
            | Lane (_, cells) :: _ -> cells |> List.map (fun (Cell (address, _)) -> address.Date)
            | _ -> []

        let events = {|
            OnViewChange = fun view ->
                state.update (fun state ->
                    let oldState =
                        { state with
                            View = view
                            Selection = [] }
                    createState oldState
                )
        |}


        Text.div [ Props [ Style [ Height "100%" ] ]
                   Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is7) ] ][

//            if not props.UIState.SharedState.Debug then
//                PageLoader.pageLoader [ PageLoader.Color IsDark
//                                        PageLoader.IsActive (match props.PrivateState.Connection with
//                                                             | Client.Connected _ -> false
//                                                             | _ -> true) ][]

            navBar
                {| View = state.current.View
                   SetView = events.OnViewChange
                   Now = state.current.Now
                   ActiveSessions = state.current.ActiveSessions |}

            let viewFn =
                match state.current.View with
                | Temp.CalendarView -> Grid.calendarView
                | Temp.GroupsView   -> Grid.groupsView
                | Temp.TasksView    -> Grid.tasksView

            viewFn
                dayStart dateSequence state.current.Now
                state.current.Selection informationComments cellComments
                taskStateMap state.current.Lanes
        ]
    )



