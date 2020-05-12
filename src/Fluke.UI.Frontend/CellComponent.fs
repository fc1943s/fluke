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
          Comments: Comment list
          Sessions: TaskSession list
          Status: CellStatus
          Selected: bool
          Now: FlukeDateTime }

    type State =
        { a: unit }
        static member inline Default =
            { a = () }
        
    type Message =
        | A of unit
        
    // TODO: take this out of here
    let tooltipPopup comments =
        div [ Class Css.tooltipPopup ][
            
            comments
            |> List.map (fun (Comment x) -> x.Trim ())
            |> List.map ((+) Environment.NewLine)
            |> String.concat (Environment.NewLine + Environment.NewLine)
            |> fun text ->
                ReactBindings.React.createElement
                    (Ext.reactMarkdown,
                        {| source = text |}, [])
        ]
        
    let ``default`` = FunctionComponent.Of (fun props ->
        let hasComments = not props.Comments.IsEmpty
        
        div [ classList [ props.Status.CellClass props.Now.Time, true
                          Css.tooltipContainer, hasComments
                          Css.cellSelected, props.Selected
                          Css.cellToday, props.Date = props.Now.Date ] ][
                
            div [ Style [ Functions.getCellSeparatorBorderLeft props.Date ] ][
                match props.Sessions.Length with
//                | x -> str (string x)
                | x when x > 0 -> str (string x)
                | _ -> ()
            ]
            
                
            if hasComments then
                tooltipPopup props.Comments
        ]
    , memoizeWith = equalsButFunctions)
