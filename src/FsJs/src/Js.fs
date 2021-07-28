namespace FsJs

open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open Fable.Extras
open FsCore


[<AutoOpen>]
module Operators =
    [<Emit("Object.assign({}, $0, $1)")>]
    let inline (++) _o1 _o2 : obj = jsNative

    [<Emit("Object.assign($0, $1)")>]
    let inline (<+) _o1 _o2 : unit = jsNative

    let inline (<.>) obj key = emitJsExpr (obj, key) "$0[$1]"

[<AutoOpen>]
module JsMagic =
    type ClipboardRead =
        {| getType: string -> JS.Promise<Blob>
           types: string [] |} []

    [<Emit "$0.read()">]
    let inline private clipboardRead _clipboard = jsNative

    type Clipboard with
        member this.read () : JS.Promise<ClipboardRead> = clipboardRead this

module Promise =
    let inline ignore (fn: JS.Promise<_>) = Promise.map ignore fn

module Json =
    let inline encodeFormatted<'T> obj =
        Thoth.Json.Encode.Auto.toString<'T> (4, obj, skipNullField = false)

    let inline encode<'T> obj =
        Thoth.Json.Encode.Auto.toString<'T> (0, obj, skipNullField = false)

    let inline decode<'T> data =
        Thoth.Json.Decode.Auto.unsafeFromString<'T> data

module JS =
    [<Emit "new Event($0, $1)">]
    let inline createEvent _eventType _props = jsNative

    [<Emit "process.env.JEST_WORKER_ID">]
    let jestWorkerId: bool = jsNative




    [<Emit "(w => $0 instanceof w[$1])(window)">]
    let inline instanceof (_obj: obj, _typeName: string) : bool = jsNative


    [<Emit "(() => { var audio = new Audio($1); audio.volume = $0 || 1; return audio; })().play();">]
    let playAudioVolume (_volume: float) (_file: string) : unit = jsNative

    let playAudio = playAudioVolume 0.5


    [<Emit "$0($1,$2)">]
    let inline jsCall _fn _a _b = jsNative

    let inline invoke fn p =
        emitJsExpr (fn, p) "((...p) => $0(...p))($1)"

    let inline newObj<'T> fn = jsOptions<'T> fn
    let inline cloneDeep<'T> (_: 'T) : 'T = importDefault "lodash.clonedeep"
    let debounce<'T, 'U> (_: 'T -> 'U) (_: int) : 'T -> 'U = importDefault "lodash.debounce"

    let inline blobToUint8Array (_blob: Blob) : JS.Promise<JSe.Uint8Array> =
        import "blobToUint8Array" "binconv/dist/src/blobToUint8Array"

    let inline uint8ArrayToBlob (_arr: JSe.Uint8Array) (_type: string) : Blob =
        //        let x = JSe.Uint8Array(_arr)
//        new Blob([new Uint8Array(BYTEARRAY)], { type: 'video/mp4' })
        import "uint8ArrayToBlob" "binconv/dist/src/uint8ArrayToBlob"



    //    let uint8ArrayToBlob (_arr: int []) (_type: string) : Blob =
//        emitJsExpr (_arr, _type) "new Blob($0, {type: $1})"

    //    let base64ToUint8Array (_str: string) : int [] =
//        import "base64ToUint8Array" "binconv/dist/src/base64ToUint8Array"
//
//    let uint8ArrayToBase64 (_arr: int []) : string =
//        import "uint8ArrayToBase64" "binconv/dist/src/uint8ArrayToBase64"

    let inline byteArrayToHexString byteArray =
        byteArray
        |> Array.map (fun b -> ("0" + (b &&& 0xFFuy)?toString 16).Substring -2)
        |> String.concat ""


    [<Emit "parseInt($0, $1)">]
    let inline parseInt (_a: string) (_n: int) : int = jsNative

    let inline hexStringToByteArray (text: string) =
        let rec loop acc =
            function
            | a :: b :: tail -> loop ((parseInt $"{a}{b}" 16 |> byte) :: acc) tail
            | [ _ ] -> failwith "invalid string length"
            | [] -> acc

        text
        |> Seq.map string
        |> Seq.toList
        |> loop []
        |> List.rev
        |> List.toArray

    let inline chunkString (_str: string) (_options: {| size: int; unicodeAware: bool |}) : string [] =
        importDefault "@shelf/fast-chunk-string"

    //    let cloneObj<'T> (obj: 'T) (fn: 'T -> 'T) = fn (cloneDeep obj)
    let inline toJsArray a = a |> Array.toList |> List.toArray

    let inline sleep (ms: int) = Async.Sleep ms
    //        Promise.sleep ms |> Async.AwaitPromise
//        Async.FromContinuations (fun (res, _, _) -> JS.setTimeout res ms |> ignore)

    let inline ofNonEmptyObj obj =
        obj
        |> Option.ofObjUnbox
        |> Option.bind
            (fun obj ->
                if (jsTypeof obj = "object"
                    && (JS.Constructors.Object.keys obj).Count = 0) then
                    None
                else
                    Some obj)
