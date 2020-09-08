namespace Fluke.Shared

open System
open Suigetsu.Core

module Model =
    [<StructuredFormatDisplay "{Name}">]
    type Area = { Name: AreaName }

    and AreaName = AreaName of name: string

    [<StructuredFormatDisplay "{Area}/{Name}">]
    type Project = { Area: Area; Name: ProjectName }

    and ProjectName = ProjectName of name: string

    [<StructuredFormatDisplay "{Area}/{Name}">]
    type Resource = { Area: Area; Name: ResourceName }

    and ResourceName = ResourceName of name: string

    type Information =
        | Project of Project
        | Area of Area
        | Resource of Resource
        | Archive of Information

    and InformationName = InformationName of name: string


    [<StructuredFormatDisplay "{Year}-{Month}-{Day}">]
    type FlukeDate =
        {
            Year: Year
            Month: Month
            Day: Day
        }

        override this.ToString () =
            let (Year year) = this.Year
            let (Day day) = this.Day
            sprintf "%02i-%02i-%02i" year (int this.Month) day

    and Year = Year of int

    and Month =
        | January = 1
        | February = 2
        | March = 3
        | April = 4
        | May = 5
        | June = 6
        | July = 7
        | August = 8
        | September = 9
        | October = 10
        | November = 11
        | December = 12

    and Day = Day of int


    [<StructuredFormatDisplay "{Hour}h{Minute}m">]
    type FlukeTime =
        {
            Hour: Hour
            Minute: Minute
        }

        override this.ToString () =
            let (Hour hour) = this.Hour
            let (Minute minute) = this.Minute
            sprintf "%02f:%02f" hour minute

    and Hour = Hour of float

    and Minute = Minute of float



    [<StructuredFormatDisplay "{Date} {Time}">]
    type FlukeDateTime =
        {
            Date: FlukeDate
            Time: FlukeTime
        }

        override this.ToString () = sprintf "%A %A" this.Date this.Time


    type Task =
        {
            Name: TaskName
            Information: Information
            Scheduling: Scheduling
            PendingAfter: FlukeTime option
            MissedAfter: FlukeTime option
            Priority: Priority option
            Duration: Minute option
        }

    and TaskName = TaskName of name: string

    and Scheduling =
        | Manual of ManualScheduling
        | Recurrency of Recurrency

    and ManualScheduling =
        | WithSuggestion
        | WithoutSuggestion

    and Recurrency =
        | Offset of RecurrencyOffset
        | Fixed of FixedRecurrency list

    and FixedRecurrency =
        | Weekly of dayOfWeek: DayOfWeek
        | Monthly of day: Day
        | Yearly of day: Day * month: Month

    and RecurrencyOffset =
        | Days of length: int
        | Weeks of length: int
        | Months of length: int

    and Priority =
        | Low1
        | Low2
        | Low3
        | Medium4
        | Medium5
        | Medium6
        | High7
        | High8
        | High9
        | Critical10

        member this.Value =
            match this with
            | Low1 -> 1
            | Low2 -> 2
            | Low3 -> 3
            | Medium4 -> 4
            | Medium5 -> 5
            | Medium6 -> 6
            | High7 -> 7
            | High8 -> 8
            | High9 -> 9
            | Critical10 -> 10


    type User =
        {
            Username: string
            Color: UserColor
            WeekStart: DayOfWeek
            DayStart: FlukeTime
            SessionLength: Minute
            SessionBreakLength: Minute
        }

    and [<RequireQualifiedAccess>] UserColor =
        | Black
        | Pink
        | Blue

    type Comment = Comment of comment: string

    // Link: Auto:[Title, Favicon, Screenshot]
    // Image: Embed
    [<RequireQualifiedAccess>]
    type Attachment =
        | Comment of user: User * comment: Comment
        | Link
        | Video
        | Image
        | Attachment of attachment: Attachment

    [<RequireQualifiedAccess>]
    type AttachmentInteraction_ =
        // ?
        | Pin_

    [<RequireQualifiedAccess>]
    type DayInteraction_ =
        // ?
        | Holiday_

    type DateId = DateId of referenceDay: FlukeDate

    type CellAddress = { Task: Task; DateId: DateId }

    [<RequireQualifiedAccess>]
    type Interaction =
        | Information of information: Information * interaction: InformationInteraction
        | Task of task: Task * interaction: TaskInteraction
        | Cell of cellAddress: CellAddress * interaction: CellInteraction

    and [<RequireQualifiedAccess>] InformationInteraction =
        | Attachment of attachment: Attachment
        | Sort of top: Information option * bottom: Information option

    and [<RequireQualifiedAccess>] TaskInteraction =
        | Attachment of attachment: Attachment
        | Archive
        | Session of session: TaskSession
        | Sort of top: Task option * bottom: Task option

    and TaskSession = TaskSession of start: FlukeDateTime * duration: Minute * breakDuration: Minute

    and [<RequireQualifiedAccess>] CellInteraction =
        | Attachment of attachment: Attachment
        | StatusChange of cellStatusChange: CellStatusChange

    and [<RequireQualifiedAccess>] CellStatusChange =
        | Postpone of until: FlukeTime option
        | Complete
        | Dismiss
        | Schedule

    type UserInteraction = UserInteraction of user: User * moment: FlukeDateTime * interaction: Interaction

    type Cell = Cell of address: CellAddress * status: CellStatus

    and CellStatus =
        | Disabled
        | Suggested
        | Pending
        | Missed
        | MissedToday
        | UserStatus of user: User * status: ManualCellStatus

    and ManualCellStatus =
        | Postponed of until: FlukeTime option
        | Completed
        | Dismissed
        | ManualPending


















    module State =
        type InformationState =
            {
                Information: Information
                Attachments: Attachment list
                SortList: (Information option * Information option) list
            }


        type CellState =
            {
                User: User
                Status: CellStatus
                Attachments: Attachment list
                Sessions: TaskSession list
            }

        type TaskState =
            {
                Task: Task
                Sessions: TaskSession list
                Attachments: Attachment list
                SortList: (Task option * Task option) list
                CellStateMap: Map<DateId, CellState>
                InformationMap: Map<Information, unit>
            }


        type TreeState =
            {
                Id: TreeId
                Name: TreeName
                Owner: User
                SharedWith: TreeAccess list
                Position: FlukeDateTime option
                InformationStateMap: Map<Information, InformationState>
                TaskStateMap: Map<Task, TaskState>
            }

        and TreeId = TreeId of guid: Guid

        and TreeName = TreeName of name: string

        and [<RequireQualifiedAccess>] TreeAccess =
            | Admin of user: User
            | ReadOnly of user: User

        type TreeSelection =
            {
                InformationStateMap: Map<Information, InformationState>
                TaskStateMap: Map<Task, TaskState>
            }

        let informationListToStateMap informationList =
            informationList
            |> List.map (fun information ->
                let informationState: InformationState =
                    {
                        Information = information
                        Attachments = []
                        SortList = []
                    }

                information, informationState)
            |> Map.ofList

        let hasAccess treeState user =
            match treeState with
            | tree when tree.Owner = user -> true
            | tree ->
                tree.SharedWith
                |> List.exists (function
                    | TreeAccess.Admin dbUser -> dbUser = user
                    | TreeAccess.ReadOnly dbUser -> dbUser = user)

        let treeStateWithInteractions (userInteractionList: UserInteraction list) (treeState: TreeState) =
            let treeState =
                (treeState, userInteractionList)
                ||> List.fold (fun treeState (UserInteraction (user, moment, interaction)) ->
                        match interaction with
                        | Interaction.Information (information, informationInteraction) ->
                            let informationState =
                                treeState.InformationStateMap
                                |> Map.tryFind information
                                |> Option.defaultValue
                                    {
                                        Information = information
                                        Attachments = []
                                        SortList = []
                                    }

                            let newInformationState =
                                match informationInteraction with
                                | InformationInteraction.Attachment attachment ->
                                    let attachments =
                                        attachment :: informationState.Attachments

                                    let newInformationState =
                                        { informationState with
                                            Attachments = attachments
                                        }

                                    newInformationState

                                | InformationInteraction.Sort (top, bottom) ->
                                    let sortList =
                                        (top, bottom) :: informationState.SortList

                                    let newInformationState =
                                        { informationState with
                                            SortList = sortList
                                        }

                                    newInformationState

                            let newInformationStateMap =
                                treeState.InformationStateMap
                                |> Map.add information newInformationState

                            { treeState with
                                InformationStateMap = newInformationStateMap
                            }

                        | Interaction.Task (task, taskInteraction) ->
                            let taskState =
                                treeState.TaskStateMap
                                |> Map.tryFind task
                                |> Option.defaultValue
                                    {
                                        Task = task
                                        Sessions = []
                                        Attachments = []
                                        SortList = []
                                        CellStateMap = Map.empty
                                        InformationMap = Map.empty
                                    }

                            let newTaskState =
                                match taskInteraction with
                                | TaskInteraction.Attachment attachment ->
                                    let attachments = attachment :: taskState.Attachments

                                    let newTaskState =
                                        { taskState with
                                            Attachments = attachments
                                        }

                                    newTaskState

                                | TaskInteraction.Sort (top, bottom) ->
                                    let sortList = (top, bottom) :: taskState.SortList

                                    let newTaskState = { taskState with SortList = sortList }

                                    newTaskState
                                | TaskInteraction.Session session ->
                                    let sessions = session :: taskState.Sessions

                                    let newTaskState = { taskState with Sessions = sessions }

                                    newTaskState
                                | TaskInteraction.Archive -> taskState

                            let newTaskStateMap =
                                treeState.TaskStateMap
                                |> Map.add task newTaskState

                            { treeState with
                                TaskStateMap = newTaskStateMap
                            }
                        | Interaction.Cell ({ Task = task; DateId = dateId } as cellAddress, cellInteraction) ->
                            let taskState =
                                treeState.TaskStateMap
                                |> Map.tryFind task
                                |> Option.defaultValue
                                    {
                                        Task = task
                                        Sessions = []
                                        Attachments = []
                                        SortList = []
                                        CellStateMap = Map.empty
                                        InformationMap = Map.empty
                                    }

                            let cellState =
                                taskState.CellStateMap
                                |> Map.tryFind dateId
                                |> Option.defaultValue
                                    {
                                        User = user
                                        Status = Disabled
                                        Attachments = []
                                        Sessions = []
                                    }


                            let newCellState =
                                match cellInteraction with
                                | CellInteraction.Attachment attachment ->
                                    let attachments = attachment :: cellState.Attachments

                                    let newCellState =
                                        { cellState with
                                            Attachments = attachments
                                        }

                                    newCellState
                                | CellInteraction.StatusChange cellStatusChange ->
                                    let manualCellStatus =
                                        match cellStatusChange with
                                        | CellStatusChange.Complete -> Completed
                                        | CellStatusChange.Dismiss -> Dismissed
                                        | CellStatusChange.Postpone until -> Postponed until
                                        | CellStatusChange.Schedule -> ManualPending

                                    let newCellState =
                                        { cellState with
                                            Status = UserStatus (user, manualCellStatus)
                                        }

                                    newCellState

                            let newTaskState =
                                { taskState with
                                    CellStateMap =
                                        taskState.CellStateMap
                                        |> Map.add dateId newCellState
                                }

                            let newTaskStateMap =
                                treeState.TaskStateMap
                                |> Map.add task newTaskState

                            { treeState with
                                TaskStateMap = newTaskStateMap
                            })

            treeState




















    type Area with

        static member inline Default = { Name = AreaName "<null>" }

    type Project with

        static member inline Default =
            {
                Name = ProjectName "<null>"
                Area = Area.Default
            }

    type Resource with

        static member inline Default =
            {
                Name = ResourceName "<null>"
                Area = Area.Default
            }

    type FlukeDate with

        member inline this.DateTime =
            let Year year, Day day = this.Year, this.Day
            DateTime (year, int this.Month, day, 12, 0, 0)

        static member inline Create year month day =
            {
                Year = Year year
                Month = month
                Day = Day day
            }

        static member inline FromDateTime (date: DateTime) =
            {
                Year = Year date.Year
                Month = Enum.Parse (typeof<Month>, string date.Month) :?> Month
                Day = Day date.Day
            }
        static member inline MinValue = FlukeDate.FromDateTime DateTime.MinValue


    type FlukeTime with

        static member Create hour minute =
            {
                Hour = Hour (float hour)
                Minute = Minute (float minute)
            }

        static member inline FromDateTime (date: DateTime) =
            {
                Hour = float date.Hour |> Hour
                Minute = float date.Minute |> Minute
            }

        member inline this.GreaterEqualThan time =
            this.Hour > time.Hour
            || this.Hour = time.Hour
               && this.Minute >= time.Minute


    type FlukeDateTime with

        member inline this.DateTime =
            let Year year, Day day, Hour hour, Minute minute =
                this.Date.Year, this.Date.Day, this.Time.Hour, this.Time.Minute

            DateTime (year, int this.Date.Month, day, int hour, int minute, 0)

        member inline this.GreaterEqualThan (dayStart: FlukeTime) (DateId referenceDay) time =
            let testingAfterMidnight = dayStart.GreaterEqualThan time
            let currentlyBeforeMidnight = this.Time.GreaterEqualThan dayStart

            let newDate =
                if testingAfterMidnight && currentlyBeforeMidnight then
                    referenceDay.DateTime.AddDays 1.
                    |> FlukeDate.FromDateTime
                else
                    referenceDay

            let dateToCompare = { Date = newDate; Time = time }

            this.DateTime >= dateToCompare.DateTime

        static member inline FromDateTime (date: DateTime) =
            {
                Date = FlukeDate.FromDateTime date
                Time = FlukeTime.FromDateTime date
            }

        static member inline Create year month day hour minute =
            {
                Date = FlukeDate.Create year month day
                Time = FlukeTime.Create hour minute
            }



    type Information with

        member this.Name =
            match this with
            | Project { Name = ProjectName name } -> InformationName name
            | Area { Name = AreaName name } -> InformationName name
            | Resource { Name = ResourceName name } -> InformationName name
            | Archive information ->
                let (InformationName name) = information.Name
                sprintf "[%s]" name |> InformationName

        member this.KindName =
            match this with
            | Project _ -> "projects"
            | Area _ -> "areas"
            | Resource _ -> "resources"
            | Archive _ -> "archives"

        member this.Order =
            match this with
            | Project _ -> 1
            | Area _ -> 2
            | Resource _ -> 3
            | Archive _ -> 4



    type Task with

        static member inline Default =
            {
                Name = TaskName "<null>"
                Information = Area Area.Default
                PendingAfter = None
                MissedAfter = None
                Scheduling = Manual WithoutSuggestion
                Priority = None
                Duration = None
            }


    //    let ofTaskSession =
