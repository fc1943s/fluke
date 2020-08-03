namespace Fluke.Shared

open System
open Expecto
open Fluke.Shared
open Fluke.Shared.Model
open Expecto.Flip
open Fluke.Shared.TempData
open Suigetsu.Core

module Data =
    let task1 = { Task.Default with Name = "1" }
    let task2 = { Task.Default with Name = "2" }
    let task3 = { Task.Default with Name = "3" }
    let task4 = { Task.Default with Name = "4" }
    let task5 = { Task.Default with Name = "5" }

    let orderedEventList = [
        { Task = task1; Priority = TaskOrderPriority.First }
        { Task = task2; Priority = TaskOrderPriority.LessThan task1 }
        { Task = task3; Priority = TaskOrderPriority.LessThan task2 }
        { Task = task4; Priority = TaskOrderPriority.LessThan task3 }
        { Task = task5; Priority = TaskOrderPriority.Last }
    ]
    let orderedList = [ task1; task2; task3; task4; task5 ]

module Tests =
    open Data

    [<Tests>]
    let tests = testList "Tests" [

        testList "GetManualSortedTaskListTests" [
            test "1" {
                orderedEventList
                |> Sorting.getManualSortedTaskList
                |> Expect.equal "" orderedList

                [
                    { Task = task5; Priority = TaskOrderPriority.Last }
                    { Task = task4; Priority = TaskOrderPriority.LessThan task3 }
                    { Task = task3; Priority = TaskOrderPriority.LessThan task2 }
                    { Task = task2; Priority = TaskOrderPriority.LessThan task1 }
                    { Task = task1; Priority = TaskOrderPriority.First }
                ]
                |> Sorting.getManualSortedTaskList
                |> Expect.equal "" orderedList

                [
                    { Task = task3; Priority = TaskOrderPriority.LessThan task2 }
                    { Task = task5; Priority = TaskOrderPriority.Last }
                    { Task = task4; Priority = TaskOrderPriority.LessThan task3 }
                    { Task = task1; Priority = TaskOrderPriority.First }
                    { Task = task2; Priority = TaskOrderPriority.LessThan task1 }
                ]
                |> Sorting.getManualSortedTaskList
                |> Expect.equal "" orderedList

                [
                    { Task = task3; Priority = TaskOrderPriority.First }
                ]
                |> List.append orderedEventList
                |> Sorting.getManualSortedTaskList
                |> Expect.equal "" [ task3; task4; task1; task2; task5 ]

                [
                    { Task = task3; Priority = TaskOrderPriority.First }
                    { Task = task4; Priority = TaskOrderPriority.LessThan task2 }
                ]
                |> List.append orderedEventList
                |> Sorting.getManualSortedTaskList
                |> Expect.equal "" [ task3; task1; task2; task4; task5 ]

                [
                    { Task = task1; Priority = TaskOrderPriority.First }
                    { Task = task2; Priority = TaskOrderPriority.First }
                    { Task = task3; Priority = TaskOrderPriority.First }
                    { Task = task4; Priority = TaskOrderPriority.First }
                    { Task = task5; Priority = TaskOrderPriority.LessThan task4 }
                ]
                |> Sorting.getManualSortedTaskList
                |> Expect.equal "" [ task4; task5; task3; task2; task1 ]

            }
        ]

        testList "SetPriorityTests" [
            test "1" {
                let createPriorityEvents task priority taskList =
                    match taskList |> List.tryFindIndexBack ((=) task) with
                    | None -> None
                    | Some i ->
                        let closest =
                            (i + 1, i - 1)
                            |> Tuple2.map (fun x ->
                                taskList
                                |> List.tryItem x
                            )
                        match closest, priority with
                        | (Some _, None),
                            TaskOrderPriority.First when i = 0 -> None

                        | (Some below, None), _ ->
                            Some { Task = below; Priority = TaskOrderPriority.First }

                        | (Some below, Some above), _ ->
                            Some { Task = below; Priority = TaskOrderPriority.LessThan above }

                        | _, TaskOrderPriority.First when i > 0 ->
                            Some { Task = taskList |> List.head; Priority = TaskOrderPriority.LessThan task }

                        | _ -> None
                    |> Option.toList
                    |> List.append [ { Task = task; Priority = priority } ]

                [ task1; task2; task3; task4; task5 ]
                |> createPriorityEvents task3 TaskOrderPriority.First
                |> Expect.equal "" [
                    { Task = task3; Priority = TaskOrderPriority.First }
                    { Task = task4; Priority = TaskOrderPriority.LessThan task2 }
                ]

                [ task1; task2; task3; task4; task5 ]
                |> createPriorityEvents task1 TaskOrderPriority.First
                |> Expect.equal "" [
                    { Task = task1; Priority = TaskOrderPriority.First }
                ]

                [ task1; task2; task3; task4; task5 ]
                |> createPriorityEvents task1 TaskOrderPriority.Last
                |> Expect.equal "" [
                    { Task = task1; Priority = TaskOrderPriority.Last }
                    { Task = task2; Priority = TaskOrderPriority.First }
                ]

                [ task1; task2; task3; task4; task5 ]
                |> createPriorityEvents task5 TaskOrderPriority.Last
                |> Expect.equal "" [
                    { Task = task5; Priority = TaskOrderPriority.Last }
                ]

                [ task1; task2; task3; task4; task5 ]
                |> createPriorityEvents task5 TaskOrderPriority.First
                |> Expect.equal "" [
                    { Task = task5; Priority = TaskOrderPriority.First }
                    { Task = task1; Priority = TaskOrderPriority.LessThan task5 }
                ]

                [ task1; task2; task3; task4; task5 ]
                |> createPriorityEvents task2 TaskOrderPriority.First
                |> Expect.equal "" [
                    { Task = task2; Priority = TaskOrderPriority.First }
                    { Task = task3; Priority = TaskOrderPriority.LessThan task1 }
                ]

                [ task1; task2; task3; task4; task5 ]
                |> createPriorityEvents task4 (TaskOrderPriority.LessThan task1)
                |> Expect.equal "" [
                    { Task = task4; Priority = TaskOrderPriority.LessThan task1 }
                    { Task = task5; Priority = TaskOrderPriority.LessThan task3 }
                ]
            }
        ]

        testList "Lane Sorting" [

            let (|NoSorting|TimeOfDay|Frequency|) = function
                | Choice1Of3 _ -> NoSorting
                | Choice2Of3 _ -> TimeOfDay
                | Choice3Of3 _ -> Frequency
            let _noSorting = Choice1Of3 ()
            let sortByTimeOfDay = Choice2Of3 ()
            let sortByFrequency = Choice3Of3 ()

            let testWithLaneSortingData (props: {| Sort: Choice<_, _, _>
                                                   Data: (Task * TempTaskEvent list) list
                                                   Expected: string list
                                                   Now: FlukeDateTime |}) =

                let taskList =
                    props.Data
                    |> List.map (fun (task, events) ->
                        applyTaskEvents Consts.testDayStart task (events |> List.map (fun x -> x, Users.testUser))
                    )

                let dateSequence =
                    taskList
                    |> List.collect (fun x -> x.StatusEntries)
                    |> List.map ofTaskStatusEntry
                    |> List.map (fun (user, moment, manualCellStatus) -> moment.Date)
                    |> Rendering.getDateSequence (35, 35)

                taskList
                |> List.map (Rendering.renderLane Consts.testDayStart props.Now dateSequence)
                |> fun lanes ->
                    match props.Sort with
                    | NoSorting -> lanes
                    | TimeOfDay -> Sorting.sortLanesByTimeOfDay Consts.testDayStart props.Now [] lanes
                    | Frequency -> Sorting.sortLanesByFrequency lanes
                |> List.map (fun (OldLane (task, _)) -> task.Name)
                |> Expect.equal "" props.Expected

            test "Sort by Frequency: All task types mixed" {
                testWithLaneSortingData
                    {| Sort = sortByFrequency
                       Now = { Date = flukeDate 2020 Month.March 10
                               Time = Consts.testDayStart }
                       Data = [
                           { Task.Default with Name = "01"; Scheduling = Manual WithSuggestion },
                           []

                           { Task.Default with Name = "02"; Scheduling = Manual WithSuggestion },
                           [ TempStatusEntry (flukeDate 2020 Month.March 10, Postponed None)
                             TempStatusEntry (flukeDate 2020 Month.March 08, Postponed None) ]

                           { Task.Default with Name = "03"; Scheduling = Manual WithoutSuggestion },
                           [ TempStatusEntry (flukeDate 2020 Month.March 09, ManualPending) ]

                           { Task.Default with Name = "04"; Scheduling = Recurrency (Offset (Days 1));
                                                           PendingAfter = flukeTime 20 00 |> Some },
                           []

                           { Task.Default with Name = "05"; Scheduling = Manual WithoutSuggestion },
                           [ TempStatusEntry (flukeDate 2020 Month.March 10, ManualPending) ]

                           { Task.Default with Name = "06"; Scheduling = Manual WithoutSuggestion },
                           [ TempStatusEntry (flukeDate 2020 Month.March 04, Postponed None)
                             TempStatusEntry (flukeDate 2020 Month.March 06, Dismissed) ]

                           { Task.Default with Name = "07"; Scheduling = Recurrency (Offset (Days 4)) },
                           [ TempStatusEntry (flukeDate 2020 Month.March 08, Completed) ]

                           { Task.Default with Name = "08"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ TempStatusEntry (flukeDate 2020 Month.March 10, Completed) ]

                           { Task.Default with Name = "09"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ TempStatusEntry (flukeDate 2020 Month.March 10, Dismissed) ]

                           { Task.Default with Name = "10"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ TempStatusEntry (flukeDate 2020 Month.March 10, Postponed None) ]

                           { Task.Default with Name = "11"; Scheduling = Recurrency (Offset (Days 1)) },
                           []

                           { Task.Default with Name = "12"; Scheduling = Manual WithoutSuggestion },
                           []

                           { Task.Default with Name = "13"
                                               Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Tuesday ]) },
                           []

                           { Task.Default with Name = "14"
                                               Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Wednesday ]) },
                           []

                           { Task.Default with Name = "15"
                                               Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Friday ]) },
                           [ TempStatusEntry (flukeDate 2020 Month.March 07, Postponed None)
                             TempStatusEntry (flukeDate 2020 Month.March 09, Dismissed) ]
                       ]
                       Expected = [ "11"; "04"; "10"; "08"; "09"
                                    "07"; "15"; "13"; "14"; "02"
                                    "06"; "03"; "05"; "01"; "12" ] |}
            }

            test "Sort by Time of Day: All task types mixed" {
                testWithLaneSortingData
                    {| Sort = sortByTimeOfDay
                       Now = { Date = flukeDate 2020 Month.March 10
                               Time = flukeTime 14 00 }
                       Data = [
                           { Task.Default with Name = "01"; Scheduling = Manual WithSuggestion },
                           []

                           { Task.Default with Name = "02"; Scheduling = Manual WithSuggestion },
                           [ TempStatusEntry (flukeDate 2020 Month.March 10, Postponed None)
                             TempStatusEntry (flukeDate 2020 Month.March 08, Postponed None) ]

                           { Task.Default with Name = "03"; Scheduling = Manual WithoutSuggestion },
                           [ TempStatusEntry (flukeDate 2020 Month.March 09, ManualPending) ]

                           { Task.Default with Name = "04"; Scheduling = Recurrency (Offset (Days 1));
                                                           PendingAfter = flukeTime 20 00 |> Some },
                           []

                           { Task.Default with Name = "05"; Scheduling = Manual WithoutSuggestion },
                           [ TempStatusEntry (flukeDate 2020 Month.March 10, ManualPending) ]

                           { Task.Default with Name = "06"; Scheduling = Manual WithoutSuggestion },
                           [ TempStatusEntry (flukeDate 2020 Month.March 04, Postponed None)
                             TempStatusEntry (flukeDate 2020 Month.March 06, Dismissed) ]

                           { Task.Default with Name = "07"; Scheduling = Recurrency (Offset (Days 4)) },
                           [ TempStatusEntry (flukeDate 2020 Month.March 08, Completed) ]

                           { Task.Default with Name = "08"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ TempStatusEntry (flukeDate 2020 Month.March 10, Completed) ]

                           { Task.Default with Name = "09"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ TempStatusEntry (flukeDate 2020 Month.March 10, Dismissed) ]

                           { Task.Default with Name = "10"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ TempStatusEntry (flukeDate 2020 Month.March 10, Postponed None) ]

                           { Task.Default with Name = "11"; Scheduling = Recurrency (Offset (Days 1)) },
                           [ TempStatusEntry (flukeDate 2020 Month.March 10, Postponed (flukeTime 13 00 |> Some)) ]

                           { Task.Default with Name = "12"; Scheduling = Manual WithoutSuggestion },
                           []

                           { Task.Default with Name = "13"
                                               Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Tuesday ]) },
                           []

                           { Task.Default with Name = "14"
                                               Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Wednesday ]) },
                           []

                           { Task.Default with Name = "15"
                                               Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Friday ]) },
                           [ TempStatusEntry (flukeDate 2020 Month.March 07, Postponed None)
                             TempStatusEntry (flukeDate 2020 Month.March 09, Dismissed) ]

                           { Task.Default with Name = "16"; Scheduling = Recurrency (Offset (Days 1));
                                                            MissedAfter = (flukeTime 13 00 |> Some) },
                           []

                           { Task.Default with Name = "17"; Scheduling = Recurrency (Offset (Days 1)) },
                           [ TempStatusEntry (flukeDate 2020 Month.March 10, Postponed (flukeTime 15 00 |> Some)) ]

                           { Task.Default with Name = "18"; Scheduling = Recurrency (Offset (Days 1)) },
                           []
                       ]
                       Expected = [ "16"; "05"; "03"; "11"; "13"
                                    "18"; "17"; "04"; "01"; "02"
                                    "10"; "08"; "09"; "07"; "14"
                                    "15"; "12"; "06" ] |}
            }
        ]

        testList "Lane Rendering" [

            let testWithLaneRenderingData (props: {| Task: Task
                                                     Now: FlukeDateTime
                                                     Expected: (FlukeDate * CellStatus) list
                                                     Events: TempTaskEvent list |}) =

                let task =
                    applyTaskEvents
                        Consts.testDayStart
                        props.Task
                        (props.Events |> List.map (fun x -> x, Users.testUser))

                let dateSequence = props.Expected |> List.map fst

                let toString =
                    List.map string
                    >> String.concat Environment.NewLine

                Rendering.renderLane Consts.testDayStart props.Now dateSequence task
                |> fun (OldLane (task, cells)) ->
                    cells
                    |> List.map (fun (Cell (address, status)) -> string address.DateId, status)
                |> toString
                |> Expect.equal "" (props.Expected
                                    |> List.map (fun (date, cellStatus) -> string (DateId date), cellStatus)
                                    |> toString)

            testList "Postponed Until" [

                test "Postponed until later" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 1)) }
                           Now = { Date = flukeDate 2020 Month.March 10
                                   Time = Consts.testDayStart }
                           Expected = [
                               flukeDate 2020 Month.March 09, Disabled
                               flukeDate 2020 Month.March 10, UserStatus (Users.testUser, Postponed (Some (flukeTime 23 00)))
                               flukeDate 2020 Month.March 11, Pending
                               flukeDate 2020 Month.March 12, Pending
                           ]
                           Events = [
                               TempStatusEntry (flukeDate 2020 Month.March 10, Postponed (Some (flukeTime 23 00)))
                           ] |}
                }

                test "Postponed until after midnight" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 1)) }
                           Now = { Date = flukeDate 2020 Month.March 10
                                   Time = Consts.testDayStart }
                           Expected = [
                               flukeDate 2020 Month.March 09, Disabled
                               flukeDate 2020 Month.March 10, UserStatus (Users.testUser, Postponed (Some (flukeTime 01 00)))
                               flukeDate 2020 Month.March 11, Pending
                               flukeDate 2020 Month.March 12, Pending
                           ]
                           Events = [
                               TempStatusEntry (flukeDate 2020 Month.March 10, Postponed (Some (flukeTime 01 00)))
                           ] |}
                }

                test "Pending after expiration of Postponed (before midnight)" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 1)) }
                           Now = { Date = flukeDate 2020 Month.March 11
                                   Time = flukeTime 02 00 }
                           Expected = [
                               flukeDate 2020 Month.March 09, Disabled
                               flukeDate 2020 Month.March 10, Pending
                               flukeDate 2020 Month.March 11, Pending
                               flukeDate 2020 Month.March 12, Pending
                           ]
                           Events = [
                               TempStatusEntry (flukeDate 2020 Month.March 10, Postponed (Some (flukeTime 23 00)))
                           ] |}
                }

                test "Pending after expiration of Postponed (after midnight)" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 1)) }
                           Now = { Date = flukeDate 2020 Month.March 11
                                   Time = flukeTime 02 00 }
                           Expected = [
                               flukeDate 2020 Month.March 09, Disabled
                               flukeDate 2020 Month.March 10, Pending
                               flukeDate 2020 Month.March 11, Pending
                               flukeDate 2020 Month.March 12, Pending
                           ]
                           Events = [
                               TempStatusEntry (flukeDate 2020 Month.March 10, Postponed (Some (flukeTime 01 00)))
                           ] |}
                }

                test "Past PostponedUntil events are shown" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 1)) }
                           Now = { Date = flukeDate 2020 Month.March 13
                                   Time = flukeTime 02 00 }
                           Expected = [
                               flukeDate 2020 Month.March 07, Disabled
                               flukeDate 2020 Month.March 08, UserStatus (Users.testUser, Completed)
                               flukeDate 2020 Month.March 09, Missed
                               flukeDate 2020 Month.March 10, UserStatus (Users.testUser, Postponed (Some (flukeTime 01 00)))
                               flukeDate 2020 Month.March 11, Missed
                               flukeDate 2020 Month.March 12, Pending
                               flukeDate 2020 Month.March 13, Pending
                           ]
                           Events = [
                               TempStatusEntry (flukeDate 2020 Month.March 08, Completed)
                               TempStatusEntry (flukeDate 2020 Month.March 10, Postponed (Some (flukeTime 01 00)))
                           ] |}
                }

                test "Future PostponedUntil events are shown" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 1)) }
                           Now = { Date = flukeDate 2020 Month.March 10
                                   Time = Consts.testDayStart }
                           Expected = [
                               flukeDate 2020 Month.March 09, Disabled
                               flukeDate 2020 Month.March 10, Pending
                               flukeDate 2020 Month.March 11, Pending
                               flukeDate 2020 Month.March 12, UserStatus (Users.testUser, Postponed (Some (flukeTime 13 00)))
                               flukeDate 2020 Month.March 13, Pending
                           ]
                           Events = [
                               TempStatusEntry (flukeDate 2020 Month.March 12, Postponed (Some (flukeTime 13 00)))
                           ] |}
                }

            ]

            testList "Recurrency Offset" [

                test "Start scheduling today without any events" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 2)) }
                           Now = { Date = flukeDate 2020 Month.March 9
                                   Time = Consts.testDayStart }
                           Expected = [
                               flukeDate 2020 Month.March 7, Disabled
                               flukeDate 2020 Month.March 8, Disabled
                               flukeDate 2020 Month.March 9, Pending
                               flukeDate 2020 Month.March 10, Disabled
                               flukeDate 2020 Month.March 11, Pending
                               flukeDate 2020 Month.March 12, Disabled
                           ]
                           Events = [] |}
                }

                test "Disabled today after a Completed event yesterday" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 3)) }
                           Now = { Date = flukeDate 2020 Month.March 9
                                   Time = Consts.testDayStart }
                           Expected = [
                               flukeDate 2020 Month.March 8, UserStatus (Users.testUser, Completed)
                               flukeDate 2020 Month.March 9, Disabled
                               flukeDate 2020 Month.March 10, Disabled
                               flukeDate 2020 Month.March 11, Pending
                               flukeDate 2020 Month.March 12, Disabled
                               flukeDate 2020 Month.March 13, Disabled
                               flukeDate 2020 Month.March 14, Pending
                               flukeDate 2020 Month.March 15, Disabled
                           ]
                           Events = [
                               TempStatusEntry (flukeDate 2020 Month.March 8, Completed)
                           ] |}
                }

                test "Postponing today should schedule for tomorrow" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 2)) }
                           Now = { Date = flukeDate 2020 Month.March 10
                                   Time = Consts.testDayStart }
                           Expected = [
                               flukeDate 2020 Month.March 9, Disabled
                               flukeDate 2020 Month.March 10, UserStatus (Users.testUser, Postponed None)
                               flukeDate 2020 Month.March 11, Pending
                               flukeDate 2020 Month.March 12, Disabled
                               flukeDate 2020 Month.March 13, Pending
                           ]
                           Events = [
                               TempStatusEntry (flukeDate 2020 Month.March 10, Postponed None)
                           ] |}
                }

                test "Postponing today should schedule for tomorrow with PendingAfter" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 2))
                                                      PendingAfter = flukeTime 03 00 |> Some }
                           Now = { Date = flukeDate 2020 Month.March 10
                                   Time = Consts.testDayStart }
                           Expected = [
                               flukeDate 2020 Month.March 9, Disabled
                               flukeDate 2020 Month.March 10, UserStatus (Users.testUser, Postponed None)
                               flukeDate 2020 Month.March 11, Pending
                               flukeDate 2020 Month.March 12, Disabled
                               flukeDate 2020 Month.March 13, Pending
                           ]
                           Events = [
                               TempStatusEntry (flukeDate 2020 Month.March 10, Postponed None)
                           ] |}
                }

                test "(Postponed None) yesterday schedules for today" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 2)) }
                           Now = { Date = flukeDate 2020 Month.March 11
                                   Time = Consts.testDayStart }
                           Expected = [
                               flukeDate 2020 Month.March 9, Disabled
                               flukeDate 2020 Month.March 10, UserStatus (Users.testUser, Postponed None)
                               flukeDate 2020 Month.March 11, Pending
                               flukeDate 2020 Month.March 12, Disabled
                               flukeDate 2020 Month.March 13, Pending
                           ]
                           Events = [
                               TempStatusEntry (flukeDate 2020 Month.March 10, Postponed None)
                           ] |}
                }

                test "Pending today after missing yesterday,
                      then resetting the schedule with a future Completed event" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 2)) }
                           Now = { Date = flukeDate 2020 Month.March 11
                                   Time = Consts.testDayStart }
                           Expected = [
                               flukeDate 2020 Month.March 7, Disabled
                               flukeDate 2020 Month.March 8, UserStatus (Users.testUser, Completed)
                               flukeDate 2020 Month.March 9, Disabled
                               flukeDate 2020 Month.March 10, Missed
                               flukeDate 2020 Month.March 11, Pending
                               flukeDate 2020 Month.March 12, UserStatus (Users.testUser, Completed)
                               flukeDate 2020 Month.March 13, Disabled
                               flukeDate 2020 Month.March 14, Pending
                           ]
                           Events = [
                               TempStatusEntry (flukeDate 2020 Month.March 8, Completed)
                               TempStatusEntry (flukeDate 2020 Month.March 12, Completed)
                           ] |}
                }

                test "Recurring task only Suggested before PendingAfter" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 1))
                                                      PendingAfter = flukeTime 20 00 |> Some }
                           Now = { Date = flukeDate 2020 Month.March 10
                                   Time = flukeTime 19 30 }
                           Expected = [
                               flukeDate 2020 Month.March 9, Disabled
                               flukeDate 2020 Month.March 10, Suggested
                               flukeDate 2020 Month.March 11, Pending
                           ]
                           Events = [] |}
                }

                test "Recurring task Pending after PendingAfter" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 1))
                                                      PendingAfter = flukeTime 20 00 |> Some }
                           Now = { Date = flukeDate 2020 Month.March 10
                                   Time = flukeTime 21 00 }
                           Expected = [
                               flukeDate 2020 Month.March 9, Disabled
                               flukeDate 2020 Month.March 10, Pending
                               flukeDate 2020 Month.March 11, Pending
                           ]
                           Events = [] |}
                }

                test "Recurrency for the next days should work normally
                      while today is still optional/suggested (before PendingAfter)" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 2))
                                                      PendingAfter = flukeTime 18 00 |> Some }
                           Now = { Date = flukeDate 2020 Month.March 27
                                   Time = flukeTime 17 00 }
                           Expected = [
                               flukeDate 2020 Month.March 25, Disabled
                               flukeDate 2020 Month.March 26, Disabled
                               flukeDate 2020 Month.March 27, Suggested
                               flukeDate 2020 Month.March 28, Disabled
                               flukeDate 2020 Month.March 29, Pending
                               flukeDate 2020 Month.March 29, Disabled
                           ]
                           Events = [] |}
                }

                test "Reset counting after a future ManualPending event" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 3)) }
                           Now = { Date = flukeDate 2020 Month.March 28
                                   Time = Consts.testDayStart }
                           Expected = [
                               flukeDate 2020 Month.March 27, Disabled
                               flukeDate 2020 Month.March 28, Pending
                               flukeDate 2020 Month.March 29, Disabled
                               flukeDate 2020 Month.March 30, UserStatus (Users.testUser, ManualPending)
                               flukeDate 2020 Month.March 31, UserStatus (Users.testUser, ManualPending)
                               flukeDate 2020 Month.April 01, Disabled
                               flukeDate 2020 Month.April 02, Disabled
                               flukeDate 2020 Month.April 03, Pending
                           ]
                           Events = [
                               TempStatusEntry (flukeDate 2020 Month.March 30, ManualPending)
                               TempStatusEntry (flukeDate 2020 Month.March 31, ManualPending)
                           ] |}
                }
            ]

            testList "Recurrency Fixed" [

                test "Weekly task, pending today, initialized by past completion" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Saturday ]) }
                           Now = { Date = flukeDate 2020 Month.March 21
                                   Time = Consts.testDayStart }
                           Expected = [
                               for d in 13 .. 29 do
                                   flukeDate 2020 Month.March d,
                                   match d with
                                   | 14 -> UserStatus (Users.testUser, Completed)
                                   | 21 | 28 -> Pending
                                   | _ -> Disabled
                           ]
                           Events = [
                               TempStatusEntry (flukeDate 2020 Month.March 14, Completed)
                           ] |}
                }

                test "Weekly task, missed until today, initialized by past completion" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Wednesday ]) }
                           Now = { Date = flukeDate 2020 Month.March 20
                                   Time = Consts.testDayStart }
                           Expected = [
                               for d in 10 .. 26 do
                                   flukeDate 2020 Month.March d,
                                   match d with
                                   | 13 -> UserStatus (Users.testUser, Completed)
                                   | 18 | 19 -> Missed
                                   | 20 | 25 -> Pending
                                   | _ -> Disabled
                           ]
                           Events = [
                               TempStatusEntry (flukeDate 2020 Month.March 13, Completed)
                           ] |}
                }

                test "Weekly task, (Postponed None) then missed until today, pending tomorrow" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Saturday ]) }
                           Now = { Date = flukeDate 2020 Month.March 20
                                   Time = Consts.testDayStart }
                           Expected = [
                               for d in 13 .. 29 do
                                   flukeDate 2020 Month.March d,
                                   match d with
                                   | 18 -> UserStatus (Users.testUser, Postponed None)
                                   | 19 -> Missed
                                   | 20 | 21 | 28 -> Pending
                                   | _ -> Disabled
                           ]
                           Events = [
                               TempStatusEntry (flukeDate 2020 Month.March 18, Postponed None)
                           ] |}
                }

                test "Weekly task, without past events, pending in a few days" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Wednesday ]) }
                           Now = { Date = flukeDate 2020 Month.March 20
                                   Time = Consts.testDayStart }
                           Expected = [
                               for d in 17 .. 26 do
                                   flukeDate 2020 Month.March d,
                                   match d with
                                   | 25 -> Pending
                                   | _ -> Disabled
                           ]
                           Events = [
                           ] |}
                }

                test "Fixed weekly task, without past events, pending tomorrow" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Saturday ]) }
                           Now = { Date = flukeDate 2020 Month.March 20
                                   Time = Consts.testDayStart }
                           Expected = [
                               for d in 13 .. 29 do
                                   flukeDate 2020 Month.March d,
                                   match d with
                                   | 21 | 28 -> Pending
                                   | _ -> Disabled
                           ]
                           Events = [
                           ] |}
                }
            ]

            testList "Manual" [

                test "Empty manual task" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Manual WithoutSuggestion }
                           Now = { Date = flukeDate 2020 Month.March 11
                                   Time = Consts.testDayStart }
                           Expected = [
                               flukeDate 2020 Month.March 9, Disabled
                               flukeDate 2020 Month.March 10, Disabled
                               flukeDate 2020 Month.March 11, Suggested
                               flukeDate 2020 Month.March 12, Disabled
                               flukeDate 2020 Month.March 13, Disabled
                           ]
                           Events = [] |}
                }

                test "ManualPending task scheduled for today after missing" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Manual WithoutSuggestion }
                           Now = { Date = flukeDate 2020 Month.March 11
                                   Time = Consts.testDayStart }
                           Expected = [
                               flukeDate 2020 Month.March 8, Disabled
                               flukeDate 2020 Month.March 9, UserStatus (Users.testUser, ManualPending)
                               flukeDate 2020 Month.March 10, Missed
                               flukeDate 2020 Month.March 11, Pending
                               flukeDate 2020 Month.March 12, Disabled
                               flukeDate 2020 Month.March 13, Disabled
                           ]
                           Events = [
                               TempStatusEntry (flukeDate 2020 Month.March 9, ManualPending)
                           ] |}
                }

                test "Manual Suggested task Suggested before PendingAfter" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Manual WithSuggestion
                                                      PendingAfter = flukeTime 20 00 |> Some }
                           Now = { Date = flukeDate 2020 Month.March 10
                                   Time = flukeTime 19 30 }
                           Expected = [
                               flukeDate 2020 Month.March 09, Suggested
                               flukeDate 2020 Month.March 10, Suggested
                               flukeDate 2020 Month.March 11, Suggested
                           ]
                           Events = [] |}
                }

                test "Manual Suggested task Pending after PendingAfter" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Manual WithSuggestion
                                                      PendingAfter = flukeTime 20 00 |> Some }
                           Now = { Date = flukeDate 2020 Month.March 10
                                   Time = flukeTime 21 00 }
                           Expected = [
                               flukeDate 2020 Month.March 09, Suggested
                               flukeDate 2020 Month.March 10, Pending
                               flukeDate 2020 Month.March 11, Suggested
                           ]
                           Events = [] |}
                }

                test "Manual Suggested task: Missed ManualPending propagates until today" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Manual WithSuggestion }
                           Now = { Date = flukeDate 2020 Month.March 28
                                   Time = Consts.testDayStart }
                           Expected = [
                               flukeDate 2020 Month.March 25, Suggested
                               flukeDate 2020 Month.March 26, UserStatus (Users.testUser, ManualPending)
                               flukeDate 2020 Month.March 27, Missed
                               flukeDate 2020 Month.March 28, Pending
                               flukeDate 2020 Month.March 29, Suggested
                               flukeDate 2020 Month.March 30, UserStatus (Users.testUser, ManualPending)
                               flukeDate 2020 Month.March 31, Suggested
                           ]
                           Events = [
                               TempStatusEntry (flukeDate 2020 Month.March 26, ManualPending)
                               TempStatusEntry (flukeDate 2020 Month.March 30, ManualPending)
                           ] |}
                }

                test "Manual Suggested task: Suggested mode restored after completing a forgotten ManualPending event" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Manual WithSuggestion }
                           Now = { Date = flukeDate 2020 Month.March 28
                                   Time = Consts.testDayStart }
                           Expected = [
                               flukeDate 2020 Month.March 24, Suggested
                               flukeDate 2020 Month.March 25, UserStatus (Users.testUser, ManualPending)
                               flukeDate 2020 Month.March 26, UserStatus (Users.testUser, Completed)
                               flukeDate 2020 Month.March 27, Suggested
                               flukeDate 2020 Month.March 28, Suggested
                               flukeDate 2020 Month.March 29, Suggested
                           ]
                           Events = [
                               TempStatusEntry (flukeDate 2020 Month.March 25, ManualPending)
                               TempStatusEntry (flukeDate 2020 Month.March 26, Completed)
                           ] |}
                }

                test "Manual Suggested task: Pending today after missing a ManualPending event" {
                    testWithLaneRenderingData
                        {| Task = { Task.Default with Scheduling = Manual WithSuggestion }
                           Now = { Date = flukeDate 2020 Month.March 28
                                   Time = Consts.testDayStart }
                           Expected = [
                               flukeDate 2020 Month.March 24, Suggested
                               flukeDate 2020 Month.March 25, UserStatus (Users.testUser, ManualPending)
                               flukeDate 2020 Month.March 26, Missed
                               flukeDate 2020 Month.March 27, Missed
                               flukeDate 2020 Month.March 28, Pending
                               flukeDate 2020 Month.March 29, Suggested
                           ]
                           Events = [
                               TempStatusEntry (flukeDate 2020 Month.March 25, ManualPending)
                           ] |}
                }
            ]

            testList "Sessions" [
                test "Respect dayStart on session events" {
                    let laneRenderingTestData =
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 1)) }
                           Now = { Date = flukeDate 2020 Month.March 04
                                   Time = Consts.testDayStart }
                           Expected = [
                               flukeDate 2020 Month.February 29, Disabled
                               flukeDate 2020 Month.March 1, Disabled
                               flukeDate 2020 Month.March 2, Disabled
                               flukeDate 2020 Month.March 3, Disabled
                               flukeDate 2020 Month.March 4, Pending
                               flukeDate 2020 Month.March 5, Pending
                               flukeDate 2020 Month.March 6, Pending
                               flukeDate 2020 Month.March 7, Pending
                               flukeDate 2020 Month.March 8, Pending
                           ]
                           Events = [
                               TempSession (flukeDateTime 2020 Month.March 01 11 00)
                               TempSession (flukeDateTime 2020 Month.March 01 13 00)
                               TempSession (flukeDateTime 2020 Month.March 08 11 00)
                               TempSession (flukeDateTime 2020 Month.March 08 13 00)
                           ] |}

                    testWithLaneRenderingData laneRenderingTestData

                    let taskState =
                        applyTaskEvents
                            Consts.testDayStart
                            laneRenderingTestData.Task
                            (laneRenderingTestData.Events |> List.map (fun x -> x, Users.testUser))

                    let sessionsExpected = [
                        flukeDate 2020 Month.February 29, 1
                        flukeDate 2020 Month.March 1, 1
                        flukeDate 2020 Month.March 2, 0
                        flukeDate 2020 Month.March 3, 0
                        flukeDate 2020 Month.March 4, 0
                        flukeDate 2020 Month.March 5, 0
                        flukeDate 2020 Month.March 6, 0
                        flukeDate 2020 Month.March 7, 1
                        flukeDate 2020 Month.March 8, 1
                    ]

                    let dateSequence = sessionsExpected |> List.map fst

                    let sessionCountList =
                        dateSequence
                        |> List.map (fun date ->
                            let sessionCount =
                                taskState.Sessions
                                |> List.filter (fun (TaskSession start) ->
                                    isToday Consts.testDayStart start (DateId date)
                                )
                                |> List.length
                            date, sessionCount
                        )

                    let toString = List.map string >> String.concat Environment.NewLine

                    sessionCountList
                    |> toString
                    |> Expect.equal "" (toString sessionsExpected)
                }
            ]
        ]
    ]

