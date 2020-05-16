namespace Fluke.UI.Frontend

open Browser.Types
open FSharpPlus
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
//    let tempDataType = TempPublic
//    let tempDataType = Test


    let cellComments = PrivateData.Journal.journalComments @ PrivateData.CellComments.cellComments
    let taskStateList, getNow, cellStatusEntries, informationComments, taskOrderList, dayStart, informationList =
        match tempDataType with
        | TempPrivate ->
            let taskData = PrivateData.Tasks.tempManualTasks
            
            let taskStateList =
                taskData.TaskStateList
                |> List.map (fun taskState ->
                    { taskState with
                        Comments =
                            PrivateData.TaskComments.taskComments
                            |> List.filter (fun (TaskComment (task, _)) -> task = taskState.Task)
                            |> List.map (ofTaskComment >> snd)
                            |> List.prepend taskState.Comments })
            
            taskStateList,
            TempData.getNow,
            PrivateData.CellStatusEntries.cellStatusEntries,
            PrivateData.InformationComments.informationComments |> List.groupBy (fun x -> x.Information) |> Map.ofList,
            taskData.TaskOrderList @ PrivateData.Tasks.taskOrderList,
            PrivateData.PrivateData.dayStart,
            taskData.InformationList
        | TempPublic ->
            let taskData = TempData.tempData.ManualTasks
            
            taskData.TaskStateList,
            TempData.getNow,
            [],
            Map.empty,
            taskData.TaskOrderList,
            TempData.dayStart,
            taskData.InformationList
        | Test ->
//            let testData = TempData.tempData.RenderLaneTests
            let testData = TempData.tempData.SortLanesTests
            
            let taskStateList =
                testData.TaskList
                |> List.map (fun task ->
                    { Task = task
                      Comments = []
                      Sessions = []
                      PriorityValue = None })
            
            taskStateList,
            testData.GetNow,
            testData.CellEvents,
            Map.empty,
            testData.TaskOrderList,
            TempData.dayStart,
            [] // informationList
            
    let taskStateMap =
        taskStateList
        |> List.map (fun taskState -> taskState.Task, taskState)
        |> Map.ofList
        
    
    
module HomePageComponent =
    open Model
    
    
    type Props =
        { Dispatch: SharedState.SharedServerMessage -> unit
          UIState: UIState.State
          PrivateState: Client.PrivateState<UIState.State> }
        
    type State =
        { Now: FlukeDateTime
          Selection: CellAddress list
          Lanes: Lane list
          View: Temp.View }
        static member inline Default =
            let date = flukeDate 0000 Month.January 01
            { Now = { Date = date; Time = Temp.dayStart }
              Selection = []
              Lanes = []
              View = Temp.view }
        
    let navBar (props: {| View: Temp.View
                          SetView: Temp.View -> unit |}) =
        
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
                                               Padding "8px 0 0 10px"
                                               Display DisplayOptions.Flex
                                               JustifyContent "space-around" ]]][
            
            let checkbox view text =
                Navbar.Item.div [ Navbar.Item.Props [ Class "field"
                                                      OnClick (fun _ -> events.OnViewChange view) ] ][

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
                
                div [ classList [ Css.tooltipContainer, comments |> Option.defaultValue [] |> fun x -> x.Length > 0 ] ][
                    
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
                        |> List.filter (fun (commentAddress, _) -> commentAddress.Task = task && commentAddress.Date = address.Date)
                        |> List.map snd
                        
                    let sessions =
                        taskState
                        |> Option.map (fun x -> x.Sessions)
                        |> Option.defaultValue []
                        |> List.filter (fun (TaskSession start) -> start.Date = address.Date)
                        
                    CellComponent.``default``
                        { Date = address.Date
                          Task = task
                          Comments = comments
                          Sessions = sessions
                          Selected = selection |> List.contains address
                          Status = status
                          Now = now }
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
                                   Color (if date = now.Date then "#f22" else "") ] ][
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
            let taskStateList =
                Temp.taskStateList
                |> List.map (fun taskState ->
                    let events =
                        Temp.cellStatusEntries
                        |> List.filter (fun (CellStatusEntry (address, _)) -> address.Task = taskState.Task)
                        |> List.sortBy (fun (CellStatusEntry (address, _)) -> address.Date)
                        
                    taskState, events
                )
                
            match view with
            | Temp.CalendarView ->
                taskStateList
                |> List.filter (function { Task = { Task.Scheduling = Manual false }}, [] -> false | _ -> true)
                |> List.map (fun (taskState, statusEntries) -> Rendering.renderLane Temp.dayStart now dateSequence taskState.Task statusEntries)
                |> Sorting.sortLanesByFrequency
                |> Sorting.sortLanesByIncomingRecurrency now.Date
                |> Sorting.sortLanesByTimeOfDay now Temp.taskOrderList
            | Temp.GroupsView ->
                let lanes =
                    taskStateList
                    |> List.filter (function { Task = { Task.Scheduling = Manual false }}, [] -> true | _ -> false)
//                    |> List.filter (fun (_, statusEntries) ->
//                        statusEntries
//                        |> List.filter (function { Cell = { Date = date } } when date.DateTime <= now.Date.DateTime -> true | _ -> false)
//                        |> List.tryLast
//                        |> function Some { Status = Dismissed } -> false | _ -> true
//                    )
                    |> List.map (fun (taskState, statusEntries) -> Rendering.renderLane Temp.dayStart now dateSequence taskState.Task statusEntries)
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
                taskStateList
                |> List.filter (function { Task = { Task.Scheduling = Manual _ }}, _ -> true | _ -> false)
                |> List.map (fun (taskState, statusEntries) -> Rendering.renderLane Temp.dayStart now dateSequence taskState.Task statusEntries)
                |> Sorting.applyManualOrder Temp.taskOrderList
            
                    
        let getState oldState =
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
                        |> List.tryFind (fun (Cell (address, _)) -> address.Date = now.Date)
                        |> Option.map (fun (Cell (address, _)) -> [ address ])
                        |> Option.defaultValue []
                    )
                    |> Option.defaultValue []
                | x -> x
            
            { oldState with
                  Now = now
                  Lanes = lanes
                  Selection = selection }
        
        let state =
            Hooks.useState (getState State.Default)
            
        CustomHooks.useInterval (fun () ->
            state.update getState
        ) (60 * 1000)
        
        let dateSequence =
            match state.current.Lanes with
            | Lane (_, cells) :: _ -> cells |> List.map (fun (Cell (address, _)) -> address.Date)
            | _ -> []
            
        let events = {|
            OnViewChange = fun view ->
                state.update (fun state ->
                    getState { state with
                                 View = view
                                 Selection = [] }
                )
        |}
        
        
        Text.div [ Props [ Style [ Height "100%" ] ]
                   Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is7) ] ][

//            if not props.UIState.SharedState.Debug then
//                PageLoader.pageLoader [ PageLoader.Color IsDark
//                                        PageLoader.IsActive (match props.PrivateState.Connection with Client.Connected _ -> false | _ -> true) ][]

            navBar
                {| View = state.current.View
                   SetView = events.OnViewChange |}
                   
                
            match state.current.View with
            | Temp.CalendarView -> Grid.calendarView dateSequence state.current.Now state.current.Selection state.current.Lanes
            | Temp.GroupsView   -> Grid.groupsView dateSequence state.current.Now state.current.Selection state.current.Lanes
            | Temp.TasksView    -> Grid.tasksView dateSequence state.current.Now state.current.Selection state.current.Lanes
        ]
    , memoizeWith = equalsButFunctions)
    
