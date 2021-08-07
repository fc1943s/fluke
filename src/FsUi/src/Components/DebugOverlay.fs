namespace FsUi.Components

open Fable.Core
open Fable.Core.JsInterop
open Fable.React
open Feliz
open FsJs
open FsStore
open FsUi.Bindings
open FsUi.Hooks


module DebugOverlay =

    [<ReactComponent>]
    let DebugOverlay () =
        let text, setText = React.useState ""
        let oldJson, setOldJson = React.useState ""
        let showDebug = Store.useValue Atoms.showDebug

        let isTesting = Store.useValue Atoms.isTesting
        let deviceInfo = Store.useValue Selectors.deviceInfo


        Scheduling.useScheduling
            Scheduling.Interval
            1000
            (fun _ _ ->
                promise {
                    match Dom.window () with
                    | Some window -> if not window?Debug then window?showDebug <- showDebug
                    | None -> ()

                    if isTesting || not showDebug then
                        ()
                    else
                        let json =
                            {|
                                DeviceInfo = deviceInfo
                                SortedCallCount =
                                    Profiling.profilingState.CallCount
                                    |> Seq.map (fun (KeyValue (k, v)) -> k, v |> string |> box)
                                    |> Seq.sortBy fst
                                    |> createObj
                                CallCount =
                                    Profiling.profilingState.CallCount
                                    |> Seq.map (fun (KeyValue (k, v)) -> k, v |> string |> box)
                                    |> createObj
                                Timestamps =
                                    Profiling.profilingState.Timestamps
                                    |> Seq.map (fun (k, v) -> $"{k} = {v}")
                                    |> Seq.toArray
                            |}
                            |> fun obj -> JS.JSON.stringify (obj, unbox null, 4)

                        if json = oldJson then
                            ()
                        else
                            setText json
                            setOldJson json
                })

        React.fragment [
            //            if debug then
//                Chakra.box
//                    (fun x ->
//                        x.id <- "test1"
//                        x.position <- "absolute"
//                        x.width <- "100px"
//                        x.height <- "80px"
//                        x.top <- "40px"
//                        x.right <- "24px"
//                        x.backgroundColor <- "#ccc3"
//                        x.zIndex <- 1)
//                    [
//                        str "test1"
//                    ]

            UI.box
                (fun x ->
                    x.width <- "min-content"
                    x.height <- if showDebug then "60%" else "initial"
                    x.position <- "fixed"
                    x.right <- "24px"
                    x.bottom <- "0"
                    x.fontSize <- "9px"
                    x.backgroundColor <- "#44444455"
                    x.zIndex <- 4
                    x.overflow <- if showDebug then "scroll" else "initial")
                [
                    if showDebug then
                        Html.pre [
                            prop.id "diag"
                            prop.children [ str text ]
                        ]
                ]
        ]
