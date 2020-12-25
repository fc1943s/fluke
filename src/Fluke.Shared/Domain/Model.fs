namespace Fluke.Shared.Domain

open System


module Model =
#if !FABLE_COMPILER
    open Myriad.Plugins
#else
    module Generator =
        type DuCases(_configGroup: string) =
            inherit Attribute()

        type Fields(_configGroup: string) =
            inherit Attribute()
#endif

    [<Generator.DuCases "Domain">]
    type Information =
        | Project of project: Project * tasks: Task list
        | Area of area: Area * tasks: Task list
        | Resource of resource: Resource * tasks: Task list
        | Archive of information: Information

    and Area = { Name: AreaName }

    and AreaName = AreaName of name: string

    and Project = { Name: ProjectName; Area: Area }

    and ProjectName = ProjectName of name: string

    and Resource = { Name: ResourceName; Area: Area }

    and ResourceName = ResourceName of name: string

    and Task =
        {
            Name: TaskName
            Information: Information
            Duration: Minute option
            PendingAfter: FlukeTime option
            MissedAfter: FlukeTime option
            Scheduling: Scheduling
            Priority: Priority option
        }

    and TaskName = TaskName of name: string

    and Minute = Minute of float

    and [<Generator.Fields "Domain">] FlukeTime = { Hour: Hour; Minute: Minute }

    and Hour = Hour of float

    and Scheduling =
        | Manual of ManualScheduling
        | Recurrency of Recurrency

    and ManualScheduling =
        | WithSuggestion
        | WithoutSuggestion

    and Recurrency =
        | Offset of RecurrencyOffset
        | Fixed of FixedRecurrency list

    and RecurrencyOffset =
        | Days of length: int
        | Weeks of length: int
        | Months of length: int

    and FixedRecurrency =
        | Weekly of dayOfWeek: DayOfWeek
        | Monthly of day: Day
        | Yearly of day: Day * month: Month

    and Day = Day of int

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

    and [<Generator.DuCases "Domain">] Priority =
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


    and Information with
        member this.Name =
            match this with
            | Project ({ Name = ProjectName name }, _) -> InformationName name
            | Area ({ Name = AreaName name }, _) -> InformationName name
            | Resource ({ Name = ResourceName name }, _) -> InformationName name
            | Archive information ->
                let (InformationName name) = information.Name
                sprintf "[%s]" name |> InformationName

    and InformationName = InformationName of name: string

    and Area with
        static member inline Default = { Name = AreaName "<null>" }

    and Project with
        static member inline Default: Project =
            {
                Name = ProjectName "<null>"
                Area = Area.Default
            }

    and Resource with
        static member inline Default =
            {
                Name = ResourceName "<null>"
                Area = Area.Default
            }

    type Task with
        static member inline Default =
            {
                Name = TaskName "<null>"
                Information = Area (Area.Default, [])
                PendingAfter = None
                MissedAfter = None
                Scheduling = Manual WithoutSuggestion
                Priority = None
                Duration = None
            }

    and FlukeTime with
        static member inline Create hour minute =
            {
                Hour = Hour (float hour)
                Minute = Minute (float minute)
            }

        member inline this.Stringify () =
            let { Hour = Hour hour; Minute = Minute minute } = this
            sprintf "%02.0f:%02.0f" hour minute

        static member inline FromDateTime (date: DateTime) =
            {
                Hour = float date.Hour |> Hour
                Minute = float date.Minute |> Minute
            }

        member inline this.GreaterEqualThan time =
            this.Hour > time.Hour
            || this.Hour = time.Hour
               && this.Minute >= time.Minute
