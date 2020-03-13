namespace Fluke.UI.Frontend

open Fluke.Shared
open Fluke.UI.Frontend
open Fable.React
open Fable.React.Props
open Fulma
open Suigetsu.UI.ElmishBridge.Frontend
open System

        
module HomePageComponent =
    
    type Props =
        { Dispatch: SharedState.SharedServerMessage -> unit
          UIState: UIState.State
          PrivateState: Client.PrivateState<UIState.State> }
        
    type State =
        { Now: Model.FlukeDateTime
          GridView: bool
          TreeView: bool }
        static member inline Default =
            { Now = { Date = { Year = 0; Month = 0; Day = 0 }
                      Time = Model.midnight }
              GridView = true
              TreeView = false }
            
    type ToggleBindingSource =
        | ToggleBindingSource of string * string
        
    let ``default`` = FunctionComponent.Of (fun (__props: Props) ->
        let getNow () =
            let rawDate = DateTime.Now.AddHours -(float PrivateData.hourOffset)
            { Model.Date = Model.FlukeDate.FromDateTime rawDate
              Model.Time = Model.FlukeTime.FromDateTime rawDate }
            
        let state = Hooks.useState { State.Default with Now = getNow () }
            
        Temp.CustomHooks.useInterval (fun () ->
            state.update (fun state -> { state with Now = getNow () })
        ) (60 * 1000)
            
        let dateSequence = 
            PrivateData.cellEvents
            |> List.map (fun x -> x.Date)
            |> List.append [ state.current.Now.Date ]
            |> Functions.getDateSequence (3, 70)
            
        let lanes =
            Functions.getManualSortedTaskList PrivateData.taskOrderList
            |> List.map (fun task ->
                PrivateData.cellEvents
                |> List.filter (fun x -> x.Task = task)
                |> Functions.renderLane task state.current.Now dateSequence
            )
            |> Functions.sortLanes state.current.Now.Date
            // |> List.filter (function Model.Lane ({ InformationType = Model.Project _ }, _) -> false | _ -> true)
            
        let events = {|
            OnGridViewToggle = fun _ ->
                state.update (fun state -> { state with GridView = not state.GridView })
                
            OnTreeViewToggle = fun _ ->
                state.update (fun state -> { state with TreeView = not state.TreeView })
        |}

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
                                                      OnClick events.OnGridViewToggle ] ][

                    Checkbox.input [ CustomClass "switch is-small is-dark"
                                     Props [ Checked state.current.GridView
                                             OnChange (fun _ -> ()) ]]
                    
                    Checkbox.checkbox [][
                        str "Default View"
                    ]
                ]
                
                Navbar.Item.div [ Navbar.Item.Props [ ClassName "field"
                                                      OnClick events.OnTreeViewToggle ] ][

                    Checkbox.input [ CustomClass "switch is-small is-dark"
                                     Props [ Checked state.current.TreeView
                                             OnChange (fun _ -> ()) ]]
                    
                    Checkbox.checkbox [][
                        str "Tree View"
                    ]
                ]
            ]
            
            // Grid View
            div [ Style [ Display DisplayOptions.Flex ] ][
                
                let topPadding =
                    div [ DangerouslySetInnerHTML { __html = "&nbsp;" } ][]
                    |> List.replicate 3
                    
                // Information Type
                lanes
                |> List.map (fun (Model.Lane (task, _)) ->
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
                |> List.map (fun (Model.Lane (task, _)) ->
                    
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
                                
                                str (date.Month.ToString ("D2"))
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
                    |> List.map (fun (Model.Lane (task, cells)) ->
                        cells
                        |> List.map (fun cell ->
                            CellComponent.``default``
                                { Date = cell.Date
                                  Task = task
                                  Status = cell.Status
                                  Today = state.current.Now.Date }
                        )
                        |> div [ Class "lane"
                                 Style [ Display DisplayOptions.Flex ] ]
                    ) |> div []
                ]
            ]
            |> fun x -> if state.current.GridView then x else div [][]
            
            // Tree View
            div [ Style [ Display DisplayOptions.Flex ] ][
                str "Tree View"
            ]
            |> fun x -> if state.current.TreeView then x else div [][]
        ]
    , memoizeWith = equalsButFunctions)
