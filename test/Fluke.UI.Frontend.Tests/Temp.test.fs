namespace Fluke.UI.Frontend.Tests

open System
open Fable.ReactTestingLibrary
open Fable.Jester
open Fable.React
open Feliz
open Feliz.Recoil
open Fluke.Shared.Domain.Information
open Fluke.Shared.Domain.UserInteraction
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

        let componentWrapper (initializer: MutableSnapshot -> unit) cmp =
            React.memo (fun () ->
                React.strictMode
                    [
                        Recoil.root [
                            root.init initializer
                            root.children
                                [
                                    cmp
                                ]
                        ]
                    ]
                |> ReactErrorBoundary.renderCatchFn (fun (error, info) ->
                    printfn "ERROR %A %A" info.componentStack error) (str "error"))


        let render initializer (cmp: ReactElement) =
            let mutable peekFn: (CallbackMethods -> Fable.Core.JS.Promise<unit>) -> Fable.Core.JS.Promise<unit> =
                fun _ -> failwith "called empty callback"

            let cmpWrapper =
                React.memo (fun () ->
                    let peek =
                        Recoil.useCallbackRef (fun (setter: CallbackMethods) (fn: CallbackMethods -> Fable.Core.JS.Promise<unit>) ->
                            promise { do! fn setter })

                    peekFn <- peek

                    cmp)

            let subject = RTL.render ((componentWrapper initializer (cmpWrapper ())) ())

            subject, peekFn

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

    let getUser () =
        let users = TempData.getUsers ()
        users.fluke

    Jest.describe
        ("cell selection",
         (fun () ->
             let user = getUser ()

             let taskList =
                 [
                     { Task.Default with Name = TaskName "1" }
                     { Task.Default with Name = TaskName "2" }
                     { Task.Default with Name = TaskName "3" }
                 ]

             let taskIdList = taskList |> List.map Recoil.Atoms.Task.taskId

             let position = FlukeDateTime.Create 2020 Month.January 01 18 00

             let baseInitializer (initializer: MutableSnapshot) =
                 initializer.set (Recoil.Atoms.lanePaddingLeft, 2)
                 initializer.set (Recoil.Atoms.lanePaddingRight, 2)
                 initializer.set (Recoil.Atoms.selectedPosition, Some position)

             let cells =
                 CellsComponent.render
                     {|
                         Username = user.Username
                         TaskIdList = taskIdList
                     |}

             Jest.test
                 ("single cell selection",
                  promise {
                      Jest.useFakeTimers ()
                      let subject, peek = cells |> Testing.render baseInitializer

                      let taskId = taskIdList.[0]
                      let dateId = DateId position.Date
                      let cellId = Recoil.Atoms.Cell.cellId taskId dateId


                      let! cell = subject.findByTestId (sprintf "cell-%A" cellId)

                      RTL.fireEvent.click cell

                      do! peek (fun setter ->
                              promise {
                                  let! cellSelectionMap = setter.snapshot.getPromise Recoil.Atoms.cellSelectionMap

                                  let expectedCellSelectionMap =
                                      [
                                          taskId,
                                          set
                                              [
                                                  position.Date
                                              ]
                                      ]
                                      |> Map.ofList

                                  Jest.expect(string cellSelectionMap).toEqual(string expectedCellSelectionMap)
                              })
                  })
             ()))

    Jest.describe
        ("tree selection",
         (fun () ->
             let user = getUser ()

             let position1 = FlukeDateTime.FromDateTime DateTime.MinValue
             let position2 = FlukeDateTime.FromDateTime DateTime.MaxValue

             let treeList =
                 [
                     State.TreeId (Guid "F8488D67-0305-40AA-8CAB-AF46E486F1DC"), Some position1
                     State.TreeId (Guid "63F40B3D-5B5F-419C-ADA2-5A5CDCA180DD"), Some position1
                     State.TreeId (Guid "43B942D6-880E-4788-A6B2-EB670CD48B14"), Some position2
                     State.TreeId (Guid "4F2CEB09-F646-4EFE-AC34-A3BDA24DDB71"), None
                     State.TreeId (Guid "C1D06F9A-154A-4BBC-9D7F-0C78C6C44C5C"), None
                 ]

             let queryMenuItems (subject: Bindings.render<_, _>) =
                 treeList
                 |> List.map fst
                 |> List.map (fun (State.TreeId guid) -> subject.queryByTestId ("menu-item-" + guid.ToString ()))
                 |> List.toArray

             let testMenuItemsVisibility array menuItems =
                 menuItems
                 |> Array.map Option.isSome
                 |> fun menuItemsVisibility -> Jest.expect(menuItemsVisibility).toEqual array

             let baseInitializer (initializer: MutableSnapshot) =
                 initializer.set (Recoil.Atoms.Session.availableTreeIds user.Username, treeList |> List.map fst)
                 treeList
                 |> List.iter (fun (treeId, position) -> initializer.set (Recoil.Atoms.Tree.position treeId, position))

             let treeSelector = TreeSelectorComponent.render {| Username = user.Username |}

             Jest.test
                 ("tree list updates correctly with user clicks",
                  promise {
                      let subject, _ = treeSelector |> Testing.render baseInitializer

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
                      let subject, _ =
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
                  })
             ()))
