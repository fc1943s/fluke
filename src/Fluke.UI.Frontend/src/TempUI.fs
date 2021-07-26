namespace Fluke.UI.Frontend

open Fluke.Shared.Domain
open Fluke.Shared.Domain.UserInteraction
open Fable.DateFunctions
open Fluke.Shared


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

    let rec informationColor =
        function
        | Project _ -> "#999"
        | Area _ -> "#666"
        | Resource _ -> "#333"


    type DateId with
        static member inline Value (DateId referenceDay) = referenceDay

        static member inline Format dateIdFormat dateId =
            match dateId
                  |> Option.ofObjUnbox
                  |> Option.map DateId.Value,
                  dateIdFormat with
            | Some { Day = Day day }, DateIdFormat.Day -> day.ToString "D2"
            | Some date, DateIdFormat.DayOfWeek -> (date |> FlukeDate.DateTime).Format "EEEEEE"
            | Some date, DateIdFormat.Month -> (date |> FlukeDate.DateTime).Format "MMM"
            | _ -> "??"

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
