namespace FsUi.Components

open Fable.React
open FsJs
open FsUi.Bindings


module MenuItem =
    let inline MenuItem icon label onClick props =
        UI.menuItem
            (fun x ->
                x.closeOnSelect <- true

                x.icon <-
                    icon
                    |> Icons.renderWithProps
                        (fun x ->
                            x.fontSize <- "13px"
                            x.marginTop <- "1px")

                x.paddingLeft <- "11px"
                x.paddingRight <- "10px"
                x.paddingTop <- "5px"
                x.paddingBottom <- "5px"
                x.marginTop <- "2px"
                x.marginBottom <- "2px"
                x.color <- "gray.87"

                x._hover <- JS.newObj (fun x -> x.backgroundColor <- "gray.10")

                x.onClick <-
                    fun e ->
                        e.preventDefault ()

                        promise {
                            match onClick with
                            | Some onClick -> do! onClick ()
                            | None -> ()
                        }

                props x)
            [
                str label
            ]
