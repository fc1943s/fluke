namespace Fluke.UI.Frontend

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
open Suigetsu.UI.Frontend.ElmishBridge
open Suigetsu.UI.Frontend.React
open Suigetsu.Core


module Temp =

    let testData = TempData.tempData.RenderLaneTests
//    let testData = TempData.tempData.SortLanesTests


    let taskStateList, informationComments, taskOrderList, informationList =
        match Recoil.Temp.tempDataType with
        | Recoil.Temp.TempPrivate ->
            let taskData = PrivateData.Tasks.tempManualTasks
            let sharedTaskData = SharedPrivateData.SharedTasks.tempManualTasks

            let applyState statusEntries comments (taskState: Model.TaskState) =
                { taskState with
                    StatusEntries =
                        statusEntries
                        |> Model.createTaskStatusEntries taskState.Task
                        |> List.prepend taskState.StatusEntries
                    Comments =
                        comments
                        |> List.filter (fun (Model.TaskComment (task, _)) -> task = taskState.Task)
                        |> List.map (Model.ofTaskComment >> snd)
                        |> List.prepend taskState.Comments }

            let cellComments =
                PrivateData.Journal.journalComments
                |> List.append PrivateData.CellComments.cellComments
                |> List.append SharedPrivateData.Data.cellComments

            let taskStateList =
                taskData.TaskStateList
                |> List.map (applyState
                                 PrivateData.CellStatusEntries.cellStatusEntries
                                 PrivateData.TaskComments.taskComments)

            let sharedTaskStateList =
                sharedTaskData.TaskStateList
                |> List.map (applyState
                                 SharedPrivateData.Data.cellStatusEntries
                                 SharedPrivateData.Data.taskComments)

            let informationComments =
                PrivateData.InformationComments.informationComments
                |> List.append SharedPrivateData.Data.informationComments
                |> List.groupBy (fun x -> x.Information)
                |> Map.ofList
                |> Map.mapValues (List.map (fun x -> x.Comment))

            taskStateList |> List.append sharedTaskStateList,
            informationComments,
            taskData.TaskOrderList @ PrivateData.Tasks.taskOrderList,
            taskData.InformationList
        | Recoil.Temp.TempPublic ->
            let taskData = TempData.tempData.ManualTasks

            taskData.TaskStateList,
            Map.empty,
            taskData.TaskOrderList,
            taskData.InformationList
        | Recoil.Temp.Test ->
            testData.TaskStateList,
            Map.empty,
            testData.TaskOrderList,
            [] // informationList


module CellComponent =
    open Model



    let tooltipPopup = React.memo (fun (input: {| Comments: Comment list |}) ->
        Html.div [
            prop.className Css.tooltipPopup
            prop.children [
                input.Comments
                |> List.map (fun (Comment (_user, comment)) -> comment.Trim ())
                |> List.map ((+) Environment.NewLine)
                |> String.concat (Environment.NewLine + Environment.NewLine)
                |> fun text ->
                    ReactBindings.React.createElement
                        (Ext.reactMarkdown,
                            {| source = text |}, [])
            ]
        ]
    )

    let cell = React.memo (fun (input: {| CellAddress: CellAddress
                                          OnSelect: unit -> unit
                                          Comments: Comment list
                                          Sessions: TaskSession list
                                          Status: CellStatus
                                          IsSelected: bool
                                          IsToday: bool |}) ->
        let hasComments = not input.Comments.IsEmpty

        Html.div [
            prop.classes [
                input.Status.CellClass
                if hasComments then
                    Css.tooltipContainer
                if input.IsSelected then
                    Css.cellSelected
                if input.IsToday then
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
                        input.OnSelect ()
                    )
                    prop.children [
                        match input.Sessions.Length with
        //                | x -> str (string x)
                        | x when x > 0 -> str (string x)
                        | _ -> ()
                    ]
                ]

                if hasComments then
                    tooltipPopup {| Comments = input.Comments |}
            ]
        ]
    )

