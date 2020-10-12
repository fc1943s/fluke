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

module DebugOverlayComponent =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun () ->
            let text, setText = React.useState ""
            let oldJson, setOldJson = React.useState ""
            let debug = Recoil.useValue Recoil.Atoms.debug

            Scheduling.useScheduling Scheduling.Interval 100 (fun () ->
                if not debug then
                    ()
                else
                    let indent n = String (' ', n)

                    let json =
                        Profiling.profilingState
                        |> Fable.SimpleJson.SimpleJson.stringify
                        |> JS.JSON.parse
                        |> fun obj -> JS.JSON.stringify (obj, unbox null, 4)
                        |> String.replace (sprintf ",\n%s" (indent 3)) ""
                        |> String.replace (indent 1) ""
                        |> String.replace "][\n" ""
                        |> String.replace "\"" " "

                    if json = oldJson then
                        ()
                    else
                        setText json
                        setOldJson json)

            if not debug then
                nothing
            else
                React.fragment [
                    Html.pre [
                        prop.id "diag"
                        prop.style [
                            style.custom ("width", "min-content")
                            style.custom ("height", "80%")
                            style.position.fixedRelativeToWindow
                            style.right 0
                            style.bottom 0
                            style.fontSize 9
                            style.backgroundColor "#44444488"
                            style.zIndex 1
                            style.overflow.scroll
                        ]
                        prop.children
                            [
                                str text
                            ]
                    ]

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
                ]
            )
