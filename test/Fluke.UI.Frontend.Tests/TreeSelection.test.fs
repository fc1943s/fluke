namespace Fluke.UI.Frontend.Tests

open System
open Fable.ReactTestingLibrary
open Fable.Jester
open Feliz.Recoil
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Recoil
open Fluke.Shared
open FSharpPlus

module TreeSelection =
    open Model
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State
    open View
    open Templates


    Jest.describe
        ("tree selection",
         (fun () ->
             let user = Setup.getUser ()

             let position1 = FlukeDateTime.FromDateTime DateTime.MinValue
             let position2 = FlukeDateTime.FromDateTime DateTime.MaxValue

             let treeList =
                 [
                     TreeId (Guid "F8488D67-0305-40AA-8CAB-AF46E486F1DC"), Some position1
                     TreeId (Guid "63F40B3D-5B5F-419C-ADA2-5A5CDCA180DD"), Some position1
                     TreeId (Guid "43B942D6-880E-4788-A6B2-EB670CD48B14"), Some position2
                     TreeId (Guid "4F2CEB09-F646-4EFE-AC34-A3BDA24DDB71"), None
                     TreeId (Guid "C1D06F9A-154A-4BBC-9D7F-0C78C6C44C5C"), None
                 ]

             let queryMenuItems (subject: Bindings.render<_, _>) =
                 treeList
                 |> List.map fst
                 |> List.map (fun (TreeId guid) -> subject.queryByTestId ("menu-item-" + guid.ToString ()))
                 |> List.toArray

             let testMenuItemsVisibility array menuItems =
                 menuItems
                 |> Array.map Option.isSome
                 |> fun menuItemsVisibility -> Jest.expect(menuItemsVisibility).toEqual array

             let initialSetter (setter: CallbackMethods) =
                 promise {
                     setter.set (Atoms.Session.availableTreeIds user.Username, treeList |> List.map fst)
                     treeList
                     |> List.iter (fun (treeId, position) -> setter.set (Atoms.Tree.position treeId, position))
                 }

             let treeSelector = TreeSelectorComponent.render {| Username = user.Username |}

             Jest.test
                 ("tree list updates correctly with user clicks",
                  promise {
                      let! subject, peek = treeSelector |> Setup.render
                      do! peek initialSetter

                      let menuItems = queryMenuItems subject

                      menuItems
                      |> testMenuItemsVisibility [|
                          true
                          true
                          true
                          true
                          true
                         |]

                      RTL.fireEvent.click menuItems.[2].Value

                      let menuItems = queryMenuItems subject

                      menuItems
                      |> testMenuItemsVisibility [|
                          false
                          false
                          true
                          false
                          false
                         |]

                      RTL.fireEvent.click menuItems.[2].Value

                      let menuItems = queryMenuItems subject

                      menuItems
                      |> testMenuItemsVisibility [|
                          true
                          true
                          true
                          true
                          true
                         |]

                      RTL.fireEvent.click menuItems.[1].Value

                      let menuItems = queryMenuItems subject

                      menuItems
                      |> testMenuItemsVisibility [|
                          true
                          true
                          false
                          false
                          false
                         |]

                      RTL.fireEvent.click menuItems.[0].Value

                      let menuItems = queryMenuItems subject

                      menuItems
                      |> testMenuItemsVisibility [|
                          true
                          true
                          false
                          false
                          false
                         |]

                      RTL.fireEvent.click menuItems.[1].Value

                      let menuItems = queryMenuItems subject

                      menuItems
                      |> testMenuItemsVisibility [|
                          true
                          true
                          false
                          false
                          false
                         |]

                      RTL.fireEvent.click menuItems.[0].Value

                      let menuItems = queryMenuItems subject

                      menuItems
                      |> testMenuItemsVisibility [|
                          true
                          true
                          true
                          true
                          true
                         |]

                      RTL.fireEvent.click menuItems.[3].Value

                      let menuItems = queryMenuItems subject

                      menuItems
                      |> testMenuItemsVisibility [|
                          false
                          false
                          false
                          true
                          true
                         |]

                      RTL.fireEvent.click menuItems.[4].Value

                      let menuItems = queryMenuItems subject

                      menuItems
                      |> testMenuItemsVisibility [|
                          false
                          false
                          false
                          true
                          true
                         |]
                  })

             Jest.test
                 ("tree list populated correctly with initial data",
                  promise {
                      let! subject, peek = treeSelector |> Setup.render
                      do! peek initialSetter

                      do! peek (fun setter ->
                              promise {
                                  setter.set
                                      (Atoms.treeSelectionIds,
                                       [|
                                           fst treeList.Head
                                       |])

                                  setter.set (Atoms.selectedPosition, snd treeList.Head)
                              })

                      let menuItems = queryMenuItems subject

                      menuItems
                      |> testMenuItemsVisibility [|
                          true
                          true
                          false
                          false
                          false
                         |]
                  })

             ()))
