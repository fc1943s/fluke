namespace Fluke.UI.Frontend

open FsJs
open Fluke.Shared.Domain
open Fluke.Shared.Domain.UserInteraction


module TempUI =
    open Model

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
        | Search
        | Filter

    type ActiveSession = ActiveSession of taskName: string * duration: Minute

    let inline informationColor information =
        match information with
        | Project _ -> "#999"
        | Area _ -> "#666"
        | Resource _ -> "#333"


    type FlukeDate with
        static member inline Format dateIdFormat date =
            match date, dateIdFormat with
            | { Day = Day day }, DateIdFormat.Day -> day.ToString "D2"
            | date, DateIdFormat.DayOfWeek ->
                date
                |> FlukeDate.DateTime
                |> DateTime.format "EEEEEE"
            | date, DateIdFormat.Month ->
                date
                |> FlukeDate.DateTime
                |> DateTime.format "MMM"

    and [<RequireQualifiedAccess>] DateIdFormat =
        | Day
        | DayOfWeek
        | Month

//    let manualCellStatusColor =
//        function
//        | Postponed (Some _) -> "#604800"
//        | Postponed _ -> "#b08200"
//        | Completed -> "#339933"
//        | Dismissed -> "#673ab7"
//        | Scheduled -> "#003038"
//    //                | Session -> "#a9a9a9"
//
//    let rec cellStatusColor =
//        function
//        | Disabled -> "#595959"
//        | Suggested -> "#4c664e"
//        | Pending -> "#262626"
//        | Missed -> "#990022"
//        | MissedToday -> "#530011"
//        | UserStatus (_, manualCellStatus) -> manualCellStatusColor manualCellStatus
