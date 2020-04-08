namespace Fluke.UI.Frontend

open System
open Fable.React
open Fable.React.Props
open Fluke.Shared
open Fluke.UI.Frontend
    
module CellComponent =
    open Model

    type Props =
        { Date: FlukeDate
          Task: Task
          Comments: CellComment list
          Status: CellStatus
          Selected: bool
          Today: FlukeDate }

    type State =
        { a: unit }
        static member inline Default =
            { a = () }
        
    type Message =
        | A of unit
        
        
    let ``default`` = FunctionComponent.Of (fun props ->
        let hasComments = props.Comments |> List.isEmpty |> not
        
        div [ classList [ props.Status.CellClass, true
                          Css.tooltipIndicator, hasComments
                          Css.cellSelected, props.Selected
                          Css.cellToday, props.Date = props.Today ] ][
                
            div [ Style [ Functions.getCellSeparatorBorderLeft props.Date ] ][]
                
            if hasComments then
                div [ Class Css.tooltipPopup ][
                    
                    props.Comments
                    |> List.map (fun x -> x.Comment)
                    |> List.map ((+) Environment.NewLine)
                    |> String.concat (Environment.NewLine + Environment.NewLine)
                    |> fun text ->
                        ReactBindings.React.createElement
                            (Ext.reactMarkdown,
                                {| source = text |}, [])
                ]
        ]
    , memoizeWith = equalsButFunctions)
