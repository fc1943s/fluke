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
                    | "" -> Error $"Invalid EnvVar: {name}"
                    | s -> s |> Ok

                match getEnvVar "FLUKE_TEMP_DATA_PATH" with
                | Ok tempDataPath ->
                    let fullPath = tempDataPath </> path

                    try
                        File.ReadAllText fullPath
                    with ex ->
                        printfn $"path readAllText error: {fullPath} {ex}"
                        ""
                | Error error ->
                    printfn $"{error}"
                    ""

            {
                currentUser =
                    async {
                        let currentUserJson = readFile "currentUser.json"
                        return Json.deserialize<User> currentUserJson
                    }
                databaseStateList =
                    fun username moment ->
                        async {
                            let databaseStateListJson = readFile "databaseStateList.json"

                            let databaseStateList = Json.deserialize<DatabaseState list> databaseStateListJson

                            let templates =
                                getDatabaseMap TempData.testUser
                                |> Map.toList
                                |> List.map (fun (templateName, dslTemplate) ->
                                    databaseStateFromDslTemplate
                                        TempData.testUser
                                        (DatabaseId (Guid.NewGuid ()))
                                        templateName
                                        dslTemplate)

                            let databasesWithAccess =
                                databaseStateList
                                |> List.append templates
                                |> List.filter (fun databaseState ->
                                    match databaseState.Database with
                                    | { Owner = owner } when owner.Username = username -> true
                                    | { SharedWith = DatabaseAccess.Public } -> true
                                    | { SharedWith = DatabaseAccess.Private accessList } ->
                                        accessList
                                        |> List.exists (function
                                            | (DatabaseAccessItem.Admin user
                                            | DatabaseAccessItem.ReadOnly user) when user.Username = username -> true
                                            | _ -> false)
                                    | _ -> false)


                            return databasesWithAccess
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
            url $"https://0.0.0.0:{Sync.serverPort}/"
            use_router webApp
            use_gzip
            force_ssl
        }
