namespace Fluke.UI.Frontend.Tests

open Fable.ReactTestingLibrary
open Fable.Jester
open Fluke.UI.Frontend.Components
open Fluke.Shared.Domain
open Feliz.Recoil

module Temp =
    module Testing =
        let render children =
            RTL.render
                (Recoil.root
                    [
                        root.children
                            [
                                children
                            ]
                    ])


    Jest.test
        ("TreeSelector",
         async {
             let treeSelector =
                 TreeSelectorComponent.render {| Username = UserInteraction.Username "hi" |}
                 |> Testing.render

             ()
         })
