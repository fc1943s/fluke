namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.UseListener
open Feliz.Recoil
open FSharpPlus
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open System
open Fable.Core
open Fluke.UI.Frontend.Hooks

module DebugOverlay =
    open Domain.Model
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun () ->
            let text, setText = React.useState ""
            let oldJson, setOldJson = React.useState ""
            let debug, setDebug = Recoil.useState Recoil.Atoms.debug

            Scheduling.useScheduling Scheduling.Interval 1000 (fun () ->
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
                                |> Seq.map (fun (k, v) -> sprintf "%A = %A" k v)
                                |> Seq.toList
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
                            height = "100px"
                            top = 0
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
                        Chakra.checkbox
                            {|
                                isChecked = debug
                                onClick =
                                    fun (e: {| preventDefault: unit -> unit |}) ->
                                        setDebug (not debug)
                                        e.preventDefault ()
                            |}
                            [
                                str
                                    (if debug then
                                        "Debug"
                                     else
                                         "")
                            ]

                        if debug then
                            Html.pre [
                                prop.id "diag"
                                prop.children
                                    [
                                        str text
                                    ]
                            ]
                    ]
            ])
