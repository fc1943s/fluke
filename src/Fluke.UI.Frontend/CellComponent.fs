namespace Fluke.UI.Frontend

open System
open Fable.React
open Fable.React.Props
open Fluke.Shared
open Fluke.UI.Frontend
    
module CellComponent =

    type Props =
        { Date: Model.FlukeDate
          Task: Model.Task
          Status: Model.CellStatus
          Today: Model.FlukeDate }

    type State =
        { a: unit }
        static member inline Default =
            { a = () }
        
    type Message =
        | A of unit
        
        
    let ``default`` = FunctionComponent.Of (fun props ->
        let cellComments =
            PrivateData.cellComments
            |> List.filter (fun cell -> cell.Task.Name = props.Task.Name && cell.Date = props.Date)
        let hasComments = cellComments |> List.isEmpty |> not
        
        div [ Class ([ "cell"
                       "tooltip-container"
                       if hasComments then "tooltip-indicator" else "" ]
                     |> String.concat " ") ][
                
            div [ Style [ Width 18
                          Height 18
                          Functions.getCellSeparatorBorderLeft props.Date
                          if hasComments then
                              Border "1px solid #ffffff77"
                          BackgroundColor (props.Status.CellColor + (if props.Date = props.Today then "bb" else "ff")) ] ][]
                
            if hasComments then
                div [ Class "tooltip-popup"
                      Style [ Padding 20
                              MinWidth 200
                              Left 18
                              Top 0 ] ][
                    
                    cellComments
                    |> List.map (fun x -> x.Comment + Environment.NewLine)
                    |> String.concat (Environment.NewLine + Environment.NewLine)
                    |> fun text ->
                        ReactBindings.React.createElement
                            (Ext.reactMarkdown,
                                {| source = text |}, [])
                ]
        ]
    , memoizeWith = equalsButFunctions)
