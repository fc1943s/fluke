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


    [<RequireQualifiedAccess>]
    type TemplateExpect =
        | Status of status:CellStatus
        | Session of count:int

    type TemplateTask =
        {
            Task: Task
            Events: DslTask list
            Expected: (FlukeDate * TemplateExpect list) list
        }

    type DslTemplate =
        {
            Position: FlukeDateTime
            Tasks: TemplateTask list
        }

    [<RequireQualifiedAccess>]
    type Template =
        | Single of DslTemplate
        | Multiple of DslTemplate

    let getTree user =
        [
            "Lane Sorting",
            [
                "Sort by Frequency: All task types mixed", []
            ]
            "Lane Rendering",
            [
                "Manual",
                [
                    "Empty manual task",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 11
                                Time = user.DayStart
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Manual WithoutSuggestion
                                        }
                                    Events = []
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 9,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 10,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 11,
                                            [
                                                TemplateExpect.Status Suggested
                                            ]
                                            FlukeDate.Create 2020 Month.March 12,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 13,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                        ]
                                }
                            ]
                    }


                    "ManualPending task scheduled for today after missing",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 11
                                Time = user.DayStart
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Manual WithoutSuggestion
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 9, ManualPending)
                                        ]
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 8,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 9,
                                            [
                                                TemplateExpect.Status (UserStatus (user, ManualPending))
                                            ]
                                            FlukeDate.Create 2020 Month.March 10,
                                            [
                                                TemplateExpect.Status Missed
                                            ]
                                            FlukeDate.Create 2020 Month.March 11,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 12,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 13,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                        ]
                                }
                            ]
                    }


                    "Manual Suggested task Suggested before PendingAfter",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 10
                                Time = FlukeTime.Create 19 30
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Manual WithSuggestion
                                            PendingAfter = FlukeTime.Create 20 00 |> Some
                                        }
                                    Events = []
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 09,
                                            [
                                                TemplateExpect.Status Suggested
                                            ]
                                            FlukeDate.Create 2020 Month.March 10,
                                            [
                                                TemplateExpect.Status Suggested
                                            ]
                                            FlukeDate.Create 2020 Month.March 11,
                                            [
                                                TemplateExpect.Status Suggested
                                            ]
                                        ]
                                }
                            ]
                    }


                    "Manual Suggested task Pending after PendingAfter",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 10
                                Time = FlukeTime.Create 21 00
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Manual WithSuggestion
                                            PendingAfter = FlukeTime.Create 20 00 |> Some
                                        }
                                    Events = []
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 09,
                                            [
                                                TemplateExpect.Status Suggested
                                            ]
                                            FlukeDate.Create 2020 Month.March 10,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 11,
                                            [
                                                TemplateExpect.Status Suggested
                                            ]
                                        ]
                                }
                            ]
                    }


                    "Manual Suggested task: Missed ManualPending propagates until today",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 28
                                Time = user.DayStart
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Manual WithSuggestion
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 26, ManualPending)
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 30, ManualPending)
                                        ]
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 25,
                                            [
                                                TemplateExpect.Status Suggested
                                            ]
                                            FlukeDate.Create 2020 Month.March 26,
                                            [
                                                TemplateExpect.Status (UserStatus (user, ManualPending))
                                            ]
                                            FlukeDate.Create 2020 Month.March 27,
                                            [
                                                TemplateExpect.Status Missed
                                            ]
                                            FlukeDate.Create 2020 Month.March 28,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 29,
                                            [
                                                TemplateExpect.Status Suggested
                                            ]
                                            FlukeDate.Create 2020 Month.March 30,
                                            [
                                                TemplateExpect.Status (UserStatus (user, ManualPending))
                                            ]
                                            FlukeDate.Create 2020 Month.March 31,
                                            [
                                                TemplateExpect.Status Suggested
                                            ]
                                        ]
                                }
                            ]
                    }


                    "Manual Suggested task: Suggested mode restored after completing a forgotten ManualPending event",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 28
                                Time = user.DayStart
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Manual WithSuggestion
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 25, ManualPending)
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 26, Completed)
                                        ]
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 24,
                                            [
                                                TemplateExpect.Status Suggested
                                            ]
                                            FlukeDate.Create 2020 Month.March 25,
                                            [
                                                TemplateExpect.Status (UserStatus (user, ManualPending))
                                            ]
                                            FlukeDate.Create 2020 Month.March 26,
                                            [
                                                TemplateExpect.Status (UserStatus (user, Completed))
                                            ]
                                            FlukeDate.Create 2020 Month.March 27,
                                            [
                                                TemplateExpect.Status Suggested
                                            ]
                                            FlukeDate.Create 2020 Month.March 28,
                                            [
                                                TemplateExpect.Status Suggested
                                            ]
                                            FlukeDate.Create 2020 Month.March 29,
                                            [
                                                TemplateExpect.Status Suggested
                                            ]
                                        ]
                                }
                            ]
                    }


                    "Manual Suggested task: Pending today after missing a ManualPending event",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 28
                                Time = user.DayStart
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Manual WithSuggestion
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 25, ManualPending)
                                        ]
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 24,
                                            [
                                                TemplateExpect.Status Suggested
                                            ]
                                            FlukeDate.Create 2020 Month.March 25,
                                            [
                                                TemplateExpect.Status (UserStatus (user, ManualPending))
                                            ]
                                            FlukeDate.Create 2020 Month.March 26,
                                            [
                                                TemplateExpect.Status Missed
                                            ]
                                            FlukeDate.Create 2020 Month.March 27,
                                            [
                                                TemplateExpect.Status Missed
                                            ]
                                            FlukeDate.Create 2020 Month.March 28,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 29,
                                            [
                                                TemplateExpect.Status Suggested
                                            ]
                                        ]
                                }
                            ]
                    }
                ]
                "Recurrency Offset",
                [
                    "Start scheduling today without any events",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 9
                                Time = user.DayStart
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 2))
                                        }
                                    Events = []
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 7,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 8,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 9,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 10,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 11,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 12,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                        ]
                                }
                            ]
                    }


                    "Disabled today after a Completed event yesterday",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 9
                                Time = user.DayStart
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 3))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 8, Completed)
                                        ]
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 8,
                                            [
                                                TemplateExpect.Status (UserStatus (user, Completed))
                                            ]
                                            FlukeDate.Create 2020 Month.March 9,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 10,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 11,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 12,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 13,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 14,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 15,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                        ]
                                }
                            ]
                    }


                    "Postponing today should schedule for tomorrow",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 10
                                Time = user.DayStart
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 2))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 10, Postponed None)
                                        ]
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 9,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 10,
                                            [
                                                TemplateExpect.Status (UserStatus (user, Postponed None))
                                            ]
                                            FlukeDate.Create 2020 Month.March 11,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 12,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 13,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                        ]
                                }
                            ]
                    }


                    "Postponing today should schedule for tomorrow with PendingAfter",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 10
                                Time = user.DayStart
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 2))
                                            PendingAfter = FlukeTime.Create 03 00 |> Some
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 10, Postponed None)
                                        ]
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 9,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 10,
                                            [
                                                TemplateExpect.Status (UserStatus (user, Postponed None))
                                            ]
                                            FlukeDate.Create 2020 Month.March 11,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 12,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 13,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                        ]
                                }
                            ]
                    }


                    "(Postponed None) yesterday schedules for today",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 11
                                Time = user.DayStart
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 2))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 10, Postponed None)
                                        ]
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 9,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 10,
                                            [
                                                TemplateExpect.Status (UserStatus (user, Postponed None))
                                            ]
                                            FlukeDate.Create 2020 Month.March 11,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 12,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 13,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                        ]
                                }
                            ]
                    }

                    "Pending today after missing yesterday, then resetting the schedule with a future Completed event",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 11
                                Time = user.DayStart
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 2))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 8, Completed)
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 12, Completed)
                                        ]
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 7,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 8,
                                            [
                                                TemplateExpect.Status (UserStatus (user, Completed))
                                            ]
                                            FlukeDate.Create 2020 Month.March 9,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 10,
                                            [
                                                TemplateExpect.Status Missed
                                            ]
                                            FlukeDate.Create 2020 Month.March 11,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 12,
                                            [
                                                TemplateExpect.Status (UserStatus (user, Completed))
                                            ]
                                            FlukeDate.Create 2020 Month.March 13,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 14,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                        ]
                                }
                            ]
                    }


                    "Recurring task only Suggested before PendingAfter",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 10
                                Time = FlukeTime.Create 19 30
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 1))
                                            PendingAfter = FlukeTime.Create 20 00 |> Some
                                        }
                                    Events = []
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 9,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 10,
                                            [
                                                TemplateExpect.Status Suggested
                                            ]
                                            FlukeDate.Create 2020 Month.March 11,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                        ]
                                }
                            ]
                    }


                    "Recurring task Pending after PendingAfter",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 10
                                Time = FlukeTime.Create 21 00
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 1))
                                            PendingAfter = FlukeTime.Create 20 00 |> Some
                                        }
                                    Events = []
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 9,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 10,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 11,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                        ]
                                }
                            ]
                    }


                    "Recurrency for the next days should work normally while today is still optional/suggested (before PendingAfter)",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 27
                                Time = FlukeTime.Create 17 00
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 2))
                                            PendingAfter = FlukeTime.Create 18 00 |> Some
                                        }
                                    Events = []
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 25,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 26,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 27,
                                            [
                                                TemplateExpect.Status Suggested
                                            ]
                                            FlukeDate.Create 2020 Month.March 28,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 29,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 30,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                        ]
                                }
                            ]
                    }


                    "Reset counting after a future ManualPending event",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 28
                                Time = user.DayStart
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 3))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 30, ManualPending)
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 31, ManualPending)
                                        ]
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 27,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 28,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 29,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 30,
                                            [
                                                TemplateExpect.Status (UserStatus (user, ManualPending))
                                            ]
                                            FlukeDate.Create 2020 Month.March 31,
                                            [
                                                TemplateExpect.Status (UserStatus (user, ManualPending))
                                            ]
                                            FlukeDate.Create 2020 Month.April 01,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.April 02,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.April 03,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                        ]
                                }
                            ]
                    }
                ]
                "Recurrency Fixed",
                [
                    "Weekly task, pending today, initialized by past completion",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 21
                                Time = user.DayStart
                            }
                        Tasks =
                            [
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
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 14, Completed)
                                        ]
                                    Expected =
                                        [
                                            for d in 13 .. 29 do
                                                FlukeDate.Create 2020 Month.March d,
                                                [
                                                    TemplateExpect.Status
                                                        (match d with
                                                         | 14 -> UserStatus (user, Completed)
                                                         | 21
                                                         | 28 -> Pending
                                                         | _ -> Disabled)
                                                ]
                                        ]
                                }
                            ]
                    }


                    "Weekly task, missed until today, initialized by past completion",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 20
                                Time = user.DayStart
                            }
                        Tasks =
                            [
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
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 13, Completed)
                                        ]
                                    Expected =
                                        [
                                            for d in 10 .. 26 do
                                                FlukeDate.Create 2020 Month.March d,
                                                [
                                                    TemplateExpect.Status
                                                        (match d with
                                                         | 13 -> UserStatus (user, Completed)
                                                         | 18
                                                         | 19 -> Missed
                                                         | 20
                                                         | 25 -> Pending
                                                         | _ -> Disabled)
                                                ]
                                        ]
                                }
                            ]
                    }


                    "Weekly task, (Postponed None) then missed until today, pending tomorrow",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 20
                                Time = user.DayStart
                            }
                        Tasks =
                            [
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
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 18, Postponed None)
                                        ]
                                    Expected =
                                        [
                                            for d in 13 .. 29 do
                                                FlukeDate.Create 2020 Month.March d,
                                                [
                                                    TemplateExpect.Status
                                                        (match d with
                                                         | 18 -> UserStatus (user, Postponed None)
                                                         | 19 -> Missed
                                                         | 20
                                                         | 21
                                                         | 28 -> Pending
                                                         | _ -> Disabled)
                                                ]
                                        ]
                                }
                            ]
                    }


                    "Weekly task, without past events, pending in a few days",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 20
                                Time = user.DayStart
                            }
                        Tasks =
                            [
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
                                    Events = []
                                    Expected =
                                        [
                                            for d in 17 .. 26 do
                                                FlukeDate.Create 2020 Month.March d,
                                                [
                                                    TemplateExpect.Status
                                                        (match d with
                                                         | 25 -> Pending
                                                         | _ -> Disabled)
                                                ]
                                        ]
                                }
                            ]
                    }


                    "Fixed weekly task, without past events, pending tomorrow",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 20
                                Time = user.DayStart
                            }
                        Tasks =
                            [
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
                                    Events = []
                                    Expected =
                                        [
                                            for d in 13 .. 29 do
                                                FlukeDate.Create 2020 Month.March d,
                                                [
                                                    TemplateExpect.Status
                                                        (match d with
                                                         | 21
                                                         | 28 -> Pending
                                                         | _ -> Disabled)
                                                ]
                                        ]
                                }
                            ]
                    }


                    "Fixed weekly task only Suggested before PendingAfter",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.August 26
                                Time = user.DayStart
                            }
                        Tasks =
                            [
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
                                    Events = []
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.August 25,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.August 26,
                                            [
                                                TemplateExpect.Status Suggested
                                            ]
                                            FlukeDate.Create 2020 Month.August 27,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                        ]
                                }
                            ]
                    }
                ]
                "Postponed Until",
                [
                    "Postponed until later",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 10
                                Time = user.DayStart
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 1))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry
                                                (FlukeDate.Create 2020 Month.March 10,
                                                 Postponed (Some (FlukeTime.Create 23 00)))
                                        ]
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 09,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 10,
                                            [
                                                TemplateExpect.Status
                                                    (UserStatus (user, Postponed (Some (FlukeTime.Create 23 00))))
                                            ]
                                            FlukeDate.Create 2020 Month.March 11,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 12,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                        ]
                                }
                            ]
                    }
                    "Postponed until after midnight",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 10
                                Time = user.DayStart
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 1))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry
                                                (FlukeDate.Create 2020 Month.March 10,
                                                 Postponed (Some (FlukeTime.Create 01 00)))
                                        ]
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 09,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 10,
                                            [
                                                TemplateExpect.Status
                                                    (UserStatus (user, Postponed (Some (FlukeTime.Create 01 00))))
                                            ]
                                            FlukeDate.Create 2020 Month.March 11,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 12,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                        ]
                                }
                            ]
                    }
                    "Pending after expiration of Postponed (before midnight)",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 11
                                Time = FlukeTime.Create 02 00
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 1))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry
                                                (FlukeDate.Create 2020 Month.March 10,
                                                 Postponed (Some (FlukeTime.Create 23 00)))
                                        ]
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 09,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 10,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 11,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 12,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                        ]
                                }
                            ]
                    }


                    "Pending after expiration of Postponed (after midnight)",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 11
                                Time = FlukeTime.Create 02 00
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 1))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry
                                                (FlukeDate.Create 2020 Month.March 10,
                                                 Postponed (Some (FlukeTime.Create 01 00)))
                                        ]
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 09,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 10,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 11,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 12,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                        ]
                                }
                            ]
                    }


                    "Past PostponedUntil events are shown",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 13
                                Time = FlukeTime.Create 02 00
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 1))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 08, Completed)
                                            DslStatusEntry
                                                (FlukeDate.Create 2020 Month.March 10,
                                                 Postponed (Some (FlukeTime.Create 01 00)))
                                        ]
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 07,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 08,
                                            [
                                                TemplateExpect.Status (UserStatus (user, Completed))
                                            ]
                                            FlukeDate.Create 2020 Month.March 09,
                                            [
                                                TemplateExpect.Status Missed
                                            ]
                                            FlukeDate.Create 2020 Month.March 10,
                                            [
                                                TemplateExpect.Status
                                                    (UserStatus (user, Postponed (Some (FlukeTime.Create 01 00))))
                                            ]
                                            FlukeDate.Create 2020 Month.March 11,
                                            [
                                                TemplateExpect.Status Missed
                                            ]
                                            FlukeDate.Create 2020 Month.March 12,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 13,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                        ]
                                }
                            ]
                    }


                    "Future PostponedUntil events are shown",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 10
                                Time = user.DayStart
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 1))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry
                                                (FlukeDate.Create 2020 Month.March 12,
                                                 Postponed (Some (FlukeTime.Create 13 00)))
                                        ]
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 09,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 10,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 11,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 12,
                                            [
                                                TemplateExpect.Status
                                                    (UserStatus (user, Postponed (Some (FlukeTime.Create 13 00))))
                                            ]
                                            FlukeDate.Create 2020 Month.March 13,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                        ]
                                }
                            ]
                    }
                ]
            ]
            "Session Data",
            [
                "Sessions",
                [
                    "Respect dayStart on session events",
                    {
                        Position =
                            {
                                Date = FlukeDate.Create 2020 Month.March 04
                                Time = user.DayStart
                            }
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 1))
                                        }
                                    Events =
                                        [
                                            DslSession (FlukeDateTime.Create 2020 Month.March 01 11 00)
                                            DslSession (FlukeDateTime.Create 2020 Month.March 01 13 00)
                                            DslSession (FlukeDateTime.Create 2020 Month.March 08 11 00)
                                            DslSession (FlukeDateTime.Create 2020 Month.March 08 13 00)
                                        ]
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.February 29,
                                            [
                                                TemplateExpect.Status Disabled
                                                TemplateExpect.Session 1
                                            ]
                                            FlukeDate.Create 2020 Month.March 1,
                                            [
                                                TemplateExpect.Status Disabled
                                                TemplateExpect.Session 1
                                            ]
                                            FlukeDate.Create 2020 Month.March 2,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 3,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 4,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 5,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 6,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.March 7,
                                            [
                                                TemplateExpect.Status Pending
                                                TemplateExpect.Session 1
                                            ]
                                            FlukeDate.Create 2020 Month.March 8,
                                            [
                                                TemplateExpect.Status Pending
                                                TemplateExpect.Session 1
                                            ]
                                        ]
                                }
                            ]
                    }
                ]
            ]
        ]

    let getTreeMap user =
        let tree = getTree user

        tree
        |> List.collect (fun (name1, list) ->
            list
            |> List.collect (fun (name2, list) ->
                list
                |> List.map (fun (name3, dslTemplate) ->
                    let name = sprintf "%s/%s/%s" name1 name2 name3

                    let newDslTemplate =
                        { dslTemplate with
                            Tasks =
                                dslTemplate.Tasks
                                |> List.map (fun templateTask ->
                                    { templateTask with
                                        Task = { templateTask.Task with Name = TaskName name }
                                    })
                        }

                    name, newDslTemplate)))
        |> Map.ofList
