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
        React.memo (fun (input: {| Username: Username; TreeId: TreeId |}) ->
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

            let onChange = fun (_e: {| target: obj |}) -> ()

            let onClick =
                fun (e: {| target: Browser.Types.HTMLElement |}) ->
                    if Ext.JS.instanceOf (e.target, nameof Browser.Types.HTMLInputElement) then
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

            let (|RenderCheckbox|HideCheckbox|) =
                function
                | None, None -> RenderCheckbox
                | None, Some _ when treeSelectionIds.Length = 0 -> RenderCheckbox
                | Some _, Some _ when selectedPosition = treePosition -> RenderCheckbox
                | _ -> HideCheckbox

            match selectedPosition, treePosition with
            | RenderCheckbox ->
                Chakra.menuItem
                    ()
                    [
                        Chakra.checkbox
                            {|
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
                                               menuItemComponent {| Username = input.Username; TreeId = treeId |})
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
