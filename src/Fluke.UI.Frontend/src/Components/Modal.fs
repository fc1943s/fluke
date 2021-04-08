namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings
open Fable.React


module Modal =

    [<ReactComponent>]
    let Modal
        (input: {| IsOpen: bool
                   OnClose: unit -> unit
                   children: seq<ReactElement> |})
        =
        Chakra.modal
            {|
                isCentered = true
                isOpen = input.IsOpen
                onClose = input.OnClose
            |}
            [
                Chakra.modalOverlay () []
                Chakra.modalContent
                    {| backgroundColor = "gray.13" |}
                    [
                        Chakra.modalBody
                            {| padding = "40px" |}
                            [
                                yield! input.children
                            ]
                        Chakra.modalCloseButton () []
                    ]
            ]
