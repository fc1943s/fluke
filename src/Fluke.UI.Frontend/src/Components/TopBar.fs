namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings
open Fable.Core
open Fluke.Shared

module TopBarIconButton =
    let inline TopBarIconButton (input: {| Props: Chakra.IChakraProps |}) =
        Chakra.iconButton
            (fun x ->
                x <+ input.Props
                x.backgroundColor <- "transparent"
                x.variant <- "outline"
                x.border <- "0"
                x.fontSize <- "18px"
                x.width <- "30px"
                x.height <- "30px"
                x.borderRadius <- "0")
            []

module TopBar =
    [<ReactComponent>]
    let TopBar () =
        let logout = Auth.useLogout ()


        let onLogoClick =
            Recoil.useCallbackRef
                (fun setter _ ->
                    promise {
                        let! username = setter.snapshot.getPromise Recoil.Atoms.username

                        match username with
                        | Some username ->
                            setter.set (Recoil.Atoms.User.leftDock username, None)
                            setter.set (Recoil.Atoms.User.view username, View.View.HabitTracker)
                        | None -> ()
                    })

        Chakra.flex
            (fun x ->
                x.height <- "31px"
                x.align <- "center"
                x.backgroundColor <- "gray.10"
                x.padding <- "7px")
            [

                Chakra.flex
                    (fun x ->
                        x.cursor <- "pointer"
                        x.align <- "center"
                        x.onClick <- onLogoClick)
                    [
                        Logo.Logo ()

                        Chakra.box
                            (fun x -> x.marginLeft <- "4px")
                            [
                                str "Fluke"
                            ]
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
                                        TopBarIconButton.TopBarIconButton
                                            {|
                                                Props =
                                                    JS.newObj
                                                        (fun x -> x.icon <- Icons.ai.AiOutlineGithub |> Icons.render)
                                            |}
                                    ]
                            ]

                        Tooltip.wrap
                            (str "Logout")
                            [
                                TopBarIconButton.TopBarIconButton
                                    {|
                                        Props =
                                            JS.newObj
                                                (fun x ->
                                                    x.icon <- Icons.fi.FiLogOut |> Icons.render
                                                    x.onClick <- logout)
                                    |}
                            ]
                    ]
            ]
