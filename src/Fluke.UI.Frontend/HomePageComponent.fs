namespace Fluke.UI.Frontend

open Browser
open Browser.Types
open Fluke.Shared
open Fluke.UI.Frontend
open Fable.React
open Fable.React.Props
open Fable.Core.JsInterop
open Fulma
open Suigetsu.UI.ElmishBridge.Frontend
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
            
    type ToggleBindingSource =
        | ToggleBindingSource of string * string
        
    let ``default`` = FunctionComponent.Of (fun (__props: Props) ->
            
        let state = Hooks.useState { State.Default with Now = TempData._getNow TempData._hourOffset }
            
        Temp.CustomHooks.useInterval (fun () ->
            state.update (fun state -> { state with Now = TempData._getNow TempData._hourOffset })
        ) (60 * 1000)
        
        let dateSequence = 
            TempData._cellEvents
            |> List.map (fun x -> x.Date)
            |> List.append [ state.current.Now.Date ]
            |> Rendering.getDateSequence (3, 70)
            
        let tasks =
            TempData._taskList
            |> List.map (fun task ->
                TempData._cellEvents
                |> List.filter (fun x -> x.Task = task)
                |> fun events -> task, events
            )
                
        let lanes =
            tasks
            |> List.filter (function ({ Scheduling = Manual false }, []) -> false | _ -> true)
            |> List.map (fun (task, events) ->
                events
                |> LaneRendering.renderLane task state.current.Now dateSequence
            )
            |> Sorting.sortLanes state.current.Now.Date
            
        let events = {|
            OnFlatViewToggle = fun () ->
                state.update (fun state -> { state with FlatView = not state.FlatView })
                
            OnTreeViewToggle = fun () ->
                state.update (fun state -> { state with TreeView = not state.TreeView })
        |}

        Ext.useEventListener "keydown" (fun (e: KeyboardEvent) ->
            printfn "AE"
            match e.ctrlKey, e.shiftKey, e.key with
            | _, true, "F" -> events.OnFlatViewToggle ()
            | _, true, "T" -> events.OnTreeViewToggle ()
            | _, _,    _   -> ()
        )
            
        Text.div [ Props [ Style [ Height "100%" ] ]
                   Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is7) ] ][

//            if not props.UIState.SharedState.Debug then
//                PageLoader.pageLoader [ PageLoader.Color IsDark
//                                        PageLoader.IsActive (match props.PrivateState.Connection with Client.Connected _ -> false | _ -> true) ][]

            Navbar.navbar [ Navbar.Color IsBlack
                            Navbar.Props [ Style [ Height 36
                                                   MinHeight 36
                                                   Padding "8px 0 0 10px"
                                                   Display DisplayOptions.Flex
                                                   JustifyContent "space-around" ]]][
                
                Navbar.Item.div [ Navbar.Item.Props [ ClassName "field"
                                                      OnClick (fun _ -> events.OnFlatViewToggle ()) ] ][

                    Checkbox.input [ CustomClass "switch is-small is-dark"
                                     Props [ Checked state.current.FlatView
                                             OnChange (fun _ -> ()) ]]
                    
                    Checkbox.checkbox [][
                        str "Flat View"
                    ]
                ]
                
                Navbar.Item.div [ Navbar.Item.Props [ ClassName "field"
                                                      OnClick (fun _ -> events.OnTreeViewToggle ()) ] ][

                    Checkbox.input [ CustomClass "switch is-small is-dark"
                                     Props [ Checked state.current.TreeView
                                             OnChange (fun _ -> ()) ]]
                    
                    Checkbox.checkbox [][
                        str "Tree View"
                    ]
                ]
            ]
            
            // Flat View
            div [ Style [ Display DisplayOptions.Flex ] ][
                
                let topPadding =
                    div [ DangerouslySetInnerHTML { __html = "&nbsp;" } ][]
                    |> List.replicate 3
                    
                // Information Type
                lanes
                |> List.map (fun (Lane (task, _)) ->
                    div [ Style [ Padding 0
                                  Color task.InformationType.Color
                                  WhiteSpace WhiteSpaceOptions.Nowrap ] ][
                        
                        str task.InformationType.Name
                    ]
                )
                |> List.append topPadding
                |> div [ Style [ PaddingRight 10 ] ]
                
                // Task Name
                lanes
                |> List.map (fun (Lane (task, _)) ->
                    
                    div [ Class ([ "tooltip-container"
                                   if task.Comments |> List.isEmpty then "" else "tooltip-indicator" ]
                                 |> String.concat " ") ][
                        
                        div [ Style [ CSSProp.Overflow OverflowOptions.Hidden
                                      WhiteSpace WhiteSpaceOptions.Nowrap
                                      TextOverflow "ellipsis" ] ][
                            
                            str task.Name
                        ]
                        
                        div [ Class "tooltip-popup"
                              Style [ Padding 20
                                      MinWidth 200
                                      Left 18
                                      Top 0 ] ][
                            
                            task.Comments
                            |> List.map (fun x -> x + Environment.NewLine)
                            |> List.append [ "# " + task.Name ]
                            |> String.concat (Environment.NewLine + Environment.NewLine)
                            |> fun text ->
                                ReactBindings.React.createElement
                                    (Ext.reactMarkdown,
                                        {| source = text |}, [])
                        ]
                    ]
                )
                |> List.append topPadding
                |> div [ Style [ Width 200 ] ]
                
                div [][
                    
                    dateSequence
                    |> Temp.Core.recFn (fun dayOfWeekRow -> function
                        | date :: tail -> 
                            span [ Style [ Width 18
                                           Functions.getCellSeparatorBorderLeft date
                                           TextAlign TextAlignOptions.Center ] ][
                                
                                str (date.DateTime.ToString().ToLower().Substring (0, 2))
                            ] :: dayOfWeekRow tail
                        | [] -> [])
                    |> div [ Style [ Display DisplayOptions.Flex ] ]
                    
                    dateSequence
                    |> Temp.Core.recFn (fun monthRow -> function
                        | date :: tail -> 
                            span [ Style [ Width 18
                                           Functions.getCellSeparatorBorderLeft date
                                           TextAlign TextAlignOptions.Center ] ][
                                
                                str ((int date.Month).ToString ("D2"))
                            ] :: monthRow tail
                        | [] -> [])
                    |> div [ Style [ Display DisplayOptions.Flex ] ]
                    
                    // Day Row
                    dateSequence
                    |> Seq.map (fun date ->
                        span [ Style [ Width 18
                                       Functions.getCellSeparatorBorderLeft date
                                       TextAlign TextAlignOptions.Center
                                       Color (if date = state.current.Now.Date then "#f22" else "") ] ][
                            str (date.Day.ToString "D2")
                        ]
                    )
                    |> div [ Style [ Display DisplayOptions.Flex ] ]
                    
                    // Cells
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
                                  Today = state.current.Now.Date }
                        )
                        |> div [ Class "lane"
                                 Style [ Display DisplayOptions.Flex ] ]
                    ) |> div []
                ]
            ]
            |> fun x -> if state.current.FlatView then x else div [][]
            
            // Tree View
            div [ Style [ Display DisplayOptions.Flex ] ][
                str "Tree View"
            ]
            |> fun x -> if state.current.TreeView then x else div [][]
        ]
    , memoizeWith = equalsButFunctions)
