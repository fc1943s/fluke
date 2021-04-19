namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings


module TopBar =
    [<ReactComponent>]
    let TopBar () =
        let logout = Auth.useLogout ()

        Chakra.flex
            (fun x ->
                x.height <- "31px"
                x.backgroundColor <- "gray.10"
                x.align <- "center"
                x.padding <- "7px")
            [
                Logo.Logo ()

                Chakra.box
                    (fun x -> x.marginLeft <- "4px")
                    [
                        str "Fluke"
                    ]

                AddDatabaseButton.AddDatabaseButton
                    {|
                        Props = JS.newObj (fun x -> x.marginLeft <- "37px")
                    |}
                AddTaskButton.AddTaskButton
                    {|
                        Props = JS.newObj (fun x -> x.marginLeft <- "10px")
                    |}

                Chakra.spacer (fun _ -> ()) []

                Chakra.stack
                    (fun x ->
                        x.spacing <- "10px"
                        x.direction <- "row")
                    [
                        Chakra.link
                            (fun x ->
                                x.href <- "https://github.com/fc1943s/fluke"
                                x.isExternal <- true)
                            [

                                Tooltip.wrap
                                    (ExternalLink.ExternalLink
                                        {|
                                            Text = "GitHub repository"
                                            Props = JS.newObj (fun _ -> ())
                                        |})
                                    [
                                        Chakra.iconButton
                                            (fun x ->
                                                x.icon <- Icons.aiOutlineGithub ()
                                                x.backgroundColor <- "transparent"
                                                x.variant <- "outline"
                                                x.border <- "0"
                                                x.fontSize <- "18px"
                                                x.width <- "30px"
                                                x.height <- "30px"
                                                x.borderRadius <- 0
                                                x.onClick <- logout)
                                            []
                                    ]
                            ]

                        Tooltip.wrap
                            (str "Logout")
                            [
                                Chakra.iconButton
                                    (fun x ->
                                        x.icon <- Icons.fiLogOut ()
                                        x.backgroundColor <- "transparent"
                                        x.variant <- "outline"
                                        x.border <- "0"
                                        x.fontSize <- "18px"
                                        x.width <- "30px"
                                        x.height <- "30px"
                                        x.borderRadius <- 0
                                        x.onClick <- logout)
                                    []
                            ]
                    ]
            ]
