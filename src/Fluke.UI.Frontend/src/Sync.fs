namespace Fluke.UI.Frontend

open Fable.Remoting.Client
open Fluke.Shared

module Sync =
    open Sync

    let createApi baseUrl =
        Remoting.createApi ()
        |> Remoting.withBinarySerialization
        |> Remoting.withBaseUrl baseUrl
        |> Remoting.buildProxy<Api>


    let internalHandleRequest fn =
        async {
            let! result = fn |> Async.Catch

            return
                match result with
                | Choice1Of2 output -> Some output
                | Choice2Of2 ex ->
                    match ex with
                    | :? ProxyRequestException as ex ->
                        let _response : HttpResponse = ex.Response
                        let responseText : string = ex.ResponseText
                        let statusCode : int = ex.StatusCode

                        printfn $"Proxy exception: {ex.Message}. responseText={responseText}; statusCode={statusCode}"
                    | ex -> printfn $"API exception: {ex}"

                    None
        }

    let handleRequest (fn: Async<'T> option) =
        async {
            match fn with
            | None -> return None
            | Some request -> return! internalHandleRequest request
        }
