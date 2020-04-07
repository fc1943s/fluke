namespace Fluke.UI.Frontend

open Fable.React.Props
open Fluke.Shared
open System

module SharedState =
    type SharedClientMessage = unit
    type SharedServerMessage = unit
        
module UIState =
    type State =
        { x: unit }
        static member inline Default = { x = () }

module Model =
    open Model
    
    type InformationType with
        member this.Name = 
            match this with
            | Project project -> project.Name
            | Area area -> area.Name
            | Resource resource -> resource.Name
            | Archive archive -> sprintf "[%s]" archive.Name
            
        member this.Color = 
            match this with
            | Project _ -> "#999"
            | Area _ -> "#666"
            | Resource _ -> "#333"
            | Archive archive -> sprintf "[%s]" archive.Color
            
    type CellStatus with
        member this.CellClass =
            match this with
            | Disabled -> "cell-disabled"
            | Suggested -> "cell-suggested"
            | Pending -> "cell-pending"
            | Missed -> "cell-missed"
            | EventStatus status ->
                match status with
                | Postponed -> "cell-postponed"
                | Complete -> "cell-complete"
                | Dropped -> "cell-dropped"
                | ManualPending -> "cell-manualpending"
    
    
module Functions =
    open Model
    
    let getCellSeparatorBorderLeft (date: FlukeDate) =
        match date with
        | { Day = 1 } -> Some "#ffffff3d"
        | date when date.DateTime.DayOfWeek = DayOfWeek.Sunday -> Some "#000"
        | _ -> None
        |> Option.map ((+) "1px solid ")
        |> fun x -> CSSProp.BorderLeft x

