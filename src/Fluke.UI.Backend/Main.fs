namespace Fluke.UI.Backend

open FSharp.Control
open System.Threading
open FsCore
open Fable.SignalR
open Fluke.Shared
open FsStore.Shared
open Fumble
open Microsoft.Extensions.Logging
open System.Text.RegularExpressions
open FSharp.Control.Tasks.V2
open Saturn


module Main =
    module Model =
        let getConnectionString username =
            $"Data Source=./data/{username}.sqlite3"

        let createTable username =
            getConnectionString username
            |> Sqlite.connect
            |> Sqlite.command
                " CREATE TABLE IF NOT EXISTS data (
                    key string PRIMARY KEY,
                    value string
                  ) WITHOUT ROWID; "
            |> Sqlite.executeCommand
            |> function
                | Ok rows ->
                    printfn $"table data created. rows affected %A{rows}"
                    Thread.Sleep 50
                | Error err -> failwith $"create table error err={err}"

        let getMemoizedCreateTable () =
            let mutable map = Map.empty
            let table = "data"

            fun username ->
                let set =
                    map
                    |> Map.tryFind username
                    |> Option.defaultValue Set.empty

                if set |> Set.contains table |> not then
                    map <- map |> Map.add username (set |> Set.add table)
                    printfn $"creating table {table} username={username}"
                    createTable username

        let memoizedCreateTable = getMemoizedCreateTable ()

        let insert username key value =
            getConnectionString username
            |> Sqlite.connect
            |> Sqlite.command
                " INSERT into data (key, value)
                  values (@Key, @Value)
                  ON CONFLICT(key)
                  DO UPDATE SET value=@Value; "
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

        let query username key =
            getConnectionString username
            |> Sqlite.connect
            |> Sqlite.query " SELECT * FROM data where key=@Key "
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

        let queryTableKeys username key =
            getConnectionString username
            |> Sqlite.connect
            |> Sqlite.query $""" SELECT key FROM data where key like "%%/{key}/%%" """
            |> Sqlite.execute (fun read -> {| Key = read.string "key" |})
            |> function
                | Ok result ->
                    //                    printfn $"result %A{result}"
                    result |> List.map (fun x -> x.Key)
                | Error err ->
                    printfn $"error %A{err}"
                    []

        let tryTestKey table key =
            let result = Regex.Match (key, $"^.*?/{table}/([a-fA-F0-9\\-]{{36}})")
            if result.Groups.Count = 2 then Some result.Groups.[1].Value else None

        let fetchTableKeys username table =
            queryTableKeys username table
            |> Seq.choose (tryTestKey table)
            |> Seq.distinct
            |> Seq.toArray



        let update (msg: Sync.Request) (hubContext: FableHub<Sync.Request, Sync.Response> option) =
            //            printfn $"Model.update() msg={msg}"

            match msg with
            | Sync.Request.Connect username ->
                memoizedCreateTable username
                printfn $"@@@ Sync.Request.Connect username={username}"
                Sync.Response.ConnectResult
            | Sync.Request.Set (username, key, value) ->
                memoizedCreateTable username
                let result = insert username key value
                //                printfn $"set {key} {value}"

                match hubContext with
                | Some _hub when result ->
                    printfn $"Sync.Request.Set. username={username} key={key}. result=true hub.IsSome. broadcasting."

                //                    (hub.Clients.All.Send (
//                        Sync.Response.GetResult (
//                            key,
//                            match value with
//                            | String.ValidString _ -> Some value
//                            | _ -> None
//                        )
//                    ))
//                        .Start ()
                | _ -> ()

                Sync.Response.SetResult result
            | Sync.Request.Get (username, key) ->
                memoizedCreateTable username
                let result = query username key
                let value = result |> Option.bind (fun x -> x.Value)
                //                printfn $"get username={username} key={key} value={value}"
                Sync.Response.GetResult (key, value)
            | Sync.Request.Filter (username, collection) ->
                memoizedCreateTable username

                let result = fetchTableKeys username collection

                printfn $"Sync.Request.Filter username={username} collection={collection} result={result.Length}"

                Sync.Response.FilterResult result

        //        let update2 msg hubContext =
//            asyncSeq {
//                update msg hubContext
//            }
//            |> AsyncSeq.toAsyncEnum

        let invoke (msg: Sync.Request) _ = task { return update msg None }

        let send (msg: Sync.Request) (hubContext: FableHub<Sync.Request, Sync.Response>) =
            hubContext.Clients.Caller.Send (update msg (Some hubContext))

        [<RequireQualifiedAccess>]
        module Stream =

            let sendToClient (msg: Sync.Request) (hubContext: FableHub<Sync.Request, Sync.Response>) =
                asyncSeq {
                    try
                        update msg (Some hubContext)
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
