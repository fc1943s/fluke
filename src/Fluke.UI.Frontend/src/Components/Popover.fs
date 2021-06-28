namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings


module Popover =
    [<ReactComponent>]
    let CustomPopover
        (input: {| Trigger: ReactElement
                   Placement: string option
                   CloseButton: bool
                   Padding: string
                   Body: Chakra.Disclosure * IRefValue<unit> -> ReactElement list |})
        =
        let disclosure = Chakra.react.useDisclosure ()

        let initialFocusRef = React.useRef ()


        if not disclosure.isOpen then
            Chakra.box
                (fun x -> x.onClick <- fun _ -> promise { disclosure.onOpen () })
                [
                    input.Trigger
                ]
        else
            Chakra.popover
                (fun x ->
                    //                            x.isLazy <- true

                    match input.Placement with
                    | Some placement -> x.placement <- placement
                    | None -> ()

                    x.closeOnBlur <- true
                    x.autoFocus <- true
                    //                            x.computePositionOnMount <- false
                    x.defaultIsOpen <- true
                    x.initialFocusRef <- initialFocusRef
                    x.isOpen <- disclosure.isOpen
                    x.onOpen <- disclosure.onOpen

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

                    match input.Body (disclosure, initialFocusRef) with
                    | [ x ] when x = nothing -> nothing
                    | content ->
                        Chakra.popoverContent
                            (fun x ->
                                x.width <- "auto"
                                x.borderRadius <- "0px")
                            [
                                Chakra.popoverArrow (fun _ -> ()) []

                                if not input.CloseButton then
                                    nothing
                                else
                                    Button.Button
                                        {|
                                            Hint = None
                                            Icon = Some (Icons.io.IoMdClose |> Icons.wrap, Button.IconPosition.Left)
                                            Props =
                                                fun x ->
                                                    x.position <- "absolute"
                                                    x.right <- "0"
                                                    x.margin <- "5px"
                                                    x.minWidth <- "20px"
                                                    x.height <- "20px"

                                                    x.onClick <- fun _ -> promise { disclosure.onClose () }
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

    let inline Popover
        (input: {| Trigger: ReactElement
                   Body: Chakra.Disclosure * IRefValue<unit> -> ReactElement list |})
        =
        CustomPopover
            {| input with
                CloseButton = true
                Placement = None
                Padding = "10px"
            |}
