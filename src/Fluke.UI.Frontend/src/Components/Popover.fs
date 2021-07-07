namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks


module Popover =
    [<ReactComponent>]
    let CustomPopover
        (input: {| Trigger: ReactElement
                   RenderOnHover: bool
                   CloseButton: bool
                   Padding: string
                   Body: Chakra.Disclosure * IRefValue<unit> -> ReactElement list
                   Props: Chakra.IChakraProps -> unit |})
        =
        let disclosure = Chakra.react.useDisclosure ()

        let initialFocusRef = React.useRef ()

        if not disclosure.isOpen then
            Chakra.box
                (fun x ->
                    x.display <- "inline"

                    x.onClick <-
                        fun e ->
                            promise {
                                e.preventDefault ()
                                disclosure.onOpen ()
                            })
                [
                    input.Trigger
                ]
        else
            Chakra.box
                (fun x -> x.display <- "inline")
                [
                    Chakra.popover
                        (fun x ->
                            //                            x.isLazy <- true

                            x.closeOnBlur <- true
                            x.autoFocus <- true
                            //                            x.computePositionOnMount <- false
                            x.defaultIsOpen <- true
                            x.initialFocusRef <- initialFocusRef
                            x.isOpen <- disclosure.isOpen
                            x.onOpen <- disclosure.onOpen

                            x.onClose <- fun x -> promise { disclosure.onClose x }
                            input.Props x)
                        [
                            Chakra.popoverTrigger
                                (fun _ -> ())
                                [
                                    Chakra.box
                                        (fun x -> x.display <- "inline")
                                        [
                                            input.Trigger
                                        ]
                                ]

                            match input.Body (disclosure, initialFocusRef) with
                            | [ x ] when x = nothing -> nothing
                            | content ->
                                Chakra.popoverContent
                                    (fun x ->
                                        x.width <- "auto"
                                        x.border <- "0"
                                        x.borderRadius <- "0")
                                    [
                                        Chakra.popoverArrow (fun _ -> ()) []

                                        if not input.CloseButton then
                                            nothing
                                        else
                                            Button.Button
                                                {|
                                                    Hint = None
                                                    Icon =
                                                        Some (
                                                            Icons.io.IoMdClose |> Icons.render,
                                                            Button.IconPosition.Left
                                                        )
                                                    Props =
                                                        fun x ->
                                                            x.position <- "absolute"
                                                            x.right <- "0"
                                                            x.margin <- "5px"
                                                            x.minWidth <- "20px"
                                                            x.height <- "20px"

                                                            x.onClick <-
                                                                fun e ->
                                                                    promise {
                                                                        e.preventDefault ()
                                                                        disclosure.onClose ()
                                                                    }
                                                    Children = []
                                                |}

                                        Chakra.popoverBody
                                            (fun x ->
                                                x.padding <- input.Padding
                                                x.backgroundColor <- "gray.13"
                                                x.maxWidth <- "95vw"
                                                x.maxHeight <- "95vh"
                                                x.overflow <- "auto")
                                            [
                                                yield! content
                                            ]
                                    ]
                        ]
                ]

    let inline Popover
        (input: {| Trigger: ReactElement
                   Body: Chakra.Disclosure * IRefValue<unit> -> ReactElement list
                   Props: Chakra.IChakraProps -> unit |})
        =
        CustomPopover
            {| input with
                CloseButton = true
                RenderOnHover = false
                Padding = "10px"
            |}
