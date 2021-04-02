namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.UseListener
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fable.Core
open Fluke.UI.Frontend.Hooks


module DebugOverlay =

    [<ReactComponent>]
    let DebugOverlay () =
        let text, setText = React.useState ""
        let oldJson, setOldJson = React.useState ""
        let debug, setDebug = Recoil.useState Recoil.Atoms.debug

        Scheduling.useScheduling
            Scheduling.Interval
            1000
            (fun () ->
                if not debug then
                    ()
                else
                    let json =
                        {|
                            CallCount =
                                Profiling.profilingState.CallCount
                                |> Seq.map (fun (KeyValue (k, v)) -> k, box <| string v)
                                |> JsInterop.createObj
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
                        setOldJson json)

        React.fragment [
            if debug then
                Chakra.box
                    {|
                        id = "test1"
                        position = "absolute"
                        width = "100px"
                        height = "80px"
                        top = "40px"
                        right = 0
                        backgroundColor = "#ccc3"
                        zIndex = 1
                    |}
                    [
                        str "test1"
                    ]

            Chakra.box
                {|
                    width = "min-content"
                    height =
                        if debug then
                            "80%"
                        else
                            "initial"
                    position = "fixed"
                    right = 0
                    bottom = 0
                    fontSize = "9px"
                    backgroundColor = "#44444488"
                    zIndex = 1
                    overflow =
                        if debug then
                            "scroll"
                        else
                            "initial"
                |}
                [
                    Checkbox.Checkbox
                        {|
                            isChecked = debug
                            onChange =
                                fun (e: {| target: Browser.Types.HTMLInputElement |}) -> setDebug e.target.``checked``
                        |}
                        [
                            str (
                                if debug then
                                    "Debug"
                                else
                                    ""
                            )
                        ]

                    if debug then
                        Html.pre [
                            prop.id "diag"
                            prop.children [ str text ]
                        ]
                ]
        ]
