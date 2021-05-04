namespace Fluke.Shared.Domain

open System


module Model =
#if !FABLE_COMPILER
    open Myriad.Plugins
#else
    module Generator =
        type DuCases (_configGroup: string) =
            inherit Attribute ()

        type Fields (_configGroup: string) =
            inherit Attribute ()
#endif

    [<Generator.DuCases "Domain">]
    type Information =
        | Project of project: Project
        | Area of area: Area
        | Resource of resource: Resource
        | Archive of information: Information

    and Area = { Id: AreaId; Name: AreaName }

    and AreaId = AreaId of guid: Guid

    and AreaName = AreaName of name: string

    and Project =
        {
            Id: ProjectId
            Area: Area
            Name: ProjectName
        }

    and ProjectId = ProjectId of guid: Guid

    and ProjectName = ProjectName of name: string

    and Resource =
        {
            Id: ResourceId
            Area: Area
            Name: ResourceName
        }

    and ResourceId = ResourceId of guid: Guid

    and ResourceName = ResourceName of name: string

    and [<RequireQualifiedAccess>] InformationId =
        | Area of id: AreaId
        | Project of id: ProjectId
        | Resource of id: ResourceId

    and [<RequireQualifiedAccess>] InformationName =
        | Area of name: AreaName
        | Project of name: ProjectName
        | Resource of name: ResourceName

    and Task =
        {
            Id: TaskId
            Name: TaskName
            Information: Information
            Duration: Minute option
            PendingAfter: FlukeTime option
            MissedAfter: FlukeTime option
            Scheduling: Scheduling
            Priority: Priority option
        }

    and TaskId = TaskId of guid: Guid

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
        static member Id information =
            match information with
            | Project { Id = id } -> InformationId.Project id
            | Area { Id = id } -> InformationId.Area id
            | Resource { Id = id } -> InformationId.Resource id
            | Archive information -> information |> Information.Id

        static member Name information =
            match information with
            | Project { Name = name } -> InformationName.Project name
            | Area { Name = name } -> InformationName.Area name
            | Resource { Name = name } -> InformationName.Resource name
            | Archive information -> information |> Information.Name

    and InformationName with
        static member Value informationName =
            match informationName with
            | Project (ProjectName name) -> name
            | Area (AreaName name) -> name
            | Resource (ResourceName name) -> name

    and Area with
        static member inline Default =
            {
                Id = AreaId Guid.Empty
                Name = AreaName ""
            }

    and Project with
        static member inline Default : Project =
            {
                Id = ProjectId Guid.Empty
                Name = ProjectName ""
                Area = Area.Default
            }

    and Resource with
        static member inline Default =
            {
                Id = ResourceId Guid.Empty
                Name = ResourceName ""
                Area = Area.Default
            }

    type Task with
        static member inline Default =
            {
                Id = TaskId Guid.Empty
                Name = TaskName ""
                Information = Area Area.Default
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
            let {
                    Hour = Hour hour
                    Minute = Minute minute
                } =
                this

            $"%02.0f{hour}:%02.0f{minute}"

        static member inline FromDateTime (date: DateTime) =
            {
                Hour = float date.Hour |> Hour
                Minute = float date.Minute |> Minute
            }

        member inline this.GreaterEqualThan time =
            this.Hour > time.Hour
            || this.Hour = time.Hour
               && this.Minute >= time.Minute
