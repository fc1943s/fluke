namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings


module Button =

    [<ReactComponent>]
    let Button (input: {| RightIcon: bool
                          Icon: obj
                          props: {| marginLeft: string; onClick: unit -> unit |}
                          children: seq<ReactElement> |}) =
        match input.children |> Seq.toList with
        | [] -> Chakra.iconButton {| input.props with icon = input.Icon |} []
        | children ->
            let icon = Chakra.box {| ``as`` = input.Icon; fontSize = "21px" |} []

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
                            if not input.RightIcon then
                                icon

                            Chakra.box
                                ()
                                [
                                    yield! children
                                ]

                            if input.RightIcon then
                                icon

                        ]
                ]
