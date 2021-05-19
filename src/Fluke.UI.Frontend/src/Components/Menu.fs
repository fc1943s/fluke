namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings


module Menu =
    [<ReactComponent>]
    let Menu
        (input: {| Tooltip: string
                   Trigger: ReactElement
                   Menu: seq<ReactElement>
                   MenuListProps: Chakra.IChakraProps -> unit |})
        =
        Chakra.menu
            (fun x ->
                x.isLazy <- false
                x.closeOnSelect <- false)
            [
                Tooltip.wrap
                    (str input.Tooltip)
                    [
                        input.Trigger
                    ]
                Chakra.menuList
                    (fun x ->
                        x.backgroundColor <- "gray.13"
                        input.MenuListProps x)
                    [
                        yield! input.Menu
                    ]
            ]
