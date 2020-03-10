namespace Fluke.Shared

open Fluke.Shared
open Suigetsu.Core
open Suigetsu.Testing
open Xunit
open FsUnit.Xunit
open Fluke.Shared.Model
    
[<AutoOpen>]
module Data =
    let defaultTask =
        { Name = "<blank>"
          InformationType = Area { Name = "Area" }
          Comments = []
          Scheduling = TaskScheduling.Disabled
          Duration = None
          PendingAfter = None }
    
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
    
    
module ModelTests =
    type ModelTests (output) =
        inherit DefaultTestRunner (output)
        
        [<Fact>]
        member _.GetManualSortedTaskListTests () =
            orderedEventList
            |> Functions.getManualSortedTaskList
            |> should equal orderedList
            
            [
                { Task = task5; Priority = Last }
                { Task = task4; Priority = LessThan task3 }
                { Task = task3; Priority = LessThan task2 }
                { Task = task2; Priority = LessThan task1 }
                { Task = task1; Priority = First }
            ]
            |> Functions.getManualSortedTaskList
            |> should equal orderedList
            
            [
                { Task = task3; Priority = LessThan task2 }
                { Task = task5; Priority = Last }
                { Task = task4; Priority = LessThan task3 }
                { Task = task1; Priority = First }
                { Task = task2; Priority = LessThan task1 }
            ]
            |> Functions.getManualSortedTaskList
            |> should equal orderedList
            
            [
                { Task = task3; Priority = First }
            ]
            |> List.append orderedEventList
            |> Functions.getManualSortedTaskList
            |> should equal [ task3; task4; task1; task2; task5 ]
            
            [
                { Task = task3; Priority = First }
                { Task = task4; Priority = LessThan task2 }
            ]
            |> List.append orderedEventList
            |> Functions.getManualSortedTaskList
            |> should equal [ task3; task1; task2; task4; task5 ]
            
            [
                { Task = task1; Priority = First }
                { Task = task2; Priority = First }
                { Task = task3; Priority = First }
                { Task = task4; Priority = First }
                { Task = task5; Priority = LessThan task4 }
            ]
            |> Functions.getManualSortedTaskList
            |> should equal [ task4; task5; task3; task2; task1 ]
            
        [<Fact>]
        member _.GetSortedTaskListTests () =
            let today = { Year = 2020; Month = 3; Day = 10 }
            let data = [
                { defaultTask with Name = "1"; Scheduling = TaskScheduling.Optional },
                [] 
               
                { defaultTask with Name = "2"; Scheduling = TaskScheduling.Optional },
                [ { Year = 2020; Month = 3; Day = 10 }, Postponed ]
                
                { defaultTask with Name = "3"; Scheduling = TaskScheduling.Recurrency 4 },
                [ { Year = 2020; Month = 3; Day = 8 }, Complete ]
                
                { defaultTask with Name = "4"; Scheduling = TaskScheduling.Recurrency 2 },
                [ { Year = 2020; Month = 3; Day = 10 }, Complete ]
                
                { defaultTask with Name = "5"; Scheduling = TaskScheduling.Recurrency 2 },
                [ { Year = 2020; Month = 3; Day = 10 }, Postponed ]
                
                { defaultTask with Name = "6"; Scheduling = TaskScheduling.Recurrency 1 },
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
                |> Functions.renderLane task today dateSequence
            )
            |> Functions.sortLanes today
            |> List.map (fun (Lane (task, _)) -> task.Name)
            |> should equal [ "6"; "1"; "2"; "5"; "4"; "3"; "7" ]
            
            
        [<Fact>]
        member _.SetPriorityTests () =
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
            |> should equal [
                { Task = task3; Priority = First }
                { Task = task4; Priority = LessThan task2 }
            ]
            
            [ task1; task2; task3; task4; task5 ]
            |> createPriorityEvents task1 First
            |> should equal [
                { Task = task1; Priority = First }
            ]
            
            [ task1; task2; task3; task4; task5 ]
            |> createPriorityEvents task1 Last
            |> should equal [
                { Task = task1; Priority = Last }
                { Task = task2; Priority = First }
            ]
            
            [ task1; task2; task3; task4; task5 ]
            |> createPriorityEvents task5 Last
            |> should equal [
                { Task = task5; Priority = Last }
            ]
            
            [ task1; task2; task3; task4; task5 ]
            |> createPriorityEvents task5 First
            |> should equal [
                { Task = task5; Priority = First }
                { Task = task1; Priority = LessThan task5 }
            ]
            
            [ task1; task2; task3; task4; task5 ]
            |> createPriorityEvents task2 First
            |> should equal [
                { Task = task2; Priority = First }
                { Task = task3; Priority = LessThan task1 }
            ]
            
            [ task1; task2; task3; task4; task5 ]
            |> createPriorityEvents task4 (LessThan task1)
            |> should equal [
                { Task = task4; Priority = LessThan task1 }
                { Task = task5; Priority = LessThan task3 }
            ]
            
        [<Fact>]
        member _.RenderLaneTests () =
            let unwrapLane (Lane (_, cells)) =
               cells
               |> List.map (fun x -> x.Date, x.Status)
            
            let today = { Year = 2020; Month = 3; Day = 9 }
            let data = [
                  { Year = 2020; Month = 3; Day = 7 }, EventStatus Missed
                  { Year = 2020; Month = 3; Day = 8 }, EventStatus Missed
                  { Year = 2020; Month = 3; Day = 9 }, Pending
                  { Year = 2020; Month = 3; Day = 10 }, Disabled
                  { Year = 2020; Month = 3; Day = 11 }, Pending
                  { Year = 2020; Month = 3; Day = 12 }, Disabled
            ]
            let task = { defaultTask with Scheduling = Recurrency 2 }
            let cellEvents = []
            
            Functions.renderLane task today (data |> List.map fst) cellEvents
            |> unwrapLane
            |> should equal data
            
            //
            
            let today = { Year = 2020; Month = 3; Day = 9 }
            let data = [
                { Year = 2020; Month = 3; Day = 8 }, EventStatus Complete
                { Year = 2020; Month = 3; Day = 9 }, Disabled
                { Year = 2020; Month = 3; Day = 10 }, Disabled
                { Year = 2020; Month = 3; Day = 11 }, Pending
                { Year = 2020; Month = 3; Day = 12 }, Disabled
                { Year = 2020; Month = 3; Day = 13 }, Disabled
                { Year = 2020; Month = 3; Day = 14 }, Pending
                { Year = 2020; Month = 3; Day = 15 }, Disabled
            ]
            let task = { defaultTask with Scheduling = Recurrency 3 }
            let cellEvents = [
                { Task = task
                  Date = { Year = 2020; Month = 3; Day = 8 }
                  Status = Complete }
            ]
            
            Functions.renderLane task today (data |> List.map fst) cellEvents
            |> unwrapLane
            |> should equal data
            
            //
            
            let today = { Year = 2020; Month = 3; Day = 10 }
            let data = [
                { Year = 2020; Month = 3; Day = 9 }, EventStatus Missed
                { Year = 2020; Month = 3; Day = 10 }, EventStatus Postponed
                { Year = 2020; Month = 3; Day = 11 }, Pending
                { Year = 2020; Month = 3; Day = 12 }, Disabled
            ]
            let task = { defaultTask with Scheduling = Recurrency 2 }
            let cellEvents = [
                { Task = task
                  Date = { Year = 2020; Month = 3; Day = 10 }
                  Status = Postponed }
            ]
            
            Functions.renderLane task today (data |> List.map fst) cellEvents
            |> unwrapLane
            |> should equal data
            
            
