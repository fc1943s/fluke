namespace Fluke.Shared.Domain

open FsCore
open System
open Fluke.Shared
open FsCore.BaseModel

module UserInteraction =
#if !FABLE_COMPILER
    open Myriad.Plugins
#else
    module Generator =
        type DuCases (_configGroup: string) =
            inherit Attribute ()

        type Fields (_configGroup: string) =
            inherit Attribute ()
#endif

    open Model


    type UserInteraction = UserInteraction of moment: FlukeDateTime * username: Username * interaction: Interaction

    and FlukeDateTime =
        {
            Date: FlukeDate
            Time: FlukeTime
            Second: Second
        }

    and FlukeDate = { Year: Year; Month: Month; Day: Day }

    and Second = Second of int

    and TempSchedule =
        | TimeSchedule1 of date: FlukeDate * Schedulings: (FlukeTime * Task) list
        | TimeSchedule2 of date: FlukeDate * Schedulings: Map<FlukeTime, Task>

    and Year = Year of int

    and [<RequireQualifiedAccess>] Interaction =
        | Information of information: Information * interaction: InformationInteraction
        | Task of task: TaskId * interaction: TaskInteraction
        | Cell of taskId: TaskId * date: FlukeDate * interaction: CellInteraction

    and [<RequireQualifiedAccess>] InformationInteraction =
        | Attachment of attachment: Attachment
        | Sort of top: Information option * bottom: Information option

    // Link: Auto:[Title, Favicon, Screenshot]
    // Image: Embed
    and [<RequireQualifiedAccess; Generator.DuCases "Domain">] Attachment =
        | Comment of comment: Comment
        | Link of url: string
        | Video of url: string
        | Image of fileId: FileId
        | List of list: Attachment list


    and [<RequireQualifiedAccess>] Comment = Comment of comment: string

    and User =
        {
            Username: Username
            Color: Color
            WeekStart: DayOfWeek
            DayStart: FlukeTime
            SessionDuration: Minute
            SessionBreakDuration: Minute
        }


    and [<RequireQualifiedAccess>] Language =
        | English
        | Portuguese

    and [<RequireQualifiedAccess>] TaskInteraction =
        | Attachment of attachment: Attachment
        | Archive
        | Session of session: Session
        | Sort of top: TaskId option * bottom: TaskId option

    and Session = Session of start: FlukeDateTime

    and [<RequireQualifiedAccess>] CellInteraction =
        | Attachment of attachment: Attachment
        | StatusChange of cellStatusChange: CellStatusChange

    and [<RequireQualifiedAccess>] CellStatusChange =
        | Postpone of until: FlukeTime option
        | Complete
        | Dismiss
        | Schedule

    and AttachmentId = AttachmentId of guid: TicksGuid

    and AttachmentId with
        static member inline NewId () = AttachmentId (Guid.newTicksGuid ())
        static member inline Value (AttachmentId guid) = guid
        static member inline Default = AttachmentId Guid.Empty

    and Year with
        static member inline Value (Year year) = year



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
        static member inline Stringify
            {
                Date = date
                Time = time
                Second = Second second
            }
            =
            $"{date |> FlukeDate.Stringify} {time |> FlukeTime.Stringify}:%02d{second}"

        static member inline DateTime dateTime =
            let {
                    Date = {
                               Year = Year year
                               Month = month
                               Day = Day day
                           }
                    Time = {
                               Hour = Hour hour
                               Minute = Minute minute
                           }
                    Second = Second second
                } =
                dateTime

            DateTime (year, int month, day, int hour, int minute, second)

        static member inline GreaterEqualThan (dayStart: FlukeTime) (date: FlukeDate) time position =
            let testingAfterMidnight = dayStart |> FlukeTime.GreaterEqualThan time

            let currentlyBeforeMidnight =
                position.Time
                |> FlukeTime.GreaterEqualThan dayStart

            let newDate =
                if testingAfterMidnight
                   && currentlyBeforeMidnight
                   && dayStart <> time then
                    (date |> FlukeDate.DateTime).AddDays 1.
                    |> FlukeDate.FromDateTime
                else
                    date

            let dateToCompare: FlukeDateTime =
                {
                    Date = newDate
                    Time = time
                    Second = Second 0
                }

            (position |> FlukeDateTime.DateTime)
            >= (dateToCompare |> FlukeDateTime.DateTime)


        static member inline Create (date, time, second) : FlukeDateTime =
            {
                Date = date
                Time = time
                Second = second
            }

        static member inline Create (year, month, day, hour, minute, second) =
            FlukeDateTime.Create (FlukeDate.Create year month day, FlukeTime.Create hour minute, second)

        static member inline FromDateTime (date: DateTime) : FlukeDateTime =
            FlukeDateTime.Create (FlukeDate.FromDateTime date, FlukeTime.FromDateTime date, Second date.Second)


    and Attachment with
        static member inline Stringify attachment =
            match attachment with
            | Attachment.Comment (Comment.Comment comment) ->
                if comment.Length > 60 then
                    $"{comment |> String.substring 0 60}..."
                else
                    comment
            | Attachment.Image fileId -> $"Image ID: {fileId |> FileId.Value}"
            | attachment -> $"<attachment {attachment}>"

    let inline (|BeforeToday|Today|AfterToday|)
        (
            dayStart: FlukeTime,
            position: FlukeDateTime,
            referenceDay: FlukeDate
        ) =
        let dateStart =
            FlukeDateTime.Create (referenceDay, dayStart, Second 0)
            |> FlukeDateTime.DateTime

        let dateEnd = dateStart.AddDays 1.

        match position |> FlukeDateTime.DateTime with
        | position when position >=< (dateStart, dateEnd) -> Today
        | position when dateStart < position -> BeforeToday
        | _ -> AfterToday

    let inline (|StartOfMonth|StartOfWeek|NormalDay|) (weekStart, date) =
        match date with
        | { Day = Day 1 } -> StartOfMonth
        | date when (date |> FlukeDate.DateTime).DayOfWeek = weekStart -> StartOfWeek
        | _ -> NormalDay

    let inline isToday dayStart position date =
        match (dayStart, position, date) with
        | Today -> true
        | _ -> false

    let inline getReferenceDay dayStart position =
        match isToday dayStart position position.Date with
        | true -> position.Date
        | false ->
            (position.Date |> FlukeDate.DateTime).AddDays -1.
            |> FlukeDate.FromDateTime



    module Temp =

        [<RequireQualifiedAccess>]
        type AttachmentInteraction_ =
            // ?
            | Pin_

        [<RequireQualifiedAccess>]
        type DayInteraction_ =
            // ?
            | Holiday_
