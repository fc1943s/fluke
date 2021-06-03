namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings


module Popover =
    [<ReactComponent>]
    let Popover
        (input: {| Trigger: ReactElement
                   Body: Chakra.Disclosure * IRefValue<unit> -> ReactElement list |})
        =
        let disclosure = Chakra.react.useDisclosure ()

        let initialFocusRef = React.useRef ()

        let content =
            React.useMemo (
                (fun () ->
                    React.fragment [
                        yield! input.Body (disclosure, initialFocusRef)
                    ]),
                [|
                    box disclosure
                    box input
                    box initialFocusRef
                |]
            )

        Chakra.box
            (fun _ -> ())
            [
                Chakra.popover
                    (fun x ->
                        x.isLazy <- true
                        x.closeOnBlur <- true
                        x.isOpen <- disclosure.isOpen
                        x.onOpen <- disclosure.onOpen
                        x.initialFocusRef <- initialFocusRef
                        x.onClose <- fun x -> promise { disclosure.onClose x })
                    [
                        Chakra.popoverTrigger
                            (fun _ -> ())
                            [
                                Chakra.box
                                    (fun _ -> ())
                                    [
                                        input.Trigger
                                    ]
                            ]
                        Chakra.popoverContent
                            (fun x -> x.width <- "auto")
                            [
                                Chakra.popoverArrow (fun _ -> ()) []
                                Chakra.popoverCloseButton (fun _ -> ()) []
                                Chakra.popoverBody
                                    (fun x ->
                                        x.padding <- "10px"
                                        x.backgroundColor <- "gray.13"
                                        x.maxWidth <- "95vw"
                                        x.maxHeight <- "95vh"
                                        x.overflow <- "auto")
                                    [
                                        content
                                    ]
                            ]
                    ]
            ]
