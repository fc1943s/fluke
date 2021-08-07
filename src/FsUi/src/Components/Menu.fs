namespace FsUi.Components

open Fable.Core
open Fable.React
open Feliz
open FsStore
open FsUi.Bindings
open FsJs
open FsCore

module Menu =
    [<ReactComponent>]
    let FakeMenuButton (cmp: (UI.IChakraProps -> unit) -> ReactElement) (props: UI.IChakraProps -> unit) =
        let menuContext = UI.react.useMenuContext ()

        let menuButtonProps =
            match menuContext |> Option.ofObjUnbox with
            | Some _ -> UI.react.useMenuButton (box {|  |})
            | None -> React.useMemo ((fun () -> JS.newObj (fun _ -> ())), [||])

        cmp
            (fun x ->
                x.``as`` <- UI.react.Box
                x.tabIndex <- 0
                x <+ menuButtonProps

                x.onClick <-
                    fun e ->
                        menuButtonProps.onClick e |> ignore
                        e.preventDefault ()
                        JS.undefined

                x.onKeyDown <-
                    fun e ->
                        if e.key = " " then
                            menuButtonProps.onClick (unbox e) |> ignore
                        else
                            menuButtonProps.onKeyDown e |> ignore

                        if e.key = " " || e.key = "Enter" then e.preventDefault ()
                        JS.undefined

                props x)

    let inline Menu
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
