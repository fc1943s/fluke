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

    let menuItemComponent =
        React.memo (fun (input: {| TreeId: TreeId |}) ->
            let (TreeName treeName) = Recoil.useValue (Recoil.Atoms.Tree.name input.TreeId)
            //            let position = Recoil.useValue (Recoil.Atoms.Tree.position input.TreeId)

            let treeSelectionIds, setTreeSelectionIds = Recoil.useState Recoil.Atoms.treeSelectionIds
            let treeSelectionIdsSet = treeSelectionIds |> Set.ofArray

            let selected = treeSelectionIdsSet.Contains input.TreeId

            Chakra.checkbox
                {|
                    value = input.TreeId
                    disabled = false
                    isChecked = selected
                    onClick =
                        fun (e: {| target: Browser.Types.HTMLElement |}) ->
                            if Ext.JS.instanceOf (e.target, nameof Browser.Types.HTMLInputElement) then
                                let swap value set =
                                    if set |> Set.contains value then
                                        set |> Set.remove value
                                    else
                                        set |> Set.add value

                                treeSelectionIdsSet
                                |> swap input.TreeId
                                |> Set.toArray
                                |> setTreeSelectionIds
                    onChange = fun (_e: {| target: obj |}) -> ()
                |}
                [
                    str treeName
                ])

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
                                    ``as`` = Chakra.chakraCore.Button
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
                                               Chakra.menuItem
                                                   ()
                                                   [
                                                       menuItemComponent {| TreeId = treeId |}
                                                   ])
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
