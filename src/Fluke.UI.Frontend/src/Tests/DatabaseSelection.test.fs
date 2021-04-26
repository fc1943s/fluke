namespace Fluke.UI.Frontend.Tests

open System
open Fable.ReactTestingLibrary
open Fable.Jester
open Feliz.Recoil
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Tests.Core
open Fluke.UI.Frontend.Recoil
open Fluke.Shared
open Fluke.Shared.Domain

module DatabaseSelection =
    open State
    open TempData

    [<RequireQualifiedAccess>]
    type MenuItemStatus =
        | Disabled
        | Enabled

    Jest.describe (
        "database selection",
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
                |> Array.map
                    (fun (el: Browser.Types.HTMLElement option) ->
                        match el with
                        | None -> MenuItemStatus.Disabled
                        | Some el ->
                            match el.getAttribute "data-disabled" |> Option.ofObj with
                            | Some _ -> MenuItemStatus.Disabled
                            | None -> MenuItemStatus.Enabled)

                |> fun menuItemsVisibility -> Jest.expect(menuItemsVisibility).toEqual array

            let initialSetter (setter: CallbackMethods) =
                promise {
                    setter.set (Atoms.Session.availableDatabaseIds testUser.Username, databaseList |> List.map fst)

                    databaseList
                    |> List.iter
                        (fun (databaseId, position) -> setter.set (Atoms.Database.position (Some databaseId), position))
                }

            let getDatabaseSelector () =
                Databases.Databases
                    {|
                        Username = testUser.Username
                        Props =
                            JS.newObj
                                (fun x ->
                                    x.flex <- 1
                                    x.overflowY <- "auto"
                                    x.flexBasis <- 0)
                    |}

            let click el =
                promise {
                    RTL.fireEvent.click el
                    do! RTL.waitFor id
                }

            Jest.beforeEach (
                promise {
                    printfn "Before each"
                    Browser.Dom.window.localStorage.clear ()
                }
            )

            Jest.test (
                "database list updates correctly with user clicks",
                promise {
                    let! subject, peek = getDatabaseSelector () |> Setup.render
                    do! peek initialSetter

                    let menuItems = queryMenuItems subject

                    menuItems
                    |> testMenuItemsState [|
                        MenuItemStatus.Enabled
                        MenuItemStatus.Enabled
                        MenuItemStatus.Enabled
                        MenuItemStatus.Enabled
                        MenuItemStatus.Enabled
                       |]

                    do! click menuItems.[2].Value

                    let menuItems = queryMenuItems subject

                    menuItems
                    |> testMenuItemsState [|
                        MenuItemStatus.Disabled
                        MenuItemStatus.Disabled
                        MenuItemStatus.Enabled
                        MenuItemStatus.Disabled
                        MenuItemStatus.Disabled
                       |]

                    do! click menuItems.[2].Value

                    let menuItems = queryMenuItems subject

                    menuItems
                    |> testMenuItemsState [|
                        MenuItemStatus.Enabled
                        MenuItemStatus.Enabled
                        MenuItemStatus.Enabled
                        MenuItemStatus.Enabled
                        MenuItemStatus.Enabled
                       |]

                    do! click menuItems.[1].Value

                    let menuItems = queryMenuItems subject

                    menuItems
                    |> testMenuItemsState [|
                        MenuItemStatus.Enabled
                        MenuItemStatus.Enabled
                        MenuItemStatus.Disabled
                        MenuItemStatus.Disabled
                        MenuItemStatus.Disabled
                       |]

                    do! click menuItems.[0].Value

                    let menuItems = queryMenuItems subject

                    menuItems
                    |> testMenuItemsState [|
                        MenuItemStatus.Enabled
                        MenuItemStatus.Enabled
                        MenuItemStatus.Disabled
                        MenuItemStatus.Disabled
                        MenuItemStatus.Disabled
                       |]

                    do! click menuItems.[1].Value

                    let menuItems = queryMenuItems subject

                    menuItems
                    |> testMenuItemsState [|
                        MenuItemStatus.Enabled
                        MenuItemStatus.Enabled
                        MenuItemStatus.Disabled
                        MenuItemStatus.Disabled
                        MenuItemStatus.Disabled
                       |]

                    do! click menuItems.[0].Value

                    let menuItems = queryMenuItems subject

                    menuItems
                    |> testMenuItemsState [|
                        MenuItemStatus.Enabled
                        MenuItemStatus.Enabled
                        MenuItemStatus.Enabled
                        MenuItemStatus.Enabled
                        MenuItemStatus.Enabled
                       |]

                    do! click menuItems.[3].Value

                    let menuItems = queryMenuItems subject

                    menuItems
                    |> testMenuItemsState [|
                        MenuItemStatus.Disabled
                        MenuItemStatus.Disabled
                        MenuItemStatus.Disabled
                        MenuItemStatus.Enabled
                        MenuItemStatus.Enabled
                       |]

                    do! click menuItems.[4].Value

                    let menuItems = queryMenuItems subject

                    menuItems
                    |> testMenuItemsState [|
                        MenuItemStatus.Disabled
                        MenuItemStatus.Disabled
                        MenuItemStatus.Disabled
                        MenuItemStatus.Enabled
                        MenuItemStatus.Enabled
                       |]
                }
            )

            Jest.test (
                "database list populated correctly with initial data",
                promise {
                    let! subject, peek = getDatabaseSelector () |> Setup.render
                    do! peek initialSetter

                    do!
                        peek
                            (fun setter ->
                                promise {
                                    setter.set (
                                        Atoms.selectedDatabaseIds,
                                        [|
                                            fst databaseList.Head
                                        |]
                                    )

                                    setter.set (Atoms.position, snd databaseList.Head)
                                })

                    let menuItems = queryMenuItems subject

                    menuItems
                    |> testMenuItemsState [|
                        MenuItemStatus.Enabled
                        MenuItemStatus.Enabled
                        MenuItemStatus.Disabled
                        MenuItemStatus.Disabled
                        MenuItemStatus.Disabled
                       |]
                }
            )

            ())
    )
