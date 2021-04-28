namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings


module Modal =
    type IProps =
        inherit Chakra.IChakraProps

    [<ReactComponent>]
    let Modal (input: {| Props: IProps |}) =
        let isOpenRef = React.useRef input.Props.isOpen

        React.useEffect (
            (fun () ->
                if input.Props.isOpen <> isOpenRef.current then
                    isOpenRef.current <- input.Props.isOpen),
            [|
                box input.Props.isOpen
                box isOpenRef
            |]
        )

        Chakra.modal
            (fun x ->
                x.isCentered <- true
                x.isLazy <- true
                x.isOpen <- input.Props.isOpen
                x.onClose <- input.Props.onClose)
            [
                Chakra.modalOverlay (fun _ -> ()) []
                Chakra.modalContent
                    (fun x -> x.backgroundColor <- "gray.13")
                    [
                        Chakra.modalBody
                            (fun x -> x.padding <- "40px")
                            [
                                yield! input.Props.children
                            ]
                        Chakra.modalCloseButton (fun _ -> ()) []
                    ]
            ]
