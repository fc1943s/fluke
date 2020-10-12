namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks
open Fluke.Shared


module ContentComponent =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun (input: {| Username: Username |}) ->
            Chakra.flex
                ()
                [
                    Chakra.flex
                        {| width = "24px"; position = "relative" |}
                        [
                            Chakra.flex
                                {|
                                    right = 0
                                    position = "absolute"
                                    transform = "rotate(-90deg) translate(0, -100%)"
                                    transformOrigin = "100% 0"
                                    height = "24px"
                                |}
                                [
                                    Chakra.button
                                        {|
                                            height = "100%"
                                            borderRadius = 0
                                            backgroundColor = "transparent"
                                            fontWeight = "inherit"
                                            fontSize = "14px"
                                            fontFamily = "inherit"
                                        |}
                                        [
                                            Chakra.box
                                                {|
                                                    ``as`` = Icons.md.MdSettings
                                                    marginRight = "6px"
                                                    fontSize = "12px"
                                                |}
                                                []
                                            str "Settings"
                                        ]

                                    Chakra.button
                                        {|
                                            height = "100%"
                                            borderRadius = 0
                                            backgroundColor = "transparent"
                                            fontWeight = "inherit"
                                            fontSize = "14px"
                                            fontFamily = "inherit"
                                        |}
                                        [
                                            Chakra.box
                                                {|
                                                    ``as`` = Icons.fi.FiDatabase
                                                    marginRight = "6px"
                                                    fontSize = "12px"
                                                |}
                                                []
                                            str "Databases"
                                        ]

                                ]
                        ]
                    Chakra.box
                        {| flex = 1 |}
                        [
                            Chakra.box () []
                            Chakra.box
                                ()
                                [
                                    NavBarComponent.render {| Username = input.Username |}
                                    PanelsComponent.render ()
                                ]

                        ]
                ])
