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
            let (TreeName treeName) = Recoil.useValue (Recoil.Atoms.RecoilTree.nameFamily input.TreeId)
            str treeName)

    let menuCheckbox (input: {| TreeId: TreeId |}) =
        Chakra.menuItemOption
            {| value = input.TreeId |}
            [
                treeNameComponent {| TreeId = input.TreeId |}
            ]

    let render =
        React.memo (fun (input: {| Username: Username |}) ->
            let session = Recoil.useValue (Recoil.Atoms.RecoilSession.sessionFamily input.Username)
            let treeSelectionIds, setTreeSelectionIds = Recoil.useState session.TreeSelectionIds
            let availableTreeIds = Recoil.useValue session.AvailableTreeIds

            let treeSelectionIdsArray = treeSelectionIds |> Set.toArray

            printfn "treeSelectionIdsArray %A" treeSelectionIdsArray
            printfn "availableTreeIds %A" availableTreeIds

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
                                            value = treeSelectionIdsArray
                                            onChange =
                                                fun (treeSelection: TreeId []) ->
                                                    treeSelection
                                                    |> Set.ofArray
                                                    |> fun x ->
                                                        printfn "onChange treeSelection: %A" x
                                                        x
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
