namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings


module Menu =
    [<ReactComponent>]
    let Menu
        (input: {| Tooltip: string
                   Trigger: ReactElement
                   Body: seq<ReactElement>
                   MenuListProps: UI.IChakraProps -> unit |})
        =
        UI.menu
            (fun x ->
                x.isLazy <- true
                x.closeOnSelect <- false)
            [
                Tooltip.wrap
                    (str input.Tooltip)
                    [
                        input.Trigger
                    ]
                UI.menuList
                    (fun x ->
                        x.``as`` <- UI.react.Stack
                        x.spacing <- "2px"
                        x.backgroundColor <- "gray.13"
                        input.MenuListProps x)
                    [
                        yield! input.Body
                    ]
            ]
