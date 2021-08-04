namespace FsJs

open System
open Browser.Types
open Fable.Extras
open FsCore
open System.Collections.Generic
open Fable.Core.JsInterop
open Fable.Core


module Dom =
    let inline window () =
        if jsTypeof Browser.Dom.window <> "undefined" then
            Some Browser.Dom.window
        else
            printfn "No window found"
            None

    let private domRefs = Dictionary<string, obj> ()

    match window () with
    | Some window -> window?domRefs <- domRefs
    | None -> ()

    let set key value = domRefs.[key] <- value

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
        match window () with
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
                    IsElectron = jsTypeof window?electronApi = "object"
                    IsExtension = window.location.protocol = "chrome-extension:"
                    GitHubPages = window.location.host.EndsWith "github.io"
                    IsTesting = JS.jestWorkerId || window?Cypress <> null
                }

            printfn $"deviceInfo={JS.JSON.stringify deviceInfo}"
            deviceInfo

    let isDebugStatic =
        not deviceInfo.GitHubPages
        && not deviceInfo.IsExtension
        && not deviceInfo.IsElectron
        && not deviceInfo.IsMobile

    match window () with
    | Some window ->
        window?Debug <- false

        if window?Cypress <> null then window?Debug <- true
    | None -> ()

    let inline isDebug () =
        let debug =
            match window () with
            | Some window -> window?Debug
            | None -> false

        debug <> false && (debug || isDebugStatic)

    let inline logWithFn logFn fn =
        if isDebug () then
            let result = fn ()

            if result |> Option.ofObjUnbox |> Option.isSome then
                logFn $"""[{DateTime.Now |> DateTime.format "HH:mm:ss"}] {result}"""

    let inline log fn = logWithFn (fun x -> printfn $"{x}") fn
    let inline logError fn = logWithFn (fun x -> eprintfn $"{x}") fn

    let inline logFiltered newValue fn =
        log
            (fun () ->
                if (string newValue).StartsWith "Ping " then
                    null
                else
                    let result: string = fn ()
                    if result.Contains "devicePing" then null else result)

    let inline consoleLog x = Browser.Dom.console.log x
    let inline consoleError x = Browser.Dom.console.error x


    let inline exited () =
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

                    do! JS.sleep 100
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

                    do! JS.sleep 100
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
                        do! JS.sleep 0
                    else
                        consoleLog ("waitForSome: none. waiting...", fn.ToString ())
                        do! JS.sleep 100

                    return! waitForSome fn
        }

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
