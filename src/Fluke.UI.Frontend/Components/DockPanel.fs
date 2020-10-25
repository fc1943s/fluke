namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Model
open Fluke.UI.Frontend.Bindings

module DockPanel =
    let render =
        React.memo (fun (input: {| Name: string
                                   Icon: obj
                                   Atom: RecoilValue<DockType option, ReadWrite>
                                   Children: ReactElement list |}) ->
            let setAtom = Recoil.useSetState input.Atom
            Chakra.stack
                {| spacing = 0; flex = 1 |}
                [
                    Chakra.flex
                        {|
                            paddingLeft = "10px"
                            borderBottomColor = "gray.16%"
                            borderBottom = "1px solid"
                            align = "center"
                        |}
                        [
                            Chakra.box {| ``as`` = input.Icon; marginRight = "6px" |} []
                            str input.Name

                            Chakra.spacer () []

                            Chakra.iconButton
                                {|
                                    icon = Icons.faMinus ()
                                    backgroundColor = "transparent"
                                    variant = "outline"
                                    border = 0
                                    width = "30px"
                                    height = "30px"
                                    borderRadius = 0
                                    onClick = fun () -> setAtom None
                                |}
                                []
                        ]

                    Chakra.flex
                        {|
                            direction = "column"
                            paddingTop = "10px"
                            paddingLeft = "10px"
                            paddingRight = "10px"
                            flex = 1
                        |}
                        [
                            yield! input.Children
                        ]
                ])
