namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings


module Hint =
    type IProps =
        abstract hint : ReactElement with get, set
        abstract hintTitle : ReactElement option with get, set

    [<ReactComponent>]
    let Hint (input: IProps) =
        Chakra.popover
            (fun _ -> ())
            [
                Chakra.popoverTrigger
                    (fun _ -> ())
                    [
                        Chakra.box
                            (fun _ -> ())
                            [
                                InputLabelIconButton.InputLabelIconButton
                                    {|
                                        Props = JS.newObj (fun x -> x.icon <- Icons.bs.BsQuestionCircle |> Icons.render)
                                    |}
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
                                match input.hintTitle with
                                | Some hintTitle ->
                                    Chakra.box
                                        (fun x ->
                                            x.paddingBottom <- "12px"
                                            x.fontSize <- "15px")
                                        [
                                            Chakra.icon
                                                (fun x ->
                                                    x.``as`` <- Icons.bs.BsQuestionCircle
                                                    x.marginTop <- "-3px"
                                                    x.marginRight <- "5px"
                                                    x.color <- "heliotrope")
                                                []

                                            hintTitle
                                        ]
                                | None -> ()
                                input.hint
                            ]
                    ]
            ]
