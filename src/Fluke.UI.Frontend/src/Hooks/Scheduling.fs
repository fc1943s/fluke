namespace Fluke.UI.Frontend.Hooks

open System
open Fable.Core
open Feliz
open Fluke.UI.Frontend.Bindings


module Scheduling =
    type SchedulingType =
        | Timeout
        | Interval

    let private schedulingFn =
        function
        | Timeout -> JS.setTimeout, JS.clearTimeout
        | Interval -> JS.setInterval, JS.clearInterval

    let useScheduling schedulingType duration (fn: Jotai.GetFn -> Jotai.SetFn -> JS.Promise<unit>) =
        let fnCallback = React.useCallbackRef (fun (get, set) -> fn get set)

        let savedCallback = React.useRef fnCallback

        React.useEffect (
            (fun () -> savedCallback.current <- fnCallback),
            [|
                box savedCallback
                box fnCallback
            |]
        )

        let mounted, setMounted = React.useState true

        let fn =
            Store.useCallback (
                (fun get set _ -> promise { if mounted then do! savedCallback.current (get, set) }),
                [|
                    box mounted
                    box savedCallback
                |]
            )

        React.useEffect (
            (fun () ->
                let setFn, clearFn = schedulingFn schedulingType
                let id = setFn (fn >> Promise.start) duration
                setMounted true

                { new IDisposable with
                    member _.Dispose () =
                        setMounted false
                        clearFn id
                }),
            [|
                box fn
                box setMounted
                box schedulingType
                box savedCallback
                box duration
            |]
        )
