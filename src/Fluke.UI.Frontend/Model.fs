namespace Fluke.UI.Frontend

open Fable.React.Props
open Feliz
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
            | Project project   -> project.Name
            | Area area         -> area.Name
            | Resource resource -> resource.Name
            | Archive archive   -> sprintf "[%s]" archive.Name

        member this.Color =
            match this with
            | Project _       -> "#999"
            | Area _          -> "#666"
            | Resource _      -> "#333"
            | Archive archive -> sprintf "[%s]" archive.Color

    type CellStatus with
        member this.CellClass =
            match this with
            | Disabled    -> Css.cellDisabled
            | Suggested   -> Css.cellSuggested
            | Pending     -> Css.cellPending
            | Missed      -> Css.cellMissed
            | MissedToday -> Css.cellMissedToday
            | EventStatus eventStatus ->
                match eventStatus with
                | Postponed (Some _) -> Css.cellPostponedUntil
                | Postponed _        -> Css.cellPostponed
                | Completed          -> Css.cellCompleted
                | Dismissed          -> Css.cellDismissed
                | ManualPending      -> Css.cellManualPending
                | Session _          -> Css.cellSession


module Functions =
    open Model


    let tmpGetCellSeparatorBorderLeft (date: FlukeDate) =
        match date with
        | { Day = 1 }                                          -> Some "#ffffff3d"
        | date when date.DateTime.DayOfWeek = DayOfWeek.Monday -> Some "#222"
        | _                                                    -> None

    let getCellSeparatorBorderLeft (date: FlukeDate) =
        tmpGetCellSeparatorBorderLeft date
        |> Option.map ((+) "1px solid ")
        |> fun x -> CSSProp.BorderLeft x

    let getCellSeparatorBorderLeft2 (date: FlukeDate) =
        tmpGetCellSeparatorBorderLeft date
        |> Option.map (fun color -> style.borderLeft (1, borderStyle.solid, color))


