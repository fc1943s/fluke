namespace Fluke.UI.Backend

open System
open System.IO
open Saturn
open Fluke.Shared
open FSharpPlus
open Fable.Remoting.Server
open Fable.Remoting.Giraffe


module Server =
    open Domain.State
    open Domain.UserInteraction
    open Templates

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
                        return match Thoth.Json.Net.Decode.Auto.fromString currentUserJson with
                               | Ok currentUser -> currentUser
                               | Error error ->
                                   printfn "currentUser error: %A" error
                                   TempData.testUser
                    }
                treeStateList =
                    fun username moment ->
                        async {
                            let treeStateListJson = readFile "treeStateList.json"

                            let treeStateList =
                                match Thoth.Json.Net.Decode.Auto.fromString treeStateListJson with
                                | Ok treeStateList -> treeStateList
                                | Error error ->
                                    printfn "treeStateList error: %A" error
                                    []

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
