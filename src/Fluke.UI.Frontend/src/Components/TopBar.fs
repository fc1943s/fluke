namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings
open Fable.Core
open Fluke.Shared


module TopBar =
    [<ReactComponent>]
    let TopBar () =
        let logout = Auth.useLogout ()


        let onLogoClick =
            Recoil.useCallbackRef
                (fun setter _ ->
                    promise {
                        let! username = setter.snapshot.getPromise Atoms.username

                        match username with
                        | Some username ->
                            setter.set (Atoms.User.leftDock username, None)
                            setter.set (Atoms.User.view username, View.View.HabitTracker)
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

                AddTaskButton.AddTaskButton
                    {|
                        Props = JS.newObj (fun x -> x.marginLeft <- "37px")
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
                                        TransparentIconButton.TransparentIconButton
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
                                TransparentIconButton.TransparentIconButton
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
