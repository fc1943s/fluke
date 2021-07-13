namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks


module Popover =
    [<ReactComponent>]
    let CustomPopover
        (input: {| Trigger: ReactElement
                   CloseButton: bool
                   Padding: string option
                   Body: UI.Disclosure * IRefValue<unit> -> ReactElement list
                   Props: UI.IChakraProps -> unit |})
        =
        let disclosure = UI.react.useDisclosure ()

        let initialFocusRef = React.useRef ()

        if not disclosure.isOpen then
            UI.box
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
            UI.box
                (fun x -> x.display <- "inline")
                [
                    UI.popover
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
                            UI.popoverTrigger
                                (fun _ -> ())
                                [
                                    UI.box
                                        (fun x -> x.display <- "inline")
                                        [
                                            input.Trigger
                                        ]
                                ]

                            match input.Body (disclosure, initialFocusRef) with
                            | [ x ] when x = nothing -> nothing
                            | content ->
                                UI.popoverContent
                                    (fun x ->
                                        x.width <- "auto"
                                        x.border <- "0"
                                        x.boxShadow <- "lg"
                                        x.borderRadius <- "0")
                                    [
                                        UI.popoverArrow (fun _ -> ()) []

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

                                        UI.popoverBody
                                            (fun x ->
                                                x.padding <- input.Padding |> Option.defaultValue "15px"
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
                   Body: UI.Disclosure * IRefValue<unit> -> ReactElement list |})
        =
        CustomPopover
            {| input with
                CloseButton = true
                Padding = None
                Props = fun _ -> ()
            |}

    let inline MenuItemPopover
        (input: {| Trigger: ReactElement
                   Body: UI.Disclosure * IRefValue<unit> -> ReactElement list |})
        =
        CustomPopover
            {| input with
                CloseButton = true
                Padding = None
                Props = fun x -> x.closeOnBlur <- false
            |}

    let inline ConfirmPopover trigger onConfirm children =
        Popover
            {|
                Trigger = trigger
                Body =
                    fun (disclosure, initialFocusRef) ->
                        [
                            UI.stack
                                (fun x -> x.spacing <- "10px")
                                [
                                    yield! children (disclosure, initialFocusRef)

                                    UI.box
                                        (fun _ -> ())
                                        [
                                            Button.Button
                                                {|
                                                    Hint = None
                                                    Icon =
                                                        Some (
                                                            Icons.fi.FiCheck |> Icons.render,
                                                            Button.IconPosition.Left
                                                        )
                                                    Props =
                                                        fun x ->
                                                            x.onClick <-
                                                                fun e ->
                                                                    promise {
                                                                        e.preventDefault ()
                                                                        do! onConfirm ()
                                                                        disclosure.onClose ()
                                                                    }
                                                    Children =
                                                        [
                                                            str "Confirm"
                                                        ]
                                                |}
                                        ]
                                ]
                        ]
            |}

    let inline MenuItemConfirmPopover icon label onConfirm =
        ConfirmPopover (MenuItem.MenuItem icon label None (fun x -> x.closeOnSelect <- false)) onConfirm (fun _ -> [])
