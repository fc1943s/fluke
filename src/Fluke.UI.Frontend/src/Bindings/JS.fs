namespace Fluke.UI.Frontend.Bindings

open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open Fable.Extras
open Fluke.Shared


[<AutoOpen>]
module Operators =
    [<Emit("Object.assign({}, $0, $1)")>]
    let (++) _o1 _o2 : obj = jsNative

    [<Emit("Object.assign($0, $1)")>]
    let (<+) _o1 _o2 : unit = jsNative

[<AutoOpen>]
module JSMagic =
    type ClipboardRead =
        {| getType: string -> JS.Promise<Blob>
           types: string [] |} []

    [<Emit "$0.read()">]
    let inline private clipboardRead _clipboard = jsNative

    type Clipboard with
        member this.read () : JS.Promise<ClipboardRead> = clipboardRead this

module Promise =
    let ignore (fn: JS.Promise<_>) = Promise.map ignore fn

module Json =
    let inline encodeFormatted<'T> obj =
        Thoth.Json.Encode.Auto.toString<'T> (4, obj, skipNullField = false)

    let inline encode<'T> obj =
        Thoth.Json.Encode.Auto.toString<'T> (0, obj, skipNullField = false)

    let inline decode<'T> data =
        Thoth.Json.Decode.Auto.unsafeFromString<'T> data

module JS =
    [<Emit "process.env.JEST_WORKER_ID">]
    let jestWorkerId: bool = jsNative

    let window fn =
        if jsTypeof Browser.Dom.window <> "undefined" then
            Some (fn Browser.Dom.window)
        else
            printfn "No window found"
            None

    type DeviceInfo =
        {
            Brands: (string * string) []
            IsMobile: bool
            IsElectron: bool
            IsExtension: bool
            GitHubPages: bool
            IsTesting: bool
        }
        static member inline Default =
            {
                Brands = [||]
                IsMobile = false
                IsElectron = false
                IsExtension = false
                GitHubPages = false
                IsTesting = false
            }

    let deviceInfo =
        match window id with
        | None -> DeviceInfo.Default
        | Some window ->
            let userAgentData =
                window?navigator
                |> Option.ofObjUnbox
                |> Option.bind
                    (fun navigator ->
                        navigator?userAgentData
                        |> Option.ofObjUnbox<{| mobile: bool
                                                brands: {| brand: string; version: string |} [] |}>)

            let brands =
                userAgentData
                |> Option.map
                    (fun userAgentData ->
                        userAgentData.brands
                        |> Array.map (fun brand -> brand.brand, brand.version))
                |> Option.defaultValue [||]

            let userAgentDataMobile =
                userAgentData
                |> Option.map (fun userAgentData -> userAgentData.mobile)
                |> Option.defaultValue false

            let deviceInfo =
                {
                    Brands = brands
                    IsMobile =
                        if userAgentDataMobile then
                            true
                        elif brands.Length > 0 then
                            false
                        else
                            let userAgent = if window?navigator = None then "" else window?navigator?userAgent

                            JSe
                                .RegExp(
                                    "Android|BlackBerry|iPhone|iPad|iPod|Opera Mini|IEMobile|WPDesktop",
                                    JSe.RegExpFlag().i
                                )
                                .Test userAgent
                    IsElectron = jsTypeof window?api = "object"
                    IsExtension = window.location.protocol = "chrome-extension:"
                    GitHubPages = window.location.host.EndsWith "github.io"
                    IsTesting = jestWorkerId || window?Cypress <> null
                }

            printfn $"deviceInfo={JS.JSON.stringify deviceInfo}"
            deviceInfo

    let private isDebugStatic =
        not deviceInfo.GitHubPages
        && not deviceInfo.IsExtension
        && not deviceInfo.IsElectron
        && not deviceInfo.IsMobile

    match window id with
    | None -> ()
    | Some window -> window?Debug <- false

    let isDebug () =
        let debug =
            match window id with
            | None -> false
            | Some window -> window?Debug

        debug <> false && (debug || isDebugStatic)

    let inline log fn =
        if isDebug () then
            let result = fn ()
            if result <> null then printfn $"[log] {result}"

    let inline consoleLog x = Browser.Dom.console.log x

    [<Emit "(w => $0 instanceof w[$1])(window)">]
    let instanceof (_obj: obj, _typeName: string) : bool = jsNative


    [<Emit "(() => { var audio = new Audio($1); audio.volume = $0 || 1; return audio; })().play();">]
    let playAudioVolume (_volume: float) (_file: string) : unit = jsNative

    let playAudio = playAudioVolume 0.5


    [<Emit "$0($1,$2)">]
    let inline jsCall _fn _a _b = jsNative

    let inline invoke fn p =
        emitJsExpr (fn, p) "((...p) => $0(...p))($1)"

    let newObj<'T> fn = jsOptions<'T> fn
    let cloneDeep<'T> (_: 'T) : 'T = importDefault "lodash.clonedeep"
    let debounce<'T, 'U> (_: 'T -> 'U) (_: int) : 'T -> 'U = importDefault "lodash.debounce"

    let blobToUint8Array (_blob: Blob) : JS.Promise<JSe.Uint8Array> =
        import "blobToUint8Array" "binconv/dist/src/blobToUint8Array"

    let uint8ArrayToBlob (_arr: JSe.Uint8Array) (_type: string) : Blob =
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

    let byteArrayToHexString byteArray =
        byteArray
        |> Array.map (fun b -> ("0" + (b &&& 0xFFuy)?toString 16).Substring -2)
        |> String.concat ""


    [<Emit "parseInt($0, $1)">]
    let parseInt (_a: string) (_n: int) : int = jsNative

    let hexStringToByteArray (text: string) =
        let rec loop acc =
            function
            | a :: b :: tail -> loop (parseInt $"{a}{b}" 16 :: acc) tail
            | [ _ ] -> failwith "invalid string length"
            | [] -> acc

        text
        |> Seq.map string
        |> Seq.toList
        |> loop []
        |> List.rev
        |> List.toArray

    let chunkString (_str: string) (_options: {| size: int; unicodeAware: bool |}) : string [] =
        importDefault "@shelf/fast-chunk-string"

    //    let cloneObj<'T> (obj: 'T) (fn: 'T -> 'T) = fn (cloneDeep obj)
    let toJsArray a = a |> Array.toList |> List.toArray

    let inline sleep (ms: int) = Async.Sleep ms
    //        Promise.sleep ms |> Async.AwaitPromise
