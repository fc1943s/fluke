namespace Fluke.Shared

open System

module Templates =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State


    type DslTask =
        | DslTaskComment of comment: string
        | DslSession of start: FlukeDateTime
        | DslPriority of priority: Priority
        | DslInformationReferenceToggle of information: Information
        | DslStatusEntry of date: FlukeDate * manualCellStatus: ManualCellStatus
        | DslCellComment of date: FlukeDate * comment: string
        | DslTaskSet of taskSet: DslTaskSet
        | DslTaskSort of top: TaskName option * bottom: TaskName option

    and DslTaskSet =
        | DslSetScheduling of scheduling: Scheduling * start: FlukeDate option
        | DslSetPendingAfter of start: FlukeTime
        | DslSetMissedAfter of start: FlukeTime
        | DslSetDuration of duration: int

    type DslTemplate =
        {
            Events: DslTask list
            Expected: (FlukeDate * CellStatus) list
            Position: FlukeDateTime
            Task: Task
        }

    let getUserFluke () =
        {
            Username = Username "fluke"
            Color = UserColor.Black
            WeekStart = DayOfWeek.Sunday
            DayStart = FlukeTime.Create 12 00
            SessionLength = Minute 25.
            SessionBreakLength = Minute 5.
        }

    let getTree () =
        let userFluke = getUserFluke ()
        [
            "Lane Rendering",
            [
                "Manual",
                [
                    "Empty manual task",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Manual WithoutSuggestion
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 11
                                Time = userFluke.DayStart
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.March 9, Disabled
                                FlukeDate.Create 2020 Month.March 10, Disabled
                                FlukeDate.Create 2020 Month.March 11, Suggested
                                FlukeDate.Create 2020 Month.March 12, Disabled
                                FlukeDate.Create 2020 Month.March 13, Disabled
                            ]
                        Events = []
                    }


                    "ManualPending task scheduled for today after missing",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Manual WithoutSuggestion
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 11
                                Time = userFluke.DayStart
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.March 8, Disabled
                                FlukeDate.Create 2020 Month.March 9, UserStatus (userFluke, ManualPending)
                                FlukeDate.Create 2020 Month.March 10, Missed
                                FlukeDate.Create 2020 Month.March 11, Pending
                                FlukeDate.Create 2020 Month.March 12, Disabled
                                FlukeDate.Create 2020 Month.March 13, Disabled
                            ]
                        Events =
                            [
                                DslStatusEntry (FlukeDate.Create 2020 Month.March 9, ManualPending)
                            ]
                    }


                    "Manual Suggested task Suggested before PendingAfter",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Manual WithSuggestion
                                PendingAfter = FlukeTime.Create 20 00 |> Some
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 10
                                Time = FlukeTime.Create 19 30
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.March 09, Suggested
                                FlukeDate.Create 2020 Month.March 10, Suggested
                                FlukeDate.Create 2020 Month.March 11, Suggested
                            ]
                        Events = []
                    }


                    "Manual Suggested task Pending after PendingAfter",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Manual WithSuggestion
                                PendingAfter = FlukeTime.Create 20 00 |> Some
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 10
                                Time = FlukeTime.Create 21 00
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.March 09, Suggested
                                FlukeDate.Create 2020 Month.March 10, Pending
                                FlukeDate.Create 2020 Month.March 11, Suggested
                            ]
                        Events = []
                    }


                    "Manual Suggested task: Missed ManualPending propagates until today",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Manual WithSuggestion
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 28
                                Time = userFluke.DayStart
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.March 25, Suggested
                                FlukeDate.Create 2020 Month.March 26, UserStatus (userFluke, ManualPending)
                                FlukeDate.Create 2020 Month.March 27, Missed
                                FlukeDate.Create 2020 Month.March 28, Pending
                                FlukeDate.Create 2020 Month.March 29, Suggested
                                FlukeDate.Create 2020 Month.March 30, UserStatus (userFluke, ManualPending)
                                FlukeDate.Create 2020 Month.March 31, Suggested
                            ]
                        Events =
                            [
                                DslStatusEntry (FlukeDate.Create 2020 Month.March 26, ManualPending)
                                DslStatusEntry (FlukeDate.Create 2020 Month.March 30, ManualPending)
                            ]
                    }


                    "Manual Suggested task: Suggested mode restored after completing a forgotten ManualPending event",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Manual WithSuggestion
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 28
                                Time = userFluke.DayStart
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.March 24, Suggested
                                FlukeDate.Create 2020 Month.March 25, UserStatus (userFluke, ManualPending)
                                FlukeDate.Create 2020 Month.March 26, UserStatus (userFluke, Completed)
                                FlukeDate.Create 2020 Month.March 27, Suggested
                                FlukeDate.Create 2020 Month.March 28, Suggested
                                FlukeDate.Create 2020 Month.March 29, Suggested
                            ]
                        Events =
                            [
                                DslStatusEntry (FlukeDate.Create 2020 Month.March 25, ManualPending)
                                DslStatusEntry (FlukeDate.Create 2020 Month.March 26, Completed)
                            ]
                    }


                    "Manual Suggested task: Pending today after missing a ManualPending event",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Manual WithSuggestion
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 28
                                Time = userFluke.DayStart
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.March 24, Suggested
                                FlukeDate.Create 2020 Month.March 25, UserStatus (userFluke, ManualPending)
                                FlukeDate.Create 2020 Month.March 26, Missed
                                FlukeDate.Create 2020 Month.March 27, Missed
                                FlukeDate.Create 2020 Month.March 28, Pending
                                FlukeDate.Create 2020 Month.March 29, Suggested
                            ]
                        Events =
                            [
                                DslStatusEntry (FlukeDate.Create 2020 Month.March 25, ManualPending)
                            ]
                    }
                ]
                "Recurrency Offset",
                [
                    "Start scheduling today without any events",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Recurrency (Offset (Days 2))
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 9
                                Time = userFluke.DayStart
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.March 7, Disabled
                                FlukeDate.Create 2020 Month.March 8, Disabled
                                FlukeDate.Create 2020 Month.March 9, Pending
                                FlukeDate.Create 2020 Month.March 10, Disabled
                                FlukeDate.Create 2020 Month.March 11, Pending
                                FlukeDate.Create 2020 Month.March 12, Disabled
                            ]
                        Events = []
                    }


                    "Disabled today after a Completed event yesterday",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Recurrency (Offset (Days 3))
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 9
                                Time = userFluke.DayStart
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.March 8, UserStatus (userFluke, Completed)
                                FlukeDate.Create 2020 Month.March 9, Disabled
                                FlukeDate.Create 2020 Month.March 10, Disabled
                                FlukeDate.Create 2020 Month.March 11, Pending
                                FlukeDate.Create 2020 Month.March 12, Disabled
                                FlukeDate.Create 2020 Month.March 13, Disabled
                                FlukeDate.Create 2020 Month.March 14, Pending
                                FlukeDate.Create 2020 Month.March 15, Disabled
                            ]
                        Events =
                            [
                                DslStatusEntry (FlukeDate.Create 2020 Month.March 8, Completed)
                            ]
                    }


                    "Postponing today should schedule for tomorrow",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Recurrency (Offset (Days 2))
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 10
                                Time = userFluke.DayStart
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.March 9, Disabled
                                FlukeDate.Create 2020 Month.March 10, UserStatus (userFluke, Postponed None)
                                FlukeDate.Create 2020 Month.March 11, Pending
                                FlukeDate.Create 2020 Month.March 12, Disabled
                                FlukeDate.Create 2020 Month.March 13, Pending
                            ]
                        Events =
                            [
                                DslStatusEntry (FlukeDate.Create 2020 Month.March 10, Postponed None)
                            ]
                    }


                    "Postponing today should schedule for tomorrow with PendingAfter",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Recurrency (Offset (Days 2))
                                PendingAfter = FlukeTime.Create 03 00 |> Some
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 10
                                Time = userFluke.DayStart
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.March 9, Disabled
                                FlukeDate.Create 2020 Month.March 10, UserStatus (userFluke, Postponed None)
                                FlukeDate.Create 2020 Month.March 11, Pending
                                FlukeDate.Create 2020 Month.March 12, Disabled
                                FlukeDate.Create 2020 Month.March 13, Pending
                            ]
                        Events =
                            [
                                DslStatusEntry (FlukeDate.Create 2020 Month.March 10, Postponed None)
                            ]
                    }


                    "(Postponed None) yesterday schedules for today",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Recurrency (Offset (Days 2))
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 11
                                Time = userFluke.DayStart
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.March 9, Disabled
                                FlukeDate.Create 2020 Month.March 10, UserStatus (userFluke, Postponed None)
                                FlukeDate.Create 2020 Month.March 11, Pending
                                FlukeDate.Create 2020 Month.March 12, Disabled
                                FlukeDate.Create 2020 Month.March 13, Pending
                            ]
                        Events =
                            [
                                DslStatusEntry (FlukeDate.Create 2020 Month.March 10, Postponed None)
                            ]
                    }

                    "Pending today after missing yesterday, then resetting the schedule with a future Completed event",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Recurrency (Offset (Days 2))
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 11
                                Time = userFluke.DayStart
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.March 7, Disabled
                                FlukeDate.Create 2020 Month.March 8, UserStatus (userFluke, Completed)
                                FlukeDate.Create 2020 Month.March 9, Disabled
                                FlukeDate.Create 2020 Month.March 10, Missed
                                FlukeDate.Create 2020 Month.March 11, Pending
                                FlukeDate.Create 2020 Month.March 12, UserStatus (userFluke, Completed)
                                FlukeDate.Create 2020 Month.March 13, Disabled
                                FlukeDate.Create 2020 Month.March 14, Pending
                            ]
                        Events =
                            [
                                DslStatusEntry (FlukeDate.Create 2020 Month.March 8, Completed)
                                DslStatusEntry (FlukeDate.Create 2020 Month.March 12, Completed)
                            ]
                    }


                    "Recurring task only Suggested before PendingAfter",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Recurrency (Offset (Days 1))
                                PendingAfter = FlukeTime.Create 20 00 |> Some
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 10
                                Time = FlukeTime.Create 19 30
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.March 9, Disabled
                                FlukeDate.Create 2020 Month.March 10, Suggested
                                FlukeDate.Create 2020 Month.March 11, Pending
                            ]
                        Events = []
                    }


                    "Recurring task Pending after PendingAfter",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Recurrency (Offset (Days 1))
                                PendingAfter = FlukeTime.Create 20 00 |> Some
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 10
                                Time = FlukeTime.Create 21 00
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.March 9, Disabled
                                FlukeDate.Create 2020 Month.March 10, Pending
                                FlukeDate.Create 2020 Month.March 11, Pending
                            ]
                        Events = []
                    }


                    "Recurrency for the next days should work normally while today is still optional/suggested (before PendingAfter)",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Recurrency (Offset (Days 2))
                                PendingAfter = FlukeTime.Create 18 00 |> Some
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 27
                                Time = FlukeTime.Create 17 00
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.March 25, Disabled
                                FlukeDate.Create 2020 Month.March 26, Disabled
                                FlukeDate.Create 2020 Month.March 27, Suggested
                                FlukeDate.Create 2020 Month.March 28, Disabled
                                FlukeDate.Create 2020 Month.March 29, Pending
                                FlukeDate.Create 2020 Month.March 29, Disabled
                            ]
                        Events = []
                    }


                    "Reset counting after a future ManualPending event",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Recurrency (Offset (Days 3))
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 28
                                Time = userFluke.DayStart
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.March 27, Disabled
                                FlukeDate.Create 2020 Month.March 28, Pending
                                FlukeDate.Create 2020 Month.March 29, Disabled
                                FlukeDate.Create 2020 Month.March 30, UserStatus (userFluke, ManualPending)
                                FlukeDate.Create 2020 Month.March 31, UserStatus (userFluke, ManualPending)
                                FlukeDate.Create 2020 Month.April 01, Disabled
                                FlukeDate.Create 2020 Month.April 02, Disabled
                                FlukeDate.Create 2020 Month.April 03, Pending
                            ]
                        Events =
                            [
                                DslStatusEntry (FlukeDate.Create 2020 Month.March 30, ManualPending)
                                DslStatusEntry (FlukeDate.Create 2020 Month.March 31, ManualPending)
                            ]
                    }
                ]
                "Recurrency Fixed",
                [
                    "Weekly task, pending today, initialized by past completion",
                    {
                        Task =
                            { Task.Default with
                                Scheduling =
                                    Recurrency
                                        (Fixed
                                            [
                                                Weekly DayOfWeek.Saturday
                                            ])
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 21
                                Time = userFluke.DayStart
                            }
                        Expected =
                            [
                                for d in 13 .. 29 do
                                    FlukeDate.Create 2020 Month.March d,
                                    match d with
                                    | 14 -> UserStatus (userFluke, Completed)
                                    | 21
                                    | 28 -> Pending
                                    | _ -> Disabled
                            ]
                        Events =
                            [
                                DslStatusEntry (FlukeDate.Create 2020 Month.March 14, Completed)
                            ]
                    }


                    "Weekly task, missed until today, initialized by past completion",
                    {
                        Task =
                            { Task.Default with
                                Scheduling =
                                    Recurrency
                                        (Fixed
                                            [
                                                Weekly DayOfWeek.Wednesday
                                            ])
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 20
                                Time = userFluke.DayStart
                            }
                        Expected =
                            [
                                for d in 10 .. 26 do
                                    FlukeDate.Create 2020 Month.March d,
                                    match d with
                                    | 13 -> UserStatus (userFluke, Completed)
                                    | 18
                                    | 19 -> Missed
                                    | 20
                                    | 25 -> Pending
                                    | _ -> Disabled
                            ]
                        Events =
                            [
                                DslStatusEntry (FlukeDate.Create 2020 Month.March 13, Completed)
                            ]
                    }


                    "Weekly task, (Postponed None) then missed until today, pending tomorrow",
                    {
                        Task =
                            { Task.Default with
                                Scheduling =
                                    Recurrency
                                        (Fixed
                                            [
                                                Weekly DayOfWeek.Saturday
                                            ])
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 20
                                Time = userFluke.DayStart
                            }
                        Expected =
                            [
                                for d in 13 .. 29 do
                                    FlukeDate.Create 2020 Month.March d,
                                    match d with
                                    | 18 -> UserStatus (userFluke, Postponed None)
                                    | 19 -> Missed
                                    | 20
                                    | 21
                                    | 28 -> Pending
                                    | _ -> Disabled
                            ]
                        Events =
                            [
                                DslStatusEntry (FlukeDate.Create 2020 Month.March 18, Postponed None)
                            ]
                    }


                    "Weekly task, without past events, pending in a few days",
                    {
                        Task =
                            { Task.Default with
                                Scheduling =
                                    Recurrency
                                        (Fixed
                                            [
                                                Weekly DayOfWeek.Wednesday
                                            ])
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 20
                                Time = userFluke.DayStart
                            }
                        Expected =
                            [
                                for d in 17 .. 26 do
                                    FlukeDate.Create 2020 Month.March d,
                                    match d with
                                    | 25 -> Pending
                                    | _ -> Disabled
                            ]
                        Events = []
                    }


                    "Fixed weekly task, without past events, pending tomorrow",
                    {
                        Task =
                            { Task.Default with
                                Scheduling =
                                    Recurrency
                                        (Fixed
                                            [
                                                Weekly DayOfWeek.Saturday
                                            ])
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 20
                                Time = userFluke.DayStart
                            }
                        Expected =
                            [
                                for d in 13 .. 29 do
                                    FlukeDate.Create 2020 Month.March d,
                                    match d with
                                    | 21
                                    | 28 -> Pending
                                    | _ -> Disabled
                            ]
                        Events = []
                    }


                    "Fixed weekly task only Suggested before PendingAfter",
                    {
                        Task =
                            { Task.Default with
                                Scheduling =
                                    Recurrency
                                        (Fixed [
                                            Weekly DayOfWeek.Monday
                                            Weekly DayOfWeek.Tuesday
                                            Weekly DayOfWeek.Wednesday
                                            Weekly DayOfWeek.Thursday
                                            Weekly DayOfWeek.Friday
                                         ])
                                PendingAfter = Some (FlukeTime.Create 19 00)
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.August 26
                                Time = userFluke.DayStart
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.August 25, Disabled
                                FlukeDate.Create 2020 Month.August 26, Suggested
                                FlukeDate.Create 2020 Month.August 27, Pending
                            ]
                        Events = []
                    }
                ]
                "Postponed Until",
                [
                    "Postponed until later",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Recurrency (Offset (Days 1))
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 10
                                Time = userFluke.DayStart
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.March 09, Disabled
                                FlukeDate.Create 2020 Month.March 10,
                                UserStatus (userFluke, Postponed (Some (FlukeTime.Create 23 00)))
                                FlukeDate.Create 2020 Month.March 11, Pending
                                FlukeDate.Create 2020 Month.March 12, Pending
                            ]
                        Events =
                            [
                                DslStatusEntry
                                    (FlukeDate.Create 2020 Month.March 10, Postponed (Some (FlukeTime.Create 23 00)))
                            ]
                    }
                    "Postponed until after midnight",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Recurrency (Offset (Days 1))
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 10
                                Time = userFluke.DayStart
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.March 09, Disabled
                                FlukeDate.Create 2020 Month.March 10,
                                UserStatus (userFluke, Postponed (Some (FlukeTime.Create 01 00)))
                                FlukeDate.Create 2020 Month.March 11, Pending
                                FlukeDate.Create 2020 Month.March 12, Pending
                            ]
                        Events =
                            [
                                DslStatusEntry
                                    (FlukeDate.Create 2020 Month.March 10, Postponed (Some (FlukeTime.Create 01 00)))
                            ]
                    }
                    "Pending after expiration of Postponed (before midnight)",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Recurrency (Offset (Days 1))
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 11
                                Time = FlukeTime.Create 02 00
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.March 09, Disabled
                                FlukeDate.Create 2020 Month.March 10, Pending
                                FlukeDate.Create 2020 Month.March 11, Pending
                                FlukeDate.Create 2020 Month.March 12, Pending
                            ]
                        Events =
                            [
                                DslStatusEntry
                                    (FlukeDate.Create 2020 Month.March 10, Postponed (Some (FlukeTime.Create 23 00)))
                            ]
                    }


                    "Pending after expiration of Postponed (after midnight)",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Recurrency (Offset (Days 1))
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 11
                                Time = FlukeTime.Create 02 00
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.March 09, Disabled
                                FlukeDate.Create 2020 Month.March 10, Pending
                                FlukeDate.Create 2020 Month.March 11, Pending
                                FlukeDate.Create 2020 Month.March 12, Pending
                            ]
                        Events =
                            [
                                DslStatusEntry
                                    (FlukeDate.Create 2020 Month.March 10, Postponed (Some (FlukeTime.Create 01 00)))
                            ]
                    }


                    "Past PostponedUntil events are shown",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Recurrency (Offset (Days 1))
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 13
                                Time = FlukeTime.Create 02 00
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.March 07, Disabled
                                FlukeDate.Create 2020 Month.March 08, UserStatus (userFluke, Completed)
                                FlukeDate.Create 2020 Month.March 09, Missed
                                FlukeDate.Create 2020 Month.March 10,
                                UserStatus (userFluke, Postponed (Some (FlukeTime.Create 01 00)))
                                FlukeDate.Create 2020 Month.March 11, Missed
                                FlukeDate.Create 2020 Month.March 12, Pending
                                FlukeDate.Create 2020 Month.March 13, Pending
                            ]
                        Events =
                            [
                                DslStatusEntry (FlukeDate.Create 2020 Month.March 08, Completed)
                                DslStatusEntry
                                    (FlukeDate.Create 2020 Month.March 10, Postponed (Some (FlukeTime.Create 01 00)))
                            ]
                    }


                    "Future PostponedUntil events are shown",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Recurrency (Offset (Days 1))
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 10
                                Time = userFluke.DayStart
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.March 09, Disabled
                                FlukeDate.Create 2020 Month.March 10, Pending
                                FlukeDate.Create 2020 Month.March 11, Pending
                                FlukeDate.Create 2020 Month.March 12,
                                UserStatus (userFluke, Postponed (Some (FlukeTime.Create 13 00)))
                                FlukeDate.Create 2020 Month.March 13, Pending
                            ]
                        Events =
                            [
                                DslStatusEntry
                                    (FlukeDate.Create 2020 Month.March 12, Postponed (Some (FlukeTime.Create 13 00)))
                            ]
                    }
                ]
                "Sessions",
                [
                    "Respect dayStart on session events",
                    {
                        Task =
                            { Task.Default with
                                Scheduling = Recurrency (Offset (Days 1))
                            }
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 04
                                Time = userFluke.DayStart
                            }
                        Expected =
                            [
                                FlukeDate.Create 2020 Month.February 29, Disabled
                                FlukeDate.Create 2020 Month.March 1, Disabled
                                FlukeDate.Create 2020 Month.March 2, Disabled
                                FlukeDate.Create 2020 Month.March 3, Disabled
                                FlukeDate.Create 2020 Month.March 4, Pending
                                FlukeDate.Create 2020 Month.March 5, Pending
                                FlukeDate.Create 2020 Month.March 6, Pending
                                FlukeDate.Create 2020 Month.March 7, Pending
                                FlukeDate.Create 2020 Month.March 8, Pending
                            ]
                        Events =
                            [
                                DslSession (FlukeDateTime.Create 2020 Month.March 01 11 00)
                                DslSession (FlukeDateTime.Create 2020 Month.March 01 13 00)
                                DslSession (FlukeDateTime.Create 2020 Month.March 08 11 00)
                                DslSession (FlukeDateTime.Create 2020 Month.March 08 13 00)
                            ]
                    }
                ]
            ]
        ]

    let getTreeMap () =
        let tree = getTree ()

        tree
        |> List.collect (fun (name1, list) ->
            list
            |> List.collect (fun (name2, list) ->
                list
                |> List.map (fun (name3, dslTemplate) ->
                    let name = sprintf "%s/%s/%s" name1 name2 name3

                    let newDslTemplate =
                        { dslTemplate with
                            Task = { dslTemplate.Task with Name = TaskName name }
                        }

                    name, newDslTemplate)))
        |> Map.ofList
