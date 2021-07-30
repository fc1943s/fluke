namespace FsUi.Hooks

open FsCore
open Fable.Core
open Feliz
open FsStore
open FsStore.Model


module Scheduling =
    type SchedulingType =
        | Timeout
        | Interval

    let private schedulingFn =
        function
        | Timeout -> JS.setTimeout, JS.clearTimeout
        | Interval -> JS.setInterval, JS.clearInterval

    let useScheduling schedulingType duration (fn: GetFn -> SetFn -> JS.Promise<unit>) =
        let fnCallback = React.useCallbackRef (fun (getter, setter) -> fn getter setter)

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
            Store.useCallbackRef
                (fun getter setter _ -> promise { if mounted then do! savedCallback.current (getter, setter) })

        React.useEffect (
            (fun () ->
                let setFn, clearFn = schedulingFn schedulingType
                let id = setFn (fn >> Promise.start) duration
                setMounted true

                Object.newDisposable
                    (fun () ->
                        setMounted false
                        clearFn id)),
            [|
                box fn
                box setMounted
                box schedulingType
                box savedCallback
                box duration
            |]
        )
