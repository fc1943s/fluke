﻿namespace Fluke.UI.Backend

open System.Collections.Generic
open Fable.SignalR
open Fluke.Shared
open Fluke.Shared.Domain.UserInteraction
open Fumble
open Microsoft.Extensions.Logging
open System.Text.RegularExpressions
open Fluke.Shared.Api
open FSharp.Control.Tasks.V2
open Saturn


module Main =
    module Model =
        let connectionString = "Data Source=.\db.db"

        let createTable (table: string) =
            connectionString
            |> Sqlite.connect
            |> Sqlite.command
                $"
            CREATE TABLE IF NOT EXISTS {table} (
                    key string PRIMARY KEY,
                    value string
                ) WITHOUT ROWID;
            "
            |> Sqlite.executeCommand
            |> function
                | Ok rows ->
                    printfn $"table {table} created. rows affected %A{rows}"
                    ()
                | Error err -> failwith $"create table error err={err}"

        let insert (table: string) key value =
            connectionString
            |> Sqlite.connect
            |> Sqlite.command
                $"INSERT into {table} (key, value)
                 values (@Key, @Value)
                 ON CONFLICT(key)
                 DO UPDATE SET value=@Value;
                "
            |> Sqlite.insertData [
                {| Key = key; Value = value |}
               ]
            |> function
                | Ok _rows ->
                    //                    printfn $"rows affected %A{rows.Length}"
                    true
                | Error err ->
                    printfn $"error %A{err}"
                    false

        let query (table: string) key =
            connectionString
            |> Sqlite.connect
            |> Sqlite.query $"SELECT * FROM {table} where key=@Key "
            |> Sqlite.parameters [
                "@Key", Sqlite.string key
               ]
            |> Sqlite.execute
                (fun read ->
                    {|
                        Key = read.string "key"
                        Value = read.stringOrNone "value"
                    |})
            |> function
                | Ok result ->
                    //                    printfn $"result %A{result}"
                    result |> List.tryHead
                | Error err ->
                    printfn $"error %A{err}"
                    None

        let preKeyFilter (table: string) key =
            connectionString
            |> Sqlite.connect
            |> Sqlite.query $" SELECT key FROM {table} where key like '%%/@Key/%%' "
            |> Sqlite.parameters [
                "@Key", Sqlite.string key
               ]
            |> Sqlite.execute (fun read -> {| Key = read.string "key" |})
            |> function
                | Ok result ->
                    //                    printfn $"result %A{result}"
                    result |> List.map (fun x -> x.Key)
                | Error err ->
                    printfn $"error %A{err}"
                    []

        let getMemoizedCreateTable () =
            let mutable set = Set.empty

            fun (table: string) ->
                if set |> Set.contains table |> not then
                    set <- set |> Set.add table
                    printfn $"creating table {table}"
                    createTable table

        let memoizedCreateTable = getMemoizedCreateTable ()

        let update (msg: Api.Action) =
            printfn $"Model.update() msg={msg}"

            match msg with
            | Api.Action.Connect (Username username) ->
                memoizedCreateTable username
                printfn $"Api.Action.Connect username={username}"
                Response.ConnectResult
            | Api.Action.Set (Username username, key, value) ->
                memoizedCreateTable username
                let result = insert username key value
                //                printfn $"set {key} {value}"
                Response.SetResult result
            | Api.Action.Get (Username username, key) ->
                memoizedCreateTable username
                let result = query username key

                result
                |> Option.bind (fun x -> x.Value)
                |> Response.GetResult
            | Api.Action.Filter (Username username, key) ->
                memoizedCreateTable username
                let preResult = preKeyFilter username key

                let result =
                    preResult
                    |> Seq.choose
                        (fun key' ->
                            let result = Regex.Match (key', $"^.*?/{key}/([a-fA-F0-9\\-]{{36}})/")
                            if result.Groups.Count = 2 then Some result.Groups.[1].Value else None)
                    |> Seq.distinct
                    |> Seq.toArray

                printfn $"filter {key} total={preResult.Length} result={result.Length}"

                Response.FilterResult result

        let invoke (msg: Api.Action) _ = task { return update msg }

        let send (msg: Api.Action) (hubContext: FableHub<Api.Action, Response>) =
            hubContext.Clients.Caller.Send (update msg)

        [<RequireQualifiedAccess>]
        module Stream =
            open FSharp.Control

            let sendToClient (msg: Api.Action) (_hubContext: FableHub<Api.Action, Api.Response>) =
                asyncSeq {
                    try
                        update msg
                    with
                    | ex -> printfn $"sendToClient exception. ex={ex}"
                }
                |> AsyncSeq.toAsyncEnum

    [<EntryPoint>]
    let main _ =
        let app =
            application {
                use_signalr (
                    configure_signalr {
                        endpoint Api.endpoint
                        send Model.send
                        invoke Model.invoke
                        stream_from Model.Stream.sendToClient

                        with_on_disconnected (fun ex _hub -> task { printfn $"saturn.with_on_disconnected() ex={ex}" })

                        with_on_connected
                            (fun _hub ->
                                task {
                                    //                                    let! result = Model.send Api.Action.Connect hub
                                    printfn "saturn.with_on_connected()"
                                //                                    return result
                                })
                    //                                    return result
                    }
                )

                use_cors
                    "cors"
                    (fun corsBuilder ->
                        corsBuilder
                            .AllowCredentials()
                            .WithHeaders(
                                [|
                                    "x-requested-with"
                                    "x-signalr-user-agent"
                                |]
                            )
                            .WithOrigins [|
                                "https://localhost:33929"
                            |]
                        |> ignore)

                url "https://0.0.0.0:33921/"
                use_gzip
                //                disable_diagnostics
                use_developer_exceptions
                memory_cache
                no_router
                logging (fun logging -> logging.SetMinimumLevel LogLevel.Warning |> ignore)
                force_ssl
            //                                    return result
            }

        printfn "starting..."
        run app
        0
