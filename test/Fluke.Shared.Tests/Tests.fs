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

    let task1 = { Task.Default with Name = TaskName "1" }

    let task2 = { Task.Default with Name = TaskName "2" }

    let task3 = { Task.Default with Name = TaskName "3" }

    let task4 = { Task.Default with Name = TaskName "4" }

    let task5 = { Task.Default with Name = TaskName "5" }

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
    open Templates



    let testWithLaneRenderingData (props: DslTemplate) =

        let user = getUsers().fluke

        let treeState =
            let dslData =
                TempData.Testing.createLaneRenderingDslData
                    {|
                        User = user
                        Position = props.Position
                        Task = props.Task
                        Events = props.Events
                        Expected = props.Expected
                    |}

            TreeState.Create (name = TreeName "Test", owner = user)
            |> mergeDslDataIntoTreeState dslData

        let taskState = treeState.TaskStateMap.[props.Task]

        let dateSequence = props.Expected |> List.map fst

        let toString =
            List.map string
            >> String.concat Environment.NewLine

        Rendering.renderLane user.DayStart props.Position dateSequence taskState
        |> fun (taskState, cells) ->
            cells
            |> List.map (fun (address, status) -> string address.DateId, status)
        |> toString
        |> Expect.equal
            ""
               (props.Expected
                |> List.map (fun (date, cellStatus) -> string (DateId date), cellStatus)
                |> toString)

    let createTests testTree =
        testTree
        |> List.map (fun (name1, list) ->
            testList
                name1
                [
                    yield! list
                           |> List.map (fun (name2, list) ->
                               testList
                                   name2
                                   [
                                       yield! list
                                              |> List.map (fun (name3, obj: DslTemplate) ->
                                                  test name3 { testWithLaneRenderingData obj })
                                   ])
                ])

    let getTreeTests () =
        let tree = getTree ()
        let tests = createTests tree
        tests



    [<Tests>]
    let tests =
        let user = getUsers().fluke
        testList
            "Tests"
            [
                yield! getTreeTests ()

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
                                            User = user
                                            Position = props.Position
                                            Expected = props.Expected
                                            Data = props.Data
                                        |}

                                TreeState.Create (name = TreeName "Test", owner = user)
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
                            |> Seq.map (Rendering.renderLane user.DayStart props.Position dateSequence)
                            |> Seq.toList
                            |> fun lanes ->
                                match props.Sort with
                                | NoSorting -> lanes
                                | TimeOfDay -> Sorting.sortLanesByTimeOfDay user.DayStart props.Position lanes
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
                                            Time = user.DayStart
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
                                                Scheduling =
                                                    Recurrency
                                                        (Fixed
                                                            [
                                                                Weekly DayOfWeek.Tuesday
                                                            ])
                                            },
                                            []

                                            { Task.Default with
                                                Name = TaskName "14"
                                                Scheduling =
                                                    Recurrency
                                                        (Fixed
                                                            [
                                                                Weekly DayOfWeek.Wednesday
                                                            ])
                                            },
                                            []

                                            { Task.Default with
                                                Name = TaskName "15"
                                                Scheduling =
                                                    Recurrency
                                                        (Fixed
                                                            [
                                                                Weekly DayOfWeek.Friday
                                                            ])
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
                                                Scheduling =
                                                    Recurrency
                                                        (Fixed
                                                            [
                                                                Weekly DayOfWeek.Tuesday
                                                            ])
                                            },
                                            []

                                            { Task.Default with
                                                Name = TaskName "14"
                                                Scheduling =
                                                    Recurrency
                                                        (Fixed
                                                            [
                                                                Weekly DayOfWeek.Wednesday
                                                            ])
                                            },
                                            []

                                            { Task.Default with
                                                Name = TaskName "15"
                                                Scheduling =
                                                    Recurrency
                                                        (Fixed
                                                            [
                                                                Weekly DayOfWeek.Friday
                                                            ])
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
                    "Session Data"
                    [
                        testList
                            "Sessions"
                            [
                                test "Session Data/Sessions/Respect dayStart on session events" {
                                    let treeMap = getTreeMap ()

                                    let dslTemplate =
                                        treeMap.["Session Data/Sessions/Respect dayStart on session events"]

                                    testWithLaneRenderingData dslTemplate






                                    let treeState =
                                        let dslData =
                                            TempData.Testing.createLaneRenderingDslData
                                                {|
                                                    User = user
                                                    Position = dslTemplate.Position
                                                    Task = dslTemplate.Task
                                                    Events = dslTemplate.Events
                                                    Expected = dslTemplate.Expected
                                                |}

                                        TreeState.Create (name = TreeName "Test", owner = user)
                                        |> mergeDslDataIntoTreeState dslData


                                    let dateSequence = dslTemplate.Expected |> List.map fst


                                    let treeId = TreeId Guid.Empty

                                    let treeStateMap =
                                        [
                                            treeId, treeState
                                        ]
                                        |> Map.ofList


                                    let sessionData =
                                        View.getSessionData
                                            {|
                                                User = user
                                                DateSequence = dateSequence
                                                View = View.View.Calendar
                                                Position = dslTemplate.Position
                                                TreeStateMap = treeStateMap
                                                TreeSelectionIds =
                                                    [
                                                        treeId
                                                    ]
                                                    |> Set.ofList
                                            |}

                                    let taskState = sessionData.TaskStateMap.[dslTemplate.Task]

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
                                                    isToday user.DayStart start (DateId date))
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
                            let tree1 =
                                [
                                    "a"
                                    "b"
                                    "c"
                                ]

                            let tree2 =
                                [
                                    "d"
                                    "e"
                                    "f"
                                ]

                            ()
                        }
                    ]
            ]
