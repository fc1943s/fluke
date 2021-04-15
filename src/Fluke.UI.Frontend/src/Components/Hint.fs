namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings


module Hint =
    type IProps =
        abstract hint : ReactElement option with get, set
        abstract hintTitle : ReactElement option with get, set

    [<ReactComponent>]
    let Hint (input: IProps) =
        match input.hint with
        | Some hint ->
            Chakra.popover
                {|  |}
                [
                    Chakra.popoverTrigger
                        {|  |}
                        [
                            Chakra.iconButton
                                {|
                                    icon = Icons.bsQuestionCircle ()
                                    backgroundColor = "transparent"
                                    variant = "outline"
                                    border = 0
                                    marginLeft = "1px"
                                    marginTop = "-2px"
                                    padding = "2px"
                                    minWidth = 0
                                    width = "auto"
                                    height = "auto"
                                |}
                                []
                        ]
                    Chakra.popoverContent
                        {|  |}
                        [
                            Chakra.popoverArrow {|  |} []
                            Chakra.popoverCloseButton {|  |} []
                            Chakra.popoverBody
                                {|
                                    padding = "10px"
                                    backgroundColor = "gray.13"
                                |}
                                [
                                    match input.hintTitle with
                                    | Some hintTitle ->
                                        Chakra.box
                                            {|
                                                paddingBottom = "12px"
                                                fontSize = "15px"
                                            |}
                                            [
                                                Chakra.icon
                                                    {|
                                                        marginTop = "-3px"
                                                        marginRight = "5px"
                                                        icon = Icons.bsQuestionCircle ()
                                                    |}
                                                    []

                                                hintTitle
                                            ]
                                    | None -> ()
                                    hint
                                ]
                        ]
                ]
        | None -> nothing
