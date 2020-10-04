namespace Fluke.UI.Frontend.Tests

open System
open System.Text.RegularExpressions
open Fable.ReactTestingLibrary
open Fable.Jester
open Fable.React
open Fable.React.Helpers
open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Components
open Fluke.Shared.Domain
open Fluke.Shared

module Temp =
    module ReactErrorBoundary =

        [<AllowNullLiteral>]
        type InfoComponentObject =
            abstract componentStack: string

        type ErrorBoundaryProps =
            {
                Inner: ReactElement
                ErrorComponent: ReactElement
                OnError: exn * InfoComponentObject -> unit
            }

        type ErrorBoundaryState = { HasErrors: bool }

        type ErrorBoundary (props) =
            inherit Component<ErrorBoundaryProps, ErrorBoundaryState>(props)
            do base.setInitState ({ HasErrors = false })

            override x.componentDidCatch (error, info) =
                let info = info :?> InfoComponentObject
                x.props.OnError (error, info)
                x.setState (fun state _props -> { state with HasErrors = true })

            override x.render () =
                if (x.state.HasErrors) then
                    x.props.ErrorComponent
                else
                    x.props.Inner

        let renderCatchSimple errorElement element =
            ofType<ErrorBoundary, _, _>
                {
                    Inner = element
                    ErrorComponent = errorElement
                    OnError = fun _ -> ()
                }
                []

        let renderCatchFn onError errorElement element =
            ofType<ErrorBoundary, _, _>
                {
                    Inner = element
                    ErrorComponent = errorElement
                    OnError = onError
                }
                []

    module Testing =

        let componentWrapper (initializer: MutableSnapshot -> unit) children =
            React.memo (fun () ->
                React.strictMode
                    [
                        Recoil.root [
                            root.init initializer
                            root.children children
                        ]
                    ]
                |> ReactErrorBoundary.renderCatchFn (fun (error, info) ->
                    printfn "ERROR %A %A" info.componentStack error) (str "error"))


        let render initializer cmp =
            RTL.render
                ((componentWrapper
                    initializer
                      [
                          cmp
                      ]) ())

        let flush () =
            promise {
                RTL.act Jest.runAllTimers
                do! Promise.sleep 100
            }


    //export function flushPromisesAndTimers() {
//  return new Promise(resolve => {
//    setTimeout(resolve, 100);
//    act(() => jest.runAllTimers());
//  });
//}

    Jest.describe
        ("TreeSelector",
         (fun () ->
             let ofTreeId (State.TreeId guid) = guid.ToString ()

             let users = TempData.getUsers ()
             let user = users.fluke

             let position1 = UserInteraction.FlukeDateTime.FromDateTime DateTime.MinValue
             let position2 = UserInteraction.FlukeDateTime.FromDateTime DateTime.MaxValue

             let treeList =
                 [
                     State.TreeId (Guid "F8488D67-0305-40AA-8CAB-AF46E486F1DC"), Some position1
                     State.TreeId (Guid "63F40B3D-5B5F-419C-ADA2-5A5CDCA180DD"), Some position1
                     State.TreeId (Guid "43B942D6-880E-4788-A6B2-EB670CD48B14"), Some position2
                     State.TreeId (Guid "4F2CEB09-F646-4EFE-AC34-A3BDA24DDB71"), None
                     State.TreeId (Guid "C1D06F9A-154A-4BBC-9D7F-0C78C6C44C5C"), None
                 ]

             let treeSelector = TreeSelectorComponent.render {| Username = users.fluke.Username |}

             let baseInitializer (initializer: MutableSnapshot) =
                 initializer.set (Recoil.Atoms.Session.availableTreeIds user.Username, treeList |> List.map fst)

                 treeList
                 |> List.iter (fun (treeId, position) -> initializer.set (Recoil.Atoms.Tree.position treeId, position))

             let queryMenuItems (subject: Bindings.render<_, _>) =
                 treeList
                 |> List.map fst
                 |> List.map (fun (State.TreeId guid) -> subject.queryByTestId ("menu-item-" + guid.ToString ()))
                 |> List.toArray

             let testMenuItemsVisibility array menuItems =
                 menuItems
                 |> Array.map Option.isSome
                 |> fun menuItemsVisibility -> Jest.expect(menuItemsVisibility).toEqual array

             Jest.test
                 ("1",
                  promise {
                      let subject = treeSelector |> Testing.render baseInitializer

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
                 ("2",
                  promise {
                      let subject =
                          treeSelector
                          |> Testing.render (fun initializer ->
                              baseInitializer initializer
                              initializer.set
                                  (Recoil.Atoms.treeSelectionIds,
                                   [|
                                       fst treeList.Head
                                   |])

                              initializer.set (Recoil.Atoms.selectedPosition, snd treeList.Head))

                      let menuItems = queryMenuItems subject

                      menuItems
                      |> testMenuItemsVisibility [|
                          true
                          true
                          false
                          false
                          false
                         |]
                  })))
