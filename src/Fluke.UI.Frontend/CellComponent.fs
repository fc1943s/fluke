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
          Comments: string list
          Status: CellStatus
          Selected: bool
          Today: FlukeDate }

    type State =
        { a: unit }
        static member inline Default =
            { a = () }
        
    type Message =
        | A of unit
        
    // TODO: take this out of here
    let tooltipPopup (comments: string list) =
        div [ Class Css.tooltipPopup ][
            
            comments
            |> List.map (fun x -> x.Trim ())
            |> List.map ((+) Environment.NewLine)
            |> String.concat (Environment.NewLine + Environment.NewLine)
            |> fun text ->
                ReactBindings.React.createElement
                    (Ext.reactMarkdown,
                        {| source = text |}, [])
        ]
        
    let ``default`` = FunctionComponent.Of (fun props ->
        let hasComments = not props.Comments.IsEmpty
        
        div [ classList [ props.Status.CellClass, true
                          Css.tooltipContainer, hasComments
                          Css.cellSelected, props.Selected
                          Css.cellToday, props.Date = props.Today ] ][
                
            div [ Style [ Functions.getCellSeparatorBorderLeft props.Date ] ][]
                
            if hasComments then
                tooltipPopup props.Comments
        ]
    , memoizeWith = equalsButFunctions)
