namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings


module Popover =
    [<ReactComponent>]
    let Popover
        (input: {| Trigger: ReactElement
                   Body: seq<ReactElement> |})
        =
        Chakra.popover
            (fun x -> x.isLazy <- true)
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
                    (fun _ -> ())
                    [
                        Chakra.popoverArrow (fun _ -> ()) []
                        Chakra.popoverCloseButton (fun _ -> ()) []
                        Chakra.popoverBody
                            (fun x ->
                                x.padding <- "10px"
                                x.backgroundColor <- "gray.13")
                            [
                                yield! input.Body
                            ]
                    ]
            ]
