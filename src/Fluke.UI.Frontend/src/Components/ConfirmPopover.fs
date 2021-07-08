namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.Bindings

module ConfirmPopover =
    type ConfirmPopoverType =
        | MenuItem
        | Element of ReactElement

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
                    | Element element -> element
                Body =
                    fun (disclosure, initialFocusRef) ->
                        [
                            UI.box
                                (fun _ -> ())
                                [
                                    UI.stack
                                        (fun x -> x.spacing <- "10px")
                                        [
                                            UI.box
                                                (fun x ->
                                                    x.paddingBottom <- "5px"
                                                    x.marginRight <- "24px"
                                                    x.fontSize <- "15px")
                                                [
                                                    str label
                                                ]

                                            UI.box
                                                (fun _ -> ())
                                                [
                                                    Button.Button
                                                        {|
                                                            Hint = None
                                                            Icon = Some (icon |> Icons.render, Button.IconPosition.Left)
                                                            Props =
                                                                fun x ->
                                                                    x.ref <- initialFocusRef

                                                                    x.onClick <-
                                                                        fun e ->
                                                                            promise {
                                                                                e.preventDefault ()
                                                                                do! fn ()
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
                        ]
            |}
