namespace Fluke.UI.Backend

open Fable.SignalR
open Fluke.Shared
open Fumble
open Microsoft.Extensions.Logging
open System.Text.RegularExpressions
open Fluke.Shared.Api
open FSharp.Control.Tasks.V2
open Saturn


module Main =
    module Model =
        let connectionString = "Data Source=.\db.db"

        let createTable () =
            connectionString
            |> Sqlite.connect
            |> Sqlite.command
                "
            CREATE TABLE IF NOT EXISTS data (
                    key string PRIMARY KEY,
                    value string
                ) WITHOUT ROWID;
            "
            |> Sqlite.executeCommand
            |> function
                | Ok _rows ->
                    //                    printfn $"rows affected %A{rows}"
                    ()
                | Error err -> failwith $"create table error err={err}"

        let insert key value =
            connectionString
            |> Sqlite.connect
            |> Sqlite.command
                "INSERT into data(key, value)
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

        let query key =
            connectionString
            |> Sqlite.connect
            |> Sqlite.query " SELECT * FROM data where key=@Key "
            |> Sqlite.parameters [
                "@Key", Sqlite.string key
               ]
            |> Sqlite.execute
                (fun read ->
                    {|
                        Key = read.string "key"
                        Value = read.string "value"
                    |})
            |> function
                | Ok result ->
                    //                    printfn $"result %A{result}"
                    result |> List.tryHead
                | Error err ->
                    printfn $"error %A{err}"
                    None

        let preKeyFilter key =
            connectionString
            |> Sqlite.connect
            |> Sqlite.query " SELECT key FROM data where key like '%/@Key/%' "
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

        let update (msg: Api.Action) =
            printfn $"Model.update() msg={msg}"

            match msg with
            | Api.Action.Connect -> Response.ConnectResult
            | Api.Action.Set (key, value) ->
                let result = insert key value
                //                printfn $"set {key} {value}"
                Response.SetResult result
            | Api.Action.Get key ->
                let result = query key

                result
                |> Option.map (fun x -> x.Key)
                |> Response.GetResult
            | Api.Action.Filter key ->
                let preResult = preKeyFilter key

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
                            (fun hub ->
                                task {
                                    let! result = Model.send Api.Action.Connect hub
                                    printfn $"saturn.with_on_connected() Action.Connect result={result}"
                                    return result
                                })

                    //                        use_messagepack
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

            //                        use_messagepack
            }

        printfn "creating table..."
        Model.createTable ()
        printfn "starting..."
        run app
        0
