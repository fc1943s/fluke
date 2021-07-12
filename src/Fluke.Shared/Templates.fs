namespace Fluke.Shared

open System

module Templates =
    open Domain.Model
    open Domain.UserInteraction
    open Domain.State


    type DslTask =
        | DslTaskComment of comment: string
        | DslSession of start: FlukeDateTime
        | DslPriority of priority: Priority
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
        | Status of status: CellStatus
        | Session of count: int

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

    type DslData =
        {
            InformationStateMap: Map<Information, InformationState>
            TaskStateList: (TaskState * UserInteraction list) list
        }

    [<RequireQualifiedAccess>]
    type Template =
        | Single of DslTemplate
        | Multiple of DslTemplate


    let getDatabase (user: User) =
        [
            "Lane Sorting",
            [
                "Default",
                [
                    "All task types mixed",
                    {
                        Position =
                            FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 10, FlukeTime.Create 14 0, Second 0)
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Name = TaskName "01"
                                            Scheduling = Manual WithSuggestion
                                        }
                                    Events = []
                                    Expected = []
                                }

                                {
                                    Task =
                                        { Task.Default with
                                            Name = TaskName "02"
                                            Scheduling = Manual WithSuggestion
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 9, Postponed None)
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 10, Postponed None)
                                        ]
                                    Expected = []
                                }

                                {
                                    Task =
                                        { Task.Default with
                                            Name = TaskName "03"
                                            Scheduling = Manual WithoutSuggestion
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 9, Scheduled)
                                        ]
                                    Expected = []
                                }

                                {
                                    Task =
                                        { Task.Default with
                                            Name = TaskName "04"
                                            Scheduling = Recurrency (Offset (Days 1))
                                            PendingAfter = Some <| FlukeTime.Create 20 0
                                        }
                                    Events = []
                                    Expected = []
                                }

                                {
                                    Task =
                                        { Task.Default with
                                            Name = TaskName "05"
                                            Scheduling = Manual WithoutSuggestion
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 10, Scheduled)
                                        ]
                                    Expected = []
                                }

                                {
                                    Task =
                                        { Task.Default with
                                            Name = TaskName "06"
                                            Scheduling = Manual WithoutSuggestion
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 4, Scheduled)
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 6, Dismissed)
                                        ]
                                    Expected = []
                                }

                                {
                                    Task =
                                        { Task.Default with
                                            Name = TaskName "07"
                                            Scheduling = Recurrency (Offset (Days 4))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 8, Completed)
                                        ]
                                    Expected = []
                                }

                                {
                                    Task =
                                        { Task.Default with
                                            Name = TaskName "08"
                                            Scheduling = Recurrency (Offset (Days 2))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 10, Postponed None)
                                        ]
                                    Expected = []
                                }

                                {
                                    Task =
                                        { Task.Default with
                                            Name = TaskName "09"
                                            Scheduling = Recurrency (Offset (Days 2))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 10, Dismissed)
                                        ]
                                    Expected = []
                                }

                                {
                                    Task =
                                        { Task.Default with
                                            Name = TaskName "10"
                                            Scheduling = Recurrency (Offset (Days 2))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 10, Completed)
                                        ]
                                    Expected = []
                                }

                                {
                                    Task =
                                        { Task.Default with
                                            Name = TaskName "11"
                                            Scheduling = Recurrency (Offset (Days 1))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (
                                                FlukeDate.Create 2020 Month.March 10,
                                                Postponed (FlukeTime.Create 15 0 |> Some)
                                            )
                                        ]
                                    Expected = []
                                }

                                {
                                    Task =
                                        { Task.Default with
                                            Name = TaskName "12"
                                            Scheduling = Manual WithoutSuggestion
                                        }
                                    Events = []
                                    Expected = []
                                }

                                {
                                    Task =
                                        { Task.Default with
                                            Name = TaskName "13"
                                            Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Tuesday ])
                                        }
                                    Events = []
                                    Expected = []
                                }

                                {
                                    Task =
                                        { Task.Default with
                                            Name = TaskName "14"
                                            Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Wednesday ])
                                        }
                                    Events = []
                                    Expected = []
                                }

                                {
                                    Task =
                                        { Task.Default with
                                            Name = TaskName "15"
                                            Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Friday ])
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 7, Postponed None)
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 9, Dismissed)
                                        ]
                                    Expected = []
                                }

                                {
                                    Task =
                                        { Task.Default with
                                            Name = TaskName "16"
                                            Scheduling = Recurrency (Offset (Days 1))
                                            MissedAfter = (FlukeTime.Create 13 0 |> Some)
                                        }
                                    Events = []
                                    Expected = []
                                }

                                {
                                    Task =
                                        { Task.Default with
                                            Name = TaskName "17"
                                            Scheduling = Recurrency (Offset (Days 1))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (
                                                FlukeDate.Create 2020 Month.March 10,
                                                Postponed (FlukeTime.Create 13 0 |> Some)
                                            )
                                        ]
                                    Expected = []
                                }

                                {
                                    Task =
                                        { Task.Default with
                                            Name = TaskName "18"
                                            Scheduling = Recurrency (Offset (Days 1))
                                        }
                                    Events = []
                                    Expected = []
                                }
                            ]
                    }
                ]
            ]
            "Lane Rendering",
            [
                "Manual",
                [
                    "Empty manual task",
                    {
                        Position = FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 11, user.DayStart, Second 0)
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
                        Position = FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 11, user.DayStart, Second 0)
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Manual WithoutSuggestion
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 9, Scheduled)
                                        ]
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 8,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 9,
                                            [
                                                TemplateExpect.Status (UserStatus (user.Username, Scheduled))
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
                            FlukeDateTime.Create (
                                FlukeDate.Create 2020 Month.March 10,
                                FlukeTime.Create 19 30,
                                Second 0
                            )
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Manual WithSuggestion
                                            PendingAfter = FlukeTime.Create 20 0 |> Some
                                        }
                                    Events = []
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 9,
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
                            FlukeDateTime.Create (
                                FlukeDate.Create 2020 Month.January 1,
                                FlukeTime.Create 21 0,
                                Second 0
                            )
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Manual WithSuggestion
                                            PendingAfter = FlukeTime.Create 20 0 |> Some
                                        }
                                    Events = []
                                    Expected =
                                        [
                                            FlukeDate.Create 2019 Month.December 31,
                                            [
                                                TemplateExpect.Status Suggested
                                            ]
                                            FlukeDate.Create 2020 Month.January 1,
                                            [
                                                TemplateExpect.Status Pending
                                            ]
                                            FlukeDate.Create 2020 Month.January 2,
                                            [
                                                TemplateExpect.Status Suggested
                                            ]
                                        ]
                                }
                            ]
                    }


                    "Manual Suggested task: Missed ManualPending propagates until today",
                    {
                        Position = FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 28, user.DayStart, Second 0)
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Manual WithSuggestion
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 26, Scheduled)
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 30, Scheduled)
                                        ]
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 25,
                                            [
                                                TemplateExpect.Status Suggested
                                            ]
                                            FlukeDate.Create 2020 Month.March 26,
                                            [
                                                TemplateExpect.Status (UserStatus (user.Username, Scheduled))
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
                                                TemplateExpect.Status (UserStatus (user.Username, Scheduled))
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
                        Position = FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 28, user.DayStart, Second 0)
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Manual WithSuggestion
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 25, Scheduled)
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
                                                TemplateExpect.Status (UserStatus (user.Username, Scheduled))
                                            ]
                                            FlukeDate.Create 2020 Month.March 26,
                                            [
                                                TemplateExpect.Status (UserStatus (user.Username, Completed))
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
                        Position = FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 28, user.DayStart, Second 0)
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Manual WithSuggestion
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 25, Scheduled)
                                        ]
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 24,
                                            [
                                                TemplateExpect.Status Suggested
                                            ]
                                            FlukeDate.Create 2020 Month.March 25,
                                            [
                                                TemplateExpect.Status (UserStatus (user.Username, Scheduled))
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
                        Position = FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 9, user.DayStart, Second 0)
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
                        Position = FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 9, user.DayStart, Second 0)
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
                                                TemplateExpect.Status (UserStatus (user.Username, Completed))
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
                        Position = FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 10, user.DayStart, Second 0)
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
                                                TemplateExpect.Status (UserStatus (user.Username, Postponed None))
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
                        Position = FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 10, user.DayStart, Second 0)
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 2))
                                            PendingAfter = FlukeTime.Create 3 0 |> Some
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
                                                TemplateExpect.Status (UserStatus (user.Username, Postponed None))
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
                        Position = FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 11, user.DayStart, Second 0)
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
                                                TemplateExpect.Status (UserStatus (user.Username, Postponed None))
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
                        Position = FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 11, user.DayStart, Second 0)
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
                                                TemplateExpect.Status (UserStatus (user.Username, Completed))
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
                                                TemplateExpect.Status (UserStatus (user.Username, Completed))
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
                            FlukeDateTime.Create (
                                FlukeDate.Create 2020 Month.March 10,
                                FlukeTime.Create 19 30,
                                Second 0
                            )
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 1))
                                            PendingAfter = FlukeTime.Create 20 0 |> Some
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
                            FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 10, FlukeTime.Create 21 0, Second 0)
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 1))
                                            PendingAfter = FlukeTime.Create 20 0 |> Some
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


                    "Recurrency for the next days should work normally while today is still suggested (before PendingAfter)",
                    {
                        Position =
                            FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 27, FlukeTime.Create 17 0, Second 0)
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 2))
                                            PendingAfter = FlukeTime.Create 18 0 |> Some
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
                        Position = FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 28, user.DayStart, Second 0)
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 3))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 30, Scheduled)
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 31, Scheduled)
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
                                                TemplateExpect.Status (UserStatus (user.Username, Scheduled))
                                            ]
                                            FlukeDate.Create 2020 Month.March 31,
                                            [
                                                TemplateExpect.Status (UserStatus (user.Username, Scheduled))
                                            ]
                                            FlukeDate.Create 2020 Month.April 1,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.April 2,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.April 3,
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
                        Position = FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 21, user.DayStart, Second 0)
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Saturday ])
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
                                                    TemplateExpect.Status (
                                                        match d with
                                                        | 14 -> UserStatus (user.Username, Completed)
                                                        | 21
                                                        | 28 -> Pending
                                                        | _ -> Disabled
                                                    )
                                                ]
                                        ]
                                }
                            ]
                    }


                    "Weekly task, missed until today, initialized by past completion",
                    {
                        Position = FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 20, user.DayStart, Second 0)
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Wednesday ])
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
                                                    TemplateExpect.Status (
                                                        match d with
                                                        | 13 -> UserStatus (user.Username, Completed)
                                                        | 18
                                                        | 19 -> Missed
                                                        | 20
                                                        | 25 -> Pending
                                                        | _ -> Disabled
                                                    )
                                                ]
                                        ]
                                }
                            ]
                    }


                    "Weekly task, (Postponed None) then missed until today, pending tomorrow",
                    {
                        Position = FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 20, user.DayStart, Second 0)
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Saturday ])
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
                                                    TemplateExpect.Status (
                                                        match d with
                                                        | 18 -> UserStatus (user.Username, Postponed None)
                                                        | 19 -> Missed
                                                        | 20
                                                        | 21
                                                        | 28 -> Pending
                                                        | _ -> Disabled
                                                    )
                                                ]
                                        ]
                                }
                            ]
                    }


                    "Weekly task, without past events, pending in a few days",
                    {
                        Position = FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 20, user.DayStart, Second 0)
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Wednesday ])
                                        }
                                    Events = []
                                    Expected =
                                        [
                                            for d in 17 .. 26 do
                                                FlukeDate.Create 2020 Month.March d,
                                                [
                                                    TemplateExpect.Status (
                                                        match d with
                                                        | 25 -> Pending
                                                        | _ -> Disabled
                                                    )
                                                ]
                                        ]
                                }
                            ]
                    }


                    "Fixed weekly task, without past events, pending tomorrow",
                    {
                        Position = FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 20, user.DayStart, Second 0)
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Saturday ])
                                        }
                                    Events = []
                                    Expected =
                                        [
                                            for d in 13 .. 29 do
                                                FlukeDate.Create 2020 Month.March d,
                                                [
                                                    TemplateExpect.Status (
                                                        match d with
                                                        | 21
                                                        | 28 -> Pending
                                                        | _ -> Disabled
                                                    )
                                                ]
                                        ]
                                }
                            ]
                    }


                    "Fixed weekly task only Suggested before PendingAfter",
                    {
                        Position = FlukeDateTime.Create (FlukeDate.Create 2020 Month.August 26, user.DayStart, Second 0)
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling =
                                                Recurrency (
                                                    Fixed [
                                                        Weekly DayOfWeek.Monday
                                                        Weekly DayOfWeek.Tuesday
                                                        Weekly DayOfWeek.Wednesday
                                                        Weekly DayOfWeek.Thursday
                                                        Weekly DayOfWeek.Friday
                                                    ]
                                                )
                                            PendingAfter = Some (FlukeTime.Create 19 0)
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
                        Position = FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 10, user.DayStart, Second 0)
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 1))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (
                                                FlukeDate.Create 2020 Month.March 10,
                                                Postponed (Some (FlukeTime.Create 23 0))
                                            )
                                        ]
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 9,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 10,
                                            [
                                                TemplateExpect.Status (
                                                    UserStatus (user.Username, Postponed (Some (FlukeTime.Create 23 0)))
                                                )
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
                        Position = FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 10, user.DayStart, Second 0)
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 1))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (
                                                FlukeDate.Create 2020 Month.March 10,
                                                Postponed (Some (FlukeTime.Create 1 0))
                                            )
                                        ]
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 9,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 10,
                                            [
                                                TemplateExpect.Status (
                                                    UserStatus (user.Username, Postponed (Some (FlukeTime.Create 1 0)))
                                                )
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
                            FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 11, FlukeTime.Create 2 0, Second 0)
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 1))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (
                                                FlukeDate.Create 2020 Month.March 10,
                                                Postponed (Some (FlukeTime.Create 23 0))
                                            )
                                        ]
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
                            FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 11, FlukeTime.Create 2 0, Second 0)
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 1))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (
                                                FlukeDate.Create 2020 Month.March 10,
                                                Postponed (Some (FlukeTime.Create 1 0))
                                            )
                                        ]
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
                            FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 13, FlukeTime.Create 2 0, Second 0)
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 1))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (FlukeDate.Create 2020 Month.March 8, Completed)
                                            DslStatusEntry (
                                                FlukeDate.Create 2020 Month.March 10,
                                                Postponed (Some (FlukeTime.Create 1 0))
                                            )
                                        ]
                                    Expected =
                                        [
                                            FlukeDate.Create 2020 Month.March 7,
                                            [
                                                TemplateExpect.Status Disabled
                                            ]
                                            FlukeDate.Create 2020 Month.March 8,
                                            [
                                                TemplateExpect.Status (UserStatus (user.Username, Completed))
                                            ]
                                            FlukeDate.Create 2020 Month.March 9,
                                            [
                                                TemplateExpect.Status Missed
                                            ]
                                            FlukeDate.Create 2020 Month.March 10,
                                            [
                                                TemplateExpect.Status (
                                                    UserStatus (user.Username, Postponed (Some (FlukeTime.Create 1 0)))
                                                )
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
                        Position = FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 10, user.DayStart, Second 0)
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 1))
                                        }
                                    Events =
                                        [
                                            DslStatusEntry (
                                                FlukeDate.Create 2020 Month.March 12,
                                                Postponed (Some (FlukeTime.Create 13 0))
                                            )
                                        ]
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
                                            FlukeDate.Create 2020 Month.March 12,
                                            [
                                                TemplateExpect.Status (
                                                    UserStatus (user.Username, Postponed (Some (FlukeTime.Create 13 0)))
                                                )
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
                        Position = FlukeDateTime.Create (FlukeDate.Create 2020 Month.March 4, user.DayStart, Second 0)
                        Tasks =
                            [
                                {
                                    Task =
                                        { Task.Default with
                                            Scheduling = Recurrency (Offset (Days 1))
                                        }
                                    Events =
                                        [
                                            DslSession (FlukeDateTime.Create (2020, Month.March, 1, 11, 0, Second 0))
                                            DslSession (FlukeDateTime.Create (2020, Month.March, 1, 13, 0, Second 0))
                                            DslSession (FlukeDateTime.Create (2020, Month.March, 8, 11, 0, Second 0))
                                            DslSession (FlukeDateTime.Create (2020, Month.March, 8, 13, 0, Second 0))
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

    let getDatabaseMap user =
        let database = getDatabase user

        database
        |> List.collect
            (fun (name1, list) ->
                list
                |> List.collect
                    (fun (name2, list) ->
                        list
                        |> List.map
                            (fun (name3, dslTemplate) ->
                                let name = $"{name1}/{name2}/{name3}"

                                let newDslTemplate =
                                    { dslTemplate with
                                        Tasks =
                                            dslTemplate.Tasks
                                            |> List.map
                                                (fun templateTask ->
                                                    { templateTask with
                                                        Task =
                                                            { templateTask.Task with
                                                                Name =
                                                                    if templateTask.Task.Name = Task.Default.Name then
                                                                        TaskName name
                                                                    else
                                                                        templateTask.Task.Name
                                                            }
                                                    })
                                    }

                                name, newDslTemplate)))
        |> Map.ofSeq

    let createCellStatusChangeInteraction (user: User) (task: Task) date manualCellStatus =
        let cellStatusChange =
            match manualCellStatus with
            | ManualCellStatus.Completed -> CellStatusChange.Complete
            | ManualCellStatus.Dismissed -> CellStatusChange.Dismiss
            | ManualCellStatus.Postponed until -> CellStatusChange.Postpone until
            | ManualCellStatus.Scheduled -> CellStatusChange.Schedule

        let cellInteraction = CellInteraction.StatusChange cellStatusChange
        let dateId = DateId date
        let interaction = Interaction.Cell (task.Id, dateId, cellInteraction)
        let moment = FlukeDateTime.Create (date, user.DayStart, Second 0)
        let userInteraction = UserInteraction (moment, user.Username, interaction)
        userInteraction

    let createTaskState
        moment
        (task: Task)
        (sortTaskMap: Map<TaskName, Task> option)
        (dslTasks: (DslTask * User) list)
        =
        let taskState, userInteractions =
            (({ TaskState.Default with Task = task }, []), dslTasks)
            ||> List.fold
                    (fun (taskState, userInteractions) (dslTask, user) ->
                        match dslTask with
                        | DslTaskComment comment ->
                            let interaction =
                                Interaction.Task (
                                    task.Id,
                                    TaskInteraction.Attachment (Attachment.Comment (Comment.Comment comment))
                                )

                            let userInteraction = UserInteraction (moment, user.Username, interaction)

                            let newUserInteractions =
                                userInteractions
                                @ [
                                    userInteraction
                                ]

                            taskState, newUserInteractions
                        | DslCellComment (date, comment) ->
                            let interaction =
                                Interaction.Cell (
                                    task.Id,
                                    DateId date,
                                    CellInteraction.Attachment (Attachment.Comment (Comment.Comment comment))
                                )

                            let userInteraction = UserInteraction (moment, user.Username, interaction)

                            let newUserInteractions =
                                userInteractions
                                @ [
                                    userInteraction
                                ]

                            taskState, newUserInteractions
                        | DslSession start ->
                            let session = Session start

                            let taskInteraction = TaskInteraction.Session session
                            let interaction = Interaction.Task (task.Id, taskInteraction)

                            let userInteraction = UserInteraction (moment, user.Username, interaction)

                            let newUserInteractions =
                                userInteractions
                                @ [
                                    userInteraction
                                ]

                            taskState, newUserInteractions
                        | DslTaskSort (top, bottom) ->
                            let newUserInteractions =
                                match sortTaskMap with
                                | Some sortTaskMap ->
                                    let getTask taskName =
                                        taskName
                                        |> Option.map
                                            (fun taskName ->
                                                sortTaskMap
                                                |> Map.tryFind taskName
                                                |> function
                                                    | Some task -> task
                                                    | None ->
                                                        failwithf
                                                            $"DslTaskSort. Task not found: {taskName}. Map length: {sortTaskMap.Count}")

                                    let interaction =
                                        Interaction.Task (
                                            task.Id,
                                            TaskInteraction.Sort (
                                                getTask top |> Option.map (fun task -> task.Id),
                                                getTask bottom |> Option.map (fun task -> task.Id)
                                            )
                                        )

                                    let userInteraction = UserInteraction (moment, user.Username, interaction)

                                    userInteractions
                                    @ [
                                        userInteraction
                                    ]
                                | None -> userInteractions

                            taskState, newUserInteractions
                        | DslStatusEntry (date, manualCellStatus) ->
                            let userInteraction = createCellStatusChangeInteraction user task date manualCellStatus

                            let newUserInteractions =
                                userInteractions
                                @ [
                                    userInteraction
                                ]

                            taskState, newUserInteractions
                        | DslPriority priority ->
                            let newTaskState =
                                { taskState with
                                    Task =
                                        { taskState.Task with
                                            Priority = Some priority
                                        }
                                }

                            newTaskState, userInteractions
                        | DslTaskSet set ->
                            match set with
                            | DslSetScheduling (scheduling, _start) ->
                                let newTaskState =
                                    { taskState with
                                        Task =
                                            { taskState.Task with
                                                Scheduling = scheduling
                                            }
                                    }

                                newTaskState, userInteractions
                            | DslSetPendingAfter start ->
                                let newTaskState =
                                    { taskState with
                                        Task =
                                            { taskState.Task with
                                                PendingAfter = Some start
                                            }
                                    }

                                newTaskState, userInteractions
                            | DslSetMissedAfter start ->
                                let newTaskState =
                                    { taskState with
                                        Task =
                                            { taskState.Task with
                                                MissedAfter = Some start
                                            }
                                    }

                                newTaskState, userInteractions

                            | DslSetDuration minutes ->
                                let newTaskState =
                                    { taskState with
                                        Task =
                                            { taskState.Task with
                                                Duration = Some (Minute minutes)
                                            }
                                    }

                                newTaskState, userInteractions

                        )

        taskState, userInteractions

    let createLaneRenderingDslData
        (input: {| User: User
                   Position: FlukeDateTime
                   Task: Task
                   Events: DslTask list |})
        =
        let eventsWithUser = input.Events |> List.map (fun x -> x, input.User)

        let dslData =
            {
                TaskStateList =
                    [
                        createTaskState input.Position input.Task None eventsWithUser
                    ]
                InformationStateMap =
                    [
                        input.Task.Information
                    ]
                    |> informationListToStateMap
            }

        dslData

    let mergeDslDataIntoDatabaseState (dslData: DslData) (databaseState: DatabaseState) =
        let newInformationStateMap =
            mergeInformationStateMap databaseState.InformationStateMap dslData.InformationStateMap

        let taskStateList, userInteractionsBundle = dslData.TaskStateList |> List.unzip
        let userInteractions = userInteractionsBundle |> List.collect id

        let newTaskStateMap =
            (databaseState.TaskStateMap, taskStateList)
            ||> List.fold
                    (fun taskStateMap taskState ->
                        let oldTaskState =
                            databaseState.TaskStateMap
                            |> Map.tryFind taskState.Task.Id

                        let newTaskState =
                            match oldTaskState with
                            | Some oldTaskState -> mergeTaskState oldTaskState taskState
                            | None -> taskState

                        taskStateMap
                        |> Map.add taskState.Task.Id newTaskState)

        { databaseState with
            InformationStateMap = newInformationStateMap
            TaskStateMap = newTaskStateMap
        }
        |> databaseStateWithInteractions userInteractions

    let databaseStateFromDslTemplate user databaseId templateName dslTemplate =
        let dslDataList =
            dslTemplate.Tasks
            |> List.map
                (fun templateTask ->
                    createLaneRenderingDslData
                        {|
                            User = user
                            Position = dslTemplate.Position
                            Task = templateTask.Task
                            Events = templateTask.Events
                        |})

        let databaseState =
            DatabaseState.Create (
                id = databaseId,
                name = DatabaseName templateName,
                owner = user.Username,
                position = dslTemplate.Position,
                sharedWith = DatabaseAccess.Public
            )

        let newDatabaseState =
            (databaseState, dslDataList)
            ||> List.fold
                    (fun databaseState dslData ->
                        databaseState
                        |> mergeDslDataIntoDatabaseState dslData)

        newDatabaseState

    let templatesUser =
        {
            Username = Username "Fluke"
            Color = Color "#000000"
            WeekStart = DayOfWeek.Sunday
            DayStart = FlukeTime.Create 12 0
            SessionDuration = Minute 25
            SessionBreakDuration = Minute 5
        }
