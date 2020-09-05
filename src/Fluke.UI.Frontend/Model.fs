namespace Fluke.UI.Frontend

open Fluke.Shared


module Model =
    open Model

    [<RequireQualifiedAccess>]
    type View =
        | Calendar
        | Groups
        | Tasks
        | Week

    [<RequireQualifiedAccess>]
    type Hover =
        | Cell
        | Task
        | Information
        | None

    type ActiveSession = ActiveSession of taskName: string * duration: Minute * totalDuration: Minute * totalBreakDuration: Minute

    type Information with
        member this.Color =
            match this with
            | Project _ -> "#999"
            | Area _ -> "#666"
            | Resource _ -> "#333"
            | Archive archive -> sprintf "[%s]" archive.Color

    type CellStatus with
        member this.CellClass =
            match this with
            | Disabled -> Css.cellDisabled
            | Suggested -> Css.cellSuggested
            | Pending -> Css.cellPending
            | Missed -> Css.cellMissed
            | MissedToday -> Css.cellMissedToday
            | UserStatus (user, manualCellStatus) ->
                match manualCellStatus with
                | Postponed (Some _) -> Css.cellPostponedUntil
                | Postponed _ -> Css.cellPostponed
                | Completed -> Css.cellCompleted
                | Dismissed -> Css.cellDismissed
                | ManualPending -> Css.cellManualPending
