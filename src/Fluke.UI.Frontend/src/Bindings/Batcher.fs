namespace Fluke.UI.Frontend.Bindings

open Fable.Core.JsInterop


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
