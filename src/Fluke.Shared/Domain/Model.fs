namespace Fluke.Shared.Domain

open System
open Fluke.Shared
open FsCore


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

    and Area = { Name: AreaName }

    and AreaName = AreaName of name: string

    and Project = { Name: ProjectName; Area: Area }

    and ProjectName = ProjectName of name: string

    and Resource = { Name: ResourceName }

    and ResourceName = ResourceName of name: string

    and [<RequireQualifiedAccess>] InformationName =
        | Area of name: AreaName
        | Project of areaName: AreaName * name: ProjectName
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

    and Minute = Minute of int

    and [<Generator.Fields "Domain">] FlukeTime = { Hour: Hour; Minute: Minute }

    and Hour = Hour of int

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

    and Day = Day of day: int

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
        static member Name information =
            match information with
            | Project {
                          Name = name
                          Area = { Name = areaName }
                      } -> InformationName.Project (areaName, name)
            | Area { Name = name } -> InformationName.Area name
            | Resource { Name = name } -> InformationName.Resource name

    and Scheduling with
        static member inline Label scheduling =
            match scheduling with
            | Manual WithoutSuggestion -> "Manual"
            | Manual WithSuggestion -> "Suggested"
            | Recurrency (Offset (Days n)) -> $"""Every {if n > 1 then $"{n} " else ""}day{if n > 1 then "s" else ""}"""
            | Recurrency (Offset (Weeks n)) ->
                $"""Every {if n > 1 then $"{n} " else ""}week{if n > 1 then "s" else ""}"""
            | Recurrency (Offset (Months n)) ->
                $"""Every {if n > 1 then $"{n} " else ""}month{if n > 1 then "s" else ""}"""
            | Recurrency (Fixed fixedRecurrencyList) ->
                fixedRecurrencyList
                |> List.sort
                |> List.map
                    (function
                    | Weekly x -> $"Every {Enum.name x}"
                    | Monthly (Day dayNumber) -> $"Every {dayNumber}"
                    | Yearly (Day dayNumber, month) -> $"Every {Enum.name month} {dayNumber}")
                |> String.concat ", "

    and InformationName with
        static member inline Value informationName =
            match informationName with
            | Project (AreaName (String.Valid areaName), ProjectName (String.Valid name)) -> $"{areaName}/{name}"
            | Area (AreaName name) -> name
            | Resource (ResourceName name) -> name
            | _ -> ""

    and Area with
        static member inline Default = { Name = AreaName "" }

    and Project with
        static member inline Default: Project =
            {
                Name = ProjectName ""
                Area = Area.Default
            }

    and Resource with
        static member inline Default = { Name = ResourceName "" }


    and Minute with
        static member inline Value (Minute minute) = minute

    and TaskId with
        static member inline NewId () = TaskId (Guid.newTicksGuid ())
        static member inline Value (TaskId guid) = guid

    and Day with
        static member inline Value (Day day) = day

    and ProjectName with
        static member inline Value (ProjectName name) = name

    and AreaName with
        static member inline Value (AreaName name) = name

    and ResourceName with
        static member inline Value (ResourceName name) = name

    and TaskName with
        static member inline Value (TaskName name) = name

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
                Hour = Hour hour
                Minute = Minute minute
            }

        static member inline Stringify
            {
                Hour = Hour hour
                Minute = Minute minute
            }
            =
            $"%02d{hour}:%02d{minute}"

        static member inline FromDateTime (date: DateTime) =
            {
                Hour = Hour date.Hour
                Minute = Minute date.Minute
            }

        static member inline GreaterEqualThan time this =
            this.Hour > time.Hour
            || this.Hour = time.Hour
               && this.Minute >= time.Minute

    and RecurrencyOffset with
        static member inline DayCount =
            function
            | Days days -> days
            | Weeks weeks -> weeks * 7
            | Months months -> months * 28
