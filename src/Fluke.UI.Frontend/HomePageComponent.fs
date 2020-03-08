namespace Fluke.UI.Frontend

open Browser
open Fluke.Shared
open Suigetsu.Core
open Fable.React
open Fable.React.Props
open Fulma
open Suigetsu.UI.ElmishBridge.Frontend
open System

module Functions =
 
    let getDateRange (cellDates: DateTime list) =
        let minDate =
            cellDates
            |> List.min
            |> fun x -> x.AddDays -10.
            
        let maxDate =
            cellDates
            |> List.max
            |> fun x -> x.AddDays 10.
            
        let rec loop date = seq {
            if date < maxDate then
                yield date
                yield! loop (date.AddDays 1.)
        }
        
        minDate
        |> loop
        |> Seq.toList
        
    let isToday (date: DateTime) =
        date.Date = DateTime.Now.Date.AddDays (if DateTime.Now.Hour < Model.hourOffset then -1. else 0.)
        
    
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
                
            let dateRange = 
                PrivateData.cellEvents
                |> List.map (fun x -> x.Date)
                |> List.append [ DateTime.UtcNow ]
                |> Functions.getDateRange
                
            let taskList =
                Model.getTaskList PrivateData.taskOrderList
                |> List.filter (function { InformationType = Model.Project _ } -> false | _ -> true)
                
            div [ Style [ Display DisplayOptions.Flex ] ][
                
                let topPadding =
                    [ 1 .. 3 ]
                    |> List.map (fun n ->
                        div [ Key (string -n)
                              DangerouslySetInnerHTML { __html = "&nbsp;" } ][]
                    )
                    
                div [ Style [ PaddingRight 10 ] ][
                    taskList
                    |> List.map (fun x ->
                        div [ Key x.Name
                              Style [ Padding 0
                                      Color x.InformationType.Color ] ][
                            
                            str x.InformationType.Name
                        ]
                    )
                    |> List.append topPadding
                    |> ofList
                ]
                
                div [][
                    taskList
                    |> List.map (fun x ->
                        div [ Key x.Name
                              Style [ Padding 0 ] ][ str x.Name ]
                    )
                    |> List.append topPadding
                    |> ofList
                ]
                
                div [][
                    div [ Style [ Display DisplayOptions.Flex ] ][
                        dateRange
                        |> List.map (fun date ->
                            span [ Key (date.ToShortDateString ())
                                   Style [ Width 18
                                           TextAlign TextAlignOptions.Center ] ][
                                
                                str (date.Month.ToString ("D2"))
                            ]
                        )
                        |> ofList
                    ]
                    
                    div [ Style [ Display DisplayOptions.Flex ] ][
                        dateRange
                        |> List.map (fun date ->
                            span [ Key (date.ToShortDateString ())
                                   Style [ Width 18
                                           TextAlign TextAlignOptions.Center ] ][
                                
                                str (date.ToString().ToLower().Substring (0, 2))
                            ]
                        )
                        |> ofList
                    ]
                    
                    div [ Style [ Display DisplayOptions.Flex ] ][
                        dateRange
                        |> List.map (fun date ->
                            span [ Key (date.ToShortDateString ())
                                   Style [ Width 18
                                           TextAlign TextAlignOptions.Center
                                           Color (if Functions.isToday date then "#f22" else "") ] ][
                                str (date.Day.ToString "D2")
                            ]
                        )
                        |> ofList
                    ]
                            
                    taskList
                    |> List.map (fun task ->
                        div [ Key task.Name
                              Style [ Display DisplayOptions.Flex ] ][
                            dateRange
                            |> List.map (fun date ->
                                div [ Key (date.ToShortDateString ())
                                      Style [ Position PositionOptions.Relative ] ][
                                    let cellEvent =
                                        PrivateData.cellEvents
                                        |> List.tryFindBack (fun cell -> cell.Task.Name = task.Name && cell.Date.Date = date.Date)
                                    
                                    let cellComments =
                                        PrivateData.cellComments
                                        |> List.filter (fun cell -> cell.Task.Name = task.Name && cell.Date.Date = date.Date)
                                        
                                    cellEvent
                                    |> function
                                        | Some event -> Model.EventStatus event.Status
                                        | None ->
                                            match task.Scheduling with
                                            | Model.Disabled -> Model.CellStatus.Disabled
                                            | Model.Optional -> Model.CellStatus.Optional
                                            | Model.Recurrency interval -> Model.Pending
                                    |> fun cellStatus ->
                                        div [ Style [ Width 18
                                                      Height 18
                                                      Opacity (if Functions.isToday date then 0.8 else 1.)
                                                      BackgroundColor cellStatus.CellColor ] ][]
                                        
                                    if cellComments |> List.isEmpty |> not then
                                        div [ Style [ Position PositionOptions.Absolute
                                                      BorderTop "8px solid #f00"
                                                      BorderLeft "8px solid transparent"
                                                      Right 0
                                                      Top 0 ] ][]
                                        
                                        div [ Style [ Position PositionOptions.Absolute
                                                      Padding 20
                                                      MinWidth 200
                                                      BackgroundColor "#000"
                                                      Opacity 0.4
                                                      Left 18
                                                      ZIndex 1
                                                      Top 0 ] ][
                                            
                                            cellComments
                                            |> List.map (fun comment ->
                                                div [ Key (date.ToShortDateString ()) ][
                                                    ReactBindings.React.createElement
                                                        (Ext.reactMarkdown,
                                                            {| source = comment.Comment |}, [])
                                                ]
                                            )
                                            |> ofList
                                            
                                        ]
                                        
                                        
                                ]
                            )
                            |> ofList
                        ]
                    )
                    |> ofList
                ]
            ]
        ]
    , memoizeWith = equalsButFunctions)
