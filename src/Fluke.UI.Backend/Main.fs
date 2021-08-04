namespace Fluke.UI.Backend

open System.Collections.Concurrent
open System.IO
open System.Threading.Tasks
open FSharp.Control
open FsCore
open Fable.SignalR
open Fluke.Shared
open FsStore.Shared
open Microsoft.Extensions.Logging
open FSharp.Control.Tasks.V2
open Saturn
open System
open System.Threading
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting


module Main =
    module Model =
        let getPath username key =
            let path = Path.Combine (".", "data", username, $"{key}")
            //            printfn $"getPath. username={username} key={key} / path={path}"
            path

        let createParentDirectory path =
            Directory.CreateDirectory (Directory.GetParent(path).FullName)
            |> ignore

        let writeFile username key value =
            task {
                try
                    let path = getPath username key

                    match Guid.TryParse (Path.GetFileName path) with
                    | true, _ when value = null -> Directory.Delete (path, true)
                    | _ ->
                        createParentDirectory path
                        do! File.WriteAllTextAsync (path, value)

                    return true
                with
                | ex ->
                    eprintfn $"writeFile error ex={ex.Message}"
                    return false
            }

        let readFile username key =
            task {
                try
                    let path = getPath username key
                    let! result = File.ReadAllTextAsync path
                    return result |> Option.ofObjUnbox
                with
                | _ex ->
                    //                    eprintfn $"readFile error ex={ex.Message}"
                    return None
            }

        let watchlist = ConcurrentDictionary<string * string * string, string [] option> ()


        let fetchTableKeys username storeRoot collection =
            let path = getPath username $"{storeRoot}/{collection}"
            Directory.CreateDirectory path |> ignore

            Directory.EnumerateDirectories path
            |> Seq.map Path.GetFileName
            |> Seq.toArray

        let update (msg: Sync.Request) (_hubContext: FableHub<Sync.Request, Sync.Response> option) =
            task {
                //            printfn $"Model.update() msg={msg}"

                match msg with
                | Sync.Request.Connect username ->
                    printfn $"@@@ Sync.Request.Connect username={username}"
                    return Sync.Response.ConnectResult
                | Sync.Request.Set (username, key, value) ->
                    let! result = writeFile username key value
                    //                printfn $"set {key} {value}"
//                    match hubContext with
//                    | Some _hub when result ->
//                        printfn
//                            $"Sync.Request.Set. username={username} key={key}. result=true hub.IsSome. broadcasting."

                    //                        do!
//                            hub.Clients.All.Send (
//                                Sync.Response.GetResult (
//                                    key,
//                                    match value with
//                                    | String.ValidString _ -> Some value
//                                    | _ -> None
//                                )
//                            )
//                    | _ -> ()

                    return Sync.Response.SetResult result
                | Sync.Request.Get (username, key) ->
                    let! value = readFile username key
                    //                printfn $"get username={username} key={key} value={value}"
                    return Sync.Response.GetResult value
                | Sync.Request.Filter (username, storeRoot, collection) ->
                    let result = fetchTableKeys username storeRoot collection
                    watchlist.[(username, storeRoot, collection)] <- Some result
                    printfn $"Sync.Request.Filter username={username} collection={collection} result={result.Length}"
                    return Sync.Response.FilterResult result

            //        let update2 msg hubContext =
//            asyncSeq {
//                update msg hubContext
//            }
//            |> AsyncSeq.toAsyncEnum
            }

        let invoke (msg: Sync.Request) _ = update msg None

        let send (msg: Sync.Request) (hubContext: FableHub<Sync.Request, Sync.Response>) =
            task {
                let! response = update msg (Some hubContext)
                do! hubContext.Clients.Caller.Send response
            }

        module AsyncSeq =
            let init x = AsyncSeq.initAsync 1L (fun _ -> x)

        [<RequireQualifiedAccess>]
        module Stream =

            let sendToClient (msg: Sync.Request) (hubContext: FableHub<Sync.Request, Sync.Response>) =
                update msg (Some hubContext)
                |> Async.AwaitTask
                |> AsyncSeq.init
                |> AsyncSeq.toAsyncEnum

            type Ticker<'T, 'U when 'T: not struct and 'U: not struct> private (hub: FableHubCaller<'T, 'U>, fn) =
                let cts = new CancellationTokenSource ()

                let ticking =
                    AsyncSeq.intervalMs 500
                    |> AsyncSeq.iterAsync (fun _ -> fn hub |> Async.AwaitTask)

                interface IHostedService with
                    member _.StartAsync ct =
                        async { do Async.Start (ticking, cts.Token) }
                        |> fun a -> upcast Async.StartAsTask (a, cancellationToken = ct)

                    member _.StopAsync ct =
                        async { do cts.Cancel () }
                        |> fun a -> upcast Async.StartAsTask (a, cancellationToken = ct)

                static member Create (services: IServiceCollection, fn) =
                    services.AddHostedService<Ticker<'T, 'U>>
                        (fun s -> Ticker (s.GetRequiredService<FableHubCaller<'T, 'U>> (), fn))

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
                            (fun _applicationBuilder ->
                                printfn "saturn.with_after_routing()"
                                _applicationBuilder)

                        with_before_routing
                            (fun _applicationBuilder ->
                                printfn "saturn.with_before_routing()"
                                _applicationBuilder)

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

                service_config
                    (fun serviceCollection ->
                        Model.Stream.Ticker.Create (
                            serviceCollection,
                            fun (hub: FableHubCaller<Sync.Request, Sync.Response>) ->
                                task {
                                    do!
                                        Model.watchlist
                                        |> Seq.choose
                                            (fun (KeyValue (collectionPath, lastValue)) ->
                                                let username, storeRoot, collection = collectionPath
                                                let result = Model.fetchTableKeys username storeRoot collection

                                                match lastValue, result with
                                                | None, _ -> None
                                                | Some lastValue, result when lastValue = result -> None
                                                | Some _, result ->
                                                    Model.watchlist.[collectionPath] <- Some result
                                                    Some (collectionPath, result)
                                                | _ -> None)
                                        |> Seq.toArray
                                        |> Seq.map (Sync.Response.FilterStream >> hub.Clients.All.Send)
                                        |> Task.WhenAll
                                }
                        ))

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
