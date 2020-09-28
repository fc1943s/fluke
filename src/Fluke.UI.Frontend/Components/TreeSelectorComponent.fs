namespace Fluke.UI.Frontend.Components

open Fluke.Shared
open Feliz
open Fable.React
open Feliz.Recoil
open FSharpPlus
open Fluke.UI.Frontend


module TreeSelectorComponent =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    let treeNameComponent =
        React.memo (fun (input: {| TreeId: TreeId |}) ->
            let (TreeName treeName) = Recoil.useValue (Recoil.Atoms.Tree.name input.TreeId)
            str treeName)

    let menuCheckbox (input: {| TreeId: TreeId |}) =
        Chakra.menuItemOption
            {| value = input.TreeId |}
            [
                treeNameComponent {| TreeId = input.TreeId |}
            ]

    let render =
        React.memo (fun (input: {| Username: Username |}) ->
            let treeSelectionIds, setTreeSelectionIds = Recoil.useState Recoil.Atoms.treeSelectionIds
            let availableTreeIds = Recoil.useValue (Recoil.Atoms.Session.availableTreeIds input.Username)

            let treeSelectionIdsArray = treeSelectionIds |> Array.toList |> List.toArray
//
//            printfn "TreeSelectorComponent.render -> treeSelectionIdsText = %A" treeSelectionIdsText

            Chakra.box
                {| position = "relative" |}
                [
                    Chakra.menu
                        {| closeOnSelect = false; autoSelect = false |}
                        [
                            Chakra.menuButton
                                {|
                                    ``as`` = Chakra.chakraCore.Button
                                    colorScheme = "black"
                                |}
                                [
                                    str "TreeSelector"
                                ]

                            Chakra.menuList
                                ()
                                [
                                    Chakra.menuOptionGroup
                                        {|
                                            title = "Private"
                                            ``type`` = "checkbox"
                                            initialValue = treeSelectionIdsArray
                                            onChange =
                                                fun (treeSelection: TreeId []) ->
                                                    treeSelection
                                                    |> fun x ->
                                                        printfn "onChange treeSelection: %A" x
                                                        x
//                                                    |> Array.toList
//                                                    |> List.map TreeId
//                                                    |> List.toArray
                                                    |> setTreeSelectionIds
                                        |}
                                        [
                                            yield! availableTreeIds
                                                   |> List.map (fun treeId ->
                                                       //                                                       React.fragment [
                                                       menuCheckbox {| TreeId = treeId |}
                                                       //                                                           menuCheckboxTest {| TreeId = treeId |}
//                                                           Chakra.menuItemOption
//                                                               {| key=treeId; value = treeId |}
//                                                               [
//                                                                   str <| string treeId
//                                                               ]
//                                                       ]
                                                       )
                                        ]
                                ]
                        ]
                ])