module HomePageComponent =
    open Model

    let playDing () =
         [ 0; 1400 ]
         |> List.map (JS.setTimeout (fun () -> Ext.playAudio "./sounds/ding.wav"))
         |> ignore

    let playTick () =
        Ext.playAudio "./sounds/tick.wav"

    let navBar = React.memo (fun () ->
        let now = Recoil.useValue Recoil.Atoms.now
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


    module Grid =
        let paddingLeftLevel level =
            PaddingLeft (20 * level)

        let emptyDiv =
            div [ DangerouslySetInnerHTML { __html = "&nbsp;" } ][]

        let taskNameList level (taskStateMap: Map<Task,TaskState>) (selection: CellAddress list) tasks =
            tasks
            |> List.map (fun task ->
                let comments =
                    taskStateMap
                    |> Map.tryFind task
                    |> Option.map (fun taskState -> taskState.Comments)

                let isSelected =
                    selection
                    |> List.exists (fun address -> address.Task = task)

                div [ classList [ Css.tooltipContainer, match comments with Some (_ :: _) -> true | _ -> false ]
                      Style [ Height 17 ] ][

                    div [ classList [ Css.selectionHighlight, isSelected ]
                          Style [ CSSProp.Overflow OverflowOptions.Hidden
                                  WhiteSpace WhiteSpaceOptions.Nowrap
                                  paddingLeftLevel level
                                  TextOverflow "ellipsis" ] ][

                        str task.Name
                    ]

                    match comments with
                    | Some comments -> CellComponent.tooltipPopup {| Comments = comments |}
                    | None -> ()
                ]
            )

        let gridCells dayStart now selection taskStateMap lanes onCellSelect =
            div [ Class Css.laneContainer ][

                yield! Rendering.getLanesState dayStart now selection taskStateMap lanes
                |> List.map (fun (_, laneState) ->

                    div [][
                        yield! laneState
                        |> List.map (fun laneCell ->

                            CellComponent.cell
                                {| CellAddress = laneCell.CellAddress
                                   OnSelect = fun () -> onCellSelect laneCell.CellAddress
                                   Comments = laneCell.Comments
                                   Sessions = laneCell.Sessions
                                   IsSelected = laneCell.IsSelected
                                   IsToday = laneCell.IsToday
                                   Status = laneCell.Status |}
                        )
                    ]
                )
            ]

        let gridHeader dayStart dateSequence (now: FlukeDateTime) (selection: CellAddress list) =
            let selectionSet =
                selection
                |> List.map (fun address -> address.Date)
                |> Set.ofList

            let datesInfo =
                dateSequence
                |> List.map (fun date ->
                    let info =
                        {| IsSelected = selectionSet.Contains date
                           IsToday = isToday dayStart now date |}
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

        let calendarView (input: {| DayStart: FlukeTime
                                    DateSequence: FlukeDate list
                                    Now: FlukeDateTime
                                    Selection: CellAddress list
                                    InformationComments: Map<Information, Comment list>
                                    TaskStateMap: Map<Task, TaskState>
                                    Lanes: Lane list
                                    OnCellSelect: CellAddress -> unit |}) =

            let tasks = input.Lanes |> List.map (fun (Lane (task, _)) -> task)

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
                                let comments = input.InformationComments |> Map.tryFind task.Information

                                div [ classList [ Css.blueIndicator, comments.IsSome
                                                  Css.tooltipContainer, comments.IsSome ]
                                      Style [ Padding 0
                                              Height 17
                                              Color task.Information.Color
                                              WhiteSpace WhiteSpaceOptions.Nowrap ] ][

                                    str task.Information.Name

                                    match comments with
                                    | Some comments -> CellComponent.tooltipPopup {| Comments = comments |}
                                    | None -> ()
                                ]
                            )
                        ]

                        // Column: Task Name
                        div [ Style [ Width 200 ] ] [
                            yield! taskNameList 0 input.TaskStateMap input.Selection tasks
                        ]
                    ]
                ]

                div [][
                    gridHeader input.DayStart input.DateSequence input.Now input.Selection

                    gridCells input.DayStart input.Now input.Selection input.TaskStateMap input.Lanes input.OnCellSelect
                ]
            ]

        let groupsView (input: {| DayStart: FlukeTime
                                  DateSequence: FlukeDate list
                                  Now: FlukeDateTime
                                  Selection: CellAddress list
                                  InformationComments: Map<Information, Comment list>
                                  TaskStateMap: Map<Task, TaskState>
                                  Lanes: Lane list
                                  OnCellSelect: CellAddress -> unit |}) =

            let tasks = input.Lanes |> List.map (fun (Lane (task, _)) -> task)

            let groups =
                input.Lanes
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
                                        let comments = input.InformationComments |> Map.tryFind information

                                        div [][
                                            // Information
                                            div [ classList [ Css.blueIndicator, comments.IsSome
                                                              Css.tooltipContainer, comments.IsSome ]
                                                  Style [ paddingLeftLevel 1
                                                          Color "#444" ] ][
                                                str information.Name

                                                match comments with
                                                | Some comments -> CellComponent.tooltipPopup {| Comments = comments |}
                                                | None -> ()
                                            ]


                                            // Task Name
                                            div [ Style [ Width 500 ] ][

                                                yield! taskNameList 2 input.TaskStateMap input.Selection tasks
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
                    gridHeader input.DayStart input.DateSequence input.Now input.Selection

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
                                            gridCells input.DayStart input.Now input.Selection input.TaskStateMap lanes input.OnCellSelect
                                        ]
                                    )
                                ]
                            ]
                        )
                    ]
                ]
            ]

        let tasksView (input: {| DayStart: FlukeTime
                                 DateSequence: FlukeDate list
                                 Now: FlukeDateTime
                                 Selection: CellAddress list
                                 InformationComments: Map<Information, Comment list>
                                 TaskStateMap: Map<Task, TaskState>
                                 Lanes: Lane list
                                 OnCellSelect: CellAddress -> unit |}) =
            let lanes =
                input.Lanes
                |> List.sortByDescending (fun (Lane (task, _)) ->
                    input.TaskStateMap
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
                                let comments = input.InformationComments |> Map.tryFind task.Information

                                div [ classList [ Css.blueIndicator, comments.IsSome
                                                  Css.tooltipContainer, comments.IsSome ]
                                      Style [ Padding 0
                                              Height 17
                                              Color task.Information.Color
                                              WhiteSpace WhiteSpaceOptions.Nowrap ] ][

                                    str task.Information.Name

                                    match comments with
                                    | Some comments -> CellComponent.tooltipPopup {| Comments = comments |}
                                    | None -> ()
                                ]
                            )
                        ]

                        // Column: Priority
                        div [ Style [ PaddingRight 10
                                      TextAlign TextAlignOptions.Center ] ] [
                            yield! tasks
                            |> List.map (fun task ->
                                let taskState = input.TaskStateMap.[task]
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
                            yield! taskNameList 0 input.TaskStateMap input.Selection tasks
                        ]
                    ]
                ]

                div [][
                    gridHeader input.DayStart input.DateSequence input.Now input.Selection

                    gridCells input.DayStart input.Now input.Selection input.TaskStateMap lanes input.OnCellSelect
                ]
            ]

        let weekView (input: {| DayStart: FlukeTime
                                DateSequence: FlukeDate list
                                Now: FlukeDateTime
                                Selection: CellAddress list
                                InformationComments: Map<Information, Comment list>
                                TaskStateMap: Map<Task, TaskState>
                                Lanes: Lane list
                                OnCellSelect: CellAddress -> unit |}) =
            nothing

    let getLanes dayStart (dateSequence: FlukeDate list) (now: FlukeDateTime) informationList taskStateList taskOrderList view =
        match dateSequence with
        | [] -> []
        | dateSequence ->
            let dateRange =
                let head = dateSequence |> List.head |> fun x -> x.DateTime
                let last = dateSequence |> List.last |> fun x -> x.DateTime
                head, last

            match view with
            | View.Calendar ->
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
            | View.Groups ->
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
            | View.Tasks ->
                taskStateList
                |> List.filter (function { Task = { Task.Scheduling = Manual _ }} -> true | _ -> false)
                |> List.map (fun taskState ->
                    Rendering.renderLane dayStart now dateSequence taskState.Task taskState.StatusEntries
                )
                |> Sorting.applyManualOrder taskOrderList
            | View.Week ->
                []


    let ``default`` = React.memo (fun () ->

        let now = Recoil.useValue Recoil.Atoms.now
        let view = Recoil.useValue Recoil.Atoms.view
        let activeSessions, setActiveSessions = Recoil.useState Recoil.Atoms.activeSessions
        let selection, setSelection = Recoil.useState Recoil.Atoms.selection
        let dayStart = Recoil.useValue Recoil.Atoms.dayStart
        let dateSequence = Recoil.useValue Recoil.Atoms.dateSequence
        let dings, setDings = Recoil.useState (Recoil.Atoms.dingsFamily now)
        let ctrlPressed = Recoil.useValue Recoil.Atoms.ctrlPressed

        printfn "RENDER DEFAULT. NOW: %A" now

        let taskStateList = Temp.taskStateList
        let taskOrderList = Temp.taskOrderList
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

        let lanes = getLanes dayStart dateSequence now informationList taskStateList taskOrderList view

        let newActiveSessions =
            lastSessions
            |> List.map (Tuple2.mapSnd (fun (TaskSession start) -> (now.DateTime - start.DateTime).TotalMinutes))
            |> List.filter (fun (_, length) -> length < TempData.sessionLength + TempData.sessionBreakLength)
            |> List.map ActiveSession

        if activeSessions <> newActiveSessions then
            setActiveSessions newActiveSessions

        newActiveSessions
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

        let onCellSelect (cell: CellAddress) =
            let taskSelection =
                selection
                |> Map.tryFind cell.Task
                |> Option.defaultValue (cell.Date |> Set.singleton)


            selection
            |> Map.add cell.Task taskSelection
            |> setSelection
//            setState
//                { state with
//                    Selection =
//                        if not state.CtrlPressed then
//                            [ cell ]
//                        else
//                            let rec loop newSelection = function
//                                | head :: tail when head = cell -> true, newSelection @ tail
//                                | head :: tail -> loop (head :: newSelection) tail
//                                | [] -> false, newSelection
//                            let removed, newSelection = loop [] state.Selection
//
//                            match removed with
//                            | true -> newSelection
//                            | false -> newSelection |> List.append [ cell ]
//                }

        Text.div [ Props [ Style [ Height "100%" ] ]
                   Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is7) ] ][

            navBar ()

            let props =
                {| DayStart = dayStart
                   DateSequence = dateSequence
                   Now = now
                   Selection =
                       selection
                       |> Seq.collect (fun (KeyValue (task, dates)) ->
                           dates
                           |> Seq.map (fun date -> { Task = task; Date = date })
                       )
                       |> Seq.toList
                   InformationComments = informationComments
                   TaskStateMap = taskStateMap
                   Lanes = lanes
                   OnCellSelect = onCellSelect |}

            let viewFn =
                match view with
                | View.Calendar -> Grid.calendarView
                | View.Groups   -> Grid.groupsView
                | View.Tasks    -> Grid.tasksView
                | View.Week     -> Grid.weekView

            viewFn props
        ]
    )



