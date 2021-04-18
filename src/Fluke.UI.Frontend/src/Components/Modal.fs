namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings
open Fable.React
open Fable.Core.JsInterop


module Modal =

    [<ReactComponent>]
    let Modal
        (input: {| IsOpen: bool
                   OnClose: unit -> unit
                   children: seq<ReactElement> |})
        =
        let isOpenRef = React.useRef input.IsOpen

        React.useEffect (
            (fun () ->
                if input.IsOpen <> isOpenRef.current then
                    isOpenRef.current <- input.IsOpen

                    let viewport = Browser.Dom.window.document.querySelector "meta[name=viewport]"
                    let initialContent = viewport.textContent
                    viewport.textContent <- initialContent.Replace ("maximum-scale=10", "maximum-scale=1")

                    Fable.Core.JS.setTimeout (fun () -> viewport.textContent <- initialContent) 100
                    |> ignore

                ),
            [|
                box input.IsOpen
                box isOpenRef
            |]
        )

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
