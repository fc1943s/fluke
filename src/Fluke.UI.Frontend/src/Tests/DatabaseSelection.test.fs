namespace Fluke.UI.Frontend.Tests

open System
open Fable.ReactTestingLibrary
open Fable.Jester
open Feliz.Recoil
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Tests.Core
open Fluke.UI.Frontend.Recoil
open Fluke.Shared
open Fluke.Shared.Domain
open FSharpPlus

module DatabaseSelection =
    open State
    open TempData


    Jest.describe
        ("database selection",
         (fun () ->
             let position1 = FlukeDateTime.FromDateTime DateTime.MinValue
             let position2 = FlukeDateTime.FromDateTime DateTime.MaxValue

             let databaseList =
                 [
                     DatabaseId (Guid "F8488D67-0305-40AA-8CAB-AF46E486F1DC"), Some position1
                     DatabaseId (Guid "63F40B3D-5B5F-419C-ADA2-5A5CDCA180DD"), Some position1
                     DatabaseId (Guid "43B942D6-880E-4788-A6B2-EB670CD48B14"), Some position2
                     DatabaseId (Guid "4F2CEB09-F646-4EFE-AC34-A3BDA24DDB71"), None
                     DatabaseId (Guid "C1D06F9A-154A-4BBC-9D7F-0C78C6C44C5C"), None
                 ]

             let queryMenuItems (subject: Bindings.render<_, _>) =
                 databaseList
                 |> List.map fst
                 |> List.map (fun (DatabaseId guid) -> subject.queryByTestId ("menu-item-" + guid.ToString ()))
                 |> List.toArray

             let testMenuItemsState array menuItems =
                 menuItems
                 |> Array.map (fun (el: Browser.Types.HTMLElement option) ->
                     match el with
                     | None -> false
                     | Some el ->
                         match el.getAttribute "data-disabled" |> Option.ofObj with
                         | Some _ -> false
                         | None -> true)

                 |> fun menuItemsVisibility -> Jest.expect(menuItemsVisibility).toEqual array

             let initialSetter (setter: CallbackMethods) =
                 promise {
                     setter.set (Atoms.Session.availableDatabaseIds testUser.Username, databaseList |> List.map fst)

                     databaseList
                     |> List.iter (fun (databaseId, position) -> setter.set (Atoms.Database.position databaseId, position))
                 }

             let getDatabaseSelector () =
                 Databases.Databases
                     {|
                         Username = testUser.Username
                         Props = {| flex = 1; overflowY = "auto"; flexBasis = 0 |}
                     |}

             Jest.test
                 ("database list updates correctly with user clicks",
                  promise {
                      let! subject, peek = getDatabaseSelector () |> Setup.render
                      do! peek initialSetter

                      let menuItems = queryMenuItems subject

                      menuItems
                      |> testMenuItemsState [|
                          true
                          true
                          true
                          true
                          true
                         |]

                      RTL.fireEvent.click menuItems.[2].Value

                      let menuItems = queryMenuItems subject

                      menuItems
                      |> testMenuItemsState [|
                          false
                          false
                          true
                          false
                          false
                         |]

                      RTL.fireEvent.click menuItems.[2].Value

                      let menuItems = queryMenuItems subject

                      menuItems
                      |> testMenuItemsState [|
                          true
                          true
                          true
                          true
                          true
                         |]

                      RTL.fireEvent.click menuItems.[1].Value

                      let menuItems = queryMenuItems subject

                      menuItems
                      |> testMenuItemsState [|
                          true
                          true
                          false
                          false
                          false
                         |]

                      RTL.fireEvent.click menuItems.[0].Value

                      let menuItems = queryMenuItems subject

                      menuItems
                      |> testMenuItemsState [|
                          true
                          true
                          false
                          false
                          false
                         |]

                      RTL.fireEvent.click menuItems.[1].Value

                      let menuItems = queryMenuItems subject

                      menuItems
                      |> testMenuItemsState [|
                          true
                          true
                          false
                          false
                          false
                         |]

                      RTL.fireEvent.click menuItems.[0].Value

                      let menuItems = queryMenuItems subject

                      menuItems
                      |> testMenuItemsState [|
                          true
                          true
                          true
                          true
                          true
                         |]

                      RTL.fireEvent.click menuItems.[3].Value

                      let menuItems = queryMenuItems subject

                      menuItems
                      |> testMenuItemsState [|
                          false
                          false
                          false
                          true
                          true
                         |]

                      RTL.fireEvent.click menuItems.[4].Value

                      let menuItems = queryMenuItems subject

                      menuItems
                      |> testMenuItemsState [|
                          false
                          false
                          false
                          true
                          true
                         |]
                  })

             Jest.test
                 ("database list populated correctly with initial data",
                  promise {
                      let! subject, peek = getDatabaseSelector () |> Setup.render
                      do! peek initialSetter

                      do! peek (fun setter ->
                              promise {
                                  setter.set
                                      (Atoms.selectedDatabaseIds,
                                       [|
                                           fst databaseList.Head
                                       |])

                                  setter.set (Atoms.selectedPosition, snd databaseList.Head)
                              })

                      let menuItems = queryMenuItems subject

                      menuItems
                      |> testMenuItemsState [|
                          true
                          true
                          false
                          false
                          false
                         |]
                  })

             ()))
