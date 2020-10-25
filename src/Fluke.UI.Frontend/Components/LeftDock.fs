namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open FSharpPlus
open Fluke.UI.Frontend.Model


module LeftDock =
    open Domain.Model
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun (input: {| Username: Username |}) ->
            let leftDock = Recoil.useValue Recoil.Atoms.leftDock

            let items =
                [
                    DockType.Settings,
                    ("Settings",
                     Icons.md.MdSettings,
                     (fun () ->
                         Settings.render
                             {|
                                 Props = {| flex = 1; overflowY = "auto"; flexBasis = 0 |}
                             |}))

                    DockType.Databases,
                    ("Databases",
                     Icons.fi.FiDatabase,
                     (fun () ->
                         TreeSelector.render
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
                                    yield! items
                                           |> List.map (fun (dockType, (name, icon, _)) ->
                                               DockButton.render
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
                        let name, icon, content = itemsMap.[leftDock]
                        Chakra.flex
                            {|
                                width = "300px"
                                borderRightColor = "gray.16%"
                                borderRight = "1px solid"
                                flex = 1
                            |}
                            [
                                DockPanel.render
                                    {|
                                        Name = name
                                        Icon = icon
                                        Atom = Recoil.Atoms.leftDock
                                        Children =
                                            [
                                                content ()
                                            ]
                                    |}
                            ]
                ])
