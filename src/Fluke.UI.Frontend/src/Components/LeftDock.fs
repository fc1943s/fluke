namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open FSharpPlus


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
                             Props = {| flex = 1; overflowY = "auto"; flexBasis = 0 |}
                         |}))

                TempUI.DockType.Databases,
                ("Databases",
                 Icons.fi.FiDatabase,
                 (fun () ->
                     Databases.Databases
                         {|
                             Username = input.Username
                             Props = {| flex = 1; overflowY = "auto"; flexBasis = 0 |}
                         |}))
            ]

        let itemsMap = items |> Map.ofList

        Chakra.flex
            ()
            [
                Chakra.box
                    {| width = "24px"; position = "relative" |}
                    [
                        Chakra.flex
                            {|
                                right = 0
                                position = "absolute"
                                transform = "rotate(-90deg) translate(0, -100%)"
                                transformOrigin = "100% 0"
                                height = "24px"
                            |}
                            [
                                yield!
                                    items
                                    |> List.map (fun (dockType, (name, icon, _)) ->
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
                            {|
                                width = "300px"
                                borderRightColor = "gray.16%"
                                borderRight = "1px solid"
                                flex = 1
                            |}
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
