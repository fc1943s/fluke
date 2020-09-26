namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.UseListener
open Feliz.Recoil
open FSharpPlus
open Fluke.UI.Frontend
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

                Html.div [
                    prop.id "test1"
                    prop.style [
                        style.position.absolute
                        style.width 100
                        style.height 100
                        style.top 0
                        style.right 0
                        style.backgroundColor "#ccc3"
                        style.zIndex 1
                    ]
                    prop.children
                        [
                            str "test1"
                        ]
                ]
            ])
