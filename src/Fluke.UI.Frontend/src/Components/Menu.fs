namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings


module Menu =
    [<ReactComponent>]
    let Menu
        (input: {| Title: string
                   Trigger: ReactElement
                   Menu: seq<ReactElement> |})
        =
        Chakra.menu
            (fun x -> x.isLazy <- false)
            [
                Tooltip.wrap
                    (str input.Title)
                    [
                        input.Trigger
                    ]
                Chakra.menuList
                    (fun x -> x.backgroundColor <- "gray.13")
                    [
                        yield! input.Menu
                    ]
            ]
