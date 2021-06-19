namespace Fluke.UI.Frontend.Components

open Fable.Core
open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings


module Menu =
    [<ReactComponent>]
    let Menu
        (input: {| Tooltip: string
                   Trigger: ReactElement
                   Body: seq<ReactElement>
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
                        x.``as`` <- Chakra.react.Stack
                        x.spacing <- "2px"
                        x.backgroundColor <- "gray.13"
                        input.MenuListProps x)
                    [
                        yield! input.Body
                    ]
            ]

    [<ReactComponent>]
    let Drawer
        (input: {| Tooltip: string
                   Left: bool
                   Trigger: bool -> (bool -> unit) -> ReactElement
                   Body: (unit -> unit) -> ReactElement list |})
        =
        let visible, setVisible = React.useState false

        Chakra.flex
            (fun x -> x.direction <- "column")
            [
                Tooltip.wrap
                    (str input.Tooltip)
                    [
                        input.Trigger visible setVisible
                    ]
                if not visible then
                    nothing
                else
                    Chakra.box
                        (fun x ->
                            x.flex <- "1"
                            x.flexDirection <- "column"
                            x.marginTop <- "-1px"

                            if input.Left then
                                x.borderLeftWidth <- "1px"
                            else
                                x.borderRightWidth <- "1px"

                            x.borderBottomWidth <- "1px"
                            x.borderColor <- "whiteAlpha.200"

                            x.background <-
                                "linear-gradient(180deg, rgba(255,255,255,0) 0%, rgba(255,255,255,0.01) 7%, rgba(255,255,255,0.02) 100%);"

                            x.padding <- "17px")
                        [
                            yield! input.Body (fun () -> setVisible false)
                        ]
            ]

    [<ReactComponent>]
    let rec DrawerMenuButton
        (input: {| Label: string
                   OnClick: unit -> JS.Promise<unit>
                   Checked: bool |})
        =
        Button.Button
            {|
                Hint = None
                Icon =
                    Some (
                        (if input.Checked then
                             Icons.fi.FiCheck |> Icons.wrap
                         else
                             fun () -> (Chakra.box (fun x -> x.width <- "11px") [])),
                        Button.IconPosition.Left
                    )
                Props =
                    fun x ->
                        x.onClick <- fun _ -> promise { do! input.OnClick () }
                        x.alignSelf <- "stretch"
                        x.backgroundColor <- "whiteAlpha.100"
                        x.borderRadius <- "2px"
                Children =
                    [
                        str input.Label
                    ]
            |}
