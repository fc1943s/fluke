namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings
open Fable.React


module Modal =
    let render =
        React.memo (fun (input: {| IsOpen: bool
                                   OnClose: unit -> unit
                                   children: seq<ReactElement> |}) ->
            Chakra.modal
                {|
                    isCentered = true
                    isOpen = input.IsOpen
                    onClose = input.OnClose
                |}
                [
                    Chakra.modalOverlay () []
                    Chakra.modalContent
                        ()
                        [
                            Chakra.modalBody
                                {| padding = "40px" |}
                                [
                                    yield! input.children
                                ]
                            Chakra.modalCloseButton () []
                        ]
                ])
