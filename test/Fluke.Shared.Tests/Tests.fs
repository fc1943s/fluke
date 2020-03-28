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
          Scheduling = TaskScheduling.Disabled
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
        
        testList "GetSortedTaskListTests" [
            test "1" {
                let now =
                    { Date = { Year = 2020; Month = Month.March; Day = 10 }
                      Time = midnight }
                let data = [
                    { defaultTask with Name = "1"; Scheduling = TaskScheduling.Optional },
                    [] 
                   
                    { defaultTask with Name = "2"; Scheduling = TaskScheduling.Optional },
                    [ { Year = 2020; Month = Month.March; Day = 10 }, Postponed ]
                    
                    { defaultTask with Name = "3"; Scheduling = TaskScheduling.Recurrency (Offset 4) },
                    [ { Year = 2020; Month = Month.March; Day = 8 }, Complete ]
                    
                    { defaultTask with Name = "4"; Scheduling = TaskScheduling.Recurrency (Offset 2) },
                    [ { Year = 2020; Month = Month.March; Day = 10 }, Complete ]
                    
                    { defaultTask with Name = "5"; Scheduling = TaskScheduling.Recurrency (Offset 2) },
                    [ { Year = 2020; Month = Month.March; Day = 10 }, Postponed ]
                    
                    { defaultTask with Name = "6"; Scheduling = TaskScheduling.Recurrency (Offset 1) },
                    []
                    
                    { defaultTask with Name = "7"; Scheduling = TaskScheduling.Disabled },
                    []
                ]
                
                let dateSequence =
                    data
                    |> List.collect (fun (_, cellEvents) ->
                        cellEvents
                        |> List.map (fun (date, _) -> date)
                    )
                    |> Functions.getDateSequence (0, 0)
                
                data
                |> List.map fst
                |> List.map (fun task ->
                    data
                    |> List.filter (fun (t, _) -> t = task)
                    |> List.collect (fun (task, events) ->
                        events
                        |> List.map (fun (date, status) ->
                            { Task = task
                              Date = date
                              Status = status }
                        )
                    )
                    |> Functions.renderLane task now dateSequence
                )
                |> Functions.sortLanes now.Date
                |> List.map (fun (Lane (task, _)) -> task.Name)
                |> Expect.equal "" [ "6"; "1"; "2"; "5"; "4"; "3"; "7" ]
            
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
                
                props.CellEvents
                |> List.map (fun (date, status) ->
                    { Task = props.Task
                      Date = date
                      Status = status }
                )
                |> Functions.renderLane props.Task props.Now dateSequence
                |> unwrapLane
                |> Expect.equal "" (props.Data |> List.map (fun (date, status) -> string date, status))
               
            test "1" {
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
            
            test "2" {
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
            
            test "3" {
                testData
                    {| Task = { defaultTask with Scheduling = Recurrency (Offset 2) }
                       Now = { Date = { Year = 2020; Month = Month.March; Day = 10 }
                               Time = midnight }
                       Data = [
                           { Year = 2020; Month = Month.March; Day = 9 }, Disabled
                           { Year = 2020; Month = Month.March; Day = 10 }, EventStatus Postponed
                           { Year = 2020; Month = Month.March; Day = 11 }, Pending
                           { Year = 2020; Month = Month.March; Day = 12 }, Disabled
                       ]
                       CellEvents = [
                           { Year = 2020; Month = Month.March; Day = 10 }, Postponed
                       ] |}
            }
            
            test "4" {
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
                           { Year = 2020; Month = Month.March; Day = 12 }, Disabled
                           { Year = 2020; Month = Month.March; Day = 13 }, Pending
                           { Year = 2020; Month = Month.March; Day = 14 }, Disabled
                       ]
                       CellEvents = [
                           { Year = 2020; Month = Month.March; Day = 8 }, Complete
                       ] |}
            }
            
            test "5" {
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
            
            test "6" {
                testData
                    {| Task = { defaultTask with Scheduling = Once }
                       Now = { Date = { Year = 2020; Month = Month.March; Day = 11 }
                               Time = midnight }
                       Data = [
                           { Year = 2020; Month = Month.March; Day = 9 }, Disabled
                           { Year = 2020; Month = Month.March; Day = 10 }, Disabled
                           { Year = 2020; Month = Month.March; Day = 11 }, Pending
                           { Year = 2020; Month = Month.March; Day = 12 }, Disabled
                           { Year = 2020; Month = Month.March; Day = 13 }, Disabled
                       ]
                       CellEvents = [] |}
            }
            
            test "7" {
                testData
                    {| Task = { defaultTask with Scheduling = Once }
                       Now = { Date = { Year = 2020; Month = Month.March; Day = 11 }
                               Time = midnight }
                       Data = [
                           { Year = 2020; Month = Month.March; Day = 8 }, Disabled
                           { Year = 2020; Month = Month.March; Day = 9 }, EventStatus Postponed
                           { Year = 2020; Month = Month.March; Day = 10 }, Missed
                           { Year = 2020; Month = Month.March; Day = 11 }, Pending
                           { Year = 2020; Month = Month.March; Day = 12 }, Disabled
                           { Year = 2020; Month = Month.March; Day = 13 }, Disabled
                       ]
                       CellEvents = [
                           { Year = 2020; Month = Month.March; Day = 9 }, Postponed
                       ] |}
            }
            
            test "8" {
                testData
                    {| Task = { defaultTask with Scheduling = Recurrency (Offset 3) }
                       Now = { Date = { Year = 2020; Month = Month.March; Day = 11 }
                               Time = midnight }
                       Data = [
                           { Year = 2020; Month = Month.March; Day = 9 }, Disabled
                           { Year = 2020; Month = Month.March; Day = 10 }, EventStatus Complete
                           { Year = 2020; Month = Month.March; Day = 11 }, Disabled
                           { Year = 2020; Month = Month.March; Day = 12 }, Disabled
                           { Year = 2020; Month = Month.March; Day = 13 }, Pending
                           { Year = 2020; Month = Month.March; Day = 14 }, Disabled
                       ]
                       CellEvents = [
                           { Year = 2020; Month = Month.March; Day = 10 }, Complete
                       ] |}
            }
            
            test "9" {
                testData
                    {| Task = { defaultTask with Scheduling = TaskScheduling.Optional
                                                 PendingAfter = { Hour = 20; Minute = 0 } }
                       Now = { Date = { Year = 2020; Month = Month.March; Day = 10 }
                               Time = { Hour = 19; Minute = 30 } }
                       Data = [
                           { Year = 2020; Month = Month.March; Day = 9 }, Optional
                           { Year = 2020; Month = Month.March; Day = 10 }, Optional
                           { Year = 2020; Month = Month.March; Day = 11 }, Optional
                       ]
                       CellEvents = [] |}
            }
            
            test "9.2" {
                testData
                    {| Task = { defaultTask with Scheduling = TaskScheduling.Optional
                                                 PendingAfter = { Hour = 20; Minute = 0 } }
                       Now = { Date = { Year = 2020; Month = Month.March; Day = 10 }
                               Time = { Hour = 21; Minute = 0 } }
                       Data = [
                           { Year = 2020; Month = Month.March; Day = 9 }, Optional
                           { Year = 2020; Month = Month.March; Day = 10 }, Pending
                           { Year = 2020; Month = Month.March; Day = 11 }, Optional
                       ]
                       CellEvents = [] |}
            }
            
            test "10" {
                testData
                    {| Task = { defaultTask with Scheduling = Recurrency (Offset 1)
                                                 PendingAfter = { Hour = 20; Minute = 0 } }
                       Now = { Date = { Year = 2020; Month = Month.March; Day = 10 }
                               Time = { Hour = 19; Minute = 30 } }
                       Data = [
                           { Year = 2020; Month = Month.March; Day = 9 }, Disabled
                           { Year = 2020; Month = Month.March; Day = 10 }, Optional
                           { Year = 2020; Month = Month.March; Day = 11 }, Pending
                       ]
                       CellEvents = [] |}
            }
            
            test "10.2" {
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
            
            test "Recurrency for the next days should work normally while today is still optional (behind pendingAfter)" {
                testData
                    {| Task = { defaultTask with Scheduling = Recurrency (Offset 2)
                                                 PendingAfter = { Hour = 18; Minute = 0 } }
                       Now = { Date = { Year = 2020; Month = Month.March; Day = 27 }
                               Time = { Hour = 17; Minute = 0 } }
                       Data = [
                           { Year = 2020; Month = Month.March; Day = 25 }, Disabled
                           { Year = 2020; Month = Month.March; Day = 26 }, Disabled
                           { Year = 2020; Month = Month.March; Day = 27 }, Optional
                           { Year = 2020; Month = Month.March; Day = 28 }, Disabled
                           { Year = 2020; Month = Month.March; Day = 29 }, Pending
                           { Year = 2020; Month = Month.March; Day = 29 }, Disabled
                       ]
                       CellEvents = [] |}
            }
            
            test "Stop generating pending tasks when finding a Dropped status" {
                testData
                    {| Task = { defaultTask with Scheduling = Recurrency (Offset 3) }
                       Now = { Date = { Year = 2020; Month = Month.March; Day = 27 }
                               Time = midnight }
                       Data = [
                           { Year = 2020; Month = Month.March; Day = 25 }, EventStatus Complete
                           { Year = 2020; Month = Month.March; Day = 26 }, Disabled
                           { Year = 2020; Month = Month.March; Day = 27 }, EventStatus Dropped
                           { Year = 2020; Month = Month.March; Day = 28 }, Disabled
                           { Year = 2020; Month = Month.March; Day = 29 }, Disabled
                           { Year = 2020; Month = Month.March; Day = 29 }, Disabled
                       ]
                       CellEvents = [
                           { Year = 2020; Month = Month.March; Day = 25 }, Complete
                           { Year = 2020; Month = Month.March; Day = 27 }, Dropped
                       ] |}
            }
            
            test "Weekly Fixed Recurrency" {
                testData
                    {| Task = { defaultTask with Scheduling = Recurrency (Fixed (Weekly DayOfWeek.Saturday)) }
                       Now = { Date = { Year = 2020; Month = Month.March; Day = 20 }
                               Time = midnight }
                       Data = [
                           for d in 13 .. 29 do
                               { Year = 2020; Month = Month.March; Day = d },
                               if d = 14
                               then EventStatus Complete
                               elif d = 21 || d = 28
                               then Pending
                               else Disabled
                       ]
                       CellEvents = [
                           { Year = 2020; Month = Month.March; Day = 14 }, Complete
                       ] |}
            }
            
        ]
    ]

