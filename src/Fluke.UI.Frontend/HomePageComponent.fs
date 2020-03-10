namespace Fluke.UI.Frontend

open Browser
open Fluke.Shared
open MechaHaze.UI.Frontend
open Suigetsu.Core
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
        { SelectedPeaks: Set<string * string> }
        static member inline Default =
            { SelectedPeaks = Set.empty }
            
    type ToggleBindingSource =
        | ToggleBindingSource of string * string
        
    

    let ``default`` = FunctionComponent.Of (fun (props: Props) ->

        Text.div [ Props [ Style [ Height "100%" ] ]
                   Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is7) ] ][

//            if not props.UIState.SharedState.Debug then
//                PageLoader.pageLoader [ PageLoader.Color IsDark
//                                        PageLoader.IsActive (match props.PrivateState.Connection with Client.Connected _ -> false | _ -> true) ][]

            Navbar.navbar [ Navbar.Color IsBlack
                            Navbar.Props [ Style [ Height 36
                                                   MinHeight 36 ]]][

            ]
            
            let today =
                Model.FlukeDate.FromDateTime (DateTime.Now.AddHours -(float PrivateData.hourOffset))
                
            let dateSequence = 
                PrivateData.cellEvents
                |> List.map (fun x -> x.Date)
                |> List.append [ today ]
                |> Functions.getDateSequence (3, 70)
                
            let lanes =
                Functions.getManualSortedTaskList PrivateData.taskOrderList
                |> List.map (fun task ->
                    PrivateData.cellEvents
                    |> List.filter (fun x -> x.Task = task)
                    |> Functions.renderLane task today dateSequence
                )
                |> Functions.sortLanes today
                |> List.filter (function Model.Lane ({ InformationType = Model.Project _ }, _) -> false | _ -> true)
                
            // Columns
            div [ Style [ Display DisplayOptions.Flex ] ][
                
                let topPadding =
                    [ 1 .. 3 ]
                    |> List.map (fun n ->
                        div [ Key (string -n)
                              DangerouslySetInnerHTML { __html = "&nbsp;" } ][]
                    )
                    
                // Information Type
                div [ Style [ PaddingRight 10 ] ][
                    lanes
                    |> List.map (fun (Model.Lane (task, _)) ->
                        div [ Key task.Name
                              Style [ Padding 0
                                      Color task.InformationType.Color ] ][
                            
                            str task.InformationType.Name
                        ]
                    )
                    |> List.append topPadding
                    |> ofList
                ]
                
                // Task Name
                div [][
                    lanes
                    |> List.map (fun (Model.Lane (task, _)) ->
                        div [ Key task.Name
                              Style [ Padding 0 ] ][ str task.Name ]
                    )
                    |> List.append topPadding
                    |> ofList
                ]
                
                div [][
                    // Month Row
                    div [ Style [ Display DisplayOptions.Flex ] ][
                        dateSequence
                        |> List.map (fun date ->
                            span [ Key (date.ToString ())
                                   Style [ Width 18
                                           TextAlign TextAlignOptions.Center ] ][
                                
                                str (date.Month.ToString ("D2"))
                            ]
                        ) |> ofList
                    ]
                    
                    // Day of Week Row
                    div [ Style [ Display DisplayOptions.Flex ] ][
                        dateSequence
                        |> List.map (fun date ->
                            span [ Key (date.ToString ())
                                   Style [ Width 18
                                           TextAlign TextAlignOptions.Center ] ][
                                
                                str (date.DateTime.ToString().ToLower().Substring (0, 2))
                            ]
                        ) |> ofList
                    ]
                    
                    // Day Row
                    div [ Style [ Display DisplayOptions.Flex ] ][
                        dateSequence
                        |> List.map (fun date ->
                            span [ Key (date.ToString ())
                                   Style [ Width 18
                                           TextAlign TextAlignOptions.Center
                                           Color (if date = today then "#f22" else "") ] ][
                                str (date.Day.ToString "D2")
                            ]
                        ) |> ofList
                    ]
                            
                    lanes
                    |> List.map (fun (Model.Lane (task, cells)) ->
                        div [ Key task.Name
                              Class "lane"
                              Style [ Display DisplayOptions.Flex ] ][
                            
                            cells
                            |> List.map (fun cell ->
                                
                                div [ Key (cell.Date.ToString ()) ][
                                    
                                    CellComponent.``default``
                                        { Date = cell.Date
                                          Task = task
                                          Status = cell.Status
                                          Today = today }
                                ]
                            ) |> ofList
                        ]
                    ) |> ofList
                ]
            ]
        ]
    , memoizeWith = equalsButFunctions)
