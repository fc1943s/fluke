namespace Fluke.UI.Frontend.Components

open Fable.React
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Feliz


module ImageModal =
    [<ReactComponent>]
    let ImageModal uiFlagType uiFlagValue title url =
        ModalFlag.ModalFlagBundle
            {|
                UIFlagType = uiFlagType
                UIFlagValue = uiFlagValue
                Trigger =
                    fun trigger _ ->
                        UI.box
                            (fun x ->
                                x.``as`` <- "img"
                                x.cursor <- "pointer"
                                x.title <- title
                                x.onClick <- fun _ -> promise { do! trigger () }
                                x.src <- url)
                            []
                Content =
                    fun onHide _ ->
                        UI.box
                            (fun _ -> ())
                            [
                                UI.box
                                    (fun x ->
                                        x.``as`` <- "img"
                                        x.cursor <- "pointer"
                                        x.title <- title
                                        x.onClick <- fun _ -> promise { do! onHide () }
                                        x.src <- url)
                                    []
                                str title
                            ]
            |}
