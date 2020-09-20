namespace Fluke.Shared

open System
open Expecto
open FSharpPlus
open Fluke.Shared
open Expecto.Flip
open Fluke.Shared.TempData
open Suigetsu.Core

module Data =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    let task1 =
        { Task.Default with
            Name = TaskName "1"
        }

    let task2 =
        { Task.Default with
            Name = TaskName "2"
        }

    let task3 =
        { Task.Default with
            Name = TaskName "3"
        }

    let task4 =
        { Task.Default with
            Name = TaskName "4"
        }

    let task5 =
        { Task.Default with
            Name = TaskName "5"
        }

//    let orderedEventList =
//        [
//            {
//                Task = task1
//                Priority = TaskOrderPriority.First
//            }
//            {
//                Task = task2
//                Priority = TaskOrderPriority.LessThan task1
//            }
//            {
//                Task = task3
//                Priority = TaskOrderPriority.LessThan task2
//            }
//            {
//                Task = task4
//                Priority = TaskOrderPriority.LessThan task3
//            }
//            {
//                Task = task5
//                Priority = TaskOrderPriority.Last
//            }
//        ]
//
//    let orderedList = [ task1; task2; task3; task4; task5 ]

module Tests =
    open Data
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    [<Tests>]
    let tests =
        let users = getUsers ()
        testList
            "Tests"
            [
//                testList
//                    "GetManualSortedTaskListTests"
//                    [
//                        test "1" {
//                            orderedEventList
//                            |> Sorting.getManualSortedTaskList
//                            |> Expect.equal "" orderedList
//
//                            [
//                                {
//                                    Task = task5
//                                    Priority = TaskOrderPriority.Last
//                                }
//                                {
//                                    Task = task4
//                                    Priority = TaskOrderPriority.LessThan task3
//                                }
//                                {
//                                    Task = task3
//                                    Priority = TaskOrderPriority.LessThan task2
//                                }
//                                {
//                                    Task = task2
//                                    Priority = TaskOrderPriority.LessThan task1
//                                }
//                                {
//                                    Task = task1
//                                    Priority = TaskOrderPriority.First
//                                }
//                            ]
//                            |> Sorting.getManualSortedTaskList
//                            |> Expect.equal "" orderedList
//
//                            [
//                                {
//                                    Task = task3
//                                    Priority = TaskOrderPriority.LessThan task2
//                                }
//                                {
//                                    Task = task5
//                                    Priority = TaskOrderPriority.Last
//                                }
//                                {
//                                    Task = task4
//                                    Priority = TaskOrderPriority.LessThan task3
//                                }
//                                {
//                                    Task = task1
//                                    Priority = TaskOrderPriority.First
//                                }
//                                {
//                                    Task = task2
//                                    Priority = TaskOrderPriority.LessThan task1
//                                }
//                            ]
//                            |> Sorting.getManualSortedTaskList
//                            |> Expect.equal "" orderedList
//
//                            [
//                                {
//                                    Task = task3
//                                    Priority = TaskOrderPriority.First
//                                }
//                            ]
//                            |> List.append orderedEventList
//                            |> Sorting.getManualSortedTaskList
//                            |> Expect.equal "" [ task3; task4; task1; task2; task5 ]
//
//                            [
//                                {
//                                    Task = task3
//                                    Priority = TaskOrderPriority.First
//                                }
//                                {
//                                    Task = task4
//                                    Priority = TaskOrderPriority.LessThan task2
//                                }
//                            ]
//                            |> List.append orderedEventList
//                            |> Sorting.getManualSortedTaskList
//                            |> Expect.equal "" [ task3; task1; task2; task4; task5 ]
//
//                            [
//                                {
//                                    Task = task1
//                                    Priority = TaskOrderPriority.First
//                                }
//                                {
//                                    Task = task2
//                                    Priority = TaskOrderPriority.First
//                                }
//                                {
//                                    Task = task3
//                                    Priority = TaskOrderPriority.First
//                                }
//                                {
//                                    Task = task4
//                                    Priority = TaskOrderPriority.First
//                                }
//                                {
//                                    Task = task5
//                                    Priority = TaskOrderPriority.LessThan task4
//                                }
//                            ]
//                            |> Sorting.getManualSortedTaskList
//                            |> Expect.equal "" [ task4; task5; task3; task2; task1 ]
//                        }
//                    ]

//                testList
//                    "SetPriorityTests"
//                    [
//                        test "1" {
//                            let createPriorityEvents task priority taskList =
//                                match taskList |> List.tryFindIndexBack ((=) task) with
//                                | None -> None
//                                | Some i ->
//                                    let closest =
//                                        (i + 1, i - 1)
//                                        |> Tuple2.map (fun x -> taskList |> List.tryItem x)
//
//                                    match closest, priority with
//                                    | (Some _, None), TaskOrderPriority.First when i = 0 -> None
//
//                                    | (Some below, None), _ ->
//                                        Some
//                                            {
//                                                Task = below
//                                                Priority = TaskOrderPriority.First
//                                            }
//
//                                    | (Some below, Some above), _ ->
//                                        Some
//                                            {
//                                                Task = below
//                                                Priority = TaskOrderPriority.LessThan above
//                                            }
//
//                                    | _, TaskOrderPriority.First when i > 0 ->
//                                        Some
//                                            {
//                                                Task = taskList |> List.head
//                                                Priority = TaskOrderPriority.LessThan task
//                                            }
//
//                                    | _ -> None
//                                |> Option.toList
//                                |> List.append [ { Task = task; Priority = priority } ]
//
//                            [ task1; task2; task3; task4; task5 ]
//                            |> createPriorityEvents task3 TaskOrderPriority.First
//                            |> Expect.equal
//                                ""
//                                   [
//                                       {
//                                           Task = task3
//                                           Priority = TaskOrderPriority.First
//                                       }
//                                       {
//                                           Task = task4
//                                           Priority = TaskOrderPriority.LessThan task2
//                                       }
//                                   ]
//
//                            [ task1; task2; task3; task4; task5 ]
//                            |> createPriorityEvents task1 TaskOrderPriority.First
//                            |> Expect.equal
//                                ""
//                                   [
//                                       {
//                                           Task = task1
//                                           Priority = TaskOrderPriority.First
//                                       }
//                                   ]
//
//                            [ task1; task2; task3; task4; task5 ]
//                            |> createPriorityEvents task1 TaskOrderPriority.Last
//                            |> Expect.equal
//                                ""
//                                   [
//                                       {
//                                           Task = task1
//                                           Priority = TaskOrderPriority.Last
//                                       }
//                                       {
//                                           Task = task2
//                                           Priority = TaskOrderPriority.First
//                                       }
//                                   ]
//
//                            [ task1; task2; task3; task4; task5 ]
//                            |> createPriorityEvents task5 TaskOrderPriority.Last
//                            |> Expect.equal
//                                ""
//                                   [
//                                       {
//                                           Task = task5
//                                           Priority = TaskOrderPriority.Last
//                                       }
//                                   ]
//
//                            [ task1; task2; task3; task4; task5 ]
//                            |> createPriorityEvents task5 TaskOrderPriority.First
//                            |> Expect.equal
//                                ""
//                                   [
//                                       {
//                                           Task = task5
//                                           Priority = TaskOrderPriority.First
//                                       }
//                                       {
//                                           Task = task1
//                                           Priority = TaskOrderPriority.LessThan task5
//                                       }
//                                   ]
//
//                            [ task1; task2; task3; task4; task5 ]
//                            |> createPriorityEvents task2 TaskOrderPriority.First
//                            |> Expect.equal
//                                ""
//                                   [
//                                       {
//                                           Task = task2
//                                           Priority = TaskOrderPriority.First
//                                       }
//                                       {
//                                           Task = task3
//                                           Priority = TaskOrderPriority.LessThan task1
//                                       }
//                                   ]
//
//                            [ task1; task2; task3; task4; task5 ]
//                            |> createPriorityEvents task4 (TaskOrderPriority.LessThan task1)
//                            |> Expect.equal
//                                ""
//                                   [
//                                       {
//                                           Task = task4
//                                           Priority = TaskOrderPriority.LessThan task1
//                                       }
//                                       {
//                                           Task = task5
//                                           Priority = TaskOrderPriority.LessThan task3
//                                       }
//                                   ]
//                        }
//                    ]

                testList
                    "Lane Sorting"
                    [

                        let (|NoSorting|TimeOfDay|Frequency|) =
                            function
                            | Choice1Of3 _ -> NoSorting
                            | Choice2Of3 _ -> TimeOfDay
                            | Choice3Of3 _ -> Frequency

                        let _noSorting = Choice1Of3 ()
                        let sortByTimeOfDay = Choice2Of3 ()
                        let sortByFrequency = Choice3Of3 ()

                        let testWithLaneSortingData (props: {| Sort: Choice<_, _, _>
                                                               Data: (Task * DslTask list) list
                                                               Expected: string list
                                                               Position: FlukeDateTime |}) =
                            let treeState =
                                let dslData =
                                    TempData.Testing.createLaneSortingDslData
                                        {|
                                            User = users.fluke
                                            Position = props.Position
                                            Expected = props.Expected
                                            Data = props.Data
                                        |}

                                TreeState.Create
                                    (id = TreeId Guid.Empty, name = TreeName "Test", owner = users.fluke)
                                |> mergeDslDataIntoTreeState dslData

                            let dateSequence =
                                treeState.TaskStateMap
                                |> Map.values
                                |> Seq.collect (fun taskState -> taskState.CellStateMap |> Map.keys)
                                |> Seq.toList
                                |> List.map (fun (DateId referenceDay) -> referenceDay)
                                |> Rendering.getDateSequence (35, 35)

                            treeState.TaskStateMap
                            |> Map.values
                            |> Seq.map (Rendering.renderLane users.fluke.DayStart props.Position dateSequence)
                            |> Seq.toList
                            |> fun lanes ->
                                match props.Sort with
                                | NoSorting -> lanes
                                | TimeOfDay -> Sorting.sortLanesByTimeOfDay users.fluke.DayStart props.Position lanes
                                | Frequency -> Sorting.sortLanesByFrequency lanes
                            |> List.map (fun ({ Task = { Name = TaskName name } }, _) -> name)
                            |> Expect.equal "" props.Expected

                        test "Sort by Frequency: All task types mixed" {
                            testWithLaneSortingData
                                {|
                                    Sort = sortByFrequency
                                    Position =
                                        {
                                            Date = FlukeDate.Create 2020 Month.March 10
                                            Time = users.fluke.DayStart
                                        }
                                    Data =
                                        [
                                            { Task.Default with
                                                Name = TaskName "01"
                                                Scheduling = Manual WithSuggestion
                                            },
                                            []

                                            { Task.Default with
                                                Name = TaskName "02"
                                                Scheduling = Manual WithSuggestion
                                            },
                                            [
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 10, Postponed None)
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 08, Postponed None)
                                            ]

                                            { Task.Default with
                                                Name = TaskName "03"
                                                Scheduling = Manual WithoutSuggestion
                                            },
                                            [
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 09, ManualPending)
                                            ]

                                            { Task.Default with
                                                Name = TaskName "04"
                                                Scheduling = Recurrency (Offset (Days 1))
                                                PendingAfter = FlukeTime.Create 20 00 |> Some
                                            },
                                            []

                                            { Task.Default with
                                                Name = TaskName "05"
                                                Scheduling = Manual WithoutSuggestion
                                            },
                                            [
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 10, ManualPending)
                                            ]

                                            { Task.Default with
                                                Name = TaskName "06"
                                                Scheduling = Manual WithoutSuggestion
                                            },
                                            [
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 04, Postponed None)
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 06, Dismissed)
                                            ]

                                            { Task.Default with
                                                Name = TaskName "07"
                                                Scheduling = Recurrency (Offset (Days 4))
                                            },
                                            [
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 08, Completed)
                                            ]

                                            { Task.Default with
                                                Name = TaskName "08"
                                                Scheduling = Recurrency (Offset (Days 2))
                                            },
                                            [
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 10, Completed)
                                            ]

                                            { Task.Default with
                                                Name = TaskName "09"
                                                Scheduling = Recurrency (Offset (Days 2))
                                            },
                                            [
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 10, Dismissed)
                                            ]

                                            { Task.Default with
                                                Name = TaskName "10"
                                                Scheduling = Recurrency (Offset (Days 2))
                                            },
                                            [
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 10, Postponed None)
                                            ]

                                            { Task.Default with
                                                Name = TaskName "11"
                                                Scheduling = Recurrency (Offset (Days 1))
                                            },
                                            []

                                            { Task.Default with
                                                Name = TaskName "12"
                                                Scheduling = Manual WithoutSuggestion
                                            },
                                            []

                                            { Task.Default with
                                                Name = TaskName "13"
                                                Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Tuesday ])
                                            },
                                            []

                                            { Task.Default with
                                                Name = TaskName "14"
                                                Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Wednesday ])
                                            },
                                            []

                                            { Task.Default with
                                                Name = TaskName "15"
                                                Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Friday ])
                                            },
                                            [
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 07, Postponed None)
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 09, Dismissed)
                                            ]
                                        ]
                                    Expected =
                                        [
                                            "11"
                                            "04"
                                            "10"
                                            "08"
                                            "09"
                                            "07"
                                            "15"
                                            "13"
                                            "14"
                                            "02"
                                            "06"
                                            "03"
                                            "05"
                                            "01"
                                            "12"
                                        ]
                                |}
                        }

                        test "Sort by Time of Day: All task types mixed" {
                            testWithLaneSortingData
                                {|
                                    Sort = sortByTimeOfDay
                                    Position =
                                        {
                                            Date = FlukeDate.Create 2020 Month.March 10
                                            Time = FlukeTime.Create 14 00
                                        }
                                    Data =
                                        [
                                            { Task.Default with
                                                Name = TaskName "01"
                                                Scheduling = Manual WithSuggestion
                                            },
                                            []

                                            { Task.Default with
                                                Name = TaskName "02"
                                                Scheduling = Manual WithSuggestion
                                            },
                                            [
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 10, Postponed None)
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 08, Postponed None)
                                            ]

                                            { Task.Default with
                                                Name = TaskName "03"
                                                Scheduling = Manual WithoutSuggestion
                                            },
                                            [
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 09, ManualPending)
                                            ]

                                            { Task.Default with
                                                Name = TaskName "04"
                                                Scheduling = Recurrency (Offset (Days 1))
                                                PendingAfter = FlukeTime.Create 20 00 |> Some
                                            },
                                            []

                                            { Task.Default with
                                                Name = TaskName "05"
                                                Scheduling = Manual WithoutSuggestion
                                            },
                                            [
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 10, ManualPending)
                                            ]

                                            { Task.Default with
                                                Name = TaskName "06"
                                                Scheduling = Manual WithoutSuggestion
                                            },
                                            [
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 04, Postponed None)
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 06, Dismissed)
                                            ]

                                            { Task.Default with
                                                Name = TaskName "07"
                                                Scheduling = Recurrency (Offset (Days 4))
                                            },
                                            [
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 08, Completed)
                                            ]

                                            { Task.Default with
                                                Name = TaskName "08"
                                                Scheduling = Recurrency (Offset (Days 2))
                                            },
                                            [
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 10, Completed)
                                            ]

                                            { Task.Default with
                                                Name = TaskName "09"
                                                Scheduling = Recurrency (Offset (Days 2))
                                            },
                                            [
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 10, Dismissed)
                                            ]

                                            { Task.Default with
                                                Name = TaskName "10"
                                                Scheduling = Recurrency (Offset (Days 2))
                                            },
                                            [
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 10, Postponed None)
                                            ]

                                            { Task.Default with
                                                Name = TaskName "11"
                                                Scheduling = Recurrency (Offset (Days 1))
                                            },
                                            [
                                                DslStatusEntry
                                                    (FlukeDate.Create 2020 Month.March 10,
                                                     Postponed (FlukeTime.Create 13 00 |> Some))
                                            ]

                                            { Task.Default with
                                                Name = TaskName "12"
                                                Scheduling = Manual WithoutSuggestion
                                            },
                                            []

                                            { Task.Default with
                                                Name = TaskName "13"
                                                Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Tuesday ])
                                            },
                                            []

                                            { Task.Default with
                                                Name = TaskName "14"
                                                Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Wednesday ])
                                            },
                                            []

                                            { Task.Default with
                                                Name = TaskName "15"
                                                Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Friday ])
                                            },
                                            [
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 07, Postponed None)
                                                DslStatusEntry (FlukeDate.Create 2020 Month.March 09, Dismissed)
                                            ]

                                            { Task.Default with
                                                Name = TaskName "16"
                                                Scheduling = Recurrency (Offset (Days 1))
                                                MissedAfter = (FlukeTime.Create 13 00 |> Some)
                                            },
                                            []

                                            { Task.Default with
                                                Name = TaskName "17"
                                                Scheduling = Recurrency (Offset (Days 1))
                                            },
                                            [
                                                DslStatusEntry
                                                    (FlukeDate.Create 2020 Month.March 10,
                                                     Postponed (FlukeTime.Create 15 00 |> Some))
                                            ]

                                            { Task.Default with
                                                Name = TaskName "18"
                                                Scheduling = Recurrency (Offset (Days 1))
                                            },
                                            []
                                        ]
                                    Expected =
                                        [
                                            "16"
                                            "05"
                                            "03"
                                            "11"
                                            "13"
                                            "18"
                                            "17"
                                            "04"
                                            "01"
                                            "02"
                                            "10"
                                            "08"
                                            "09"
                                            "06"
                                            "07"
                                            "12"
                                            "14"
                                            "15"
                                        ]
                                |}
                        }
                    ]

                testList
                    "Lane Rendering"
                    [

                        let testWithLaneRenderingData (props: {| Position: FlukeDateTime
                                                                 Task: Task
                                                                 Events: DslTask list
                                                                 Expected: (FlukeDate * CellStatus) list |}) =
                            let treeState =
                                let dslData =
                                    TempData.Testing.createLaneRenderingDslData
                                        {|
                                            User = users.fluke
                                            Position = props.Position
                                            Task = props.Task
                                            Events = props.Events
                                            Expected = props.Expected
                                        |}

                                TreeState.Create
                                    (id = TreeId Guid.Empty, name = TreeName "Test", owner = users.fluke)
                                |> mergeDslDataIntoTreeState dslData

                            let taskState = treeState.TaskStateMap.[props.Task]

                            let dateSequence = props.Expected |> List.map fst

                            let toString =
                                List.map string
                                >> String.concat Environment.NewLine

                            Rendering.renderLane users.fluke.DayStart props.Position dateSequence taskState
                            |> fun (taskState, cells) ->
                                cells
                                |> List.map (fun (address, status) -> string address.DateId, status)
                            |> toString
                            |> Expect.equal
                                ""
                                   (props.Expected
                                    |> List.map (fun (date, cellStatus) -> string (DateId date), cellStatus)
                                    |> toString)

                        testList
                            "Postponed Until"
                            [
                                test "Postponed until later" {
                                    testWithLaneRenderingData
                                        {|
                                            Task =
                                                { Task.Default with
                                                    Scheduling = Recurrency (Offset (Days 1))
                                                }
                                            Position =
                                                {
                                                    Date = FlukeDate.Create 2020 Month.March 10
                                                    Time = users.fluke.DayStart
                                                }
                                            Expected =
                                                [
                                                    FlukeDate.Create 2020 Month.March 09, Disabled
                                                    FlukeDate.Create 2020 Month.March 10,
                                                    UserStatus (users.fluke, Postponed (Some (FlukeTime.Create 23 00)))
                                                    FlukeDate.Create 2020 Month.March 11, Pending
                                                    FlukeDate.Create 2020 Month.March 12, Pending
                                                ]
                                            Events =
                                                [
                                                    DslStatusEntry
                                                        (FlukeDate.Create 2020 Month.March 10,
                                                         Postponed (Some (FlukeTime.Create 23 00)))
                                                ]
                                        |}
                                }

                                test "Postponed until after midnight" {
                                    testWithLaneRenderingData
                                        {|
                                            Task =
                                                { Task.Default with
                                                    Scheduling = Recurrency (Offset (Days 1))
                                                }
                                            Position =
                                                {
                                                    Date = FlukeDate.Create 2020 Month.March 10
                                                    Time = users.fluke.DayStart
                                                }
                                            Expected =
                                                [
                                                    FlukeDate.Create 2020 Month.March 09, Disabled
                                                    FlukeDate.Create 2020 Month.March 10,
                                                    UserStatus (users.fluke, Postponed (Some (FlukeTime.Create 01 00)))
                                                    FlukeDate.Create 2020 Month.March 11, Pending
                                                    FlukeDate.Create 2020 Month.March 12, Pending
                                                ]
                                            Events =
                                                [
                                                    DslStatusEntry
                                                        (FlukeDate.Create 2020 Month.March 10,
                                                         Postponed (Some (FlukeTime.Create 01 00)))
                                                ]
                                        |}
                                }

                                test "Pending after expiration of Postponed (before midnight)" {
                                    testWithLaneRenderingData
                                        {|
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
                                                        (FlukeDate.Create 2020 Month.March 10,
                                                         Postponed (Some (FlukeTime.Create 23 00)))
                                                ]
                                        |}
                                }

                                test "Pending after expiration of Postponed (after midnight)" {
                                    testWithLaneRenderingData
                                        {|
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
                                                        (FlukeDate.Create 2020 Month.March 10,
                                                         Postponed (Some (FlukeTime.Create 01 00)))
                                                ]
                                        |}
                                }

                                test "Past PostponedUntil events are shown" {
                                    testWithLaneRenderingData
                                        {|
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
                                                    FlukeDate.Create 2020 Month.March 08,
                                                    UserStatus (users.fluke, Completed)
                                                    FlukeDate.Create 2020 Month.March 09, Missed
                                                    FlukeDate.Create 2020 Month.March 10,
                                                    UserStatus (users.fluke, Postponed (Some (FlukeTime.Create 01 00)))
                                                    FlukeDate.Create 2020 Month.March 11, Missed
                                                    FlukeDate.Create 2020 Month.March 12, Pending
                                                    FlukeDate.Create 2020 Month.March 13, Pending
                                                ]
                                            Events =
                                                [
                                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 08, Completed)
                                                    DslStatusEntry
                                                        (FlukeDate.Create 2020 Month.March 10,
                                                         Postponed (Some (FlukeTime.Create 01 00)))
                                                ]
                                        |}
                                }

                                test "Future PostponedUntil events are shown" {
                                    testWithLaneRenderingData
                                        {|
                                            Task =
                                                { Task.Default with
                                                    Scheduling = Recurrency (Offset (Days 1))
                                                }
                                            Position =
                                                {
                                                    Date = FlukeDate.Create 2020 Month.March 10
                                                    Time = users.fluke.DayStart
                                                }
                                            Expected =
                                                [
                                                    FlukeDate.Create 2020 Month.March 09, Disabled
                                                    FlukeDate.Create 2020 Month.March 10, Pending
                                                    FlukeDate.Create 2020 Month.March 11, Pending
                                                    FlukeDate.Create 2020 Month.March 12,
                                                    UserStatus (users.fluke, Postponed (Some (FlukeTime.Create 13 00)))
                                                    FlukeDate.Create 2020 Month.March 13, Pending
                                                ]
                                            Events =
                                                [
                                                    DslStatusEntry
                                                        (FlukeDate.Create 2020 Month.March 12,
                                                         Postponed (Some (FlukeTime.Create 13 00)))
                                                ]
                                        |}
                                }

                            ]

                        testList
                            "Recurrency Offset"
                            [
                                test "Start scheduling today without any events" {
                                    testWithLaneRenderingData
                                        {|
                                            Task =
                                                { Task.Default with
                                                    Scheduling = Recurrency (Offset (Days 2))
                                                }
                                            Position =
                                                {
                                                    Date = FlukeDate.Create 2020 Month.March 9
                                                    Time = users.fluke.DayStart
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
                                        |}
                                }

                                test "Disabled today after a Completed event yesterday" {
                                    testWithLaneRenderingData
                                        {|
                                            Task =
                                                { Task.Default with
                                                    Scheduling = Recurrency (Offset (Days 3))
                                                }
                                            Position =
                                                {
                                                    Date = FlukeDate.Create 2020 Month.March 9
                                                    Time = users.fluke.DayStart
                                                }
                                            Expected =
                                                [
                                                    FlukeDate.Create 2020 Month.March 8,
                                                    UserStatus (users.fluke, Completed)
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
                                        |}
                                }

                                test "Postponing today should schedule for tomorrow" {
                                    testWithLaneRenderingData
                                        {|
                                            Task =
                                                { Task.Default with
                                                    Scheduling = Recurrency (Offset (Days 2))
                                                }
                                            Position =
                                                {
                                                    Date = FlukeDate.Create 2020 Month.March 10
                                                    Time = users.fluke.DayStart
                                                }
                                            Expected =
                                                [
                                                    FlukeDate.Create 2020 Month.March 9, Disabled
                                                    FlukeDate.Create 2020 Month.March 10,
                                                    UserStatus (users.fluke, Postponed None)
                                                    FlukeDate.Create 2020 Month.March 11, Pending
                                                    FlukeDate.Create 2020 Month.March 12, Disabled
                                                    FlukeDate.Create 2020 Month.March 13, Pending
                                                ]
                                            Events =
                                                [
                                                    DslStatusEntry
                                                        (FlukeDate.Create 2020 Month.March 10, Postponed None)
                                                ]
                                        |}
                                }

                                test "Postponing today should schedule for tomorrow with PendingAfter" {
                                    testWithLaneRenderingData
                                        {|
                                            Task =
                                                { Task.Default with
                                                    Scheduling = Recurrency (Offset (Days 2))
                                                    PendingAfter = FlukeTime.Create 03 00 |> Some
                                                }
                                            Position =
                                                {
                                                    Date = FlukeDate.Create 2020 Month.March 10
                                                    Time = users.fluke.DayStart
                                                }
                                            Expected =
                                                [
                                                    FlukeDate.Create 2020 Month.March 9, Disabled
                                                    FlukeDate.Create 2020 Month.March 10,
                                                    UserStatus (users.fluke, Postponed None)
                                                    FlukeDate.Create 2020 Month.March 11, Pending
                                                    FlukeDate.Create 2020 Month.March 12, Disabled
                                                    FlukeDate.Create 2020 Month.March 13, Pending
                                                ]
                                            Events =
                                                [
                                                    DslStatusEntry
                                                        (FlukeDate.Create 2020 Month.March 10, Postponed None)
                                                ]
                                        |}
                                }

                                test "(Postponed None) yesterday schedules for today" {
                                    testWithLaneRenderingData
                                        {|
                                            Task =
                                                { Task.Default with
                                                    Scheduling = Recurrency (Offset (Days 2))
                                                }
                                            Position =
                                                {
                                                    Date = FlukeDate.Create 2020 Month.March 11
                                                    Time = users.fluke.DayStart
                                                }
                                            Expected =
                                                [
                                                    FlukeDate.Create 2020 Month.March 9, Disabled
                                                    FlukeDate.Create 2020 Month.March 10,
                                                    UserStatus (users.fluke, Postponed None)
                                                    FlukeDate.Create 2020 Month.March 11, Pending
                                                    FlukeDate.Create 2020 Month.March 12, Disabled
                                                    FlukeDate.Create 2020 Month.March 13, Pending
                                                ]
                                            Events =
                                                [
                                                    DslStatusEntry
                                                        (FlukeDate.Create 2020 Month.March 10, Postponed None)
                                                ]
                                        |}
                                }

                                test "Pending today after missing yesterday,
                      then resetting the schedule with a future Completed event" {
                                    testWithLaneRenderingData
                                        {|
                                            Task =
                                                { Task.Default with
                                                    Scheduling = Recurrency (Offset (Days 2))
                                                }
                                            Position =
                                                {
                                                    Date = FlukeDate.Create 2020 Month.March 11
                                                    Time = users.fluke.DayStart
                                                }
                                            Expected =
                                                [
                                                    FlukeDate.Create 2020 Month.March 7, Disabled
                                                    FlukeDate.Create 2020 Month.March 8,
                                                    UserStatus (users.fluke, Completed)
                                                    FlukeDate.Create 2020 Month.March 9, Disabled
                                                    FlukeDate.Create 2020 Month.March 10, Missed
                                                    FlukeDate.Create 2020 Month.March 11, Pending
                                                    FlukeDate.Create 2020 Month.March 12,
                                                    UserStatus (users.fluke, Completed)
                                                    FlukeDate.Create 2020 Month.March 13, Disabled
                                                    FlukeDate.Create 2020 Month.March 14, Pending
                                                ]
                                            Events =
                                                [
                                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 8, Completed)
                                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 12, Completed)
                                                ]
                                        |}
                                }

                                test "Recurring task only Suggested before PendingAfter" {
                                    testWithLaneRenderingData
                                        {|
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
                                        |}
                                }

                                test "Recurring task Pending after PendingAfter" {
                                    testWithLaneRenderingData
                                        {|
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
                                        |}
                                }

                                test "Recurrency for the next days should work normally
                      while today is still optional/suggested (before PendingAfter)" {
                                    testWithLaneRenderingData
                                        {|
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
                                        |}
                                }

                                test "Reset counting after a future ManualPending event" {
                                    testWithLaneRenderingData
                                        {|
                                            Task =
                                                { Task.Default with
                                                    Scheduling = Recurrency (Offset (Days 3))
                                                }
                                            Position =
                                                {
                                                    Date = FlukeDate.Create 2020 Month.March 28
                                                    Time = users.fluke.DayStart
                                                }
                                            Expected =
                                                [
                                                    FlukeDate.Create 2020 Month.March 27, Disabled
                                                    FlukeDate.Create 2020 Month.March 28, Pending
                                                    FlukeDate.Create 2020 Month.March 29, Disabled
                                                    FlukeDate.Create 2020 Month.March 30,
                                                    UserStatus (users.fluke, ManualPending)
                                                    FlukeDate.Create 2020 Month.March 31,
                                                    UserStatus (users.fluke, ManualPending)
                                                    FlukeDate.Create 2020 Month.April 01, Disabled
                                                    FlukeDate.Create 2020 Month.April 02, Disabled
                                                    FlukeDate.Create 2020 Month.April 03, Pending
                                                ]
                                            Events =
                                                [
                                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 30, ManualPending)
                                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 31, ManualPending)
                                                ]
                                        |}
                                }
                            ]

                        testList
                            "Recurrency Fixed"
                            [
                                test "Weekly task, pending today, initialized by past completion" {
                                    testWithLaneRenderingData
                                        {|
                                            Task =
                                                { Task.Default with
                                                    Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Saturday ])
                                                }
                                            Position =
                                                {
                                                    Date = FlukeDate.Create 2020 Month.March 21
                                                    Time = users.fluke.DayStart
                                                }
                                            Expected =
                                                [
                                                    for d in 13 .. 29 do
                                                        FlukeDate.Create 2020 Month.March d,
                                                        match d with
                                                        | 14 -> UserStatus (users.fluke, Completed)
                                                        | 21
                                                        | 28 -> Pending
                                                        | _ -> Disabled
                                                ]
                                            Events =
                                                [
                                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 14, Completed)
                                                ]
                                        |}
                                }

                                test "Weekly task, missed until today, initialized by past completion" {
                                    testWithLaneRenderingData
                                        {|
                                            Task =
                                                { Task.Default with
                                                    Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Wednesday ])
                                                }
                                            Position =
                                                {
                                                    Date = FlukeDate.Create 2020 Month.March 20
                                                    Time = users.fluke.DayStart
                                                }
                                            Expected =
                                                [
                                                    for d in 10 .. 26 do
                                                        FlukeDate.Create 2020 Month.March d,
                                                        match d with
                                                        | 13 -> UserStatus (users.fluke, Completed)
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
                                        |}
                                }

                                test "Weekly task, (Postponed None) then missed until today, pending tomorrow" {
                                    testWithLaneRenderingData
                                        {|
                                            Task =
                                                { Task.Default with
                                                    Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Saturday ])
                                                }
                                            Position =
                                                {
                                                    Date = FlukeDate.Create 2020 Month.March 20
                                                    Time = users.fluke.DayStart
                                                }
                                            Expected =
                                                [
                                                    for d in 13 .. 29 do
                                                        FlukeDate.Create 2020 Month.March d,
                                                        match d with
                                                        | 18 -> UserStatus (users.fluke, Postponed None)
                                                        | 19 -> Missed
                                                        | 20
                                                        | 21
                                                        | 28 -> Pending
                                                        | _ -> Disabled
                                                ]
                                            Events =
                                                [
                                                    DslStatusEntry
                                                        (FlukeDate.Create 2020 Month.March 18, Postponed None)
                                                ]
                                        |}
                                }

                                test "Weekly task, without past events, pending in a few days" {
                                    testWithLaneRenderingData
                                        {|
                                            Task =
                                                { Task.Default with
                                                    Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Wednesday ])
                                                }
                                            Position =
                                                {
                                                    Date = FlukeDate.Create 2020 Month.March 20
                                                    Time = users.fluke.DayStart
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
                                        |}
                                }

                                test "Fixed weekly task, without past events, pending tomorrow" {
                                    testWithLaneRenderingData
                                        {|
                                            Task =
                                                { Task.Default with
                                                    Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Saturday ])
                                                }
                                            Position =
                                                {
                                                    Date = FlukeDate.Create 2020 Month.March 20
                                                    Time = users.fluke.DayStart
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
                                        |}
                                }

                                test "Fixed weekly task only Suggested before PendingAfter" {
                                    testWithLaneRenderingData
                                        {|
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
                                                    Time = users.fluke.DayStart
                                                }
                                            Expected =
                                                [
                                                    FlukeDate.Create 2020 Month.August 25, Disabled
                                                    FlukeDate.Create 2020 Month.August 26, Suggested
                                                    FlukeDate.Create 2020 Month.August 27, Pending
                                                ]
                                            Events = []
                                        |}
                                }
                            ]

                        testList
                            "Manual"
                            [
                                test "Empty manual task" {
                                    testWithLaneRenderingData
                                        {|
                                            Task =
                                                { Task.Default with
                                                    Scheduling = Manual WithoutSuggestion
                                                }
                                            Position =
                                                {
                                                    Date = FlukeDate.Create 2020 Month.March 11
                                                    Time = users.fluke.DayStart
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
                                        |}
                                }

                                test "ManualPending task scheduled for today after missing" {
                                    testWithLaneRenderingData
                                        {|
                                            Task =
                                                { Task.Default with
                                                    Scheduling = Manual WithoutSuggestion
                                                }
                                            Position =
                                                {
                                                    Date = FlukeDate.Create 2020 Month.March 11
                                                    Time = users.fluke.DayStart
                                                }
                                            Expected =
                                                [
                                                    FlukeDate.Create 2020 Month.March 8, Disabled
                                                    FlukeDate.Create 2020 Month.March 9,
                                                    UserStatus (users.fluke, ManualPending)
                                                    FlukeDate.Create 2020 Month.March 10, Missed
                                                    FlukeDate.Create 2020 Month.March 11, Pending
                                                    FlukeDate.Create 2020 Month.March 12, Disabled
                                                    FlukeDate.Create 2020 Month.March 13, Disabled
                                                ]
                                            Events =
                                                [
                                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 9, ManualPending)
                                                ]
                                        |}
                                }

                                test "Manual Suggested task Suggested before PendingAfter" {
                                    testWithLaneRenderingData
                                        {|
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
                                        |}
                                }

                                test "Manual Suggested task Pending after PendingAfter" {
                                    testWithLaneRenderingData
                                        {|
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
                                        |}
                                }

                                test "Manual Suggested task: Missed ManualPending propagates until today" {
                                    testWithLaneRenderingData
                                        {|
                                            Task =
                                                { Task.Default with
                                                    Scheduling = Manual WithSuggestion
                                                }
                                            Position =
                                                {
                                                    Date = FlukeDate.Create 2020 Month.March 28
                                                    Time = users.fluke.DayStart
                                                }
                                            Expected =
                                                [
                                                    FlukeDate.Create 2020 Month.March 25, Suggested
                                                    FlukeDate.Create 2020 Month.March 26,
                                                    UserStatus (users.fluke, ManualPending)
                                                    FlukeDate.Create 2020 Month.March 27, Missed
                                                    FlukeDate.Create 2020 Month.March 28, Pending
                                                    FlukeDate.Create 2020 Month.March 29, Suggested
                                                    FlukeDate.Create 2020 Month.March 30,
                                                    UserStatus (users.fluke, ManualPending)
                                                    FlukeDate.Create 2020 Month.March 31, Suggested
                                                ]
                                            Events =
                                                [
                                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 26, ManualPending)
                                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 30, ManualPending)
                                                ]
                                        |}
                                }

                                test
                                    "Manual Suggested task: Suggested mode restored after completing a forgotten ManualPending event" {
                                    testWithLaneRenderingData
                                        {|
                                            Task =
                                                { Task.Default with
                                                    Scheduling = Manual WithSuggestion
                                                }
                                            Position =
                                                {
                                                    Date = FlukeDate.Create 2020 Month.March 28
                                                    Time = users.fluke.DayStart
                                                }
                                            Expected =
                                                [
                                                    FlukeDate.Create 2020 Month.March 24, Suggested
                                                    FlukeDate.Create 2020 Month.March 25,
                                                    UserStatus (users.fluke, ManualPending)
                                                    FlukeDate.Create 2020 Month.March 26,
                                                    UserStatus (users.fluke, Completed)
                                                    FlukeDate.Create 2020 Month.March 27, Suggested
                                                    FlukeDate.Create 2020 Month.March 28, Suggested
                                                    FlukeDate.Create 2020 Month.March 29, Suggested
                                                ]
                                            Events =
                                                [
                                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 25, ManualPending)
                                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 26, Completed)
                                                ]
                                        |}
                                }

                                test "Manual Suggested task: Pending today after missing a ManualPending event" {
                                    testWithLaneRenderingData
                                        {|
                                            Task =
                                                { Task.Default with
                                                    Scheduling = Manual WithSuggestion
                                                }
                                            Position =
                                                {
                                                    Date = FlukeDate.Create 2020 Month.March 28
                                                    Time = users.fluke.DayStart
                                                }
                                            Expected =
                                                [
                                                    FlukeDate.Create 2020 Month.March 24, Suggested
                                                    FlukeDate.Create 2020 Month.March 25,
                                                    UserStatus (users.fluke, ManualPending)
                                                    FlukeDate.Create 2020 Month.March 26, Missed
                                                    FlukeDate.Create 2020 Month.March 27, Missed
                                                    FlukeDate.Create 2020 Month.March 28, Pending
                                                    FlukeDate.Create 2020 Month.March 29, Suggested
                                                ]
                                            Events =
                                                [
                                                    DslStatusEntry (FlukeDate.Create 2020 Month.March 25, ManualPending)
                                                ]
                                        |}
                                }
                            ]

                        testList
                            "Sessions"
                            [
                                test "Respect dayStart on session events" {
                                    let laneRenderingTestData =
                                        {|
                                            Task =
                                                { Task.Default with
                                                    Scheduling = Recurrency (Offset (Days 1))
                                                }
                                            Position =
                                                {
                                                    Date = FlukeDate.Create 2020 Month.March 04
                                                    Time = users.fluke.DayStart
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
                                        |}

                                    testWithLaneRenderingData laneRenderingTestData

                                    let taskState =
                                        createTaskState
                                            laneRenderingTestData.Position
                                            laneRenderingTestData.Task
                                            None
                                            (laneRenderingTestData.Events
                                             |> List.map (fun x -> x, users.fluke))
                                        |> fst

                                    let sessionsExpected =
                                        [
                                            FlukeDate.Create 2020 Month.February 29, 1
                                            FlukeDate.Create 2020 Month.March 1, 1
                                            FlukeDate.Create 2020 Month.March 2, 0
                                            FlukeDate.Create 2020 Month.March 3, 0
                                            FlukeDate.Create 2020 Month.March 4, 0
                                            FlukeDate.Create 2020 Month.March 5, 0
                                            FlukeDate.Create 2020 Month.March 6, 0
                                            FlukeDate.Create 2020 Month.March 7, 1
                                            FlukeDate.Create 2020 Month.March 8, 1
                                        ]

                                    let dateSequence = sessionsExpected |> List.map fst

                                    let sessionCountList =
                                        dateSequence
                                        |> List.map (fun date ->
                                            let sessionCount =
                                                taskState.Sessions
                                                |> List.filter (fun (TaskSession (start, _, _)) ->
                                                    isToday users.fluke.DayStart start (DateId date))
                                                |> List.length

                                            date, sessionCount)

                                    let toString =
                                        List.map string
                                        >> String.concat Environment.NewLine

                                    sessionCountList
                                    |> toString
                                    |> Expect.equal "" (toString sessionsExpected)
                                }
                            ]
                    ]

                testList
                    "Temp"
                    [
                        test "Temp" {
                            let tree1 = [ "a"; "b"; "c" ]
                            let tree2 = [ "d"; "e"; "f" ]

                            ()
                        }
                    ]
            ]
