namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.Bindings


module MenuItem =
    [<ReactComponent>]
    let MenuItem icon label onClick props =
        Chakra.menuItem
            (fun x ->
                x.closeOnSelect <- true

                x.icon <-
                    icon
                    |> Icons.renderChakra
                        (fun x ->
                            x.fontSize <- "13px"
                            x.marginTop <- "1px")

                x.paddingLeft <- "11px"
                x.paddingRight <- "10px"
                x.paddingTop <- "5px"
                x.paddingBottom <- "5px"

                x.color <- "gray.87"
                x._hover <- JS.newObj (fun x -> x.backgroundColor <- "gray.10")

                x.onClick <-
                    fun e ->
                        promise {
                            e.preventDefault ()
                            do! onClick ()
                        }

                props x)
            [
                str label
            ]
