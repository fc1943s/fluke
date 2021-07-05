namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.Bindings

module ConfirmPopover =
    type ConfirmPopoverType = | MenuItem

    [<ReactComponent>]
    let ConfirmPopover confirmPopoverType icon label fn =
        Popover.CustomPopover
            {|
                CloseButton = true
                RenderOnHover = true
                Props = fun x -> x.closeOnBlur <- false
                Padding = "10px"
                Trigger =
                    match confirmPopoverType with
                    | MenuItem ->
                        MenuItem.MenuItem icon label (fun () -> promise { () }) (fun x -> x.closeOnSelect <- false)
                Body =
                    fun (disclosure, initialFocusRef) ->
                        [
                            Chakra.box
                                (fun _ -> ())
                                [
                                    Chakra.stack
                                        (fun x -> x.spacing <- "10px")
                                        [
                                            Chakra.box
                                                (fun x ->
                                                    x.paddingBottom <- "5px"
                                                    x.marginRight <- "24px"
                                                    x.fontSize <- "15px")
                                                [
                                                    str label
                                                ]

                                            Chakra.box
                                                (fun _ -> ())
                                                [
                                                    Button.Button
                                                        {|
                                                            Hint = None
                                                            Icon = Some (icon |> Icons.wrap, Button.IconPosition.Left)
                                                            Props =
                                                                fun x ->
                                                                    x.ref <- initialFocusRef

                                                                    x.onClick <-
                                                                        fun e ->
                                                                            promise {
                                                                                do! fn ()
                                                                                disclosure.onClose ()
                                                                                e.preventDefault ()
                                                                            }
                                                            Children =
                                                                [
                                                                    str "Confirm"
                                                                ]
                                                        |}
                                                ]
                                        ]
                                ]
                        ]
            |}
