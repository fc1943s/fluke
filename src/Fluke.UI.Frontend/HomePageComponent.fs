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
                        (Ext.reactMarkdown,
                            {| source = text |}, [])
            ]
        ]
    )


module CellComponent =
    open Model


    let cell = React.memo (fun (input: {| CellAddress: CellAddress |}) ->
        let isToday = Recoil.useValue (Recoil.Atoms.isTodayFamily input.CellAddress.Date)
//        let selected, setSelected = Recoil.useState (Recoil.Atoms.cellSelectedFamily input.CellAddress)
//        let comments = Recoil.useValue (Recoil.Atoms.cellCommentsFamily input.CellAddress)
        let selected, setSelected = false, fun _ -> ()
        let comments = []
//        let sessions = Recoil.useValue (Recoil.Atoms.cellSessionsFamily input.CellAddress)
        let status = Recoil.useValue (Recoil.Atoms.cellStatusFamily input.CellAddress)

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
//                    prop.children [
//                        match sessions.Length with
//        //                | x -> str (string x)
//                        | x when x > 0 -> str (string x)
//                        | _ -> ()
//                    ]
                ]

                if not comments.IsEmpty then
                    TooltipPopupComponent.render {| Comments = comments |}
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



    module Grid =
        let paddingLeftLevel level =
            PaddingLeft (20 * level)

        let emptyDiv =
            div [ DangerouslySetInnerHTML { __html = "&nbsp;" } ][]

        let taskNameList level tasks =
            let selection = Recoil.useValue Recoil.Atoms.selection

            tasks
            |> List.map (fun task ->
                let taskState = Recoil.useValue (Recoil.Atoms.taskStateFamily task)

                let isSelected =
                    selection
                    |> Map.tryFind task
                    |> Option.defaultValue Set.empty
                    |> Set.isEmpty
                    |> not

                div [ classList [ Css.tooltipContainer, not taskState.Comments.IsEmpty ]
                      Style [ Height 17 ] ][

                    div [ classList [ Css.selectionHighlight, isSelected ]
                          Style [ CSSProp.Overflow OverflowOptions.Hidden
                                  WhiteSpace WhiteSpaceOptions.Nowrap
                                  paddingLeftLevel level
                                  TextOverflow "ellipsis" ] ][

                        str task.Name
                    ]

                    if not taskState.Comments.IsEmpty then
                        TooltipPopupComponent.render {| Comments = taskState.Comments |}
                ]
            )

        let gridCells = React.memo (fun (input: {| Tasks: Task list |}) ->
            let dateSequence = Recoil.useValue Recoil.Atoms.dateSequence

            div [ Class Css.laneContainer ][

                yield! input.Tasks
                |> List.map (fun task ->

                    div [][
                        yield! dateSequence
                        |> List.map (fun date ->
                            CellComponent.cell {| CellAddress = { Task = task; Date = date } |}
                        )
                    ]
                )
            ]
        )

        let gridHeader = React.memo (fun () ->
            let dateSequence = Recoil.useValue Recoil.Atoms.dateSequence
            let selection = Recoil.useValue Recoil.Atoms.selection

            let selectionSet =
//                selection
//                |> Map.values
//                |> Set.unionMany
                Set.empty

            let datesInfo =
                dateSequence
                |> List.map (fun date ->
                    let isToday = Recoil.useValue (Recoil.Atoms.isTodayFamily date)
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

        let calendarView = React.memo (fun () ->
            let dayStart = Recoil.useValue Recoil.Atoms.dayStart
            let now = Recoil.useValue Recoil.Atoms.now
            let taskOrderList = Recoil.useValue Recoil.Atoms.taskOrderList
            let dateSequence = Recoil.useValue Recoil.Atoms.dateSequence
            let taskStateList = Recoil.useValue Recoil.Atoms.taskStateList

            let lanes =
                let dateRange =
                    let head = dateSequence |> List.head |> fun x -> x.DateTime
                    let last = dateSequence |> List.last |> fun x -> x.DateTime
                    head, last
//
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
                    Recoil.useValue (Recoil.Atoms.laneFamily taskState.Task)
                )
                |> Sorting.sortLanesByFrequency
                |> Sorting.sortLanesByIncomingRecurrency dayStart now
                |> Sorting.sortLanesByTimeOfDay dayStart now taskOrderList

            let tasks =
                lanes
                |> List.map (fun (Lane (task, _)) -> task)

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
//                                let comments = Recoil.useValue (Recoil.Atoms.informationCommentsFamily task.Information)
                                let comments = []

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
                            yield! taskNameList 0 tasks
                        ]
                    ]
                ]

                div [][
                    gridHeader ()

                    gridCells {| Tasks = tasks |}
                ]
            ]
        )

        let groupsView = React.memo (fun () ->
            let dayStart = Recoil.useValue Recoil.Atoms.dayStart
            let now = Recoil.useValue Recoil.Atoms.now
            let taskOrderList = Recoil.useValue Recoil.Atoms.taskOrderList
            let dateSequence = Recoil.useValue Recoil.Atoms.dateSequence
            let taskStateList = Recoil.useValue Recoil.Atoms.taskStateList
            let informationList = Recoil.useValue Recoil.Atoms.informationList


            let lanes =
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

            let groups =
                taskStateList
                |> List.map (fun x -> x.Task)
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
                        yield! emptyDiv |> List.replicate 3
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
                                        let comments = Recoil.useValue (Recoil.Atoms.informationCommentsFamily information)

                                        div [][
                                            // Information
                                            div [ classList [ Css.blueIndicator, comments.IsEmpty
                                                              Css.tooltipContainer, comments.IsEmpty ]
                                                  Style [ paddingLeftLevel 1
                                                          Color "#444" ] ][
                                                str information.Name

                                                if not comments.IsEmpty then
                                                    TooltipPopupComponent.render {| Comments = comments |}
                                            ]


                                            // Task Name
                                            div [ Style [ Width 500 ] ][

                                                yield! taskNameList 2 tasks
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
                    gridHeader ()

                    div [][

                        yield! groups
                        |> List.map (fun (_, taskGroups) ->

                            div [][

                                emptyDiv
                                div [][

                                    yield! taskGroups
                                    |> List.map (fun (_, tasks) ->

                                        div [][
                                            emptyDiv
                                            gridCells {| Tasks = tasks |}
                                        ]
                                    )
                                ]
                            ]
                        )
                    ]
                ]
            ]
        )


        let tasksView = React.memo (fun () ->
            let dayStart = Recoil.useValue Recoil.Atoms.dayStart
            let now = Recoil.useValue Recoil.Atoms.now
            let taskOrderList = Recoil.useValue Recoil.Atoms.taskOrderList
            let dateSequence = Recoil.useValue Recoil.Atoms.dateSequence
            let taskStateList = Recoil.useValue Recoil.Atoms.taskStateList
            let informationList = Recoil.useValue Recoil.Atoms.informationList

            let tasks =
                taskStateList
                |> List.filter (function { Task = { Task.Scheduling = Manual _ }} -> true | _ -> false)
                |> List.map (fun taskState ->
                    Rendering.renderLane dayStart now dateSequence taskState.Task taskState.StatusEntries
                )
                |> Sorting.applyManualOrder taskOrderList
                |> List.map (fun (Lane (task, cells)) -> task)
                |> List.sortByDescending (fun task ->
                    let taskState = Recoil.useValue (Recoil.Atoms.taskStateFamily task)

                    taskState.PriorityValue
                    |> Option.map ofTaskPriorityValue
                    |> Option.defaultValue 0
                )

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
                                let comments = Recoil.useValue (Recoil.Atoms.informationCommentsFamily task.Information)

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
                                let taskState = Recoil.useValue (Recoil.Atoms.taskStateFamily task)
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
                            yield! taskNameList 0 tasks
                        ]
                    ]
                ]

                div [][
                    gridHeader ()

                    gridCells {| Tasks = tasks |}
                ]
            ]
        )

        let weekView = React.memo (fun () ->
            nothing
        )


    let render = React.memo (fun () ->

        let view = Recoil.useValue Recoil.Atoms.view
//        let now = Recoil.useValue Recoil.Atoms.now
//        let activeSessions, setActiveSessions = Recoil.useState Recoil.Atoms.activeSessions
//        let selection, setSelection = Recoil.useState Recoil.Atoms.selection
//        let dayStart = Recoil.useValue Recoil.Atoms.dayStart
//        let dateSequence = Recoil.useValue Recoil.Atoms.dateSequence
//        let dings, setDings = Recoil.useState (Recoil.Atoms.dingsFamily now)
//        let ctrlPressed = Recoil.useValue Recoil.Atoms.ctrlPressed
//        let taskStateList = Recoil.useValue Recoil.Atoms.taskStateList
//        let informationList = Recoil.useValue Recoil.Atoms.informationList
//        let taskOrderList = Recoil.useValue Recoil.Atoms.taskOrderList

        printfn "HomePageComponent.render"

//        taskStateList
//        |> List.iter (fun taskState ->
//            let setTaskCells = Recoil.useSetState (Recoil.Atoms.taskCellsFamily taskState.Task)
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

        Html.div [
            match view with
            | View.Calendar -> Grid.calendarView ()
            | View.Groups   -> Grid.groupsView ()
            | View.Tasks    -> Grid.tasksView ()
            | View.Week     -> Grid.weekView ()
        ]
    )



