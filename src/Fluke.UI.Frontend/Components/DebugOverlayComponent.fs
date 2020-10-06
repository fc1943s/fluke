namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.UseListener
open Feliz.Recoil
open FSharpPlus
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared

module DebugOverlay =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun (input: {| BottomRightText: string |}) ->
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
                            str input.BottomRightText
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
            ])
