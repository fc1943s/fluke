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
    
    type Information with
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
        member this.CellClass now =
            match this with
            | Disabled -> Css.cellDisabled
            | Suggested -> Css.cellSuggested
            | Pending -> Css.cellPending
            | Missed -> Css.cellMissed
            | MissedToday -> Css.cellMissedToday
            | EventStatus eventStatus ->
                match eventStatus with
                | Postponed until when until <> midnight && Rendering.isLate now until -> Css.cellPending
                | Postponed until when until <> midnight -> Css.cellPostponedTemp
                | Postponed _ -> Css.cellPostponed
                | Completed -> Css.cellCompleted
                | Dropped -> Css.cellDropped
                | ManualPending -> Css.cellManualPending
                | Session _ -> Css.cellSession
    
    
module Functions =
    open Model
    
    let getCellSeparatorBorderLeft (date: FlukeDate) =
        match date with
        | { Day = 1 } -> Some "#ffffff3d"
        | date when date.DateTime.DayOfWeek = DayOfWeek.Sunday -> Some "#000"
        | _ -> None
        |> Option.map ((+) "1px solid ")
        |> fun x -> CSSProp.BorderLeft x

