namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.React
open Feliz.Recoil
open FSharpPlus
open Fluke.UI.Frontend
open Fluke.Shared.Domain


module TreeSelectorComponent =
    let render =
        React.memo (fun () ->
            let state = Recoil.useValue Recoil.Atoms.state

            match state with
            | None -> nothing
            | Some state ->
                let treeSelection =
                    state.Session.TreeSelection
                    |> Set.map (fun treeState -> treeState.Id)
                    |> Set.toArray

                Chakra.box
                    {| position = "relative" |}
                    [
                        Chakra.menu
                            {| closeOnSelect = false |}
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
                                                value = treeSelection
                                                onChange =
                                                    fun (treeSelection: State.TreeId []) ->
                                                        printfn "menuoptiongroup onchange %A" treeSelection
                                            |}
                                            [
                                                yield! state.Session.TreeStateMap
                                                       |> Map.values
                                                       |> Seq.map (fun { Id = id; Name = State.TreeName name } ->
                                                           Chakra.menuItemOption
                                                               {| value = id |}
                                                               [
                                                                   str name
                                                               ])
                                            ]
                                    ]
                            ]
                    ])
