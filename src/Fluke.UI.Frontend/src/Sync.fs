namespace Fluke.UI.Frontend

open Fable.Remoting.Client
open Fluke.Shared
open Feliz.Recoil
open Fable.Core
open Fable.Core.JsInterop

module Sync =
    open Sync

    let createApi baseUrl =
        Remoting.createApi ()
        |> Remoting.withBinarySerialization
        |> Remoting.withBaseUrl baseUrl
        |> Remoting.buildProxy<Api>

    module Promise =
        [<Emit "Promise.race($0)">]
        let race<'T, 'U> (_arr: JS.Promise<'T> []) : JS.Promise<'U> = jsNative

        let watch<'T> (promise: JS.Promise<'T>) : JS.Promise<'T> =
            if promise?isResolved then
                promise
            else
                let mutable isPending = true
                let mutable isRejected = false
                let mutable isFulfilled = false

                let result =
                    Promise.either
                        (fun v ->
                            isFulfilled <- true
                            isPending <- false
                            v)
                        (fun e ->
                            isRejected <- true
                            isPending <- false
                            failwith e)
                        (unbox promise)

                result?isFulfilled <- fun () -> isFulfilled
                result?isPending <- fun () -> isPending
                result?isRejected <- fun () -> isRejected
                result

        let withTimeout (ms: int) (fn: JS.Promise<Choice<'T, exn>>) =
            promise {
                let timeout =
                    Promise.create
                        (fun _ reject ->
                            JS.setTimeout (fun () -> reject (exn "Timed out in X seconds")) ms
                            |> ignore)

                try
                    let racePromise : JS.Promise<'T> = race [| watch fn; watch timeout |]

                    Browser.Dom.window?racePromise <- racePromise

                    let! raceResult = racePromise

                    Browser.Dom.window?raceResult <- raceResult

                    return
                        match box raceResult with
                        | :? Choice<'T, exn> as result -> box result :?> Choice<'T, exn>
                        | _ -> Choice1Of2 raceResult
                with ex -> return Choice2Of2 ex
            }


    let internalHandleRequest<'T> (fn: Async<'T>) =
        promise {
            let! result =
                fn
                |> Async.Catch
                |> Async.StartAsPromise
                |> Promise.withTimeout 4000

            return
                match result with
                | Choice1Of2 output ->
                    printfn $"internalHandleRequest success. output={JS.JSON.stringify output}"
                    Ok output
                | Choice2Of2 ex ->
                    match ex with
                    | :? ProxyRequestException as ex ->
                        let _response : HttpResponse = ex.Response
                        let responseText : string = ex.ResponseText
                        let statusCode : int = ex.StatusCode

                        printfn $"Proxy exception: {ex.Message}. responseText={responseText}; statusCode={statusCode}"
                    | ex -> printfn $"API exception: {ex}"

                    Error ex
        }

    let handleRequest<'T> (fn: Async<'T> option) =
        promise {
            match fn with
            | None -> return Error (exn "No fn found")
            | Some request -> return! internalHandleRequest<'T> request
        }
