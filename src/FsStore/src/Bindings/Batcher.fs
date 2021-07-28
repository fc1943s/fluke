namespace FsStore.Bindings

open Fable.Core.JsInterop
open Fable.Core
open FsJs


module Batcher =
    type Cb<'TFnResult> = unit -> 'TFnResult

    let private internalBatcher<'TKey, 'TFnResult>
        (_fn: 'TKey [] -> Cb<'TFnResult> -> unit)
        (_settings: {| interval: int |})
        : 'TKey -> Cb<'TFnResult> -> unit =
        importDefault "batcher-js"

    let batcher<'TKey, 'TFnResult> fn settings =
        let newFn = internalBatcher<'TKey, 'TFnResult> (fun x _lock -> fn x) settings
        let lock = fun () -> ()

        fun (x: 'TKey) ->
            JS.jsCall newFn x lock
            ()

    [<RequireQualifiedAccess>]
    type BatchType<'TKey, 'TValue> =
        | KeysFromServer of keys: 'TKey [] * timestamp: int64 * trigger: ((int64 * 'TKey []) [] -> unit)
        | Data of data: 'TValue * timestamp: int64 * trigger: (int64 * 'TValue -> JS.Promise<unit>)
        | Subscribe of fn: (unit -> unit)
        | Set of fn: (unit -> unit)

    let batch<'TKey, 'TValue> =
        batcher
            (fun (itemsArray: BatchType<'TKey, 'TValue> []) ->
                // Set
                let set =
                    itemsArray
                    |> Array.choose
                        (fun batchType ->
                            match batchType with
                            | BatchType.Set fn -> Some fn
                            | _ -> None)

                match set with
                | [||] -> ()
                | data -> data |> Array.iter (fun fn -> fn ())

                // Data
                let data =
                    itemsArray
                    |> Array.choose
                        (fun batchType ->
                            match batchType with
                            | BatchType.Data (data, timestamp, trigger) -> Some (data, timestamp, trigger)
                            | _ -> None)

                match data with
                | [||] -> ()
                | data ->
                    let trigger =
                        data
                        |> Array.last
                        |> fun (_, _, trigger) -> trigger

                    data
                    |> Array.map (fun (data, timestamp, _) -> trigger (timestamp, data))
                    |> Promise.Parallel
                    |> Promise.start

                // Subscribe
                let subscribe =
                    itemsArray
                    |> Array.choose
                        (fun batchType ->
                            match batchType with
                            | BatchType.Subscribe fn -> Some fn
                            | _ -> None)

                match subscribe with
                | [||] -> ()
                | data -> data |> Array.iter (fun fn -> fn ())

                // Keys From Server
                let keysFromServer =
                    itemsArray
                    |> Array.choose
                        (fun batchType ->
                            match batchType with
                            | BatchType.KeysFromServer (item, timestamp, trigger) -> Some (item, timestamp, trigger)
                            | _ -> None)

                match keysFromServer with
                | [||] -> ()
                | keysFromServer ->
                    let trigger =
                        keysFromServer
                        |> Seq.map (fun (_, _, trigger) -> trigger)
                        |> Seq.last

                    let items =
                        keysFromServer
                        |> Array.map (fun (item, timestamp, _) -> timestamp, item)

                    trigger items)

            {| interval = 250 |}
