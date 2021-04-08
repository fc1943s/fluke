namespace Fluke.UI.Frontend.Components

open Fable.Core
open Feliz
open Fluke.UI.Frontend.Bindings


module Button =

    [<ReactComponent>]
    let Button
        (input: {| RightIcon: bool option
                   Icon: obj option
                   props: {| color: string option
                             flex: int option
                             autoFocus: bool option
                             marginLeft: string option
                             onClick: (unit -> JS.Promise<unit>) option |}
                   children: seq<ReactElement> |})
        =
        match input.children |> Seq.toList with
        | [] -> Chakra.iconButton {| input.props with icon = input.Icon |} []
        | children ->
            let icon =
                Chakra.box
                    {|
                        ``as`` = input.Icon
                        fontSize = "21px"
                    |}
                    []

            Chakra.button
                //                    _focus = {| backgroundColor = "white" |}
                {| input.props with
                    height = "auto"
                    paddingTop = "2px"
                    paddingBottom = "2px"
                |}
                [
                    Chakra.stack
                        {| direction = "row"; spacing = "7px" |}
                        [
                            if input.RightIcon = (Some false) then
                                icon

                            Chakra.box
                                ()
                                [
                                    yield! children
                                ]

                            if input.RightIcon = (Some true) then
                                icon

                        ]
                ]
