namespace Fluke.Shared.Domain

open System
open Fluke.Shared

module UserInteraction =
    open Model


    type UserInteraction = UserInteraction of moment: FlukeDateTime * user: User * interaction: Interaction

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
        | Comment of user: User * comment: Comment
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

    and Username = Username of string

    and [<RequireQualifiedAccess>] UserColor =
        | Black
        | Pink
        | Blue

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
        | Selection of selected: Selection

    and Selection = Selection of selection: bool

    and [<RequireQualifiedAccess>] CellStatusChange =
        | Postpone of until: FlukeTime option
        | Complete
        | Dismiss
        | Schedule


    and FlukeDate with
        member inline this.DateTime =
            let Year year, Day day = this.Year, this.Day
            DateTime (year, int this.Month, day, 12, 0, 0)

        member inline this.Stringify () =
            let {
                    Year = Year year
                    Month = month
                    Day = Day day
                } =
                this

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
        member inline this.Stringify () =
            $"{this.Date.Stringify ()} {this.Time.Stringify ()}"

        member inline this.DateTime =
            let Year year, Day day, Hour hour, Minute minute =
                this.Date.Year, this.Date.Day, this.Time.Hour, this.Time.Minute

            DateTime (year, int this.Date.Month, day, int hour, int minute, 0)

        member inline this.GreaterEqualThan (dayStart: FlukeTime) (DateId (referenceDay: FlukeDate)) time =
            let testingAfterMidnight = dayStart.GreaterEqualThan time
            let currentlyBeforeMidnight = this.Time.GreaterEqualThan dayStart

            let newDate =
                if testingAfterMidnight && currentlyBeforeMidnight then
                    referenceDay.DateTime.AddDays 1.
                    |> FlukeDate.FromDateTime
                else
                    referenceDay

            let dateToCompare : FlukeDateTime = { Date = newDate; Time = time }

            this.DateTime >= dateToCompare.DateTime

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
        let dateStart = { Date = referenceDay; Time = dayStart }.DateTime

        let dateEnd = dateStart.AddDays 1.

        match position.DateTime with
        | position when position >=< (dateStart, dateEnd) -> Today
        | position when dateStart < position -> BeforeToday
        | _ -> AfterToday

    let (|StartOfMonth|StartOfWeek|NormalDay|) (weekStart, date) =
        match date with
        | { Day = Day 1 } -> StartOfMonth
        | date when date.DateTime.DayOfWeek = weekStart -> StartOfWeek
        | _ -> NormalDay

    let isToday dayStart position dateId =
        match (dayStart, position, dateId) with
        | Today -> true
        | _ -> false

    let dateId dayStart position =
        match isToday dayStart position (DateId position.Date) with
        | true -> position.Date
        | false ->
            position.Date.DateTime.AddDays -1.
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
