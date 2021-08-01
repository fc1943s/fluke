namespace Fluke.UI.Backend

open System.Threading
open Fable.SignalR
open Fluke.Shared
open FsCore.Model
open FsStore.Shared
open Fumble
open Microsoft.Extensions.Logging
open System.Text.RegularExpressions
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
                    Thread.Sleep 50
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
            |> Sqlite.query $""" SELECT key FROM {table} where key like "%%/{key}/%%" """
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

        let update (msg: Sync.Request) (hubContext: FableHub<Sync.Request, Sync.Response> option) =
            //            printfn $"Model.update() msg={msg}"

            match msg with
            | Sync.Request.Connect (Username username) ->
                memoizedCreateTable username
                printfn $"@@@ Sync.Request.Connect username={username}"
                Sync.Response.ConnectResult
            | Sync.Request.Set (Username username, key, value) ->
                memoizedCreateTable username
                let result = insert username key value
                //                printfn $"set {key} {value}"
                match hubContext with
                | Some hub when result ->
//                    hub.Clients.All.Send (Sync.Response.GetResult (key, value))
                    ()
                | _ -> ()
                Sync.Response.SetResult result
            | Sync.Request.Get (Username username, key) ->
                memoizedCreateTable username
                let result = query username key

                result
                |> Option.bind (fun x -> x.Value)
                |> Sync.Response.GetResult
            | Sync.Request.Filter (Username username, key) ->
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

                printfn $"@@@ filter {key} total={preResult.Length} result={result.Length}"

                Sync.Response.FilterResult result

        let invoke (msg: Sync.Request) _ = task { return update msg None }

        let send (msg: Sync.Request) (hubContext: FableHub<Sync.Request, Sync.Response>) =
            hubContext.Clients.Caller.Send (update msg (Some hubContext))

        [<RequireQualifiedAccess>]
        module Stream =
            open FSharp.Control

            let sendToClient (msg: Sync.Request) (_hubContext: FableHub<Sync.Request, Sync.Response>) =
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
                        endpoint Sync.endpoint
                        send Model.send
                        invoke Model.invoke
                        stream_from Model.Stream.sendToClient
                        //                        use_messagepack
                        //                        with_log_level LogLevel.Trace
                        with_hub_options (fun options -> options.EnableDetailedErrors <- true)

                        with_after_routing
                            (fun _x ->
                                printfn "saturn.with_after_routing()"
                                _x)

                        with_before_routing
                            (fun _x ->
                                printfn "saturn.with_before_routing()"
                                _x)

                        with_on_disconnected (fun ex _hub -> task { printfn $"saturn.with_on_disconnected() ex={ex}" })

                        with_on_connected
                            (fun _hub ->
                                task {
                                    //                                    let! result = Model.send Sync.Request.Connect hub
                                    printfn "saturn.with_on_connected()"
                                //                                    return result
                                })
                    //                                    return result
                    }
                )

                //                config
                use_cors
                    "cors"
                    (fun corsBuilder ->
                        corsBuilder
                            .AllowCredentials()
                            .AllowAnyHeader()
                            .WithOrigins [|
                                "https://localhost:33929"
                            |]
                        |> ignore)

                url "https://0.0.0.0:33921/"
                use_gzip
                disable_diagnostics
                use_developer_exceptions
                memory_cache
                no_router

                logging
                    (fun logging ->
                        logging.SetMinimumLevel LogLevel.Debug |> ignore

                        logging.AddFilter ("Microsoft.", LogLevel.Warning)
                        |> ignore)

                force_ssl
            //                                    return result
            }

        printfn "starting..."
        run app
        0
