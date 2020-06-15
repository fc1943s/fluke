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

//module TooltipPopupComponent =
//    open Model
//
//    let render = React.memo (fun (input: {| Comments: Comment list |}) ->
//        Html.div [
//            prop.className Css.tooltipPopup
//            prop.children [
//                input.Comments
//                |> List.map (fun (Comment (_user, comment)) -> comment.Trim ())
//                |> List.map ((+) Environment.NewLine)
//                |> String.concat (Environment.NewLine + Environment.NewLine)
//                |> fun text ->
//                    ReactBindings.React.createElement
//                        (Ext.reactMarkdown,
//                            {| source = text |}, [])
//            ]
//        ]
//    )
//
//
//module CellComponent =
//    open Model
//
//
//    let cell = React.memo (fun (input: {| CellAddress: CellAddress |}) ->
//        let isToday = Recoil.useValue (Recoil.Selectors.isTodayFamily input.CellAddress.Date)
//        let status = Recoil.useValue (Recoil.Selectors.RecoilCell.status (input.CellAddress.Task, input.CellAddress.Date))
//        let comments = Recoil.useValue (Recoil.Selectors.RecoilCell.comments (input.CellAddress.Task, input.CellAddress.Date))
//        let sessions = Recoil.useValue (Recoil.Selectors.RecoilCell.sessions (input.CellAddress.Task, input.CellAddress.Date))
//        let selected, setSelected = Recoil.useState (Recoil.Selectors.RecoilCell.selected (input.CellAddress.Task, input.CellAddress.Date))
////        let selected, setSelected = false, fun _ -> ()
//
//        let events = {|
//            OnCellClick = fun () ->
//                setSelected (not selected)
//        |}
//
//
//        Html.div [
//            prop.classes [
//                status.CellClass
//                if not comments.IsEmpty then
//                    Css.tooltipContainer
//                if selected then
//                    Css.cellSelected
//                if isToday then
//                    Css.cellToday
//            ]
//            prop.children [
//                Html.div [
//                    prop.style [
//                        match Functions.getCellSeparatorBorderLeft2 input.CellAddress.Date with
//                        | Some borderLeft -> borderLeft
//                        | None -> ()
//                    ]
//                    prop.onClick (fun (_event: MouseEvent) ->
//                        events.OnCellClick ()
//                    )
//                    prop.children [
//                        match sessions.Length with
//        //                | x -> str (string x)
//                        | x when x > 0 -> str (string x)
//                        | _ -> ()
//                    ]
//                ]
//
//                if not comments.IsEmpty then
//                    TooltipPopupComponent.render {| Comments = comments |}
//            ]
//        ]
//    )
//
//module HomePageComponent =
//    open Model
//
//    let playDing () =
//         [ 0; 1400 ]
//         |> List.map (JS.setTimeout (fun () -> Ext.playAudio "./sounds/ding.wav"))
//         |> ignore
//
//    let playTick () =
//        Ext.playAudio "./sounds/tick.wav"
//
//
//
//    module Grid =
//        let paddingLeftLevel level =
//            PaddingLeft (20 * level)
//
//        let emptyDiv =
//            div [ DangerouslySetInnerHTML { __html = "&nbsp;" } ][]
//
//        let taskName = React.memo (fun (input: {| Level: int
//                                                  Task: Task |}) ->
//            let taskState = Recoil.useValue (Recoil.Selectors.taskStateFamily input.Task)
//            let selection = Recoil.useValue Recoil.Selectors.selectionTracker
//
//            let isSelected =
//                selection
//                |> Map.tryFind input.Task
//                |> Option.defaultValue Set.empty
//                |> Set.isEmpty
//                |> not
//
//            div [ classList [ Css.tooltipContainer, not taskState.Comments.IsEmpty ]
//                  Style [ Height 17 ] ][
//
//                div [ classList [ Css.selectionHighlight, isSelected ]
//                      Style [ CSSProp.Overflow OverflowOptions.Hidden
//                              WhiteSpace WhiteSpaceOptions.Nowrap
//                              paddingLeftLevel input.Level
//                              TextOverflow "ellipsis" ] ][
//
//                    str input.Task.Name
//                ]
//
//                if not taskState.Comments.IsEmpty then
//                    TooltipPopupComponent.render {| Comments = taskState.Comments |}
//            ]
//        )
//
//        let gridCells = React.memo (fun (input: {| Tasks: Task list |}) ->
//            let dateSequence = Recoil.useValue Recoil.Selectors.dateSequence
//
//            div [ Class Css.laneContainer ][
//
//                yield! input.Tasks
//                |> List.map (fun task ->
//
//                    div [][
//                        yield! dateSequence
//                        |> List.map (fun date ->
//                            CellComponent.cell {| CellAddress = { Task = task; Date = date } |}
//                        )
//                    ]
//                )
//            ]
//        )
//
//        let gridHeader = React.memo (fun () ->
//            let dateSequence = Recoil.useValue Recoil.Selectors.dateSequence
//            let selection = Recoil.useValue Recoil.Atoms.selection
//
//            let selectionSet =
//                selection
//                |> Map.values
//                |> Set.unionMany
//
//            let datesInfo =
//                dateSequence
//                |> List.map (fun date ->
//                    let isToday = Recoil.useValue (Recoil.Selectors.isTodayFamily date)
//                    let info =
//                        {| IsSelected = selectionSet.Contains date
//                           IsToday = isToday |}
//                    date, info
//                )
//                |> Map.ofList
//
//            div [][
//                // Month row
//                div [ Style [ Display DisplayOptions.Flex ] ][
//                    yield! dateSequence
//                    |> List.groupBy (fun date -> date.Month)
//                    |> List.map (fun (_, dates) -> dates.Head, dates.Length)
//                    |> List.map (fun (firstDay, days) ->
//                        span [ Style [ TextAlign TextAlignOptions.Center
//                                       Width (17 * days) ] ][
//                            str (firstDay.DateTime.Format "MMM")
//                        ]
//                    )
//                ]
//
//                // Day of Week row
//                div [ Style [ Display DisplayOptions.Flex ] ][
//                    yield! dateSequence
//                    |> List.map (fun date ->
//                        span [ classList [ Css.todayHeader, datesInfo.[date].IsToday
//                                           Css.selectionHighlight, datesInfo.[date].IsSelected ]
//                               Style [ Width 17
//                                       Functions.getCellSeparatorBorderLeft date
//                                       TextAlign TextAlignOptions.Center ] ][
//
//                            date.DateTime.Format "dd"
//                            |> String.toLower
//                            |> str
//                        ]
//                    )
//                ]
//
//                // Day row
//                div [ Style [ Display DisplayOptions.Flex ] ][
//
//                    yield! dateSequence
//                    |> List.map (fun date ->
//                        span [ classList [ Css.todayHeader, datesInfo.[date].IsToday
//                                           Css.selectionHighlight, datesInfo.[date].IsSelected ]
//                               Style [ Width 17
//                                       Functions.getCellSeparatorBorderLeft date
//                                       TextAlign TextAlignOptions.Center ] ][
//                            str (date.Day.ToString "D2")
//                        ]
//                    )
//                ]
//            ]
//        )
//
//        let calendarView = React.memo (fun () ->
//            let dayStart = Recoil.useValue Recoil.Atoms.dayStart
//            let now = Recoil.useValue Recoil.Atoms.now
//            let taskOrderList = Recoil.useValue Recoil.Selectors.taskOrderList
//            let dateSequence = Recoil.useValue Recoil.Selectors.dateSequence
//            let taskStateList = Recoil.useValue Recoil.Selectors.taskStateList
//
//            let lanes =
//                let dateRange =
//                    let head = dateSequence |> List.head |> fun x -> x.DateTime
//                    let last = dateSequence |> List.last |> fun x -> x.DateTime
//                    head, last
//
//                taskStateList
//                |> List.filter (function
//                    | { Task = { Task.Scheduling = Manual WithoutSuggestion }
//                        StatusEntries = statusEntries
//                        Sessions = sessions }
//                        when
//                            statusEntries
//                            |> List.exists (fun (TaskStatusEntry (date, _)) -> date.DateTime >==< dateRange)
//                            |> not
//                        &&
//                            sessions
//                            |> List.exists (fun (TaskSession start) -> start.Date.DateTime >==< dateRange)
//                            |> not
//                        -> false
//                    | _ -> true
//                )
//                |> List.map (fun taskState ->
//                    Recoil.useValue (Recoil.Selectors.laneFamily taskState.Task)
//                )
//                |> Sorting.sortLanesByFrequency
//                |> Sorting.sortLanesByIncomingRecurrency dayStart now
//                |> Sorting.sortLanesByTimeOfDay dayStart now taskOrderList
//
//            let tasks =
//                lanes
//                |> List.map (fun (Lane (task, _)) -> task)
//
//            div [ Style [ Display DisplayOptions.Flex ] ][
//
//                // Column: Left
//                div [][
//
//                    // Top Padding
//                    div [][
//                        yield! emptyDiv |> List.replicate 3
//                    ]
//
//                    div [ Style [ Display DisplayOptions.Flex ] ][
//
//                        // Column: Information Type
//                        div [ Style [ PaddingRight 10 ] ] [
//
//                            yield! tasks
//                            |> List.map (fun task ->
//                                let comments = Recoil.useValue (Recoil.Selectors.RecoilInformation.comments task.Information)
////                                let comments = []
//
//                                div [ classList [ Css.blueIndicator, not comments.IsEmpty
//                                                  Css.tooltipContainer, not comments.IsEmpty ]
//                                      Style [ Padding 0
//                                              Height 17
//                                              Color task.Information.Color
//                                              WhiteSpace WhiteSpaceOptions.Nowrap ] ][
//
//                                    str task.Information.Name
//
//                                    if not comments.IsEmpty then
//                                        TooltipPopupComponent.render {| Comments = comments |}
//                                ]
//                            )
//                        ]
//
//                        // Column: Task Name
//                        div [ Style [ Width 200 ] ] [
//                            yield! tasks
//                            |> List.map (fun task ->
//                                taskName {| Level = 0; Task = task |}
//                            )
//                        ]
//                    ]
//                ]
//
//                div [][
//                    gridHeader ()
//
//                    gridCells {| Tasks = tasks |}
//                ]
//            ]
//        )
//
//        let groupsView = React.memo (fun () ->
//            let dayStart = Recoil.useValue Recoil.Atoms.dayStart
//            let now = Recoil.useValue Recoil.Atoms.now
//            let taskOrderList = Recoil.useValue Recoil.Selectors.taskOrderList
//            let dateSequence = Recoil.useValue Recoil.Selectors.dateSequence
//            let taskStateList = Recoil.useValue Recoil.Selectors.taskStateList
//            let informationList = Recoil.useValue Recoil.Selectors.informationList
//
//
//            let lanes =
//                let lanes =
//                    taskStateList
//                    |> List.filter (function
//                        | { Task = { Task.Scheduling = Manual WithoutSuggestion }
//                            StatusEntries = []
//                            Sessions = [] } -> true
//                        | _ -> false
//                    )
//    //                    |> List.filter (fun (_, statusEntries) ->
//    //                        statusEntries
//    //                        |> List.filter (function
//    //                            | { Cell = { Date = date } } when date.DateTime <= now.Date.DateTime -> true
//    //                            | _ -> false
//    //                        )
//    //                        |> List.tryLast
//    //                        |> function Some { Status = Dismissed } -> false | _ -> true
//    //                    )
//                    |> List.map (fun taskState ->
//                        Rendering.renderLane dayStart now dateSequence taskState.Task taskState.StatusEntries
//                    )
//                    |> Sorting.applyManualOrder taskOrderList
//
//                informationList
//                |> List.map (fun information ->
//                    let lanes =
//                        lanes
//                        |> List.filter (fun (Lane (task, _)) -> task.Information = information)
//
//                    information, lanes
//                )
//                |> List.collect snd
//
//            let groups =
//                taskStateList
//                |> List.map (fun x -> x.Task)
//                |> List.groupBy (fun task -> task.Information)
//                |> List.groupBy (fun (info, _) ->
//                    match info with
//                    | Project _  -> "projects"
//                    | Area _     -> "areas"
//                    | Resource _ -> "resources"
//                    | Archive _  -> "archives"
//                )
//
//            div [ Style [ Display DisplayOptions.Flex ] ][
//
//                // Column: Left
//                div [][
//
//                    // Top Padding
//                    div [][
//                        yield! emptyDiv |> List.replicate 3
//                    ]
//
//                    div [][
//
//                        yield! groups
//                        |> List.map (fun (informationType, taskGroups) ->
//                            div [][
//                                // Information Type
//                                div [ Style [ Color "#444" ] ][
//                                    str informationType
//                                ]
//
//                                div [][
//
//                                    yield! taskGroups
//                                    |> List.map (fun (information, tasks) ->
//                                        let comments = Recoil.useValue (Recoil.Selectors.RecoilInformation.comments information)
//
//                                        div [][
//                                            // Information
//                                            div [ classList [ Css.blueIndicator, comments.IsEmpty
//                                                              Css.tooltipContainer, comments.IsEmpty ]
//                                                  Style [ paddingLeftLevel 1
//                                                          Color "#444" ] ][
//                                                str information.Name
//
//                                                if not comments.IsEmpty then
//                                                    TooltipPopupComponent.render {| Comments = comments |}
//                                            ]
//
//
//                                            // Task Name
//                                            div [ Style [ Width 500 ] ][
//
//                                                yield! tasks
//                                                |> List.map (fun task ->
//                                                    taskName {| Level = 2; Task = task |}
//                                                )
//                                            ]
//                                        ]
//                                    )
//                                ]
//                            ]
//                        )
//                    ]
//                ]
//
//                // Column: Grid
//                div [][
//                    gridHeader ()
//
//                    div [][
//
//                        yield! groups
//                        |> List.map (fun (_, taskGroups) ->
//
//                            div [][
//
//                                emptyDiv
//                                div [][
//
//                                    yield! taskGroups
//                                    |> List.map (fun (_, tasks) ->
//
//                                        div [][
//                                            emptyDiv
//                                            gridCells {| Tasks = tasks |}
//                                        ]
//                                    )
//                                ]
//                            ]
//                        )
//                    ]
//                ]
//            ]
//        )
//
//
//        let tasksView = React.memo (fun () ->
//            let dayStart = Recoil.useValue Recoil.Atoms.dayStart
//            let now = Recoil.useValue Recoil.Atoms.now
//            let taskOrderList = Recoil.useValue Recoil.Selectors.taskOrderList
//            let dateSequence = Recoil.useValue Recoil.Selectors.dateSequence
//            let taskStateList = Recoil.useValue Recoil.Selectors.taskStateList
//            let informationList = Recoil.useValue Recoil.Selectors.informationList
//
//            let tasks =
//                taskStateList
//                |> List.filter (function { Task = { Task.Scheduling = Manual _ }} -> true | _ -> false)
//                |> List.map (fun taskState ->
//                    Rendering.renderLane dayStart now dateSequence taskState.Task taskState.StatusEntries
//                )
//                |> Sorting.applyManualOrder taskOrderList
//                |> List.map (fun (Lane (task, cells)) -> task)
//                |> List.sortByDescending (fun task ->
//                    let taskState = Recoil.useValue (Recoil.Selectors.taskStateFamily task)
//
//                    taskState.PriorityValue
//                    |> Option.map ofTaskPriorityValue
//                    |> Option.defaultValue 0
//                )
//
//            div [ Style [ Display DisplayOptions.Flex ] ][
//
//                // Column: Left
//                div [][
//                    // Top Padding
//                    div [][
//                        yield! emptyDiv |> List.replicate 3
//                    ]
//
//                    div [ Style [ Display DisplayOptions.Flex ] ][
//                        // Column: Information Type
//                        div [ Style [ PaddingRight 10 ] ] [
//                            yield! tasks
//                            |> List.map (fun task ->
//                                let comments = Recoil.useValue (Recoil.Selectors.RecoilInformation.comments task.Information)
//
//                                div [ classList [ Css.blueIndicator, comments.IsEmpty
//                                                  Css.tooltipContainer, comments.IsEmpty ]
//                                      Style [ Padding 0
//                                              Height 17
//                                              Color task.Information.Color
//                                              WhiteSpace WhiteSpaceOptions.Nowrap ] ][
//
//                                    str task.Information.Name
//
//                                    if not comments.IsEmpty then
//                                        TooltipPopupComponent.render {| Comments = comments |}
//                                ]
//                            )
//                        ]
//
//                        // Column: Priority
//                        div [ Style [ PaddingRight 10
//                                      TextAlign TextAlignOptions.Center ] ] [
//                            yield! tasks
//                            |> List.map (fun task ->
//                                let taskState = Recoil.useValue (Recoil.Selectors.taskStateFamily task)
//                                div [ Style [ Height 17 ] ][
//                                    taskState.PriorityValue
//                                    |> Option.map ofTaskPriorityValue
//                                    |> Option.defaultValue 0
//                                    |> string
//                                    |> str
//                                ]
//                            )
//                        ]
//
//                        // Column: Task Name
//                        div [ Style [ Width 200 ] ] [
//                            yield! tasks
//                            |> List.map (fun task ->
//                                taskName {| Level = 0; Task = task |}
//                            )
//                        ]
//                    ]
//                ]
//
//                div [][
//                    gridHeader ()
//
//                    gridCells {| Tasks = tasks |}
//                ]
//            ]
//        )
//


