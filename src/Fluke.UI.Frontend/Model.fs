namespace Fluke.UI.Frontend

open Fluke.Shared


module Model =
    open Domain.Model
    open Domain.UserInteraction
    open Domain.State

    [<RequireQualifiedAccess>]
    type Hover =
        | Cell
        | Task
        | Information
        | None

    [<RequireQualifiedAccess>]
    type DockType =
        | Databases
        | Settings

    type ActiveSession = ActiveSession of taskName: string * duration: Minute * totalDuration: Minute * totalBreakDuration: Minute

    let colorCornerBlue = "#005688"
    let colorCornerPink = "#a91c77"

    type Information with
        member this.Color =
            match this with
            | Project _ -> "#999"
            | Area _ -> "#666"
            | Resource _ -> "#333"
            | Archive archive -> sprintf "[%s]" archive.Color

    type ManualCellStatus with
        member inline this.CellColor =
            match this with
            | Postponed (Some _) -> "#604800"
            | Postponed _ -> "#b08200"
            | Completed -> "#339933"
            | Dismissed -> "#673ab7"
            | ManualPending -> "#003038"
    //                | Session -> "#a9a9a9"

    type CellStatus with
        member inline this.CellColor =
            match this with
            | Disabled -> "#595959"
            | Suggested -> "#4c664e"
            | Pending -> "#262626"
            | Missed -> "#990022"
            | MissedToday -> "#530011"
            | UserStatus (user, manualCellStatus) -> manualCellStatus.CellColor
