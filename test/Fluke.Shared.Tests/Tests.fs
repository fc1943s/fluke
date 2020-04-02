namespace Fluke.Shared

open System
open Expecto
open Fluke.Shared
open Fluke.Shared.Model
open Expecto.Flip
open Suigetsu.Core

module Data =
    let defaultTask =
        { Name = "<blank>"
          InformationType = Area { Name = "Area" }
          Comments = []
          PendingAfter = midnight
          Scheduling = Manual false
          Duration = None }
    
    let task1 = { defaultTask with Name = "1" }
    let task2 = { defaultTask with Name = "2" }
    let task3 = { defaultTask with Name = "3" }
    let task4 = { defaultTask with Name = "4" }
    let task5 = { defaultTask with Name = "5" }
        
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
                |> Functions.getManualSortedTaskList
                |> Expect.equal "" orderedList
                
                [
                    { Task = task5; Priority = Last }
                    { Task = task4; Priority = LessThan task3 }
                    { Task = task3; Priority = LessThan task2 }
                    { Task = task2; Priority = LessThan task1 }
                    { Task = task1; Priority = First }
                ]
                |> Functions.getManualSortedTaskList
                |> Expect.equal "" orderedList
                
                [
                    { Task = task3; Priority = LessThan task2 }
                    { Task = task5; Priority = Last }
                    { Task = task4; Priority = LessThan task3 }
                    { Task = task1; Priority = First }
                    { Task = task2; Priority = LessThan task1 }
                ]
                |> Functions.getManualSortedTaskList
                |> Expect.equal "" orderedList
                
                [
                    { Task = task3; Priority = First }
                ]
                |> List.append orderedEventList
                |> Functions.getManualSortedTaskList
                |> Expect.equal "" [ task3; task4; task1; task2; task5 ]
                
                [
                    { Task = task3; Priority = First }
                    { Task = task4; Priority = LessThan task2 }
                ]
                |> List.append orderedEventList
                |> Functions.getManualSortedTaskList
                |> Expect.equal "" [ task3; task1; task2; task4; task5 ]
                
                [
                    { Task = task1; Priority = First }
                    { Task = task2; Priority = First }
                    { Task = task3; Priority = First }
                    { Task = task4; Priority = First }
                    { Task = task5; Priority = LessThan task4 }
                ]
                |> Functions.getManualSortedTaskList
                |> Expect.equal "" [ task4; task5; task3; task2; task1 ]
                
            }
        ]
        
        
        testList "SetPriorityTests" [
            
            test "1" {
                let createPriorityEvents task priority taskList =
                    taskList
                    |> List.tryFindIndexBack ((=) task)
                    |> function
                        | None -> None
                        | Some i -> 
                            match (i + 1, i - 1) |> Tuple2.map (fun x -> taskList |> List.tryItem x), priority with
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
            
            let testData (props: {| Now: FlukeDateTime
                                    Data: (Task * (FlukeDate * CellEventStatus) list) list
                                    Expected: string list |}) =
                let dateSequence =
                    props.Data
                    |> List.collect (fun (_, cellEvents) ->
                        cellEvents
                        |> List.map (fun (date, _) -> date)
                    )
                    |> Functions.getDateSequence (0, 0)
                
                props.Data
                |> List.map (fun (task, events) ->
                    events
                    |> List.map (fun (date, status) ->
                        { Task = task
                          Date = date
                          Status = status }
                    )
                    |> Functions.renderLane task props.Now dateSequence
                )
                |> Functions.sortLanes props.Now.Date
                |> List.map (fun (Lane (task, _)) -> task.Name)
                |> Expect.equal "" props.Expected
            
            test "All task types mixed" {
                testData
                    {| Now = { Date = { Year = 2020; Month = Month.March; Day = 10 }
                               Time = midnight }
                       Data = [
                           { defaultTask with Name = "1"; Scheduling = Manual true },
                           [] 
                           
                           { defaultTask with Name = "2"; Scheduling = Manual true },
                           [ { Year = 2020; Month = Month.March; Day = 10 }, Postponed
                             { Year = 2020; Month = Month.March; Day = 8 }, Postponed ]
                           
                           { defaultTask with Name = "3"; Scheduling = Manual false },
                           [ { Year = 2020; Month = Month.March; Day = 9 }, ManualPending ]
                           
                           { defaultTask with Name = "4"; Scheduling = Recurrency (Offset 1);
                                                           PendingAfter = { Hour = 20; Minute = 0 } },
                           []
                           
                           { defaultTask with Name = "5"; Scheduling = Manual false },
                           [ { Year = 2020; Month = Month.March; Day = 10 }, ManualPending ]
                           
                           { defaultTask with Name = "6"; Scheduling = Manual false },
                           [ { Year = 2020; Month = Month.March; Day = 4 }, Postponed
                             { Year = 2020; Month = Month.March; Day = 6 }, Dropped ]
                           
                           { defaultTask with Name = "7"; Scheduling = Recurrency (Offset 4) },
                           [ { Year = 2020; Month = Month.March; Day = 8 }, Complete ]
                           
                           { defaultTask with Name = "8"; Scheduling = Recurrency (Offset 2) },
                           [ { Year = 2020; Month = Month.March; Day = 10 }, Complete ]
                           
                           { defaultTask with Name = "9"; Scheduling = Recurrency (Offset 2) },
                           [ { Year = 2020; Month = Month.March; Day = 10 }, Dropped ]
                           
                           { defaultTask with Name = "10"; Scheduling = Recurrency (Offset 2) },
                           [ { Year = 2020; Month = Month.March; Day = 10 }, Postponed ]
                           
                           { defaultTask with Name = "11"; Scheduling = Recurrency (Offset 1) },
                           []
                           
                           { defaultTask with Name = "12"; Scheduling = Manual false },
                           []
                           
                           { defaultTask with Name = "13"; Scheduling = Recurrency (Fixed (Weekly DayOfWeek.Tuesday)) },
                           []
                           
                           { defaultTask with Name = "14"; Scheduling = Recurrency (Fixed (Weekly DayOfWeek.Wednesday)) },
                           []
                           
                           { defaultTask with Name = "15"; Scheduling = Recurrency (Fixed (Weekly DayOfWeek.Friday)) },
                           [ { Year = 2020; Month = Month.March; Day = 7 }, Postponed 
                             { Year = 2020; Month = Month.March; Day = 9 }, Dropped ]
                       ]
                       Expected = [ "5"; "3"; "11"; "13"; "4"; "1"; "2"; "10"; "8"; "9"; "7"; "14"; "15"; "12"; "6" ] |}
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
                   |> List.map (fun x -> string x.Date, x.Status)
                
                let toString =
                    List.map string
                    >> String.concat Environment.NewLine
                
                props.CellEvents
                |> List.map (fun (date, status) ->
                    { Task = props.Task
                      Date = date
                      Status = status }
                )
                |> Functions.renderLane props.Task props.Now dateSequence
                |> unwrapLane
                |> toString
                |> Expect.equal "" (props.Data
                                    |> List.map (fun (date, status) -> string date, status)
                                    |> toString)
               
            testList "Recurrency Offset" [
                
                test "Start scheduling today without any events" {
                    testData
                        {| Task = { defaultTask with Scheduling = Recurrency (Offset 2) }
                           Now = { Date = { Year = 2020; Month = Month.March; Day = 9 }
                                   Time = midnight }
                           Data = [
                               { Year = 2020; Month = Month.March; Day = 7 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 8 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 9 }, Pending
                               { Year = 2020; Month = Month.March; Day = 10 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 11 }, Pending
                               { Year = 2020; Month = Month.March; Day = 12 }, Disabled
                           ]
                           CellEvents = [] |}
                }
                
                test "Disabled today after a Complete event yesterday" {
                    testData
                        {| Task = { defaultTask with Scheduling = Recurrency (Offset 3) }
                           Now = { Date = { Year = 2020; Month = Month.March; Day = 9 }
                                   Time = midnight }
                           Data = [
                               { Year = 2020; Month = Month.March; Day = 8 }, EventStatus Complete
                               { Year = 2020; Month = Month.March; Day = 9 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 10 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 11 }, Pending
                               { Year = 2020; Month = Month.March; Day = 12 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 13 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 14 }, Pending
                               { Year = 2020; Month = Month.March; Day = 15 }, Disabled
                           ]
                           CellEvents = [
                               { Year = 2020; Month = Month.March; Day = 8 }, Complete
                           ] |}
                }
                
                test "Postponing today wont schedule for tomorrow" {
                    testData
                        {| Task = { defaultTask with Scheduling = Recurrency (Offset 2) }
                           Now = { Date = { Year = 2020; Month = Month.March; Day = 10 }
                                   Time = midnight }
                           Data = [
                               { Year = 2020; Month = Month.March; Day = 9 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 10 }, EventStatus Postponed
                               { Year = 2020; Month = Month.March; Day = 11 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 12 }, Pending
                               { Year = 2020; Month = Month.March; Day = 13 }, Disabled
                           ]
                           CellEvents = [
                               { Year = 2020; Month = Month.March; Day = 10 }, Postponed
                           ] |}
                }
                
                test "Postponed yesterday schedules for today" {
                    testData
                        {| Task = { defaultTask with Scheduling = Recurrency (Offset 2) }
                           Now = { Date = { Year = 2020; Month = Month.March; Day = 11 }
                                   Time = midnight }
                           Data = [
                               { Year = 2020; Month = Month.March; Day = 9 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 10 }, EventStatus Postponed
                               { Year = 2020; Month = Month.March; Day = 11 }, Pending
                               { Year = 2020; Month = Month.March; Day = 12 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 13 }, Pending
                           ]
                           CellEvents = [
                               { Year = 2020; Month = Month.March; Day = 10 }, Postponed
                           ] |}
                }
            
                test "Pending today after missing yesterday, then resetting the schedule with a future Complete event" {
                    testData
                        {| Task = { defaultTask with Scheduling = Recurrency (Offset 2) }
                           Now = { Date = { Year = 2020; Month = Month.March; Day = 11 }
                                   Time = midnight }
                           Data = [
                               { Year = 2020; Month = Month.March; Day = 7 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 8 }, EventStatus Complete
                               { Year = 2020; Month = Month.March; Day = 9 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 10 }, Missed
                               { Year = 2020; Month = Month.March; Day = 11 }, Pending
                               { Year = 2020; Month = Month.March; Day = 12 }, EventStatus Complete
                               { Year = 2020; Month = Month.March; Day = 13 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 14 }, Pending
                           ]
                           CellEvents = [
                               { Year = 2020; Month = Month.March; Day = 8 }, Complete
                               { Year = 2020; Month = Month.March; Day = 12 }, Complete
                           ] |}
                }
                
                test "Recurring task only Suggested before PendingAfter" {
                    testData
                        {| Task = { defaultTask with Scheduling = Recurrency (Offset 1)
                                                     PendingAfter = { Hour = 20; Minute = 0 } }
                           Now = { Date = { Year = 2020; Month = Month.March; Day = 10 }
                                   Time = { Hour = 19; Minute = 30 } }
                           Data = [
                               { Year = 2020; Month = Month.March; Day = 9 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 10 }, Suggested
                               { Year = 2020; Month = Month.March; Day = 11 }, Pending
                           ]
                           CellEvents = [] |}
                }
            
                test "Recurring task Pending after PendingAfter" {
                    testData
                        {| Task = { defaultTask with Scheduling = Recurrency (Offset 1)
                                                     PendingAfter = { Hour = 20; Minute = 0 } }
                           Now = { Date = { Year = 2020; Month = Month.March; Day = 10 }
                                   Time = { Hour = 21; Minute = 0 } }
                           Data = [
                               { Year = 2020; Month = Month.March; Day = 9 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 10 }, Pending
                               { Year = 2020; Month = Month.March; Day = 11 }, Pending
                           ]
                           CellEvents = [] |}
                }
            
                test "Recurrency for the next days should work normally while today is still optional/suggested (before PendingAfter)" {
                    testData
                        {| Task = { defaultTask with Scheduling = Recurrency (Offset 2)
                                                     PendingAfter = { Hour = 18; Minute = 0 } }
                           Now = { Date = { Year = 2020; Month = Month.March; Day = 27 }
                                   Time = { Hour = 17; Minute = 0 } }
                           Data = [
                               { Year = 2020; Month = Month.March; Day = 25 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 26 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 27 }, Suggested
                               { Year = 2020; Month = Month.March; Day = 28 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 29 }, Pending
                               { Year = 2020; Month = Month.March; Day = 29 }, Disabled
                           ]
                           CellEvents = [] |}
                }
                
                test "Reset counting after a future ManualPending event" {
                    testData
                        {| Task = { defaultTask with Scheduling = Recurrency (Offset 3) }
                           Now = { Date = { Year = 2020; Month = Month.March; Day = 28 }
                                   Time = midnight }
                           Data = [
                               { Year = 2020; Month = Month.March; Day = 27 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 28 }, Pending
                               { Year = 2020; Month = Month.March; Day = 29 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 30 }, EventStatus ManualPending
                               { Year = 2020; Month = Month.March; Day = 31 }, EventStatus ManualPending
                               { Year = 2020; Month = Month.April; Day = 01 }, Disabled
                               { Year = 2020; Month = Month.April; Day = 02 }, Disabled
                               { Year = 2020; Month = Month.April; Day = 03 }, Pending
                           ]
                           CellEvents = [
                               { Year = 2020; Month = Month.March; Day = 30 }, ManualPending
                               { Year = 2020; Month = Month.March; Day = 31 }, ManualPending
                           ] |}
                }
            ]
            
            testList "Recurrency Fixed" [
                
                test "Weekly task, pending today, initialized by past completion" {
                    testData
                        {| Task = { defaultTask with Scheduling = Recurrency (Fixed (Weekly DayOfWeek.Saturday)) }
                           Now = { Date = { Year = 2020; Month = Month.March; Day = 21 }
                                   Time = midnight }
                           Data = [
                               for d in 13 .. 29 do
                                   { Year = 2020; Month = Month.March; Day = d },
                                   match d with
                                   | 14 -> EventStatus Complete
                                   | 21 | 28 -> Pending
                                   | _ -> Disabled
                           ]
                           CellEvents = [
                               { Year = 2020; Month = Month.March; Day = 14 }, Complete
                           ] |}
                }
                
                test "Weekly task, missed until today, initialized by past completion" {
                    testData
                        {| Task = { defaultTask with Scheduling = Recurrency (Fixed (Weekly DayOfWeek.Wednesday)) }
                           Now = { Date = { Year = 2020; Month = Month.March; Day = 20 }
                                   Time = midnight }
                           Data = [
                               for d in 10 .. 26 do
                                   { Year = 2020; Month = Month.March; Day = d },
                                   match d with
                                   | 13 -> EventStatus Complete
                                   | 18 | 19 -> Missed
                                   | 20 | 25 -> Pending
                                   | _ -> Disabled
                           ]
                           CellEvents = [
                               { Year = 2020; Month = Month.March; Day = 13 }, Complete
                           ] |}
                }
                
                test "Weekly task, postponed then missed until today, pending tomorrow" {
                    testData
                        {| Task = { defaultTask with Scheduling = Recurrency (Fixed (Weekly DayOfWeek.Saturday)) }
                           Now = { Date = { Year = 2020; Month = Month.March; Day = 20 }
                                   Time = midnight }
                           Data = [
                               for d in 13 .. 29 do
                                   { Year = 2020; Month = Month.March; Day = d },
                                   match d with
                                   | 18 -> EventStatus Postponed
                                   | 19 -> Missed
                                   | 20 | 21 | 28 -> Pending
                                   | _ -> Disabled
                           ]
                           CellEvents = [
                               { Year = 2020; Month = Month.March; Day = 18 }, Postponed
                           ] |}
                }
                
                test "Weekly task, without past events, pending in a few days" {
                    testData
                        {| Task = { defaultTask with Scheduling = Recurrency (Fixed (Weekly DayOfWeek.Wednesday)) }
                           Now = { Date = { Year = 2020; Month = Month.March; Day = 20 }
                                   Time = midnight }
                           Data = [
                               for d in 17 .. 26 do
                                   { Year = 2020; Month = Month.March; Day = d },
                                   match d with
                                   | 25 -> Pending
                                   | _ -> Disabled
                           ]
                           CellEvents = [
                           ] |}
                }
                
                
                test "Fixed weekly task, without past events, pending tomorrow" {
                    testData
                        {| Task = { defaultTask with Scheduling = Recurrency (Fixed (Weekly DayOfWeek.Saturday)) }
                           Now = { Date = { Year = 2020; Month = Month.March; Day = 20 }
                                   Time = midnight }
                           Data = [
                               for d in 13 .. 29 do
                                   { Year = 2020; Month = Month.March; Day = d },
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
                        {| Task = { defaultTask with Scheduling = Manual false }
                           Now = { Date = { Year = 2020; Month = Month.March; Day = 11 }
                                   Time = midnight }
                           Data = [
                               { Year = 2020; Month = Month.March; Day = 9 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 10 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 11 }, Suggested
                               { Year = 2020; Month = Month.March; Day = 12 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 13 }, Disabled
                           ]
                           CellEvents = [] |}
                }
                
                test "ManualPending task scheduled for today after missing" {
                    testData
                        {| Task = { defaultTask with Scheduling = Manual false }
                           Now = { Date = { Year = 2020; Month = Month.March; Day = 11 }
                                   Time = midnight }
                           Data = [
                               { Year = 2020; Month = Month.March; Day = 8 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 9 }, EventStatus ManualPending
                               { Year = 2020; Month = Month.March; Day = 10 }, Missed
                               { Year = 2020; Month = Month.March; Day = 11 }, Pending
                               { Year = 2020; Month = Month.March; Day = 12 }, Disabled
                               { Year = 2020; Month = Month.March; Day = 13 }, Disabled
                           ]
                           CellEvents = [
                               { Year = 2020; Month = Month.March; Day = 9 }, ManualPending
                           ] |}
                }
                
                test "Manual Suggested task Suggested before PendingAfter" {
                    testData
                        {| Task = { defaultTask with Scheduling = Manual true
                                                     PendingAfter = { Hour = 20; Minute = 0 } }
                           Now = { Date = { Year = 2020; Month = Month.March; Day = 10 }
                                   Time = { Hour = 19; Minute = 30 } }
                           Data = [
                               { Year = 2020; Month = Month.March; Day = 9 }, Suggested
                               { Year = 2020; Month = Month.March; Day = 10 }, Suggested
                               { Year = 2020; Month = Month.March; Day = 11 }, Suggested
                           ]
                           CellEvents = [] |}
                }
                
                test "Manual Suggested task Pending after PendingAfter" {
                    testData
                        {| Task = { defaultTask with Scheduling = Manual true
                                                     PendingAfter = { Hour = 20; Minute = 0 } }
                           Now = { Date = { Year = 2020; Month = Month.March; Day = 10 }
                                   Time = { Hour = 21; Minute = 0 } }
                           Data = [
                               { Year = 2020; Month = Month.March; Day = 9 }, Suggested
                               { Year = 2020; Month = Month.March; Day = 10 }, Pending
                               { Year = 2020; Month = Month.March; Day = 11 }, Suggested
                           ]
                           CellEvents = [] |}
                }
                
                test "Manual Suggested task: Missed ManualPending propagates until today" {
                    testData
                        {| Task = { defaultTask with Scheduling = Manual true }
                           Now = { Date = { Year = 2020; Month = Month.March; Day = 28 }
                                   Time = midnight }
                           Data = [
                               { Year = 2020; Month = Month.March; Day = 25 }, Suggested
                               { Year = 2020; Month = Month.March; Day = 26 }, EventStatus ManualPending
                               { Year = 2020; Month = Month.March; Day = 27 }, Missed
                               { Year = 2020; Month = Month.March; Day = 28 }, Pending
                               { Year = 2020; Month = Month.March; Day = 29 }, Suggested
                               { Year = 2020; Month = Month.March; Day = 30 }, EventStatus ManualPending
                               { Year = 2020; Month = Month.March; Day = 31 }, Suggested
                           ]
                           CellEvents = [
                               { Year = 2020; Month = Month.March; Day = 26 }, ManualPending
                               { Year = 2020; Month = Month.March; Day = 30 }, ManualPending
                           ] |}
                }
                
                test "Manual Suggested task: Suggested mode restored after completing a non-postponed ManualPending event" {
                    testData
                        {| Task = { defaultTask with Scheduling = Manual true }
                           Now = { Date = { Year = 2020; Month = Month.March; Day = 28 }
                                   Time = midnight }
                           Data = [
                               { Year = 2020; Month = Month.March; Day = 24 }, Suggested
                               { Year = 2020; Month = Month.March; Day = 25 }, EventStatus ManualPending
                               { Year = 2020; Month = Month.March; Day = 26 }, EventStatus Complete
                               { Year = 2020; Month = Month.March; Day = 27 }, Suggested
                               { Year = 2020; Month = Month.March; Day = 28 }, Suggested
                               { Year = 2020; Month = Month.March; Day = 29 }, Suggested
                           ]
                           CellEvents = [
                               { Year = 2020; Month = Month.March; Day = 25 }, ManualPending
                               { Year = 2020; Month = Month.March; Day = 26 }, Complete
                           ] |}
                }
                
                test "Manual Suggested task: Pending today after missing a ManualPending event" {
                    testData
                        {| Task = { defaultTask with Scheduling = Manual true }
                           Now = { Date = { Year = 2020; Month = Month.March; Day = 28 }
                                   Time = midnight }
                           Data = [
                               { Year = 2020; Month = Month.March; Day = 24 }, Suggested
                               { Year = 2020; Month = Month.March; Day = 25 }, EventStatus ManualPending
                               { Year = 2020; Month = Month.March; Day = 26 }, Missed
                               { Year = 2020; Month = Month.March; Day = 27 }, Missed
                               { Year = 2020; Month = Month.March; Day = 28 }, Pending
                               { Year = 2020; Month = Month.March; Day = 29 }, Suggested
                           ]
                           CellEvents = [
                               { Year = 2020; Month = Month.March; Day = 25 }, ManualPending
                           ] |}
                }
            ]
        ]
    ]

