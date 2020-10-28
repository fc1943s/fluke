namespace Fluke.UI.Backend

open System
open System.IO
open Fable.Remoting.Json
open Newtonsoft.Json
open Saturn
open Fluke.Shared
open FSharpPlus
open Fable.Remoting.Server
open Fable.Remoting.Giraffe


module Server =
    open Domain.State
    open Domain.UserInteraction
    open Templates

    module Json =
        let converter = FableJsonConverter ()

        let deserialize<'a> (json: string) =
            if typeof<'a> = typeof<string> then
                unbox<'a> (box json)
            else
                JsonConvert.DeserializeObject (json, typeof<'a>, converter) :?> 'a

    module Sync =
        open Sync

        let api: Api =
            let (</>) a b = Path.Combine (a, b)

            let readFile path =
                let getEnvVar name =
                    match Environment.GetEnvironmentVariable name with
                    | null
                    | "" -> sprintf "Invalid EnvVar: %s" name |> Error
                    | s -> s |> Ok

                match getEnvVar "FLUKE_TEMP_DATA_PATH" with
                | Ok tempDataPath ->
                    let fullPath = tempDataPath </> path

                    try
                        File.ReadAllText fullPath
                    with ex ->
                        printfn "path readAllText error: %A %A" fullPath ex
                        ""
                | Error error ->
                    printfn "%s" error
                    ""

            {
                currentUser =
                    async {
                        let currentUserJson = readFile "currentUser.json"
                        return Json.deserialize<User> currentUserJson
                    }
                treeStateList =
                    fun username moment ->
                        async {
                            let treeStateListJson = readFile "treeStateList.json"

                            let treeStateList = Json.deserialize<TreeState list> treeStateListJson

                            let templates =
                                getTreeMap TempData.testUser
                                |> Map.toList
                                |> List.map (fun (templateName, dslTemplate) ->
                                    treeStateFromDslTemplate TempData.testUser templateName dslTemplate)

                            let treesWithAccess =
                                treeStateList
                                |> List.append templates
                                |> List.filter (fun treeState ->
                                    match treeState with
                                    | { Owner = owner } when owner.Username = username -> true
                                    | { SharedWith = TreeAccess.Public } -> true
                                    | { SharedWith = TreeAccess.Private accessList } ->
                                        accessList
                                        |> List.exists (function
                                            | (TreeAccessItem.Admin user
                                            | TreeAccessItem.ReadOnly user) when user.Username = username -> true
                                            | _ -> false)
                                    | _ -> false)


                            return treesWithAccess
                        }
            }

    let webApp =
        Remoting.createApi ()
        |> Remoting.fromValue Sync.api
        |> Remoting.withBinarySerialization
        |> Remoting.withDiagnosticsLogger (printfn "#> %s")
        |> Remoting.buildHttpHandler

    let app =
        application {
            url (sprintf "https://0.0.0.0:%s/" Sync.serverPort)
            use_router webApp
            use_gzip
            force_ssl
        }
