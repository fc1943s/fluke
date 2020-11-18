namespace Fluke.UI.Frontend.Components

open Fluke.Shared
open Feliz
open Fable.React
open Feliz.Recoil
open FSharpPlus
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module TreeSelector =
    open Domain.Model
    open Domain.UserInteraction
    open Domain.State

    module MenuItem =
        let render =
            React.memo (fun (input: {| Username: Username; TreeId: TreeId |}) ->
                let (TreeId treeId) = input.TreeId

                let selectedPosition, setSelectedPosition = Recoil.useState (Recoil.Atoms.selectedPosition)
                let (TreeName treeName) = Recoil.useValue (Recoil.Atoms.Tree.name input.TreeId)
                let treePosition = Recoil.useValue (Recoil.Atoms.Tree.position input.TreeId)
                let availableTreeIds = Recoil.useValue (Recoil.Atoms.Session.availableTreeIds input.Username)

                let treeSelectionIds, setTreeSelectionIds = Recoil.useState Recoil.Atoms.treeSelectionIds

                let availableTreeIdsSet = set availableTreeIds

                let treeSelectionIdsSet =
                    treeSelectionIds
                    |> set
                    |> Set.intersect availableTreeIdsSet

                let selected = treeSelectionIdsSet.Contains input.TreeId

                let changeSelection =
                    Recoil.useCallbackRef (fun setter newTreeSelectionIds ->
                        setTreeSelectionIds newTreeSelectionIds

                        match newTreeSelectionIds with
                        | [||] -> None
                        | _ -> treePosition
                        |> setSelectedPosition)

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

                            changeSelection newTreeSelectionIds

                let (|RenderCheckbox|HideCheckbox|) (selectedPosition, treePosition) =
                    match selectedPosition, treePosition with
                    | None, None -> RenderCheckbox
                    | None, Some _ when treeSelectionIdsSet.IsEmpty -> RenderCheckbox
                    | Some _, Some _ when selectedPosition = treePosition -> RenderCheckbox
                    | _ -> HideCheckbox

                let enabled =
                    match selectedPosition, treePosition with
                    | RenderCheckbox -> true
                    | _ -> false

                Chakra.checkbox
                    {|
                        ``data-testid`` = "menu-item-" + treeId.ToString ()
                        value = input.TreeId
                        isChecked = selected
                        isDisabled =
                            if enabled then
                                None
                            else
                                Some true
                        onClick = onClick
                        onChange = onChange
                    |}
                    [
                        str treeName
                    ])


    let render =
        React.memo (fun (input: {| Username: Username
                                   Props: {| flex: int; overflowY: string; flexBasis: int |} |}) ->
            let availableTreeIds = Recoil.useValue (Recoil.Atoms.Session.availableTreeIds input.Username)

            Chakra.stack
                input.Props
                [
                    yield! availableTreeIds
                           |> List.map (fun treeId -> MenuItem.render {| Username = input.Username; TreeId = treeId |})
                ])
