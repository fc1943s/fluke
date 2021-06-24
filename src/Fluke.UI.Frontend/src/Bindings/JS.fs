namespace Fluke.UI.Frontend.Bindings

open Fable.Core
open Fable.Core.JsInterop
open Fluke.Shared
open Fable.Extras


[<AutoOpen>]
module Operators =
    [<Emit("Object.assign({}, $0, $1)")>]
    let (++) _o1 _o2 : obj = jsNative

    [<Emit("Object.assign($0, $1)")>]
    let (<+) _o1 _o2 : unit = jsNative

module Promise =
    let ignore (fn: JS.Promise<_>) = Promise.map ignore fn

module JS =
    [<Emit "process.env.JEST_WORKER_ID">]
    let private jestWorkerId : bool = jsNative

    let window fn =
        if jsTypeof Browser.Dom.window <> "undefined" then
            Some (fn Browser.Dom.window)
        else
            printfn "No window found"
            None

    type DeviceInfo =
        {
            UserAgent: string
            IsEdge: bool
            IsMobile: bool
            IsElectron: bool
            IsExtension: bool
            GitHubPages: bool
            IsTesting: bool
        }

    let deviceInfo =
        match window id with
        | None ->
            {
                UserAgent = "window==null"
                IsEdge = false
                IsMobile = false
                IsElectron = false
                IsExtension = false
                GitHubPages = false
                IsTesting = false
            }
        | Some window ->
            let userAgent = if window?navigator = None then "" else window?navigator?userAgent

            let isEdge = (JSe.RegExp @"Edg\/").Test userAgent
            let isElectron = (JSe.RegExp @"Electron\/").Test userAgent

            let isMobile =
                JSe
                    .RegExp("Android|BlackBerry|iPhone|iPad|iPod|Opera Mini|IEMobile|WPDesktop", JSe.RegExpFlag().i)
                    .Test userAgent

            let isExtension = window.location.protocol = "chrome-extension:"
            let gitHubPages = window.location.host.EndsWith "github.io"
            let isTesting = jestWorkerId || window?Cypress <> null

            let deviceInfo =
                {
                    UserAgent = userAgent
                    IsEdge = isEdge
                    IsMobile = isMobile
                    IsElectron = isElectron
                    IsExtension = isExtension
                    GitHubPages = gitHubPages
                    IsTesting = isTesting
                }

            printfn $"deviceInfo={JS.JSON.stringify deviceInfo}"
            deviceInfo

    let private isDebugStatic =
        not deviceInfo.GitHubPages
        && not deviceInfo.IsExtension
        && not deviceInfo.IsElectron
        && not deviceInfo.IsMobile

    let isDebug () =
        let debug =
            match window id with
            | None -> false
            | Some window -> window?Debug

        debug <> false && (debug || isDebugStatic)

    let inline log fn =
        if isDebug () then printfn $"[log] {fn ()}"

    let inline consoleLog x = Browser.Dom.console.log x

    [<Emit "(w => $0 instanceof w[$1])(window)">]
    let instanceof (_obj: obj, _typeName: string) : bool = jsNative


    [<Emit "(() => { var audio = new Audio($1); audio.volume = $0 || 1; return audio; })().play();">]
    let playAudioVolume (_volume: float) (_file: string) : unit = jsNative

    let playAudio = playAudioVolume 0.5


    [<Emit "$0($1,$2)">]
    let inline jsCall _fn _a _b = jsNative

    let newObj fn = jsOptions<_> fn
    let cloneDeep<'T> (_: 'T) : 'T = importDefault "lodash.clonedeep"
    let debounce<'T, 'U> (_: 'T -> 'U) (_: int) : 'T -> 'U = importDefault "lodash.debounce"
    let cloneObj<'T> (obj: 'T) (fn: 'T -> 'T) = fn (cloneDeep obj)
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
                { new Browser.Types.BlobPropertyBag with
                    member _.``type`` = contentType
                    member _.endings = Browser.Types.BlobEndings.Transparent

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
