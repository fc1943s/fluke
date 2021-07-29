namespace FsStore.Bindings

open System
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
        | KeysFromServer of
            keys: 'TKey [] *
            timestamp: int64 *
            trigger: ((int64 * 'TKey []) [] -> JS.Promise<IDisposable>)
        | Data of data: 'TValue * timestamp: int64 * trigger: (int64 * 'TValue -> JS.Promise<IDisposable>)
        | Subscribe of fn: (unit -> JS.Promise<IDisposable>)
        | Set of fn: (unit -> JS.Promise<IDisposable>)

    let inline macroQueue fn =
        //        JS.setTimeout fn 0 |> ignore
        fn ()

    let batch<'TKey, 'TValue> : (BatchType<'TKey, 'TValue> -> unit) =
        let internalBatch =
            fun (itemsArray: BatchType<'TKey, 'TValue> []) ->
                promise {
                    match itemsArray
                          |> Array.choose
                              (fun batchType ->
                                  match batchType with
                                  | BatchType.Set fn -> Some fn
                                  | _ -> None) with
                    | [||] -> ()
                    | setData ->
                        do!
                            setData
                            |> Array.map (fun fn -> fn ())
                            |> Promise.Parallel
                            |> Promise.ignore
                    //                        macroQueue (fun () -> setData |> Array.iter (fun fn -> fn ()))

                    match itemsArray
                          |> Array.choose
                              (fun batchType ->
                                  match batchType with
                                  | BatchType.Subscribe fn -> Some fn
                                  | _ -> None) with
                    | [||] -> ()
                    | subscribeData ->
                        do!
                            subscribeData
                            |> Array.map (fun fn -> fn ())
                            |> Promise.Parallel
                            |> Promise.ignore
                    //                        macroQueue (fun () -> subscribeData |> Array.iter (fun fn -> fn ()))

                    match itemsArray
                          |> Array.choose
                              (fun batchType ->
                                  match batchType with
                                  | BatchType.Data (data, timestamp, trigger) -> Some (data, timestamp, trigger)
                                  | _ -> None) with
                    | [||] -> ()
                    | providerData ->
                        let trigger =
                            providerData
                            |> Array.last
                            |> fun (_, _, trigger) -> trigger


                        do!
                            providerData
                            |> Array.map (fun (data, timestamp, _) -> trigger (timestamp, data))
                            |> Promise.Parallel
                            |> Promise.ignore
                    //                        macroQueue
//                            (fun () ->
//                                providerData
//                                |> Array.iter (fun (data, timestamp, _) -> trigger (timestamp, data)))

                    match itemsArray
                          |> Array.choose
                              (fun batchType ->
                                  match batchType with
                                  | BatchType.KeysFromServer (item, timestamp, trigger) ->
                                      Some (item, timestamp, trigger)
                                  | _ -> None) with
                    | [||] -> ()
                    | keysFromServer ->
                        let trigger =
                            keysFromServer
                            |> Array.last
                            |> fun (_, _, trigger) -> trigger

                        let items =
                            keysFromServer
                            |> Array.map (fun (item, timestamp, _) -> timestamp, item)

                        let! _disposable = trigger items
                        ()
                //                        macroQueue (fun () -> trigger items)
                }
                |> Promise.start

        batcher internalBatch {| interval = 3000 |}
//        fun item -> internalBatch [| item |]
