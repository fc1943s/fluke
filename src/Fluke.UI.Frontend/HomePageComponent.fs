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

module HomePageComponent =
    PrivateData.tasks |> ignore // Just to load the module. Remove line to use TestData
    
    open Model
    
    type Props =
        unit
        
    type State =
        { Now: FlukeDateTime
          FlatView: bool
          TreeView: bool }
        static member inline Default =
            { Now = { Date = { Year = 0; Month = Month.January; Day = 1 }
                      Time = midnight }
              FlatView = true
              TreeView = false }
        
    let navBar (props: {| FlatView: bool
                          TreeView: bool
                          ToggleFlatView: bool -> unit
                          ToggleTreeView: bool -> unit |}) =
        
        let events = {|
            OnFlatViewToggle = fun () ->
                props.ToggleFlatView (not props.FlatView)
                
            OnTreeViewToggle = fun () ->
                props.ToggleTreeView (not props.TreeView)
        |}
        
        Ext.useEventListener "keydown" (fun (e: KeyboardEvent) ->
            match e.ctrlKey, e.shiftKey, e.key with
            | _, true, "F" -> events.OnFlatViewToggle ()
            | _, true, "T" -> events.OnTreeViewToggle ()
            | _, _,    _   -> ()
        )
        
        Navbar.navbar [ Navbar.Color IsBlack
                        Navbar.Props [ Style [ Height 36
                                               MinHeight 36
                                               Padding "8px 0 0 10px"
                                               Display DisplayOptions.Flex
                                               JustifyContent "space-around" ]]][
            
            Navbar.Item.div [ Navbar.Item.Props [ ClassName "field"
                                                  OnClick (fun _ -> events.OnFlatViewToggle ()) ] ][

                Checkbox.input [ CustomClass "switch is-small is-dark"
                                 Props [ Checked props.FlatView
                                         OnChange (fun _ -> ()) ]]
                
                Checkbox.checkbox [][
                    str "flat view"
                ]
            ]
            
            Navbar.Item.div [ Navbar.Item.Props [ ClassName "field"
                                                  OnClick (fun _ -> events.OnTreeViewToggle ()) ] ][

                Checkbox.input [ CustomClass "switch is-small is-dark"
                                 Props [ Checked props.TreeView
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
                
                div [ classList [ "tooltip-container", true
                                  "tooltip-indicator", task.Comments |> List.isEmpty |> not ] ][
                    
                    div [ Style [ CSSProp.Overflow OverflowOptions.Hidden
                                  WhiteSpace WhiteSpaceOptions.Nowrap
                                  paddingLeftLevel level
                                  TextOverflow "ellipsis" ] ][
                        
                        str task.Name
                    ]
                    
                    div [ Class "tooltip-popup" ][
                        
                        task.Comments
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
            
        let gridCells today lanes =
            lanes
            |> List.map (fun (Lane (task, cells)) ->
                cells
                |> List.map (fun cell ->
                    let comments =
                        TempData._cellComments
                        |> List.filter (fun x -> x.Task.Name = task.Name && x.Date = cell.Date)
                        
                    CellComponent.``default``
                        { Date = cell.Date
                          Task = task
                          Comments = comments
                          Status = cell.Status
                          Today = today }
                )
                |> div []
            ) |> div [ Class "lane-container" ]
            
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
            
        let flatView now dateSequence tasks =
            let lanes =
                tasks
                |> List.filter (function ({ Scheduling = Manual false }, []) -> false | _ -> true)
                |> List.map (fun (task, events) ->
                    LaneRendering.renderLane now dateSequence task events
                )
                |> Sorting.sortLanes now.Date
                
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
                    
                    gridCells now.Date lanes
                ]
            ]
            
        let treeView now dateSequence tasks =
            let lanes =
                tasks
//                |> List.filter (function ({ Scheduling = Manual false }, []) -> true | _ -> false)
                |> List.map (fun (task, events) ->
                    LaneRendering.renderLane now dateSequence task events
                )
                
            let groupLanes informationList = 
                informationList
                |> List.map (fun information ->
                    let lanes =
                        lanes
                        |> List.filter (fun (Lane (task, _)) -> task.InformationType = information)
                        |> Sorting.sortLanes now.Date
                        
                    information, lanes
                )
                |> List.filter (snd >> List.isEmpty >> not)
                
            let groups =
                [ "projects", TempData._projectList |> List.map Project |> groupLanes
                  "areas", TempData._areaList |> List.map Area |> groupLanes
                  "resources", TempData._resourceList |> List.map Resource |> groupLanes ]
                |> List.filter (snd >> List.isEmpty >> not)
            
            div [ Style [ Display DisplayOptions.Flex ] ][
                
                // Column: Left
                div [][
                    // Top Padding
                    emptyDiv
                    |> List.replicate 3
                    |> div []
                        
                    groups
                    |> List.map (fun (name, lanes) ->
                        div [][
                            div [ Style [ Color "#444" ] ][
                                str name
                            ]
                            
                            lanes
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
                    |> List.map (fun (_, lanes) ->
                        div [][
                            emptyDiv
                            
                            lanes
                            |> List.map (fun (_, lanes) ->
                                
                                div [][
                                    emptyDiv
                                    gridCells now.Date lanes
                                ]
                            )
                            |> div []
                        ]
                    )
                    |> div []
                ]
            ]
            
    let ``default`` = FunctionComponent.Of (fun (__props: Props) ->
            
        let state = Hooks.useState { State.Default with Now = TempData._getNow TempData._hourOffset }
            
        Temp.CustomHooks.useInterval (fun () ->
            state.update (fun state -> { state with Now = TempData._getNow TempData._hourOffset })
        ) (60 * 1000)
        
        let dateSequence = 
            [ state.current.Now.Date ]
            |> Rendering.getDateSequence (35, 35)
            
        let tasks =
            TempData._taskList
            |> List.map (fun task ->
                TempData._cellEvents
                |> List.filter (fun x -> x.Task = task) |> fun events -> task, events
            )
            

        let events = {|
            OnFlatViewToggle = fun visible ->
                state.update (fun state -> { state with FlatView = visible })
                
            OnTreeViewToggle = fun visible ->
                state.update (fun state -> { state with TreeView = visible })
        |}
        
        
        Text.div [ Props [ Style [ Height "100%" ] ]
                   Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is7) ] ][

//            if not props.UIState.SharedState.Debug then
//                PageLoader.pageLoader [ PageLoader.Color IsDark
//                                        PageLoader.IsActive (match props.PrivateState.Connection with Client.Connected _ -> false | _ -> true) ][]

            navBar
                {| FlatView = state.current.FlatView
                   TreeView = state.current.TreeView
                   ToggleFlatView = events.OnFlatViewToggle
                   ToggleTreeView = events.OnTreeViewToggle |}
                   
            if state.current.TreeView then
                Grid.treeView state.current.Now dateSequence tasks
                
            if state.current.FlatView then
                Grid.flatView state.current.Now dateSequence tasks
        ]
    , memoizeWith = equalsButFunctions)
