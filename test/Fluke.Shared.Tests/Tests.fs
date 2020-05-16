namespace Fluke.Shared

open System
open Expecto
open Fluke.Shared
open Fluke.Shared.Model
open Expecto.Flip
open Suigetsu.Core

module Data =
    let dayStart = flukeTime 00 00
    let task1 = { Task.Default with Name = "1" }
    let task2 = { Task.Default with Name = "2" }
    let task3 = { Task.Default with Name = "3" }
    let task4 = { Task.Default with Name = "4" }
    let task5 = { Task.Default with Name = "5" }
        
    let orderedEventList = [
        { Task = task1; Priority = First }
        { Task = task2; Priority = LessThan task1 }
        { Task = task3; Priority = LessThan task2 }
        { Task = task4; Priority = LessThan task3 }
        { Task = task5; Priority = Last }
    ]
    let orderedList = [ task1; task2; task3; task4; task5 ]
    
module Tests =
    open Data
    
    let tests = testList "Tests" [
        
        testList "GetManualSortedTaskListTests" [
            test "1" {
                orderedEventList
                |> Sorting.getManualSortedTaskList
                |> Expect.equal "" orderedList
                
                [
                    { Task = task5; Priority = Last }
                    { Task = task4; Priority = LessThan task3 }
                    { Task = task3; Priority = LessThan task2 }
                    { Task = task2; Priority = LessThan task1 }
                    { Task = task1; Priority = First }
                ]
                |> Sorting.getManualSortedTaskList
                |> Expect.equal "" orderedList
                
                [
                    { Task = task3; Priority = LessThan task2 }
                    { Task = task5; Priority = Last }
                    { Task = task4; Priority = LessThan task3 }
                    { Task = task1; Priority = First }
                    { Task = task2; Priority = LessThan task1 }
                ]
                |> Sorting.getManualSortedTaskList
                |> Expect.equal "" orderedList
                
                [
                    { Task = task3; Priority = First }
                ]
                |> List.append orderedEventList
                |> Sorting.getManualSortedTaskList
                |> Expect.equal "" [ task3; task4; task1; task2; task5 ]
                
                [
                    { Task = task3; Priority = First }
                    { Task = task4; Priority = LessThan task2 }
                ]
                |> List.append orderedEventList
                |> Sorting.getManualSortedTaskList
                |> Expect.equal "" [ task3; task1; task2; task4; task5 ]
                
                [
                    { Task = task1; Priority = First }
                    { Task = task2; Priority = First }
                    { Task = task3; Priority = First }
                    { Task = task4; Priority = First }
                    { Task = task5; Priority = LessThan task4 }
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
                        | (Some _, None), First when i = 0 -> None
                        | (Some below, None), _ -> Some { Task = below; Priority = First }
                        | (Some below, Some above), _ -> Some { Task = below; Priority = LessThan above }
                        | _, First when i > 0 -> Some { Task = taskList |> List.head; Priority = LessThan task }
                        | _ -> None
                    |> Option.toList
                    |> List.append [ { Task = task; Priority = priority } ]
                
                [ task1; task2; task3; task4; task5 ]
                |> createPriorityEvents task3 First
                |> Expect.equal "" [
                    { Task = task3; Priority = First }
                    { Task = task4; Priority = LessThan task2 }
                ]
                
                [ task1; task2; task3; task4; task5 ]
                |> createPriorityEvents task1 First
                |> Expect.equal "" [
                    { Task = task1; Priority = First }
                ]
                
                [ task1; task2; task3; task4; task5 ]
                |> createPriorityEvents task1 Last
                |> Expect.equal "" [
                    { Task = task1; Priority = Last }
                    { Task = task2; Priority = First }
                ]
                
                [ task1; task2; task3; task4; task5 ]
                |> createPriorityEvents task5 Last
                |> Expect.equal "" [
                    { Task = task5; Priority = Last }
                ]
                
                [ task1; task2; task3; task4; task5 ]
                |> createPriorityEvents task5 First
                |> Expect.equal "" [
                    { Task = task5; Priority = First }
                    { Task = task1; Priority = LessThan task5 }
                ]
                
                [ task1; task2; task3; task4; task5 ]
                |> createPriorityEvents task2 First
                |> Expect.equal "" [
                    { Task = task2; Priority = First }
                    { Task = task3; Priority = LessThan task1 }
                ]
                
                [ task1; task2; task3; task4; task5 ]
                |> createPriorityEvents task4 (LessThan task1)
                |> Expect.equal "" [
                    { Task = task4; Priority = LessThan task1 }
                    { Task = task5; Priority = LessThan task3 }
                ]
            }
        ]
        
        testList "Lane Sorting" [
            
            let (|NoSorting|TimeOfDay|Frequency|) = function
                | Choice1Of3 -> NoSorting
                | Choice2Of3 -> TimeOfDay
                | Choice3Of3 -> Frequency
            let _noSorting = Choice1Of3 ()
            let sortByTimeOfDay = Choice2Of3 ()
            let sortByFrequency = Choice3Of3 ()
            
            let testData (props: {| Sort: Choice<_, _, _>
                                    Data: (Task * (FlukeDate * CellEventStatus) list) list
                                    Expected: string list
                                    Now: FlukeDateTime |}) =
                let dateSequence =
                    props.Data
                    |> List.collect (snd >> List.map fst)
                    |> Rendering.getDateSequence (35, 35)
                
                props.Data
                |> List.map (fun (task, rawEvents) ->
                    rawEvents
                    |> Rendering.createCellStatusEntries task
                    |> Rendering.renderLane dayStart props.Now dateSequence task
                )
                |> fun lanes ->
                    match props.Sort with
                    | NoSorting -> lanes
                    | TimeOfDay -> Sorting.sortLanesByTimeOfDay props.Now [] lanes
                    | Frequency -> Sorting.sortLanesByFrequency lanes
                |> List.map (fun (Lane (task, _)) -> task.Name)
                |> Expect.equal "" props.Expected
            
            test "Sort by Frequency: All task types mixed" {
                testData
                    {| Sort = sortByFrequency
                       Now = { Date = flukeDate 2020 Month.March 10
                               Time = flukeTime 00 00 }
                       Data = [
                           { Task.Default with Name = "1"; Scheduling = Manual true },
                           [] 
                           
                           { Task.Default with Name = "2"; Scheduling = Manual true },
                           [ flukeDate 2020 Month.March 10, Postponed None
                             flukeDate 2020 Month.March 08, Postponed None ]
                           
                           { Task.Default with Name = "3"; Scheduling = Manual false },
                           [ flukeDate 2020 Month.March 09, ManualPending ]
                           
                           { Task.Default with Name = "4"; Scheduling = Recurrency (Offset (Days 1));
                                                           PendingAfter = flukeTime 20 00 |> Some },
                           []
                           
                           { Task.Default with Name = "5"; Scheduling = Manual false },
                           [ flukeDate 2020 Month.March 10, ManualPending ]
                           
                           { Task.Default with Name = "6"; Scheduling = Manual false },
                           [ flukeDate 2020 Month.March 04, Postponed None
                             flukeDate 2020 Month.March 06, Dismissed ]
                           
                           { Task.Default with Name = "7"; Scheduling = Recurrency (Offset (Days 4)) },
                           [ flukeDate 2020 Month.March 08, Completed ]
                           
                           { Task.Default with Name = "8"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ flukeDate 2020 Month.March 10, Completed ]
                           
                           { Task.Default with Name = "9"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ flukeDate 2020 Month.March 10, Dismissed ]
                           
                           { Task.Default with Name = "10"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ flukeDate 2020 Month.March 10, Postponed None ]
                           
                           { Task.Default with Name = "11"; Scheduling = Recurrency (Offset (Days 1)) },
                           []
                           
                           { Task.Default with Name = "12"; Scheduling = Manual false },
                           []
                           
                           { Task.Default with Name = "13"; Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Tuesday ]) },
                           []
                           
                           { Task.Default with Name = "14"; Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Wednesday ]) },
                           []
                           
                           { Task.Default with Name = "15"; Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Friday ]) },
                           [ flukeDate 2020 Month.March 07, Postponed None
                             flukeDate 2020 Month.March 09, Dismissed ]
                       ]
                       Expected = [ "11"; "4"; "8"; "9"; "10"; "7"; "15"; "13"; "14"; "2"; "6"; "3"; "5"; "1"; "12" ] |}
            }
            
            test "Sort by Today: All task types mixed" {
                testData
                    {| Sort = sortByTimeOfDay
                       Now = { Date = flukeDate 2020 Month.March 10
                               Time = flukeTime 12 00 }
                       Data = [
                           { Task.Default with Name = "1"; Scheduling = Manual true },
                           [] 
                           
                           { Task.Default with Name = "2"; Scheduling = Manual true },
                           [ flukeDate 2020 Month.March 10, Postponed None
                             flukeDate 2020 Month.March 08, Postponed None ]
                           
                           { Task.Default with Name = "3"; Scheduling = Manual false },
                           [ flukeDate 2020 Month.March 09, ManualPending ]
                           
                           { Task.Default with Name = "4"; Scheduling = Recurrency (Offset (Days 1));
                                                           PendingAfter = flukeTime 20 00 |> Some },
                           []
                           
                           { Task.Default with Name = "5"; Scheduling = Manual false },
                           [ flukeDate 2020 Month.March 10, ManualPending ]
                           
                           { Task.Default with Name = "6"; Scheduling = Manual false },
                           [ flukeDate 2020 Month.March 04, Postponed None
                             flukeDate 2020 Month.March 06, Dismissed ]
                           
                           { Task.Default with Name = "7"; Scheduling = Recurrency (Offset (Days 4)) },
                           [ flukeDate 2020 Month.March 08, Completed ]
                           
                           { Task.Default with Name = "8"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ flukeDate 2020 Month.March 10, Completed ]
                           
                           { Task.Default with Name = "9"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ flukeDate 2020 Month.March 10, Dismissed ]
                           
                           { Task.Default with Name = "10"; Scheduling = Recurrency (Offset (Days 2)) },
                           [ flukeDate 2020 Month.March 10, Postponed None ]
                           
                           { Task.Default with Name = "11"; Scheduling = Recurrency (Offset (Days 1)) },
                           [ flukeDate 2020 Month.March 10, Postponed (flukeTime 11 00 |> Some) ]
                           
                           { Task.Default with Name = "12"; Scheduling = Manual false },
                           []
                           
                           { Task.Default with Name = "13"; Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Tuesday ]) },
                           []
                           
                           { Task.Default with Name = "14"; Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Wednesday ]) },
                           []
                           
                           { Task.Default with Name = "15"; Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Friday ]) },
                           [ flukeDate 2020 Month.March 07, Postponed None
                             flukeDate 2020 Month.March 09, Dismissed ]
                           
                           { Task.Default with Name = "16"; Scheduling = Recurrency (Offset (Days 1));
                                                            MissedAfter = (flukeTime 07 00 |> Some) },
                           []
                           
                           { Task.Default with Name = "17"; Scheduling = Recurrency (Offset (Days 1)) },
                           [ flukeDate 2020 Month.March 10, Postponed (flukeTime 13 00 |> Some) ]
                           
                           { Task.Default with Name = "18"; Scheduling = Recurrency (Offset (Days 1)) },
                           []
                       ]
                       Expected = [ "16"; "5"; "3"; "11"; "13"; "18"; "17"; "4"; "1"; "2"; "10"; "8"; "9"; "7"; "14"; "15"; "12"; "6" ] |}
            }
        ]
        
        testList "Lane Rendering" [
            
            let testData (props: {| Task: Task
                                    Now: FlukeDateTime
                                    Data: (FlukeDate * CellStatus) list
                                    CellEvents: (FlukeDate * CellEventStatus) list |}) =
                   
                let dateSequence =
                    props.Data
                    |> List.map fst
                    
                let unwrapLane (Lane (_, cells)) =
                   cells
                   |> List.map (fun (Cell (address, status)) -> string address.Date, status)
                
                let toString =
                    List.map string
                    >> String.concat Environment.NewLine
                
                props.CellEvents
                |> Rendering.createCellStatusEntries props.Task
                |> Rendering.renderLane dayStart props.Now dateSequence props.Task
                |> unwrapLane
                |> toString
                |> Expect.equal "" (props.Data
                                    |> List.map (fun (date, cellStatus) -> string date, cellStatus)
                                    |> toString)
               
            testList "Recurrency Offset" [
                
                test "Start scheduling today without any events" {
                    testData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 2)) }
                           Now = { Date = flukeDate 2020 Month.March 9
                                   Time = flukeTime 00 00 }
                           Data = [
                               flukeDate 2020 Month.March 7, Disabled
                               flukeDate 2020 Month.March 8, Disabled
                               flukeDate 2020 Month.March 9, Pending
                               flukeDate 2020 Month.March 10, Disabled
                               flukeDate 2020 Month.March 11, Pending
                               flukeDate 2020 Month.March 12, Disabled
                           ]
                           CellEvents = [] |}
                }
                
                test "Disabled today after a Completed event yesterday" {
                    testData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 3)) }
                           Now = { Date = flukeDate 2020 Month.March 9
                                   Time = flukeTime 00 00 }
                           Data = [
                               flukeDate 2020 Month.March 8, EventStatus Completed
                               flukeDate 2020 Month.March 9, Disabled
                               flukeDate 2020 Month.March 10, Disabled
                               flukeDate 2020 Month.March 11, Pending
                               flukeDate 2020 Month.March 12, Disabled
                               flukeDate 2020 Month.March 13, Disabled
                               flukeDate 2020 Month.March 14, Pending
                               flukeDate 2020 Month.March 15, Disabled
                           ]
                           CellEvents = [
                               flukeDate 2020 Month.March 8, Completed
                           ] |}
                }
                
                test "Postponing today wont schedule for tomorrow" {
                    testData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 2)) }
                           Now = { Date = flukeDate 2020 Month.March 10
                                   Time = flukeTime 00 00 }
                           Data = [
                               flukeDate 2020 Month.March 9, Disabled
                               flukeDate 2020 Month.March 10, EventStatus (Postponed None)
                               flukeDate 2020 Month.March 11, Disabled
                               flukeDate 2020 Month.March 12, Pending
                               flukeDate 2020 Month.March 13, Disabled
                           ]
                           CellEvents = [
                               flukeDate 2020 Month.March 10, Postponed None
                           ] |}
                }
                
                test "(Postponed None) yesterday schedules for today" {
                    testData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 2)) }
                           Now = { Date = flukeDate 2020 Month.March 11
                                   Time = flukeTime 00 00 }
                           Data = [
                               flukeDate 2020 Month.March 9, Disabled
                               flukeDate 2020 Month.March 10, EventStatus (Postponed None)
                               flukeDate 2020 Month.March 11, Pending
                               flukeDate 2020 Month.March 12, Disabled
                               flukeDate 2020 Month.March 13, Pending
                           ]
                           CellEvents = [
                               flukeDate 2020 Month.March 10, Postponed None
                           ] |}
                }
            
                test "Pending today after missing yesterday, then resetting the schedule with a future Completed event" {
                    testData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 2)) }
                           Now = { Date = flukeDate 2020 Month.March 11
                                   Time = flukeTime 00 00 }
                           Data = [
                               flukeDate 2020 Month.March 7, Disabled
                               flukeDate 2020 Month.March 8, EventStatus Completed
                               flukeDate 2020 Month.March 9, Disabled
                               flukeDate 2020 Month.March 10, Missed
                               flukeDate 2020 Month.March 11, Pending
                               flukeDate 2020 Month.March 12, EventStatus Completed
                               flukeDate 2020 Month.March 13, Disabled
                               flukeDate 2020 Month.March 14, Pending
                           ]
                           CellEvents = [
                               flukeDate 2020 Month.March 8, Completed
                               flukeDate 2020 Month.March 12, Completed
                           ] |}
                }
                
                test "Recurring task only Suggested before PendingAfter" {
                    testData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 1))
                                                      PendingAfter = flukeTime 20 00 |> Some }
                           Now = { Date = flukeDate 2020 Month.March 10
                                   Time = flukeTime 19 30 }
                           Data = [
                               flukeDate 2020 Month.March 9, Disabled
                               flukeDate 2020 Month.March 10, Suggested
                               flukeDate 2020 Month.March 11, Pending
                           ]
                           CellEvents = [] |}
                }
            
                test "Recurring task Pending after PendingAfter" {
                    testData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 1))
                                                      PendingAfter = flukeTime 20 00 |> Some }
                           Now = { Date = flukeDate 2020 Month.March 10
                                   Time = flukeTime 21 00 }
                           Data = [
                               flukeDate 2020 Month.March 9, Disabled
                               flukeDate 2020 Month.March 10, Pending
                               flukeDate 2020 Month.March 11, Pending
                           ]
                           CellEvents = [] |}
                }
            
                test "Recurrency for the next days should work normally while today is still optional/suggested (before PendingAfter)" {
                    testData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 2))
                                                      PendingAfter = flukeTime 18 00 |> Some }
                           Now = { Date = flukeDate 2020 Month.March 27
                                   Time = flukeTime 17 00 }
                           Data = [
                               flukeDate 2020 Month.March 25, Disabled
                               flukeDate 2020 Month.March 26, Disabled
                               flukeDate 2020 Month.March 27, Suggested
                               flukeDate 2020 Month.March 28, Disabled
                               flukeDate 2020 Month.March 29, Pending
                               flukeDate 2020 Month.March 29, Disabled
                           ]
                           CellEvents = [] |}
                }
                
                test "Reset counting after a future ManualPending event" {
                    testData
                        {| Task = { Task.Default with Scheduling = Recurrency (Offset (Days 3)) }
                           Now = { Date = flukeDate 2020 Month.March 28
                                   Time = flukeTime 00 00 }
                           Data = [
                               flukeDate 2020 Month.March 27, Disabled
                               flukeDate 2020 Month.March 28, Pending
                               flukeDate 2020 Month.March 29, Disabled
                               flukeDate 2020 Month.March 30, EventStatus ManualPending
                               flukeDate 2020 Month.March 31, EventStatus ManualPending
                               flukeDate 2020 Month.April 01, Disabled
                               flukeDate 2020 Month.April 02, Disabled
                               flukeDate 2020 Month.April 03, Pending
                           ]
                           CellEvents = [
                               flukeDate 2020 Month.March 30, ManualPending
                               flukeDate 2020 Month.March 31, ManualPending
                           ] |}
                }
            ]
            
            testList "Recurrency Fixed" [
                
                test "Weekly task, pending today, initialized by past completion" {
                    testData
                        {| Task = { Task.Default with Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Saturday ]) }
                           Now = { Date = flukeDate 2020 Month.March 21
                                   Time = flukeTime 00 00 }
                           Data = [
                               for d in 13 .. 29 do
                                   flukeDate 2020 Month.March d,
                                   match d with
                                   | 14 -> EventStatus Completed
                                   | 21 | 28 -> Pending
                                   | _ -> Disabled
                           ]
                           CellEvents = [
                               flukeDate 2020 Month.March 14, Completed
                           ] |}
                }
                
                test "Weekly task, missed until today, initialized by past completion" {
                    testData
                        {| Task = { Task.Default with Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Wednesday ]) }
                           Now = { Date = flukeDate 2020 Month.March 20
                                   Time = flukeTime 00 00 }
                           Data = [
                               for d in 10 .. 26 do
                                   flukeDate 2020 Month.March d,
                                   match d with
                                   | 13 -> EventStatus Completed
                                   | 18 | 19 -> Missed
                                   | 20 | 25 -> Pending
                                   | _ -> Disabled
                           ]
                           CellEvents = [
                               flukeDate 2020 Month.March 13, Completed
                           ] |}
                }
                
                test "Weekly task, (Postponed None) then missed until today, pending tomorrow" {
                    testData
                        {| Task = { Task.Default with Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Saturday ]) }
                           Now = { Date = flukeDate 2020 Month.March 20
                                   Time = flukeTime 00 00 }
                           Data = [
                               for d in 13 .. 29 do
                                   flukeDate 2020 Month.March d,
                                   match d with
                                   | 18 -> EventStatus (Postponed None)
                                   | 19 -> Missed
                                   | 20 | 21 | 28 -> Pending
                                   | _ -> Disabled
                           ]
                           CellEvents = [
                               flukeDate 2020 Month.March 18, Postponed None
                           ] |}
                }
                
                test "Weekly task, without past events, pending in a few days" {
                    testData
                        {| Task = { Task.Default with Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Wednesday ]) }
                           Now = { Date = flukeDate 2020 Month.March 20
                                   Time = flukeTime 00 00 }
                           Data = [
                               for d in 17 .. 26 do
                                   flukeDate 2020 Month.March d,
                                   match d with
                                   | 25 -> Pending
                                   | _ -> Disabled
                           ]
                           CellEvents = [
                           ] |}
                }
                
                
                test "Fixed weekly task, without past events, pending tomorrow" {
                    testData
                        {| Task = { Task.Default with Scheduling = Recurrency (Fixed [ Weekly DayOfWeek.Saturday ]) }
                           Now = { Date = flukeDate 2020 Month.March 20
                                   Time = flukeTime 00 00 }
                           Data = [
                               for d in 13 .. 29 do
                                   flukeDate 2020 Month.March d,
                                   match d with
                                   | 21 | 28 -> Pending
                                   | _ -> Disabled
                           ]
                           CellEvents = [
                           ] |}
                }
            ]
            
            testList "Manual" [
            
                test "Empty manual task" {
                    testData
                        {| Task = { Task.Default with Scheduling = Manual false }
                           Now = { Date = flukeDate 2020 Month.March 11
                                   Time = flukeTime 00 00 }
                           Data = [
                               flukeDate 2020 Month.March 9, Disabled
                               flukeDate 2020 Month.March 10, Disabled
                               flukeDate 2020 Month.March 11, Suggested
                               flukeDate 2020 Month.March 12, Disabled
                               flukeDate 2020 Month.March 13, Disabled
                           ]
                           CellEvents = [] |}
                }
                
                test "ManualPending task scheduled for today after missing" {
                    testData
                        {| Task = { Task.Default with Scheduling = Manual false }
                           Now = { Date = flukeDate 2020 Month.March 11
                                   Time = flukeTime 00 00 }
                           Data = [
                               flukeDate 2020 Month.March 8, Disabled
                               flukeDate 2020 Month.March 9, EventStatus ManualPending
                               flukeDate 2020 Month.March 10, Missed
                               flukeDate 2020 Month.March 11, Pending
                               flukeDate 2020 Month.March 12, Disabled
                               flukeDate 2020 Month.March 13, Disabled
                           ]
                           CellEvents = [
                               flukeDate 2020 Month.March 9, ManualPending
                           ] |}
                }
                
                test "Manual Suggested task Suggested before PendingAfter" {
                    testData
                        {| Task = { Task.Default with Scheduling = Manual true
                                                      PendingAfter = flukeTime 20 00 |> Some }
                           Now = { Date = flukeDate 2020 Month.March 10
                                   Time = flukeTime 19 30 }
                           Data = [
                               flukeDate 2020 Month.March 09, Suggested
                               flukeDate 2020 Month.March 10, Suggested
                               flukeDate 2020 Month.March 11, Suggested
                           ]
                           CellEvents = [] |}
                }
                
                test "Manual Suggested task Pending after PendingAfter" {
                    testData
                        {| Task = { Task.Default with Scheduling = Manual true
                                                      PendingAfter = flukeTime 20 00 |> Some }
                           Now = { Date = flukeDate 2020 Month.March 10
                                   Time = flukeTime 21 00 }
                           Data = [
                               flukeDate 2020 Month.March 09, Suggested
                               flukeDate 2020 Month.March 10, Pending
                               flukeDate 2020 Month.March 11, Suggested
                           ]
                           CellEvents = [] |}
                }
                
                test "Manual Suggested task: Missed ManualPending propagates until today" {
                    testData
                        {| Task = { Task.Default with Scheduling = Manual true }
                           Now = { Date = flukeDate 2020 Month.March 28
                                   Time = flukeTime 00 00 }
                           Data = [
                               flukeDate 2020 Month.March 25, Suggested
                               flukeDate 2020 Month.March 26, EventStatus ManualPending
                               flukeDate 2020 Month.March 27, Missed
                               flukeDate 2020 Month.March 28, Pending
                               flukeDate 2020 Month.March 29, Suggested
                               flukeDate 2020 Month.March 30, EventStatus ManualPending
                               flukeDate 2020 Month.March 31, Suggested
                           ]
                           CellEvents = [
                               flukeDate 2020 Month.March 26, ManualPending
                               flukeDate 2020 Month.March 30, ManualPending
                           ] |}
                }
                
                test "Manual Suggested task: Suggested mode restored after completing a forgotten ManualPending event" {
                    testData
                        {| Task = { Task.Default with Scheduling = Manual true }
                           Now = { Date = flukeDate 2020 Month.March 28
                                   Time = flukeTime 00 00 }
                           Data = [
                               flukeDate 2020 Month.March 24, Suggested
                               flukeDate 2020 Month.March 25, EventStatus ManualPending
                               flukeDate 2020 Month.March 26, EventStatus Completed
                               flukeDate 2020 Month.March 27, Suggested
                               flukeDate 2020 Month.March 28, Suggested
                               flukeDate 2020 Month.March 29, Suggested
                           ]
                           CellEvents = [
                               flukeDate 2020 Month.March 25, ManualPending
                               flukeDate 2020 Month.March 26, Completed
                           ] |}
                }
                
                test "Manual Suggested task: Pending today after missing a ManualPending event" {
                    testData
                        {| Task = { Task.Default with Scheduling = Manual true }
                           Now = { Date = flukeDate 2020 Month.March 28
                                   Time = flukeTime 00 00 }
                           Data = [
                               flukeDate 2020 Month.March 24, Suggested
                               flukeDate 2020 Month.March 25, EventStatus ManualPending
                               flukeDate 2020 Month.March 26, Missed
                               flukeDate 2020 Month.March 27, Missed
                               flukeDate 2020 Month.March 28, Pending
                               flukeDate 2020 Month.March 29, Suggested
                           ]
                           CellEvents = [
                               flukeDate 2020 Month.March 25, ManualPending
                           ] |}
                }
            ]
        ]
    ]

