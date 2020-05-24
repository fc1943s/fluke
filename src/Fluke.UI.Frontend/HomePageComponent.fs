namespace Fluke.UI.Frontend

open Browser.Types
open FSharpPlus
open Fable.Core
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
    open Model
    
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

//    let testData = TempData.tempData.RenderLaneTests
    let testData = TempData.tempData.SortLanesTests
            

    let cellComments = PrivateData.Journal.journalComments @ PrivateData.CellComments.cellComments
    let taskStateList, getNow, informationComments, taskOrderList, dayStart, informationList =
        match tempDataType with
        | TempPrivate ->
            let taskData = PrivateData.Tasks.tempManualTasks
            
            let taskStateList =
                taskData.TaskStateList
                |> List.map (fun taskState ->
                    { taskState with
                        StatusEntries =
                            PrivateData.CellStatusEntries.cellStatusEntries
                            |> createTaskStatusEntries taskState.Task
                        Comments =
                            PrivateData.TaskComments.taskComments
                            |> List.filter (fun (TaskComment (task, _)) -> task = taskState.Task)
                            |> List.map (ofTaskComment >> snd)
                            |> List.prepend taskState.Comments })
            
            taskStateList,
            TempData.getNow,
            PrivateData.InformationComments.informationComments |> List.groupBy (fun x -> x.Information) |> Map.ofList,
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
            let date = flukeDate 0000 Month.January 01
            { Now = { Date = date; Time = Temp.dayStart }
              Selection = []
              Lanes = []
              ActiveSessions = []
              View = Temp.view }
            
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
            
        let taskNameList level lanes =
            lanes
            |> List.map (fun (Lane (task, _)) ->
                let comments = Temp.taskStateMap.TryFind task |> Option.map (fun taskState -> taskState.Comments)
                
                div [ classList [ Css.tooltipContainer, match comments with Some (_ :: _) -> true | _ -> false ] ][
                    
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
            
        let gridCells now selection lanes =
            lanes
            |> List.map (fun (Lane (task, cells)) ->
                let taskState = Temp.taskStateMap.TryFind task
                
                cells
                |> List.map (fun (Cell (address, status)) ->
                    
                    let comments =
                        Temp.cellComments
                        |> List.map ofCellComment
                        |> List.filter (fun (commentAddress, _) ->
                            commentAddress.Task = task && commentAddress.Date = address.Date
                        )
                        |> List.map snd
                        
                    let sessions =
                        taskState
                        |> Option.map (fun x -> x.Sessions)
                        |> Option.defaultValue []
                        |> List.filter (fun (TaskSession start) -> start.Date = address.Date)
                        
                    CellComponent.``default``
                        { CellAddress = address
                          Comments = comments
                          Sessions = sessions
                          IsSelected = selection |> List.contains address
                          IsToday = isToday Temp.dayStart now address.Date
                          Status = status }
                )
                |> div []
            ) |> div [ Class Css.laneContainer ]
            
        let gridHeader dateSequence (now: FlukeDateTime) =
            div [][
                // Month row
                dateSequence
                |> List.groupBy (fun date -> date.Month)
                |> List.map (fun (_, dates) -> dates.Head, dates.Length)
                |> List.map (fun (firstDay, days) ->
                    span [ Style [ Functions.getCellSeparatorBorderLeft firstDay
                                   TextAlign TextAlignOptions.Center
                                   Width (18 * days) ] ][
                        str (firstDay.DateTime.Format "MMM")
                    ]
                )
                |> div [ Style [ Display DisplayOptions.Flex ] ]
                
                // Day of Week row
                dateSequence
                |> List.map (fun date ->
                    span [ Style [ Width 18
                                   Functions.getCellSeparatorBorderLeft date
                                   TextAlign TextAlignOptions.Center ] ][
                            
                        date.DateTime.Format "dd"
                        |> String.toLower
                        |> str
                    ]
                )
                |> div [ Style [ Display DisplayOptions.Flex ] ]
                
                // Day row
                dateSequence
                |> List.map (fun date ->
                    span [ Style [ Width 18
                                   Functions.getCellSeparatorBorderLeft date
                                   TextAlign TextAlignOptions.Center
                                   Color (if isToday Temp.dayStart now date then "#f22" else "") ] ][
                        str (date.Day.ToString "D2")
                    ]
                )
                |> div [ Style [ Display DisplayOptions.Flex ] ]
            ]
            
        let calendarView dateSequence now selection lanes =
            div [ Style [ Display DisplayOptions.Flex ] ][
                
                // Column: Left
                div [][
                    // Top Padding
                    emptyDiv
                    |> List.replicate 3
                    |> div []
                        
                    div [ Style [ Display DisplayOptions.Flex ] ][
                        // Column: Information Type
                        lanes
                        |> List.map (fun (Lane (task, _)) ->
                            let comments = Temp.informationComments.TryFind task.Information
                            
                            div [ classList [ Css.blueIndicator, comments.IsSome
                                              Css.tooltipContainer, comments.IsSome ]
                                  Style [ Padding 0
                                          Color task.Information.Color
                                          WhiteSpace WhiteSpaceOptions.Nowrap ] ][
                                
                                str task.Information.Name
                                
                                match comments with
                                | None -> ()
                                | Some comments ->
                                    comments
                                    |> List.map (fun x -> Comment x.Comment)
                                    |> CellComponent.tooltipPopup
                            ]
                        )
                        |> div [ Style [ PaddingRight 10 ] ]
                        
                        // Column: Task Name
                        taskNameList 0 lanes
                        |> div [ Style [ Width 200 ] ]
                    ]
                ]
                    
                div [][
                    gridHeader dateSequence now
                    
                    gridCells now selection lanes
                ]
            ]
            
        let groupsView dateSequence now selection lanes =
            let groups =
                lanes
                |> List.groupBy (fun (Lane (task, _)) ->
                    task.Information
                )
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
                    emptyDiv
                    |> List.replicate 3
                    |> div []
                        
                    groups
                    |> List.map (fun (informationType, lanesGroups) ->
                        div [][
                            // Information Type
                            div [ Style [ Color "#444" ] ][
                                str informationType
                            ]
                            
                            lanesGroups
                            |> List.map (fun (information, lanes) ->
                                let comments = Temp.informationComments.TryFind information
                                
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
                                            |> List.map (fun x -> Comment x.Comment)
                                            |> CellComponent.tooltipPopup
                                    ]
                                    
                                    
                                    // Task Name
                                    taskNameList 2 lanes
                                    |> div [ Style [ Width 500 ] ]
                                ]
                            )
                            |> div []
                        ]
                    )
                    |> div []
                ]
                    
                // Column: Grid
                div [][
                    gridHeader dateSequence now
                    
                    groups
                    |> List.map (fun (_, groupLanes) ->
                        
                        div [][
                            emptyDiv
                            
                            
                            groupLanes
                            |> List.map (fun (_, lanes) ->
                                
                                div [][
                                    
                                    emptyDiv
                                    gridCells now selection lanes
                                ]
                            )
                            |> div []
                        ]
                    )
                    |> div []
                ]
            ]
            
        let tasksView dateSequence now selection lanes =
            let lanes = // TODO: Duplicated
                lanes
                |> List.sortByDescending (fun (Lane (task, _)) ->
                    Temp.taskStateMap.[task].PriorityValue
                    |> Option.map ofTaskPriorityValue
                    |> Option.defaultValue 0
                )
            
            div [ Style [ Display DisplayOptions.Flex ] ][
                
                // Column: Left
                div [][
                    // Top Padding
                    emptyDiv
                    |> List.replicate 3
                    |> div []
                        
                    div [ Style [ Display DisplayOptions.Flex ] ][
                        // Column: Information Type
                        lanes
                        |> List.map (fun (Lane (task, _)) ->
                            let comments = Temp.informationComments.TryFind task.Information
                            
                            div [ classList [ Css.blueIndicator, comments.IsSome
                                              Css.tooltipContainer, comments.IsSome ]
                                  Style [ Padding 0
                                          Color task.Information.Color
                                          WhiteSpace WhiteSpaceOptions.Nowrap ] ][
                                
                                str task.Information.Name
                                
                                match comments with
                                | None -> ()
                                | Some comments ->
                                    comments
                                    |> List.map (fun x -> Comment x.Comment)
                                    |> CellComponent.tooltipPopup
                            ]
                        )
                        |> div [ Style [ PaddingRight 10 ] ]
                        
                        // Column: Priority
                        lanes
                        |> List.map (fun (Lane (task, _)) ->
                            let taskState = Temp.taskStateMap.[task]
                            div [][
                                taskState.PriorityValue
                                |> Option.map ofTaskPriorityValue
                                |> Option.defaultValue 0
                                |> string
                                |> str
                            ]
                        )
                        |> div [ Style [ PaddingRight 10
                                         TextAlign TextAlignOptions.Center ] ]
                
                        // Column: Task Name
                        taskNameList 0 lanes
                        |> div [ Style [ Width 200 ] ]
                    ]
                ]
                    
                div [][
                    gridHeader dateSequence now
                    
                    gridCells now selection lanes
                ]
            ]
            
    let ``default`` = FunctionComponent.Of (fun (__props: Props) ->
            
        let getLanes dateSequence now view =
            match view with
            | Temp.CalendarView ->
                Temp.taskStateList
                |> List.filter (function
                    | { Task = { Task.Scheduling = Manual WithoutSuggestion }; StatusEntries = []} -> false
                    | _ -> true
                )
                |> List.map (fun taskState ->
                    Rendering.renderLane Temp.dayStart now dateSequence taskState.Task taskState.StatusEntries
                )
                |> Sorting.sortLanesByFrequency
                |> Sorting.sortLanesByIncomingRecurrency Temp.dayStart now
                |> Sorting.sortLanesByTimeOfDay Temp.dayStart now Temp.taskOrderList
            | Temp.GroupsView ->
                let lanes =
                    Temp.taskStateList
                    |> List.filter (function
                        | { Task = { Task.Scheduling = Manual WithoutSuggestion }; StatusEntries = [] } -> true
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
                        Rendering.renderLane Temp.dayStart now dateSequence taskState.Task taskState.StatusEntries
                    )
                    |> Sorting.applyManualOrder Temp.taskOrderList
                    
                Temp.informationList
                |> List.map (fun information ->
                    let lanes =
                        lanes
                        |> List.filter (fun (Lane (task, _)) -> task.Information = information)
                        
                    information, lanes
                )
                |> List.collect snd
            | Temp.TasksView ->
                Temp.taskStateList
                |> List.filter (function { Task = { Task.Scheduling = Manual _ }} -> true | _ -> false)
                |> List.map (fun taskState ->
                    Rendering.renderLane Temp.dayStart now dateSequence taskState.Task taskState.StatusEntries
                )
                |> Sorting.applyManualOrder Temp.taskOrderList
            
                    
        let createState oldState =
            let now = Temp.getNow ()
            
            let dateSequence = 
                [ now.Date ]
                |> Rendering.getDateSequence (35, 35)
                
            let lanes = getLanes dateSequence now oldState.View
            
            let selection =
                match oldState.Selection with
                | [] ->
                    lanes
                    |> List.tryHead
                    |> Option.map (fun (Lane (_, cells)) ->
                        cells
                        |> List.tryFind (fun (Cell (address, _)) -> isToday Temp.dayStart now address.Date)
                        |> Option.map (fun (Cell (address, _)) -> [ address ])
                        |> Option.defaultValue []
                    )
                    |> Option.defaultValue []
                | x -> x
            
            let activeSessions =
                Temp.lastSessions
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
        
        let state =
            Hooks.useState (createState State.Default)
            
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
                    createState { state with
                                    View = view
                                    Selection = [] }
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
                   
                
            state.current.Lanes
            |> match state.current.View with
               | Temp.CalendarView -> Grid.calendarView dateSequence state.current.Now state.current.Selection
               | Temp.GroupsView   -> Grid.groupsView dateSequence state.current.Now state.current.Selection
               | Temp.TasksView    -> Grid.tasksView dateSequence state.current.Now state.current.Selection
        ]
    , memoizeWith = equalsButFunctions)
    
