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
    
    type View =
        | CalendarView
        | GroupsView
        | TasksView
    
    type TempDataType =
        | TempPrivate
        | TempPublic
        | Test
        
    let tempDataType = TempPrivate
//    let tempDataType = TempPublic
//    let tempDataType = Test

//    let view = CalendarView
//    let view = GroupsView
    let view = TasksView



    
    let getNow, cellEvents, taskList, taskComments, informationComments, taskOrderList, hourOffset, projectList, areaList, resourceList =
        match tempDataType with
        | TempPrivate ->
            let taskData = PrivateData.Tasks.tempManualTasks |> Result.okOrThrow
            
            TempData.getNow,
            PrivateData.CellEvents.cellEvents,
            taskData.TaskList,
            PrivateData.TaskComments.taskComments |> List.groupBy (fun x -> x.Task) |> Map.ofList,
            PrivateData.InformationComments.informationComments |> List.groupBy (fun x -> x.Information) |> Map.ofList,
            taskData.TaskOrderList @ (PrivateData.Tasks.taskOrderList |> Result.okOrThrow),
            PrivateData.PrivateData.hourOffset,
            taskData.ProjectList,
            taskData.AreaList,
            taskData.ResourceList
        | TempPublic ->
            let taskData = TempData.tempData.ManualTasks |> Result.okOrThrow
            
            TempData.getNow,
            [],
            taskData.TaskList,
            [] |> Map.ofList,
            [] |> Map.ofList,
            taskData.TaskOrderList,
            TempData.hourOffset,
            taskData.ProjectList,
            taskData.AreaList,
            taskData.ResourceList
        | Test ->
//            let testData = TempData.tempData.RenderLaneTests
            let testData = TempData.tempData.SortLanesTests
            
            testData.GetNow,
            testData.CellEvents,
            testData.TaskList,
            [] |> Map.ofList,
            [] |> Map.ofList,
            testData.TaskOrderList,
            TempData.hourOffset,
            TempData.projectList,
            TempData.areaList,
            TempData.resourceList
        
    let informationList = (projectList, areaList, resourceList) |> TempData.getInformationList
    let cellComments = PrivateData.Journal.journalComments @ PrivateData.CellComments.cellComments
    
    
