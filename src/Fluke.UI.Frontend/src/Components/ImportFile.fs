namespace Fluke.UI.Frontend.Components

open Fable.Core.JsInterop
open Feliz
open Fable.React
open FsUi.Bindings
open Browser.Types
open FsUi.Components


module ImportFile =
    [<ReactComponent>]
    let ImportFile label onConfirm =
        let inputRef = React.useRef<HTMLInputElement> null
        let files, setFiles = React.useState (None: FileList option)

        Ui.stack
            (fun x -> x.spacing <- "15px")
            [
                Ui.box
                    (fun x ->
                        x.paddingBottom <- "5px"
                        x.marginRight <- "24px"
                        x.fontSize <- "1.3rem")
                    [
                        str label
                    ]

                Ui.input
                    (fun x ->
                        x.``type`` <- "file"
                        x.padding <- "10px"
                        x.height <- "43px"
                        x.ref <- inputRef
                        x.onChange <- fun x -> promise { x?target?files |> Option.ofObj |> setFiles })
                    []

                Ui.box
                    (fun _ -> ())
                    [
                        Button.Button
                            {|
                                Tooltip = None
                                Icon = Some (Icons.bi.BiImport |> Icons.render, Button.IconPosition.Left)
                                Props =
                                    fun x ->
                                        x.onClick <-
                                            fun _ ->
                                                promise {
                                                    do! onConfirm files
                                                    inputRef.current.value <- ""
                                                }
                                Children =
                                    [
                                        str "Confirm"
                                    ]
                            |}
                    ]
            ]
