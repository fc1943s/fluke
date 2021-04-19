namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module LeftDock =
    open Domain.UserInteraction

    [<ReactComponent>]
    let LeftDock (input: {| Username: Username |}) =
        let leftDock = Recoil.useValue Recoil.Atoms.leftDock

        let items =
            [
                TempUI.DockType.Settings,
                ("Settings",
                 Icons.md.MdSettings,
                 (fun () ->
                     Settings.Settings
                         {|
                             Props =
                                 JS.newObj
                                     (fun x ->
                                         x.flex <- 1
                                         x.overflowY <- "auto"
                                         x.flexBasis <- 0)
                         |}))

                TempUI.DockType.Databases,
                ("Databases",
                 Icons.fi.FiDatabase,
                 (fun () ->
                     Databases.Databases
                         {|
                             Username = input.Username
                             Props =
                                 JS.newObj
                                     (fun x ->
                                         x.flex <- 1
                                         x.overflowY <- "auto"
                                         x.flexBasis <- 0)
                         |}))
            ]

        let itemsMap = items |> Map.ofList

        Chakra.flex
            (fun _ -> ())
            [
                Chakra.box
                    (fun x ->
                        x.width <- "24px"
                        x.position <- "relative")
                    [
                        Chakra.flex
                            (fun x ->
                                x.right <- "0"
                                x.position <- "absolute"
                                x.transform <- "rotate(-90deg) translate(0, -100%)"
                                x.transformOrigin <- "100% 0"
                                x.height <- "24px")
                            [
                                yield!
                                    items
                                    |> List.map
                                        (fun (dockType, (name, icon, _)) ->
                                            DockButton.DockButton
                                                {|
                                                    DockType = dockType
                                                    Name = name
                                                    Icon = icon
                                                    Atom = Recoil.Atoms.leftDock
                                                |})
                            ]
                    ]

                match leftDock with
                | None -> nothing
                | Some leftDock ->
                    match itemsMap |> Map.tryFind leftDock with
                    | None -> nothing
                    | Some (name, icon, content) ->
                        Chakra.flex
                            (fun x ->
                                x.minWidth <- "200px"
                                x.maxWidth <- "300px"
                                x.borderRightWidth <- "1px"
                                x.borderRightColor <- "gray.16"
                                x.flex <- 1)
                            [
                                DockPanel.DockPanel
                                    {|
                                        Name = name
                                        Icon = icon
                                        Atom = Recoil.Atoms.leftDock
                                        children =
                                            [
                                                content ()
                                            ]
                                    |}
                            ]
            ]
