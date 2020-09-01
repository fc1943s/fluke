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

type InformationName = InformationName of name:string
type TaskName = TaskName of name:string
type InformationId = InformationId of id:string

let rec informationId (information: Information) : InformationId =
    match information with
    | Project x  -> sprintf "%s/%s" information.KindName x.Name
    | Area x     -> sprintf "%s/%s" information.KindName x.Name
    | Resource x -> sprintf "%s/%s" information.KindName x.Name
    | Archive x  ->
        let (InformationId archiveId) = informationId x
        sprintf "%s/%s" information.KindName archiveId
    |> InformationId



[<RequireQualifiedAccess>]
type InformationKind =
    | Project
    | Area
    | Resource
    | Archive of InformationKind
    member this.Name =
        match this with
        | Project  -> "project"
        | Area     -> "area"
        | Resource -> "resource"
        | Archive kind  -> sprintf "[%s]" kind.Name
    static member FromInformation = function
        | Information.Project _            -> InformationKind.Project
        | Information.Area _               -> InformationKind.Area
        | Information.Resource _           -> InformationKind.Resource
        | Information.Archive information  -> InformationKind.Archive (InformationKind.FromInformation information)

type Event =
    | AddInformation of id:InformationId * kind:InformationKind * name:InformationName
    | AddTask of informationId:InformationId * name:TaskName


type TaskState =
    { Name: TaskName }


let a = 3

let b =
    rawTreeData
    |> List.map (fun (information, tasks) -> [
        let informationId = informationId information
        [ AddInformation (
            informationId,
            InformationKind.FromInformation information,
            InformationName information.Name) ]

        tasks
        |> List.map (fun (taskName, events) ->
            AddTask (informationId, TaskName taskName)
        )
    ])

()
//printfn "AA: %A" a
//let d = c.Head
