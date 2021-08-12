namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend
open FsStore
open FsStore.Model
open FsUi.Bindings
open FsUi.Components


module DockPanel =
    [<RequireQualifiedAccess>]
    type DockPanelIcon =
        | Component of cmp: ReactElement
        | Menu of title: string * icon: ReactElement * children: seq<ReactElement>

    [<ReactComponent>]
    let DockPanel
        (input: {| Name: string
                   Icon: obj
                   Atom: Atom<TempUI.DockType option>
                   RightIcons: DockPanelIcon list
                   children: seq<ReactElement> |})
        =
        let setAtom = Store.useSetState input.Atom

        Ui.flex
            (fun x ->
                x.flexDirection <- "column"
                x.overflow <- "hidden"
                x.flex <- "1")
            [
                Ui.flex
                    (fun x ->
                        x.paddingLeft <- "9px"
                        x.paddingTop <- "1px"
                        x.paddingRight <- "1px"
                        x.marginLeft <- "1px"
                        x.borderBottomWidth <- "1px"
                        x.borderBottomColor <- "gray.16"
                        x.alignItems <- "center")
                    [
                        Ui.icon
                            (fun x ->
                                x.``as`` <- input.Icon
                                x.marginRight <- "6px")
                            []
                        str input.Name

                        Ui.spacer (fun _ -> ()) []

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
                                                                    x.``as`` <- Ui.react.MenuButton
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

                Ui.flex
                    (fun x ->
                        x.flexDirection <- "column"
                        x.zIndex <- 2
                        x.flex <- "1 1 0")
                    [
                        yield! input.children
                    ]
            ]
