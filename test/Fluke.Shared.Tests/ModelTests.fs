namespace Fluke.Shared

open System
open Fluke.Shared
open Serilog
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
        member _.GetTaskListTests () =
            orderedEventList
            |> Functions.getTaskList
            |> should equal orderedList
            
            [
                { Task = task5; Priority = Last }
                { Task = task4; Priority = LessThan task3 }
                { Task = task3; Priority = LessThan task2 }
                { Task = task2; Priority = LessThan task1 }
                { Task = task1; Priority = First }
            ]
            |> Functions.getTaskList
            |> should equal orderedList
            
            [
                { Task = task3; Priority = LessThan task2 }
                { Task = task5; Priority = Last }
                { Task = task4; Priority = LessThan task3 }
                { Task = task1; Priority = First }
                { Task = task2; Priority = LessThan task1 }
            ]
            |> Functions.getTaskList
            |> should equal orderedList
            
            [
                { Task = task3; Priority = First }
            ]
            |> List.append orderedEventList
            |> Functions.getTaskList
            |> should equal [ task3; task4; task1; task2; task5 ]
            
            [
                { Task = task3; Priority = First }
                { Task = task4; Priority = LessThan task2 }
            ]
            |> List.append orderedEventList
            |> Functions.getTaskList
            |> should equal [ task3; task1; task2; task4; task5 ]
            
            [
                { Task = task1; Priority = First }
                { Task = task2; Priority = First }
                { Task = task3; Priority = First }
                { Task = task4; Priority = First }
                { Task = task5; Priority = LessThan task4 }
            ]
            |> Functions.getTaskList
            |> should equal [ task4; task5; task3; task2; task1 ]
            
        [<Fact>]
        member _.SetPriorityTests () =
            let getPriorityEvents task priority taskList =
                taskList
                |> List.tryFindIndexBack ((=) task)
                |> function
                    | None -> None
                    | Some i -> 
                        match (i + 1, i - 1) |> Tuple2.map (fun x -> taskList |> List.tryItem x), priority with
                        | (Some below, None), First when i = 0 -> None
                        | (Some below, None), _ -> Some { Task = below; Priority = First }
                        | (Some below, Some above), _ -> Some { Task = below; Priority = LessThan above }
                        | _, First when i > 0 -> Some { Task = taskList |> List.head; Priority = LessThan task }
                        | _ -> None
                |> Option.toList
                |> List.append [ { Task = task; Priority = priority } ]
            
            [ task1; task2; task3; task4; task5 ]
            |> getPriorityEvents task3 First
            |> should equal [
                { Task = task3; Priority = First }
                { Task = task4; Priority = LessThan task2 }
            ]
            
            [ task1; task2; task3; task4; task5 ]
            |> getPriorityEvents task1 First
            |> should equal [
                { Task = task1; Priority = First }
            ]
            
            [ task1; task2; task3; task4; task5 ]
            |> getPriorityEvents task1 Last
            |> should equal [
                { Task = task1; Priority = Last }
                { Task = task2; Priority = First }
            ]
            
            [ task1; task2; task3; task4; task5 ]
            |> getPriorityEvents task5 Last
            |> should equal [
                { Task = task5; Priority = Last }
            ]
            
            [ task1; task2; task3; task4; task5 ]
            |> getPriorityEvents task5 First
            |> should equal [
                { Task = task5; Priority = First }
                { Task = task1; Priority = LessThan task5 }
            ]
            
            [ task1; task2; task3; task4; task5 ]
            |> getPriorityEvents task2 First
            |> should equal [
                { Task = task2; Priority = First }
                { Task = task3; Priority = LessThan task1 }
            ]
            
            [ task1; task2; task3; task4; task5 ]
            |> getPriorityEvents task4 (LessThan task1)
            |> should equal [
                { Task = task4; Priority = LessThan task1 }
                { Task = task5; Priority = LessThan task3 }
            ]
            
        [<Fact>]
        member _.RenderCellsTests () =
            
            let dateSequence =
                [ { Year = 2020; Month = 3; Day = 7 }
                  { Year = 2020; Month = 3; Day = 22 } ]
                |> Functions.getDateSequence (0, 0)
                
            let getExpectedStatusList expectedList =
                let expectedMap =
                    expectedList
                    |> Map.ofList
                    
                dateSequence
                |> List.map (fun date ->
                    expectedMap
                    |> Map.tryFind date
                    |> function
                        | Some status -> status
                        | None -> Model.CellStatus.Disabled
                    |> fun x -> date, x
                )
                
            let expectedStatusList =
                [ { Year = 2020; Month = 3; Day = 9 }, Pending
                  { Year = 2020; Month = 3; Day = 20 }, Pending ]
                |> getExpectedStatusList
            
            let today = { Year = 2020; Month = 3; Day = 9 }
            
            let task = { defaultTask with Scheduling = Recurrency 11 }
            
            let cells = []
            
            Functions.renderLane task today dateSequence cells
            |> should equal expectedStatusList
            
            
            
            let expectedStatusList =
                [ { Year = 2020; Month = 3; Day = 8 }, EventStatus Complete
                  { Year = 2020; Month = 3; Day = 11 }, Pending
                  { Year = 2020; Month = 3; Day = 14 }, Pending
                  { Year = 2020; Month = 3; Day = 17 }, Pending
                  { Year = 2020; Month = 3; Day = 20 }, Pending ]
                |> getExpectedStatusList
            
            let task = { defaultTask with Scheduling = Recurrency 3 }
            
            let cells = [
                { Task = task
                  Date = { Year = 2020; Month = 3; Day = 8 }
                  Status = Complete }
            ]
            
            Functions.renderLane task today dateSequence cells
            |> should equal expectedStatusList
            
            
            
