namespace Fluke.UI.Frontend

open Fable.Remoting.Client
open Fluke.Shared

module Sync =
    open Sync

    let api =
        Remoting.createApi ()
        |> Remoting.withBinarySerialization
        |> Remoting.buildProxy<Api>

    let handleRequest fn =
        async {
            let! result = fn |> Async.Catch

            return
                match result with
                | Choice1Of2 output -> Some output
                | Choice2Of2 ex ->
                    match ex with
                    | :? ProxyRequestException as ex ->
                        let _response: HttpResponse = ex.Response
                        let responseText: string = ex.ResponseText
                        let statusCode: int = ex.StatusCode

                        printfn "Proxy exception: %A. responseText=%A; statusCode=%A" ex.Message responseText statusCode
                    | ex -> printfn "API exception: %A" ex

                    None
        }
