namespace Fluke.Shared.Domain

open System
open Fluke.Shared

module UserInteraction =
    open Model


    type UserInteraction = UserInteraction of moment: FlukeDateTime * username: Username * interaction: Interaction

    and FlukeDateTime = { Date: FlukeDate; Time: FlukeTime }

    and FlukeDate = { Year: Year; Month: Month; Day: Day }

    and TempSchedule =
        | TimeSchedule1 of date: FlukeDate * Schedulings: (FlukeTime * Task) list
        | TimeSchedule2 of date: FlukeDate * Schedulings: Map<FlukeTime, Task>

    and Year = Year of int

    and [<RequireQualifiedAccess>] Interaction =
        | Information of information: Information * interaction: InformationInteraction
        | Task of task: Task * interaction: TaskInteraction
        | Cell of cellAddress: CellAddress * interaction: CellInteraction

    and [<RequireQualifiedAccess>] InformationInteraction =
        | Attachment of attachment: Attachment
        | Sort of top: Information option * bottom: Information option
    // Link: Auto:[Title, Favicon, Screenshot]
    // Image: Embed
    and [<RequireQualifiedAccess>] Attachment =
        | Comment of username: Username * comment: Comment
        | Link
        | Video
        | Image
        | Attachment of attachment: Attachment

    and [<RequireQualifiedAccess>] Comment = Comment of comment: string

    and User =
        {
            Username: Username
            Color: UserColor
            WeekStart: DayOfWeek
            DayStart: FlukeTime
            SessionLength: Minute
            SessionBreakLength: Minute
        }

    and Username = Username of username: string

    and [<RequireQualifiedAccess>] UserColor =
        | Black
        | Pink
        | Blue

    and [<RequireQualifiedAccess>] Language =
        | English
        | Portuguese

    and [<RequireQualifiedAccess>] TaskInteraction =
        | Attachment of attachment: Attachment
        | Archive
        | Session of session: TaskSession
        | Sort of top: Task option * bottom: Task option

    and TaskSession = TaskSession of start: FlukeDateTime * duration: Minute * breakDuration: Minute

    and CellAddress = { Task: Task; DateId: DateId }

    and DateId = DateId of referenceDay: FlukeDate

    and [<RequireQualifiedAccess>] CellInteraction =
        | Attachment of attachment: Attachment
        | StatusChange of cellStatusChange: CellStatusChange

    and [<RequireQualifiedAccess>] CellStatusChange =
        | Postpone of until: FlukeTime option
        | Complete
        | Dismiss
        | Schedule


    and Username with
        static member inline Value (Username username) = username

    and DateId with
        static member inline Value (DateId referenceDay) = referenceDay

    and FlukeDate with
        static member inline DateTime
            {
                Year = Year year
                Month = month
                Day = Day day
            }
            =
            DateTime (year, int month, day, 12, 0, 0)

        static member inline Stringify
            {
                Year = Year year
                Month = month
                Day = Day day
            }
            =
            $"%d{year}-%02d{int month}-%02d{day}"

        static member inline Create year month day : FlukeDate =
            {
                Year = Year year
                Month = month
                Day = Day day
            }

        static member inline FromDateTime (date: DateTime) : FlukeDate =
            {
                Year = Year date.Year
                Month = Enum.Parse (typeof<Month>, string date.Month) :?> Month
                Day = Day date.Day
            }

        static member inline MinValue = FlukeDate.FromDateTime DateTime.MinValue

    and FlukeDateTime with
        static member inline Stringify{ Date = date; Time = time } =
            $"{date |> FlukeDate.Stringify} {time |> FlukeTime.Stringify}"

        static member inline DateTime
            {
                Date = {
                           Year = Year year
                           Month = month
                           Day = Day day
                       }
                Time = {
                           Hour = Hour hour
                           Minute = Minute minute
                       }
            }
            =
            DateTime (year, int month, day, int hour, int minute, 0)

        member inline this.GreaterEqualThan (dayStart: FlukeTime) (DateId (referenceDay: FlukeDate)) time =
            let testingAfterMidnight = dayStart.GreaterEqualThan time
            let currentlyBeforeMidnight = this.Time.GreaterEqualThan dayStart

            let newDate =
                if testingAfterMidnight && currentlyBeforeMidnight then
                    (referenceDay |> FlukeDate.DateTime).AddDays 1.
                    |> FlukeDate.FromDateTime
                else
                    referenceDay

            let dateToCompare : FlukeDateTime = { Date = newDate; Time = time }

            (this |> FlukeDateTime.DateTime)
            >= (dateToCompare |> FlukeDateTime.DateTime)

        static member inline FromDateTime (date: DateTime) : FlukeDateTime =
            {
                Date = FlukeDate.FromDateTime date
                Time = FlukeTime.FromDateTime date
            }

        static member inline Create year month day hour minute : FlukeDateTime =
            {
                Date = FlukeDate.Create year month day
                Time = FlukeTime.Create hour minute
            }


    let (|BeforeToday|Today|AfterToday|) (dayStart: FlukeTime, position: FlukeDateTime, DateId referenceDay) =
        let dateStart =
            { Date = referenceDay; Time = dayStart }
            |> FlukeDateTime.DateTime

        let dateEnd = dateStart.AddDays 1.

        match position |> FlukeDateTime.DateTime with
        | position when position >=< (dateStart, dateEnd) -> Today
        | position when dateStart < position -> BeforeToday
        | _ -> AfterToday

    let (|StartOfMonth|StartOfWeek|NormalDay|) (weekStart, date) =
        match date with
        | { Day = Day 1 } -> StartOfMonth
        | date when (date |> FlukeDate.DateTime).DayOfWeek = weekStart -> StartOfWeek
        | _ -> NormalDay

    let isToday dayStart position dateId =
        match (dayStart, position, dateId) with
        | Today -> true
        | _ -> false

    let dateId dayStart position =
        match isToday dayStart position (DateId position.Date) with
        | true -> position.Date
        | false ->
            (position.Date |> FlukeDate.DateTime).AddDays -1.
            |> FlukeDate.FromDateTime
        |> DateId



    module Temp =

        [<RequireQualifiedAccess>]
        type AttachmentInteraction_ =
            // ?
            | Pin_

        [<RequireQualifiedAccess>]
        type DayInteraction_ =
            // ?
            | Holiday_
