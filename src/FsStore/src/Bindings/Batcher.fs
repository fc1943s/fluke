namespace FsStore.Bindings

open System
open Fable.Core.JsInterop
open Fable.Core
open FsJs


module Batcher =
    let interval = 500

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
        JS.setTimeout (fn >> Promise.start) 0 |> ignore
    //        fn () |> Promise.start

    let inline macroQueue2 fn = JS.setTimeout fn 0 |> ignore

    let batch<'TKey, 'TValue> : (BatchType<'TKey, 'TValue> -> unit) =
        let internalBatch =
            fun (itemsArray: BatchType<'TKey, 'TValue> []) ->
                promise {
                    let items =
                        itemsArray
                        |> Array.map
                            (function
                            | BatchType.Set fn -> Some fn, None, None, None
                            | BatchType.Subscribe fn -> None, Some fn, None, None
                            | BatchType.Data (data, timestamp, trigger) ->
                                None, None, Some (data, timestamp, trigger), None
                            | BatchType.KeysFromServer (item, timestamp, trigger) ->
                                None, None, None, Some (item, timestamp, trigger))

                    let! _disposables =
                        items
                        |> Array.choose (fun (setFn, _, _, _) -> setFn)
                        |> Array.map (fun setFn -> setFn ())
                        |> Promise.all

                    let! _disposables =
                        items
                        |> Array.choose (fun (_, subscribeFn, _, _) -> subscribeFn)
                        |> Array.map (fun subscribeFn -> subscribeFn ())
                        |> Promise.all

                    let! _disposables =
                        let providerData =
                            items
                            |> Array.choose (fun (_, _, data, _) -> data)

                        match providerData with
                        | [||] -> [||]
                        | _ ->
                            let trigger =
                                providerData
                                |> Array.last
                                |> fun (_, _, trigger) -> trigger

                            let providerData =
                                providerData
                                |> Array.map (fun (data, timestamp, _) -> fun () -> trigger (timestamp, data))

                            providerData |> Array.map (fun fn -> fn ())
                        |> Promise.all

                    let! _disposables =
                        let keysFromServer =
                            items
                            |> Array.choose (fun (_, _, _, keys) -> keys)

                        match keysFromServer with
                        | [||] -> []
                        | _ ->
                            let trigger =
                                keysFromServer
                                |> Array.last
                                |> fun (_, _, trigger) -> trigger

                            let items =
                                keysFromServer
                                |> Array.map (fun (item, timestamp, _) -> timestamp, item)

                            [
                                trigger items
                            ]
                        |> Promise.all

                    ()
                }
                |> Promise.start

        fun item ->
//            match item with/--
            //            | BatchType.Set _
//            | BatchType.Subscribe _ -> /--
                //                macroQueue2 (fun () ->
//                internalBatch [| item |] /--
            //                )
//            | _ ->/--
                batcher internalBatch {| interval = interval |} item
