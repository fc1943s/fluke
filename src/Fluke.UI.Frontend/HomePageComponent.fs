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


module Temp = // Just to load the modules. Comment the module to use TestData instead of PrivateData
    PrivateData.TempData.load ()
    PrivateData.Tasks.load ()
    PrivateData.CellEvents.load ()
    PrivateData.Journal.load ()
    
module HomePageComponent =
    open Model
    
    type View =
        | Flat
        | Tree
    
    type Props =
        { Dispatch: SharedState.SharedServerMessage -> unit
          UIState: UIState.State
          PrivateState: Client.PrivateState<UIState.State> }
        
    type State =
        { Now: FlukeDateTime
          Selection: Cell list
          Lanes: Lane list
          View: View }
        static member inline Default =
            let date = { Year = 0; Month = Month.January; Day = 1 }
            { Now = { Date = date; Time = midnight }
              Selection = []
              Lanes = []
              View = Flat }
        
    let navBar (props: {| View: View
                          SetView: View -> unit |}) =
        
        let events = {|
            OnViewChange = fun view ->
                props.SetView view
        |}
        
        Ext.useEventListener "keydown" (fun (e: KeyboardEvent) ->
            match e.ctrlKey, e.shiftKey, e.key with
            | _, true, "F" -> events.OnViewChange Flat
            | _, true, "T" -> events.OnViewChange Tree
            | _, _,    _   -> ()
        )
        
        Navbar.navbar [ Navbar.Color IsBlack
                        Navbar.Props [ Style [ Height 36
                                               MinHeight 36
                                               Padding "8px 0 0 10px"
                                               Display DisplayOptions.Flex
                                               JustifyContent "space-around" ]]][
            
            Navbar.Item.div [ Navbar.Item.Props [ Class "field"
                                                  OnClick (fun _ -> events.OnViewChange Flat) ] ][

                Checkbox.input [ CustomClass "switch is-small is-dark"
                                 Props [ Checked (props.View = Flat)
                                         OnChange (fun _ -> ()) ]]
                
                Checkbox.checkbox [][
                    str "flat view"
                ]
            ]
            
            Navbar.Item.div [ Navbar.Item.Props [ Class "field"
                                                  OnClick (fun _ -> events.OnViewChange Tree) ] ][

                Checkbox.input [ CustomClass "switch is-small is-dark"
                                 Props [ Checked (props.View = Tree)
                                         OnChange (fun _ -> ()) ]]
                
                Checkbox.checkbox [][
                    str "tree view"
                ]
            ]
        ]
        
        
    module Grid =
        let paddingLeftLevel level =
            PaddingLeft (20 * level)
                                  
        let emptyDiv =
            div [ DangerouslySetInnerHTML { __html = "&nbsp;" } ][]
            
        let taskName level lanes =
            lanes
            |> List.map (fun (Lane (task, _)) ->
                
                div [ classList [ Css.tooltipContainer, true
                                  Css.tooltipIndicator, task.Comments |> List.isEmpty |> not ] ][
                    
                    div [ Style [ CSSProp.Overflow OverflowOptions.Hidden
                                  WhiteSpace WhiteSpaceOptions.Nowrap
                                  paddingLeftLevel level
                                  TextOverflow "ellipsis" ] ][
                        
                        str task.Name
                    ]
                    
                    div [ Class Css.tooltipPopup ][
                        
                        task.Comments
                        |> List.map (fun x -> x.Trim ())
                        |> List.map ((+) Environment.NewLine)
                        |> List.append [ "# " + task.Name ]
                        |> String.concat (Environment.NewLine + Environment.NewLine)
                        |> fun text ->
                            ReactBindings.React.createElement
                                (Ext.reactMarkdown,
                                    {| source = text |}, [])
                    ]
                ]
            )
            
        let gridCells today selection lanes =
            lanes
            |> List.map (fun (Lane (task, cells)) ->
                cells
                |> List.map (fun cell ->
                    let comments =
                        TempData._cellComments
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
            
        let flatView dateSequence now selection lanes =
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
                            div [ Style [ Padding 0
                                          Color task.InformationType.Color
                                          WhiteSpace WhiteSpaceOptions.Nowrap ] ][
                                
                                str task.InformationType.Name
                            ]
                        )
                        |> div [ Style [ PaddingRight 10 ] ]
                
                        taskName 0 lanes
                        |> div [ Style [ Width 200 ] ]
                    ]
                ]
                    
                div [][
                    gridHeader dateSequence now
                    
                    gridCells now.Date selection lanes
                ]
            ]
            
        let treeView dateSequence now selection lanes =
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
                                
                                div [][
                                    div [ Style [ paddingLeftLevel 1
                                                  Color "#444" ] ][
                                        str information.Name
                                    ]
                                    
                                    taskName 2 lanes
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
            
    let ``default`` = FunctionComponent.Of (fun (__props: Props) ->
            
        let getLanes dateSequence now view =
            let tasks =
                TempData._taskList
                |> List.map (fun task ->
                    let events =
                        TempData._cellEvents
                        |> List.filter (fun x -> x.Cell.Task = task)
                        |> List.sortBy (fun x -> x.Cell.Date)
                    task, events
                )
            
            match view with
            | Flat ->
                tasks
                |> List.filter (function { Scheduling = Manual false }, [] -> false | _ -> true)
                |> List.map (fun (task, events) ->
                    LaneRendering.renderLane now dateSequence task events
                )
                |> Sorting.sortLanes now.Date
            | Tree ->
                let lanes =
                    tasks
                    |> List.filter (function { Scheduling = Manual false }, _ -> true | _ -> false)
                    |> List.filter (fun (_, events) ->
                        events
                        |> List.filter (function { Cell = { Date = date } } when date.DateTime <= now.Date.DateTime -> true | _ -> false)
                        |> List.tryLast
                        |> function
                            | Some { Status = Dropped } -> false
                            | _ -> true
                    )
                    |> List.map (fun (task, events) ->
                        LaneRendering.renderLane now dateSequence task events
                    )
                    
                [ TempData._projectList |> List.map Project
                  TempData._areaList |> List.map Area
                  TempData._resourceList |> List.map Resource ]
                |> List.collect (List.map (fun information ->
                    let lanes =
                        lanes
                        |> List.filter (fun (Lane (task, _)) -> task.InformationType = information)
                        |> Sorting.sortLanes now.Date
                        
                    information, lanes
                ))
                |> List.collect snd
                    
        let getState oldState =
            let now = TempData._getNow TempData._hourOffset
            
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
            | Lane (_, cells) :: _ -> 
                cells
                |> List.map (fun x -> x.Date)
            | _ -> []
            
        let events = {|
            OnViewChange = fun view ->
                state.update (fun state -> getState { state with
                                                          View = view
                                                          Selection = [] })
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
            | Tree -> Grid.treeView dateSequence state.current.Now state.current.Selection state.current.Lanes
            | Flat -> Grid.flatView dateSequence state.current.Now state.current.Selection state.current.Lanes
        ]
    , memoizeWith = equalsButFunctions)