//        Async.FromContinuations (fun (res, _, _) -> JS.setTimeout res ms |> ignore)

    let exited () =
        if not deviceInfo.IsTesting then
            false
        else
            Browser.Dom.window?exit = true

    let rec waitFor fn =
        async {
            if exited () then
                return (unbox null)
            else
                let ok = fn ()

                if ok then
                    return ()
                else
                    printfn "waitFor: false. waiting..."

                    do! sleep 100
                    return! waitFor fn
        }

    let rec waitForObject fn =
        async {
            if exited () then
                return (unbox null)
            else
                let! obj = fn ()

                match box obj with
                | null ->
                    printfn "waitForObject: null. waiting..."

                    do! sleep 100
                    return! waitForObject fn
                | _ -> return obj
        }

    let rec waitForSome fn =
        async {
            if exited () then
                return (unbox null)
            else
                let! obj = fn ()

                match obj with
                | Some obj -> return obj
                | None ->
                    if deviceInfo.IsTesting then
                        do! sleep 0
                    else
                        consoleLog ("waitForSome: none. waiting...", fn.ToString ())
                        do! sleep 100

                    return! waitForSome fn
        }

    let ofNonEmptyObj obj =
        obj
        |> Option.ofObjUnbox
        |> Option.bind
            (fun obj ->
                if (jsTypeof obj = "object"
                    && (JS.Constructors.Object.keys obj).Count = 0) then
                    None
                else
                    Some obj)

    let download content fileName contentType =
        let a = Browser.Dom.document.createElement "a"

        let file =
            Browser.Blob.Blob.Create (
                [|
                    content
                |],
                { new BlobPropertyBag with
                    member _.``type`` = contentType
                    member _.endings = BlobEndings.Transparent

                    member _.``type``
                        with set value = ()

                    member _.endings
                        with set value = ()
                }
            )

        a?href <- Browser.Url.URL.createObjectURL file
        a?download <- fileName
        a.click ()
        a.remove ()
