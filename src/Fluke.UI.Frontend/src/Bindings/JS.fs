namespace Fluke.UI.Frontend.Bindings

open Fable.Core
open Fable.Core.JsInterop
open Feliz.Recoil
open Fluke.Shared
open Fable.Extras


[<AutoOpen>]
module Operators =
    [<Emit("Object.assign({}, $0, $1)")>]
    let (++) _o1 _o2 : obj = jsNative

    [<Emit("Object.assign($0, $1)")>]
    let (<+) _o1 _o2 : unit = jsNative

module JS =
    [<Emit "process.env.JEST_WORKER_ID">]
    let private jestWorkerId : bool = jsNative

    let window fn =
        if jsTypeof Browser.Dom.window <> "undefined" then
            Some (fn Browser.Dom.window)
        else
            printfn "No window found"
            None

    let deviceInfo =
        match window id with
        | None ->
            {|
                UserAgent = "window==null"
                IsEdge = false
                IsMobile = false
                IsExtension = false
                GitHubPages = false
                IsTesting = false
            |}
        | Some window ->
            let userAgent = if window?navigator = None then "" else window?navigator?userAgent

            let isEdge = (JSe.RegExp @"Edg\/").Test userAgent

            let isMobile =
                JSe
                    .RegExp("Android|BlackBerry|iPhone|iPad|iPod|Opera Mini|IEMobile|WPDesktop", JSe.RegExpFlag().i)
                    .Test userAgent

            let isExtension = window.location.protocol = "chrome-extension:"
            let gitHubPages = window.location.host.EndsWith "github.io"
            let isTesting = jestWorkerId || window?Cypress <> null

            let deviceInfo =
                {|
                    UserAgent = userAgent
                    IsEdge = isEdge
                    IsMobile = isMobile
                    IsExtension = isExtension
                    GitHubPages = gitHubPages
                    IsTesting = isTesting
                |}

            printfn $"deviceInfo={JS.JSON.stringify deviceInfo}"
            deviceInfo

    let inline log fn =
        if not deviceInfo.GitHubPages
           && not deviceInfo.IsTesting
           && not deviceInfo.IsMobile then
            printfn $"[log] {fn ()}"
        else
            ()

    [<Emit "(w => $0 instanceof w[$1])(window)">]
    let instanceof (_obj: obj, _typeName: string) : bool = jsNative

    [<Emit "(() => { var audio = new Audio($0); audio.volume = 0.5; return audio; })().play();">]
    let playAudio (_file: string) : unit = jsNative

    let newObj fn = jsOptions<_> fn
    let cloneDeep<'T> (_: 'T) : 'T = importDefault "lodash.clonedeep"
    let cloneObj<'T> (obj: 'T) (fn: 'T -> 'T) = fn (cloneDeep obj)
    let toJsArray a = a |> Array.toList |> List.toArray

    let inline sleep (ms: int) = Async.Sleep ms
    //        Async.FromContinuations (fun (res, _, _) -> JS.setTimeout res ms |> ignore)

    let rec waitFor fn =
        async {
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
            let! obj = fn ()

            match obj with
            | Some obj -> return obj
            | None ->
                Browser.Dom.console.log ("waitForSome: none. waiting...", fn.ToString())
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
