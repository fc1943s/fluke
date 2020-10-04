namespace Fluke.UI.Frontend.Components

open Fluke.Shared
open Feliz
open Fable.React
open Feliz.Recoil
open FSharpPlus
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module TreeSelectorComponent =
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State

    let menuItemComponent =
        React.memo (fun (input: {| Username: Username; TreeId: TreeId |}) ->
            let (TreeId treeId) = input.TreeId

            let selectedPosition, setSelectedPosition = Recoil.useState (Recoil.Atoms.selectedPosition)
            let (TreeName treeName) = Recoil.useValue (Recoil.Atoms.Tree.name input.TreeId)
            let treePosition = Recoil.useValue (Recoil.Atoms.Tree.position input.TreeId)
            let availableTreeIds = Recoil.useValue (Recoil.Atoms.Session.availableTreeIds input.Username)

            let treeSelectionIds, setTreeSelectionIds = Recoil.useState Recoil.Atoms.treeSelectionIds

            let availableTreeIdsSet = availableTreeIds |> Set.ofList

            let treeSelectionIdsSet =
                treeSelectionIds
                |> Set.ofArray
                |> Set.intersect availableTreeIdsSet

            let selected = treeSelectionIdsSet.Contains input.TreeId

            printfn "availableTreeIds: %A" availableTreeIds
            printfn "treeSelectionIds: %A" treeSelectionIds
            printfn "treeSelectionIdsSet: %A" treeSelectionIdsSet

            let onChange = fun (_e: {| target: obj |}) -> ()

            let onClick =
                fun (e: {| target: Browser.Types.HTMLElement |}) ->
                    if JS.instanceof (e.target, nameof Browser.Types.HTMLInputElement) then
                        let swap value set =
                            if set |> Set.contains value then
                                set |> Set.remove value
                            else
                                set |> Set.add value

                        let newTreeSelectionIds =
                            treeSelectionIdsSet
                            |> swap input.TreeId
                            |> Set.toArray

                        setTreeSelectionIds newTreeSelectionIds

                        match newTreeSelectionIds with
                        | [||] -> None
                        | _ -> treePosition
                        |> setSelectedPosition

            let (|RenderCheckbox|HideCheckbox|) (selectedPosition, treePosition) =
                match selectedPosition, treePosition with
                | None, None -> RenderCheckbox
                | None, Some _ when treeSelectionIdsSet.IsEmpty -> RenderCheckbox
                | Some _, Some _ when selectedPosition = treePosition -> RenderCheckbox
                | _ -> HideCheckbox

            match selectedPosition, treePosition with
            | RenderCheckbox ->
                Chakra.menuItem
                    ()
                    [
                        Chakra.checkbox
                            {|
                                ``data-testid`` = "menu-item-" + treeId.ToString ()
                                value = input.TreeId
                                isChecked = selected
                                onClick = onClick
                                onChange = onChange
                            |}
                            [
                                str treeName
                            ]
                    ]
            | _ -> nothing)


    let render =
        React.memo (fun (input: {| Username: Username |}) ->
            let availableTreeIds = Recoil.useValue (Recoil.Atoms.Session.availableTreeIds input.Username)

            Chakra.box
                {| position = "relative" |}
                [
                    Chakra.menu
                        {| closeOnSelect = false; autoSelect = false |}
                        [
                            Chakra.menuButton
                                {|
                                    ``as`` = Chakra.core.Button
                                    colorScheme = "black"
                                |}
                                [
                                    str "TreeSelector"
                                ]

                            Chakra.menuList
                                {| height = "500px"; overflowY = "scroll" |}
                                [
                                    yield! availableTreeIds
                                           |> List.map (fun treeId ->
                                               menuItemComponent {| Username = input.Username; TreeId = treeId |})
                                ]

                        ]
                ])
