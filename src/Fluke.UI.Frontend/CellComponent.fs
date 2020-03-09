namespace MechaHaze.UI.Frontend

open Browser
open Fable.Core
open Fable.Core.JsInterop
open System
open Browser
open Browser.Types
open Fable.React
open Fable.React.Props
open Fluke.Shared
open Fluke.UI.Frontend
open Fulma
    
module CellComponent =

    type Props =
        { Date: DateTime
          Task: Model.Task }

    type State =
        { a: unit }
        static member inline Default =
            { a = () }
        
    type Message =
        | A of unit
        
        
    let ``default`` = FunctionComponent.Of (fun props ->
        div [ Class "cell"
              Style [ Position PositionOptions.Relative ] ][
            
            let cellEvent =
                PrivateData.cellEvents
                |> List.tryFindBack (fun cell -> cell.Task.Name = props.Task.Name && cell.Date.Date = props.Date.Date)
            
            let cellComments =
                PrivateData.cellComments
                |> List.filter (fun cell -> cell.Task.Name = props.Task.Name && cell.Date.Date = props.Date.Date)
                
            cellEvent 
            |> function
                | Some event -> Model.EventStatus event.Status
                | None ->
                    match props.Task.Scheduling with
                    | Model.Disabled -> Model.CellStatus.Disabled
                    | Model.Optional -> Model.CellStatus.Optional
                    | Model.Recurrency interval -> Model.Pending
            |> fun cellStatus ->
                div [ Style [ Width 18
                              Height 18
                              Opacity (if Functions.isToday props.Date then 0.8 else 1.)
                              BackgroundColor cellStatus.CellColor ] ][]
                
            if cellComments |> List.isEmpty |> not then
                div [ Style [ Position PositionOptions.Absolute
                              BorderTop "8px solid #f00"
                              BorderLeft "8px solid transparent"
                              Right 0
                              Top 0 ] ][]
                
                div [ Class "comment-container"
                      Style [ Position PositionOptions.Absolute
                              Padding 20
                              MinWidth 200
                              BackgroundColor "#000"
                              Opacity 0.4
                              Left 18
                              ZIndex 1
                              Top 0 ] ][
                    
                    cellComments
                    |> List.map (fun comment ->
                        div [ Key (props.Date.ToShortDateString ()) ][
                            ReactBindings.React.createElement
                                (Ext.reactMarkdown,
                                    {| source = comment.Comment |}, [])
                        ]
                    )
                    |> ofList
                ]
        ]
    , memoizeWith = equalsButFunctions)
