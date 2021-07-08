namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module DockPanel =
    [<RequireQualifiedAccess>]
    type DockPanelIcon =
        | Component of cmp: ReactElement
        | Menu of title: string * icon: ReactElement * children: seq<ReactElement>

    [<ReactComponent>]
    let DockPanel
        (input: {| Name: string
                   Icon: obj
                   Atom: Store.Atom<TempUI.DockType option>
                   RightIcons: DockPanelIcon list
                   children: seq<ReactElement> |})
        =
        let setAtom = Store.useSetState input.Atom

        UI.stack
            (fun x ->
                x.spacing <- "0"
                x.overflow <- "hidden"
                x.flex <- "1")
            [
                UI.flex
                    (fun x ->
                        x.paddingLeft <- "9px"
                        x.paddingTop <- "1px"
                        x.paddingRight <- "1px"
                        x.marginLeft <- "1px"
                        x.borderBottomWidth <- "1px"
                        x.borderBottomColor <- "gray.16"
                        x.alignItems <- "center")
                    [
                        UI.icon
                            (fun x ->
                                x.``as`` <- input.Icon
                                x.marginRight <- "6px")
                            []
                        str input.Name

                        UI.spacer (fun _ -> ()) []

                        yield!
                            input.RightIcons
                            |> List.map
                                (fun icon ->
                                    match icon with
                                    | DockPanelIcon.Component cmp -> cmp
                                    | DockPanelIcon.Menu (title, icon, menu) ->
                                        Menu.Menu
                                            {|
                                                Tooltip = title
                                                Trigger =
                                                    TransparentIconButton.TransparentIconButton
                                                        {|
                                                            Props =
                                                                fun x ->
                                                                    x.``as`` <- UI.react.MenuButton
                                                                    x.fontSize <- "14px"
                                                                    x.icon <- icon
                                                        |}
                                                Body = menu
                                                MenuListProps = fun _ -> ()
                                            |})

                        Tooltip.wrap
                            (str "Hide")
                            [
                                TransparentIconButton.TransparentIconButton
                                    {|
                                        Props =
                                            fun x ->
                                                x.icon <- Icons.fi.FiMinus |> Icons.render
                                                x.onClick <- fun _ -> promise { setAtom None }
                                    |}
                            ]
                    ]

                UI.flex
                    (fun x ->
                        x.direction <- "column"
                        x.flex <- "1"
                        x.overflow <- "auto"
                        x.flexBasis <- 0)
                    [
                        yield! input.children
                    ]
            ]
