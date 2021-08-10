namespace FsJs

open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open Fable.Extras
open FsCore


[<AutoOpen>]
module Operators =
    let inline (++) a b : obj =
        emitJsExpr (a, b) "Object.assign({}, $0, $1)"

    let inline (<+) a b : unit =
        emitJsExpr (a, b) "Object.assign($0, $1)"

    let inline (<.>) obj key = emitJsExpr (obj, key) "$0[$1]"


module JS =
    let inline isInstanceOf (typeName: string) (obj: obj) : bool =
        emitJsExpr (obj, typeName) "(w => $0 instanceof w[$1])(window)"

    let inline emit x = emitJsExpr x "$0"

    let inline jsCall fn a b = emitJsExpr (fn, a, b) "$0($1, $2)"

    let inline invoke fn p =
        emitJsExpr (fn, p) "((...p) => $0(...p))($1)"

    let inline parseInt (a: string) (n: int) : int = emitJsExpr (a, n) "parseInt($0, $1)"

    let inline createEvent eventType props =
        emitJsExpr (eventType, props) "new Event($0, $1)"

    let jestWorkerId: bool = emitJsExpr () "process.env.JEST_WORKER_ID"

    let playAudioVolume (volume: float) (file: string) : unit =
        emitJsExpr
            (volume, file)
            "(() => {
                var audio = new Audio($1);
                audio.volume = $0 || 1;
                return audio;
            })().play();"

    let playAudio = playAudioVolume 0.5

    let inline newObj<'T> fn = jsOptions<'T> fn
    let inline cloneDeep<'T> (_: 'T) : 'T = importDefault "lodash.clonedeep"
    let inline debounce<'T, 'U> (_: 'T -> 'U) (_: int) : 'T -> 'U = importDefault "lodash.debounce"

    let inline blobToUint8Array (_blob: Blob) : JS.Promise<JSe.Uint8Array> =
        import "blobToUint8Array" "binconv/dist/src/blobToUint8Array"

    let inline uint8ArrayToBlob (_arr: JSe.Uint8Array) (_type: string) : Blob =
        import "uint8ArrayToBlob" "binconv/dist/src/uint8ArrayToBlob"

    let inline chunkString (_str: string) (_options: {| size: int; unicodeAware: bool |}) : string [] =
        importDefault "@shelf/fast-chunk-string"

    let inline byteArrayToHexString byteArray =
        byteArray
        |> Array.map (fun b -> ("0" + (b &&& 0xFFuy)?toString 16).Substring -2)
        |> String.concat ""

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

    let inline objectKeys obj =
        JS.Constructors.Object.keys (obj |> Option.defaultValue (createObj [] |> unbox))
        |> Seq.toArray


[<AutoOpen>]
module JsMagic =
    type ClipboardRead =
        {| getType: string -> JS.Promise<Blob>
           types: string [] |} []

    let inline private clipboardRead clipboard = emitJsExpr clipboard "$0.read()"

    type Clipboard with
        member inline this.read () : JS.Promise<ClipboardRead> = clipboardRead this


module Promise =
    let inline ignore (fn: JS.Promise<_>) = Promise.map ignore fn


module Json =
    let inline encodeFormatted<'T> obj =
        Thoth.Json.Encode.Auto.toString<'T> (4, obj, skipNullField = false)

    let inline encode<'T> obj =
        Thoth.Json.Encode.Auto.toString<'T> (0, obj, skipNullField = false)

    let inline decode<'T> data =
        Thoth.Json.Decode.Auto.unsafeFromString<'T> data
