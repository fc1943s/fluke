namespace Fluke.UI.Frontend

open Fluke.Shared.Domain


module TempUI =
    open Model
    open State

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
        | Database
        | Information
        | Task
        | Cell

    type ActiveSession = ActiveSession of taskName: string * duration: Minute

    let rec informationColor =
        function
        | Project _ -> "#999"
        | Area _ -> "#666"
        | Resource _ -> "#333"
        | Archive archive -> $"[{informationColor archive}]"

    let manualCellStatusColor =
        function
        | Postponed (Some _) -> "#604800"
        | Postponed _ -> "#b08200"
        | Completed -> "#339933"
        | Dismissed -> "#673ab7"
        | Scheduled -> "#003038"
    //                | Session -> "#a9a9a9"

    let rec cellStatusColor =
        function
        | Disabled -> "#595959"
        | Suggested -> "#4c664e"
        | Pending -> "#262626"
        | Missed -> "#990022"
        | MissedToday -> "#530011"
        | UserStatus (_user, manualCellStatus) -> manualCellStatusColor manualCellStatus