module HomePageComponent =
    open Model
    
    
    type Props =
        { Dispatch: SharedState.SharedServerMessage -> unit
          UIState: UIState.State
          PrivateState: Client.PrivateState<UIState.State> }
        
    type State =
        { Now: FlukeDateTime
          Selection: Cell list
          Lanes: Lane list
          View: Temp.View }
        static member inline Default =
            let date = flukeDate 0 Month.January 1
            { Now = { Date = date; Time = midnight }
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
            | _, _,    _   -> ()
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
                let comments = Temp.taskComments.TryFind task
                
                div [ classList [ Css.tooltipContainer, comments.IsSome ] ][
                    
                    div [ Style [ CSSProp.Overflow OverflowOptions.Hidden
                                  WhiteSpace WhiteSpaceOptions.Nowrap
                                  paddingLeftLevel level
                                  TextOverflow "ellipsis" ] ][
                        
                        str task.Name
                    ]
                    
                    match comments with
                    | None -> ()
                    | Some comments ->
                        comments
                        |> List.map (fun x -> x.Comment)
                        |> CellComponent.tooltipPopup
                ]
            )
            
        let gridCells today selection lanes =
            lanes
            |> List.map (fun (Lane (task, cells)) ->
                cells
                |> List.map (fun cell ->
                    let comments =
                        Temp.cellComments
                        |> List.filter (fun x -> x.Cell.Task.Name = task.Name && x.Cell.Date = cell.Date)
                        
                    CellComponent.``default``
                        { Date = cell.Date
                          Task = task
                          Comments = comments
                          Selected = selection |> List.contains cell
                          Status = cell.Status
                          Today = today }
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
                            let comments = Temp.informationComments.TryFind task.InformationType
                            
                            div [ classList [ Css.blueIndicator, comments.IsSome
                                              Css.tooltipContainer, comments.IsSome ]
                                  Style [ Padding 0
                                          Color task.InformationType.Color
                                          WhiteSpace WhiteSpaceOptions.Nowrap ] ][
                                
                                str task.InformationType.Name
                                
                                match comments with
                                | None -> ()
                                | Some comments ->
                                    comments
                                    |> List.map (fun x -> x.Comment)
                                    |> CellComponent.tooltipPopup
                            ]
                        )
                        |> div [ Style [ PaddingRight 10 ] ]
                
                        taskNameList 0 lanes
                        |> div [ Style [ Width 200 ] ]
                    ]
                ]
                    
                div [][
                    gridHeader dateSequence now
                    
                    gridCells now.Date selection lanes
                ]
            ]
            
        let groupsView dateSequence now selection lanes =
            let groups =
                lanes
                |> List.groupBy (fun (Lane (task, _)) ->
                    task.InformationType
                )
                |> List.groupBy (fun (info, _) ->
                    match info with
                    | Project _ -> "projects"
                    | Area _ -> "areas"
                    | Resource _ -> "resources"
                    | Archive _ -> "archives"
                )
                
            div [ Style [ Display DisplayOptions.Flex ] ][
                
                // Column: Left
                div [][
                    // Top Padding
                    emptyDiv
                    |> List.replicate 3
                    |> div []
                        
                    groups
                    |> List.map (fun (name, groupLanes) ->
                        div [][
                            div [ Style [ Color "#444" ] ][
                                str name
                            ]
                            
                            groupLanes
                            |> List.map (fun (information, lanes) ->
                                let comments = Temp.informationComments.TryFind information
                                
                                div [][
                                    div [ classList [ Css.blueIndicator, comments.IsSome
                                                      Css.tooltipContainer, comments.IsSome ]
                                          Style [ paddingLeftLevel 1
                                                  Color "#444" ] ][
                                        str information.Name
                                        
                                        match comments with
                                        | None -> ()
                                        | Some comments ->
                                            comments
                                            |> List.map (fun x -> x.Comment)
                                            |> CellComponent.tooltipPopup
                                    ]
                                    
                                    
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
                                    gridCells now.Date selection lanes
                                ]
                            )
                            |> div []
                        ]
                    )
                    |> div []
                ]
            ]
            
        let tasksView dateSequence now selection lanes =
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
                            let comments = Temp.informationComments.TryFind task.InformationType
                            
                            div [ classList [ Css.blueIndicator, comments.IsSome
                                              Css.tooltipContainer, comments.IsSome ]
                                  Style [ Padding 0
                                          Color task.InformationType.Color
                                          WhiteSpace WhiteSpaceOptions.Nowrap ] ][
                                
                                str task.InformationType.Name
                                
                                match comments with
                                | None -> ()
                                | Some comments ->
                                    comments
                                    |> List.map (fun x -> x.Comment)
                                    |> CellComponent.tooltipPopup
                            ]
                        )
                        |> div [ Style [ PaddingRight 10 ] ]
                
                        taskNameList 0 lanes
                        |> div [ Style [ Width 200 ] ]
                    ]
                ]
                    
                div [][
                    gridHeader dateSequence now
                    
                    gridCells now.Date selection lanes
                ]
            ]
            
    let ``default`` = FunctionComponent.Of (fun (__props: Props) ->
            
        let getLanes dateSequence now view =
            let tasks =
                Temp.taskList
                |> List.map (fun task ->
                    let events =
                        Temp.cellEvents
                        |> List.filter (fun x -> x.Cell.Task = task)
                        |> List.sortBy (fun x -> x.Cell.Date)
                    task, events
                )
            
            match view with
            | Temp.CalendarView ->
                tasks
                |> List.filter (function { Scheduling = Manual false }, [] -> false | _ -> true)
                |> List.map (fun (task, events) -> LaneRendering.renderLane now dateSequence task events)
                |> Sorting.sortLanesByFrequency
                |> Sorting.sortLanesByIncomingRecurrency now.Date
                |> Sorting.sortLanesByToday now.Date
                |> Sorting.applyPendingManualOrder now.Date Temp.taskOrderList
            | Temp.GroupsView ->
                let lanes =
                    tasks
                    |> List.filter (function { Scheduling = Manual false }, _ -> true | _ -> false)
                    |> List.filter (fun (_, events) ->
                        events
                        |> List.filter (function { Cell = { Date = date } } when date.DateTime <= now.Date.DateTime -> true | _ -> false)
                        |> List.tryLast
                        |> function Some { Status = Dropped } -> false | _ -> true
                    )
                    |> List.map (fun (task, events) -> LaneRendering.renderLane now dateSequence task events)
                    |> Sorting.applyManualOrder Temp.taskOrderList
                    
                Temp.informationList
                |> List.collect (List.map (fun information ->
                    let lanes =
                        lanes
                        |> List.filter (fun (Lane (task, _)) -> task.InformationType = information)
                        
                    information, lanes
                ))
                |> List.collect snd
            | Temp.TasksView ->
                tasks
                |> List.filter (function { Scheduling = Manual _ }, _ -> true | _ -> false)
                |> List.map (fun (task, events) -> LaneRendering.renderLane now dateSequence task events)
                |> Sorting.applyManualOrder Temp.taskOrderList
            
                    
        let getState oldState =
            let now = Temp.getNow Temp.hourOffset
            
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
                        |> List.tryFind (fun cell -> cell.Date = now.Date)
                        |> Option.map (fun cell -> [ cell ])
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
            | Lane (_, cells) :: _ -> cells |> List.map (fun x -> x.Date)
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
            | Temp.GroupsView -> Grid.groupsView dateSequence state.current.Now state.current.Selection state.current.Lanes
            | Temp.TasksView -> Grid.tasksView dateSequence state.current.Now state.current.Selection state.current.Lanes
        ]
    , memoizeWith = equalsButFunctions)