//        fun (TaskInteraction (start, duration, breakDuration)) -> start, duration, breakDuration
    open State

    type TreeState with
        static member Create (id, name, owner) =
            {
                Id = id
                Name = name
                Owner = owner
                SharedWith = []
                Position = None
                InformationStateMap = Map.empty
                TaskStateMap = Map.empty
            }


    let ofDateId =
        fun (DateId referenceDay) -> referenceDay

    let ofAttachmentComment =
        function
        | Attachment.Comment (user, comment) -> Some (user, comment)
        | _ -> None

    //    let ofUserComment =
//        fun (UserComment (user, comment)) -> user, comment
//
//    let ofTaskComment =
//        fun (TaskComment (task, userComment)) -> task, userComment
//
//    let ofCellComment =
//        fun (CellComment (task, moment, userComment)) -> task, moment, userComment

    //    let ofCellSession =
//        fun (CellSession (task, start, duration)) -> task, start, duration


    //    let ofTaskStatusEntry =
//        fun (TaskStatusEntry (user, moment, manualCellStatus)) -> user, moment, manualCellStatus


    let (|BeforeToday|Today|AfterToday|) (dayStart: FlukeTime, position: FlukeDateTime, DateId referenceDay) =
        let dateStart =
            { Date = referenceDay; Time = dayStart }.DateTime

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


//    let createTaskStatusEntries task cellStatusEntries =
//        cellStatusEntries
//        |> List.filter (fun (CellStatusEntry (user, task', moment, manualCellStatus)) -> task' = task)
//        |> List.map (fun (CellStatusEntry (user, task', moment, entries)) -> TaskStatusEntry (user, moment, entries))
//        |> List.sortBy (fun (TaskStatusEntry (user, date, _)) -> date)
