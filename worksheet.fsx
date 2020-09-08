#r "C:/Users/fc194/.nuget/packages/fsharpplus/1.1.3/lib/netstandard2.0/FSharpPlus.dll"
//#r "nuget: FSharpPlus"
#r "src/Fluke.Shared/bin/Debug/netcoreapp5.0/Fluke.Shared.dll"

open Fluke.Shared.Model
open Fluke.Shared
open System.Collections.Generic
open FSharpPlus


//let a = PrivateData.PrivateData.getPrivateAreas ()
let state =
    RootPrivateData.TreeData.getState().TreeStateMap
    |> Map.values
    |> Seq.map fst
    |> Seq.map (fun treeState ->
        treeState.TaskStateMap
        |> Map.tryPick (fun k v -> if k.Name = TaskName "seethrus" then Some v else None)

        )
    |> Seq.toList

let v = ()

printfn "AA %A" v
printfn "1"

let init : Map<string, string> = Map.empty

type Cmd =
    | Set of Map<string, string>
    | Add of string * string
    | Get of Map<string, string>

let taskMailboxMap =
    MailboxProcessor.Start(fun inbox ->
        let taskDictionary = Dictionary<string, Task> ()
        let rec loop taskDictionary =
            async {
                do printfn "currentMap = %A, waiting..." map
                let! (key, value) = inbox.Receive()
                let newMap =
                    map
//                    |> Map.add key value

                return! loop taskDictionary
            }

        loop taskDictionary)

//val counter : MailboxProcessor<int>
let a = taskMailboxMap.Post ("task1", "val1")
printfn "result: %A\n" a
let b = taskMailboxMap.Post ("task1", "val2")
printfn "result: %A\n" b
let c = taskMailboxMap.Post ("task2", "val2")
printfn "result: %A\n" c

let d = taskMailboxMap.Post ("task3", "val2")

let e = taskMailboxMap.Scan (fun ((key, value) as x) ->
    match key with
    | "task2" -> Some <| async { return x }
    | "task1" -> Some <| async { return x }
    | "task3" -> Some <| async { return x }
    | _ -> None
    )

let f = e |> Async.RunSynchronously
