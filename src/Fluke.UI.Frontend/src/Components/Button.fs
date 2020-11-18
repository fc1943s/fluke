namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.Shared.Domain


module Button =
    open Domain.Model
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun (input: {| icon: obj
                                   rightIcon: bool
                                   props: {| marginLeft: string |}
                                   children: ReactElement list |}) ->
            if input.children.IsEmpty then
                Chakra.iconButton {| input.props with icon = input.icon |} []
            else
                let icon = Chakra.box {| ``as`` = input.icon; fontSize = "21px" |} []

                Chakra.button
                    {| input.props with
                        height = "auto"
                        paddingTop = "2px"
                        paddingBottom = "2px"
                    |}
                    [
                        Chakra.stack
                            {| direction = "row"; spacing = "7px" |}
                            [
                                if not input.rightIcon then
                                    icon

                                Chakra.box
                                    ()
                                    [
                                        yield! input.children
                                    ]

                                if input.rightIcon then
                                    icon

                            ]
                    ])
