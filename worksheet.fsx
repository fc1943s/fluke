//#r "C:/Users/fc194/.nuget/packages/fsharpplus/1.1.3/lib/netstandard2.0/FSharpPlus.dll"
//#r "nuget: FSharpPlus"
#r "src/Fluke.Shared/bin/Debug/netcoreapp5.0/Fluke.Shared.dll"

open Fluke.Shared.Model
open Fluke.Shared.TempData
open Fluke.Shared.PrivateData.PrivateData

//let c = Fluke.Shared.PrivateData.PrivateData.Projects.app_fluke
//let c = Tasks.rawTreeData


let rawTreeData =
    [
            Project Projects.app_fluke, [
                "random selected tasks", [
                    TempPriority High9
                    TempComment "another option would be a priority range selector. ex: select from 10 to 7"
                    TempComment "fix shift selecting so you can get the selected tasks"
                ]
            ]
    ]

type TaskClass () =
    do ()

let ``task name 1`` = TaskClass ()

module Validate =
    let name (name : string) =
//        if String.IsNullOrWhiteSpace name then
//            failwith "Server name can not be null, empty, or blank"
        if name.Length > 63 || name.Length < 3 then
            failwithf "Server name must have a length between 3 and 63, was %d" name.Length
        if name.[0] = '-' || name.[name.Length-1] = '-' then
            failwith "Server name must not start or end with a hyphen ('-')"
//        if isAsciiDigit name.[0] then
//            failwith "Server name must not start with a digit"
//        if not (Seq.forall isLegalServernameChar name) then
//            failwithf "Server name can only consist of ASCII lowercase letters, digits, or hyphens. Was '%s'" name

type TaskName = TaskName of name:string

type TaskState =
    { Name: TaskName }

type TaskBuilder () =
    member _.Yield _ =
        { Name = TaskName "" }

    [<CustomOperation "name">]
    member _.SetName(state:TaskState, name) =
        let (TaskName nameStr) = name
        Validate.name nameStr
        { state with Name = name }

let task = TaskBuilder ()


let ``random selected tasks`` = task {
    name (TaskName "b")
}


let a = 3

()
//printfn "AA: %A" a
//let d = c.Head
