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

    let menuCheckbox (input: {| TreeId: TreeId; Active: bool |}) =
        Chakra.menuItemOption
            {| value = input.TreeId |}
            [
                if input.Active then
                    str "Active: "
                else
                    nothing
                treeNameComponent {| TreeId = input.TreeId |}
            ]

    let render =
        React.memo (fun (input: {| Username: Username |}) ->
            let treeSelectionIds, setTreeSelectionIds = Recoil.useState Recoil.Atoms.treeSelectionIds
            let availableTreeIds = Recoil.useValue (Recoil.Atoms.Session.availableTreeIds input.Username)

            let treeSelectionIdsArray = treeSelectionIds |> Array.toList |> List.toArray
            let treeSelectionIdsSet = treeSelectionIds |> Set.ofArray
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
                                            _value = treeSelectionIdsArray
                                            onChange =
                                                fun (treeSelection: TreeId []) ->
                                                    treeSelection
                                                    |> fun x ->
                                                        printfn "onChange treeSelection: %A" x
                                                        x
                                                    |> setTreeSelectionIds
                                        |}
                                        [
                                            yield! availableTreeIds
                                                   |> List.map (fun treeId ->
                                                       menuCheckbox
                                                           {|
                                                               TreeId = treeId
                                                               Active = treeSelectionIdsSet.Contains treeId
                                                           |})
                                        ]
                                ]
                        ]
                ])

(*
<Menu>
  <MenuButton as={Button} rightIcon={<ChevronDownIcon />}>
    Your Cats
  </MenuButton>
  <MenuList>
    <MenuItem minH="48px">
      <Image
        boxSize="2rem"
        borderRadius="full"
        src="https://placekitten.com/100/100"
        alt="Fluffybuns the destroyer"
        mr="12px"
      />
      <span>Fluffybuns the Destroyer</span>
    </MenuItem>
    <MenuItem minH="40px">
      <Image
        boxSize="2rem"
        borderRadius="full"
        src="https://placekitten.com/120/120"
        alt="Simon the pensive"
        mr="12px"
      />
      <span>Simon the pensive</span>
    </MenuItem>
  </MenuList>
</Menu>
*)
