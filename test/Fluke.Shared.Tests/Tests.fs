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

    let treeStateFromDslTemplate user dslTemplate =
        let dslDataList =
            dslTemplate.Tasks
            |> List.map (fun templateTask ->
                TempData.Testing.createLaneRenderingDslData
                    {|
                        User = user
                        Position = dslTemplate.Position
                        Task = templateTask.Task
                        Events = templateTask.Events
                    |})

        let treeState = TreeState.Create (name = TreeName "Test", owner = user)

        let newTreeState =
            (treeState, dslDataList)
            ||> List.fold (fun treeState dslData -> treeState |> mergeDslDataIntoTreeState dslData)

        newTreeState


    let testWithTemplateData (dslTemplate: DslTemplate) =

        let user = getUsers().fluke

        let treeState = treeStateFromDslTemplate user dslTemplate

        dslTemplate.Tasks
        |> List.iter (fun taskTemplate ->
            let dateSequence = taskTemplate.Expected |> List.map fst

            let taskState = treeState.TaskStateMap.[taskTemplate.Task]

            let expectedCellMetadataList =
                taskTemplate.Expected
                |> List.map (fun (date, templateExpectList) ->
                    let defaultCellMetadata = {| CellStatus = None; Sessions = None |}

                    let cellMetadata =
                        (defaultCellMetadata, templateExpectList)
                        ||> List.fold (fun cellMetadata templateExpect ->
                                match templateExpect with
                                | TemplateExpect.Status cellStatus ->
                                    {| cellMetadata with
                                        CellStatus = Some cellStatus
                                    |}
                                | TemplateExpect.Session count -> {| cellMetadata with Sessions = Some count |})

                    date, cellMetadata)

            let expectedStatus =
                expectedCellMetadataList
                |> List.choose (fun (date, expectedCellMetadata) ->
                    match expectedCellMetadata.CellStatus with
                    | Some cellStatus -> Some (date, cellStatus)
                    | _ -> None)

            let expectedSessions =
                expectedCellMetadataList
                |> List.choose (fun (date, expectedCellMetadata) ->
                    match expectedCellMetadata.Sessions with
                    | Some count -> Some (date, count)
                    | _ -> None)

            if not expectedStatus.IsEmpty then
                let laneStatusMap =
                    Rendering.renderLane user.DayStart dslTemplate.Position dateSequence taskState
                    |> fun (_taskState, cells) ->
                        cells
                        |> List.map (fun ({ DateId = DateId referenceDay }, status) -> referenceDay, status)
                        |> Map.ofList

                expectedStatus
                |> List.iter (fun (date, cellStatus) ->
                    laneStatusMap
                    |> Map.tryFind date
                    |> Option.defaultValue Disabled
                    |> fun cellStatus -> date, cellStatus
                    |> Expect.equal "" (date, cellStatus))

            if not expectedSessions.IsEmpty then
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

                let taskState = sessionData.TaskStateMap.[taskTemplate.Task]

                expectedSessions
                |> List.iter (fun (date, count) ->
                    let sessionCount =
                        taskState.Sessions
                        |> List.filter (fun (TaskSession (start, _, _)) -> isToday user.DayStart start (DateId date))
                        |> List.length

                    sessionCount |> Expect.equal "" count))




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
                                                  test name3 { testWithTemplateData obj })
                                   ])
                ])

    let getTreeTests () =
        let users = getUsers ()
        let user = users.fluke
        let tree = getTree user
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
            ]
