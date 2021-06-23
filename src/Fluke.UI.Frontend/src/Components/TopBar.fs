namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings


module TopBar =
    [<ReactComponent>]
    let TopBar () =
        let logout = Auth.useLogout ()

        let onLogoClick =
            Store.useCallback (
                (fun get set _ ->
                    promise {
                        let username = Atoms.getAtomValue get Atoms.username

                        match username with
                        | Some username ->
                            Atoms.setAtomValue set (Atoms.User.leftDock username) None
                            Atoms.setAtomValue set (Atoms.User.rightDock username) None
                            Atoms.setAtomValue set (Atoms.User.view username) TempUI.defaultView
                        | None -> ()
                    }),
                [||]
            )

        Chakra.flex
            (fun x ->
                x.height <- "29px"
                x.alignItems <- "center"
                x.backgroundColor <- "gray.10"
                x.padding <- "7px"
                x.paddingBottom <- "8px")
            [

                Chakra.flex
                    (fun x ->
                        x.cursor <- "pointer"
                        x.alignItems <- "center"
                        x.onClick <- onLogoClick)
                    [
                        Logo.Logo ()

                        Chakra.box
                            (fun x -> x.marginLeft <- "4px")
                            [
                                str "Fluke"
                            ]
                    ]

                Chakra.spacer (fun _ -> ()) []

                Chakra.stack
                    (fun x ->
                        x.spacing <- "10px"
                        x.alignItems <- "center"
                        x.direction <- "row")
                    [
                        Tooltip.wrap
                            (React.fragment [
                                str "GitHub repository"
                                ExternalLink.externalLinkIcon
                             ])
                            [
                                Chakra.link
                                    (fun x ->
                                        x.href <- "https://github.com/fc1943s/fluke"
                                        x.isExternal <- true)
                                    [
                                        TransparentIconButton.TransparentIconButton
                                            {|
                                                Props =
                                                    fun x ->
                                                        x.icon <- Icons.ai.AiOutlineGithub |> Icons.render
                                                        x.height <- "27px"
                                                        x.fontSize <- "17px"
                                            |}
                                    ]
                            ]

                        Tooltip.wrap
                            (str "Logout")
                            [
                                TransparentIconButton.TransparentIconButton
                                    {|
                                        Props =
                                            fun x ->
                                                x.icon <- Icons.fi.FiLogOut |> Icons.render
                                                x.height <- "27px"
                                                x.fontSize <- "17px"
                                                x.onClick <- fun _ -> promise { do! logout () }
                                    |}
                            ]
                    ]
            ]
