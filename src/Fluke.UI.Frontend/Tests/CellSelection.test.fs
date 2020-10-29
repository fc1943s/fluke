namespace Fluke.UI.Frontend.Tests

open Fable.ReactTestingLibrary
open Fable.Jester
open Feliz.Recoil
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Tests.Core
open Fluke.UI.Frontend.Recoil
open Fluke.Shared
open FSharpPlus

module CellSelection =
    open Sync
    open Templates
    open TempData

    Jest.describe
        ("cell selection",
         (fun () ->
             let dslTemplate =
                 {
                     Position =
                         {
                             Date = FlukeDate.Create 2020 Month.January 10
                             Time = testUser.DayStart
                         }
                     Tasks =
                         [
                             1 .. 4
                         ]
                         |> List.map (fun n ->
                             {
                                 Task = { Task.Default with Name = TaskName (string n) }
                                 Events = []
                                 Expected = []
                             })
                 }

             let treeState = treeStateFromDslTemplate testUser "Test" dslTemplate

             let initialSetter (setter: CallbackMethods) =
                 promise {
                     setter.set
                         (Atoms.api,
                          {
                              currentUser = async { return testUser }
                              treeStateList =
                                  fun username moment ->
                                      async {
                                          return [
                                              treeState
                                          ]
                                      }
                          })
                     setter.set (Atoms.view, View.View.Priority)
                     setter.set (Atoms.daysBefore, 2)
                     setter.set (Atoms.daysAfter, 2)
                     setter.set (Atoms.selectedPosition, Some dslTemplate.Position)
                 }

             let selectTree (setter: CallbackMethods) =
                 promise {
                     let! username = setter.snapshot.getPromise Atoms.username

                     match username with
                     | Some username ->
                         let! treeStateMap = setter.snapshot.getPromise (Selectors.Session.treeStateMap username)
                         let treeId = treeStateMap |> Map.keys |> Seq.head

                         setter.set
                             (Atoms.treeSelectionIds,
                              [|
                                  treeId
                              |])
                     | None -> ()
                 }

             let getPriorityView () =
                 Chakra.box
                     ()
                     [
                         //                     MainComponent.SessionDataLoader.hook {| Username = user.Username |}
                         PriorityView.render {| Username = testUser.Username |}
                     ]

             let expectSelection peek expected =
                 let toString map =
                     map
                     |> Map.toList
                     |> List.map (fun ((Atoms.Task.TaskId (_, TaskName taskName)), dates) ->
                         taskName,
                         dates
                         |> Set.toList
                         |> List.map (fun (date: FlukeDate) -> date.Stringify ()))
                     |> string

                 peek (fun (setter: CallbackMethods) ->
                     promise {
                         let! cellSelectionMap = setter.snapshot.getPromise Atoms.cellSelectionMap
                         Jest.expect(toString cellSelectionMap).toEqual(toString expected)
                     })

             let initialize peek =
                 promise {
                     do! peek initialSetter
                     do! peek (fun (setter: CallbackMethods) ->
                             promise { do! UserLoader.loadUser setter testUser.Username })
                     do! peek selectTree
                     do! Setup.initializeSessionData testUser peek
                 }

             Jest.test
                 ("single cell toggle",
                  promise {
                      let! subject, peek = getPriorityView () |> Setup.render
                      do! initialize peek
                      let! cellMap = Setup.getCellMap subject peek

                      RTL.fireEvent.click cellMap.[TaskName "2", FlukeDate.Create 2020 Month.January 09].Value

                      do! [
                              treeState |> Setup.taskIdByName "2",
                              set
                                  [
                                      FlukeDate.Create 2020 Month.January 09
                                  ]
                          ]
                          |> Map.ofList
                          |> expectSelection peek

                      RTL.fireEvent.click cellMap.[TaskName "2", FlukeDate.Create 2020 Month.January 11].Value

                      do! [
                              treeState |> Setup.taskIdByName "2",
                              set
                                  [
                                      FlukeDate.Create 2020 Month.January 11
                                  ]
                          ]
                          |> Map.ofList
                          |> expectSelection peek

                      RTL.fireEvent.click cellMap.[TaskName "2", FlukeDate.Create 2020 Month.January 11].Value

                      do! [
                              treeState |> Setup.taskIdByName "2", Set.empty
                          ]
                          |> Map.ofList
                          |> expectSelection peek
                  })

             Jest.test
                 ("ctrl pressed",
                  promise {
                      let! subject, peek = getPriorityView () |> Setup.render
                      do! initialize peek
                      let! cellMap = Setup.getCellMap subject peek

                      RTL.fireEvent.click cellMap.[TaskName "2", FlukeDate.Create 2020 Month.January 09].Value

                      do! peek (fun setter -> promise { setter.set (Atoms.ctrlPressed, true) })

                      RTL.fireEvent.click cellMap.[TaskName "2", FlukeDate.Create 2020 Month.January 11].Value

                      do! [
                              treeState |> Setup.taskIdByName "2",
                              set [
                                  FlukeDate.Create 2020 Month.January 09
                                  FlukeDate.Create 2020 Month.January 11
                              ]
                          ]
                          |> Map.ofList
                          |> expectSelection peek
                  })

             Jest.test
                 ("horizontal shift pressed",
                  promise {
                      let! subject, peek = getPriorityView () |> Setup.render
                      do! initialize peek
                      let! cellMap = Setup.getCellMap subject peek

                      RTL.fireEvent.click cellMap.[TaskName "2", FlukeDate.Create 2020 Month.January 09].Value

                      do! peek (fun setter -> promise { setter.set (Atoms.shiftPressed, true) })

                      RTL.fireEvent.click cellMap.[TaskName "2", FlukeDate.Create 2020 Month.January 11].Value

                      do! [
                              treeState |> Setup.taskIdByName "2",
                              set [
                                  FlukeDate.Create 2020 Month.January 09
                                  FlukeDate.Create 2020 Month.January 10
                                  FlukeDate.Create 2020 Month.January 11
                              ]
                          ]
                          |> Map.ofList
                          |> expectSelection peek
                  })

             Jest.test
                 ("vertical shift pressed",
                  promise {
                      let! subject, peek = getPriorityView () |> Setup.render
                      do! initialize peek
                      let! cellMap = Setup.getCellMap subject peek

                      RTL.fireEvent.click cellMap.[TaskName "2", FlukeDate.Create 2020 Month.January 09].Value

                      do! peek (fun setter -> promise { setter.set (Atoms.shiftPressed, true) })

                      RTL.fireEvent.click cellMap.[TaskName "3", FlukeDate.Create 2020 Month.January 09].Value

                      do! [
                              treeState |> Setup.taskIdByName "2",
                              set
                                  [
                                      FlukeDate.Create 2020 Month.January 09
                                  ]

                              treeState |> Setup.taskIdByName "3",
                              set
                                  [
                                      FlukeDate.Create 2020 Month.January 09
                                  ]
                          ]
                          |> Map.ofList
                          |> expectSelection peek
                  })

             Jest.test
                 ("box selection",
                  promise {
                      let! subject, peek = getPriorityView () |> Setup.render
                      do! initialize peek
                      let! cellMap = Setup.getCellMap subject peek

                      RTL.fireEvent.click cellMap.[TaskName "2", FlukeDate.Create 2020 Month.January 09].Value

                      do! peek (fun setter -> promise { setter.set (Atoms.shiftPressed, true) })

                      RTL.fireEvent.click cellMap.[TaskName "3", FlukeDate.Create 2020 Month.January 10].Value

                      do! [
                              treeState |> Setup.taskIdByName "2",
                              set [
                                  FlukeDate.Create 2020 Month.January 09
                                  FlukeDate.Create 2020 Month.January 10
                              ]

                              treeState |> Setup.taskIdByName "3",
                              set [
                                  FlukeDate.Create 2020 Month.January 09
                                  FlukeDate.Create 2020 Month.January 10
                              ]
                          ]
                          |> Map.ofList
                          |> expectSelection peek

                      RTL.fireEvent.click cellMap.[TaskName "4", FlukeDate.Create 2020 Month.January 11].Value

                      do! [
                              treeState |> Setup.taskIdByName "2",
                              set [
                                  FlukeDate.Create 2020 Month.January 09
                                  FlukeDate.Create 2020 Month.January 10
                                  FlukeDate.Create 2020 Month.January 11
                              ]

                              treeState |> Setup.taskIdByName "3",
                              set [
                                  FlukeDate.Create 2020 Month.January 09
                                  FlukeDate.Create 2020 Month.January 10
                                  FlukeDate.Create 2020 Month.January 11
                              ]

                              treeState |> Setup.taskIdByName "4",
                              set [
                                  FlukeDate.Create 2020 Month.January 09
                                  FlukeDate.Create 2020 Month.January 10
                                  FlukeDate.Create 2020 Month.January 11
                              ]
                          ]
                          |> Map.ofList
                          |> expectSelection peek

                      RTL.fireEvent.click cellMap.[TaskName "1", FlukeDate.Create 2020 Month.January 08].Value

                      do! [
                              treeState |> Setup.taskIdByName "1",
                              set [
                                  FlukeDate.Create 2020 Month.January 08
                                  FlukeDate.Create 2020 Month.January 09
                                  FlukeDate.Create 2020 Month.January 10
                                  FlukeDate.Create 2020 Month.January 11
                              ]

                              treeState |> Setup.taskIdByName "2",
                              set [
                                  FlukeDate.Create 2020 Month.January 08
                                  FlukeDate.Create 2020 Month.January 09
                                  FlukeDate.Create 2020 Month.January 10
                                  FlukeDate.Create 2020 Month.January 11
                              ]

                              treeState |> Setup.taskIdByName "3",
                              set [
                                  FlukeDate.Create 2020 Month.January 08
                                  FlukeDate.Create 2020 Month.January 09
                                  FlukeDate.Create 2020 Month.January 10
                                  FlukeDate.Create 2020 Month.January 11
                              ]

                              treeState |> Setup.taskIdByName "4",
                              set [
                                  FlukeDate.Create 2020 Month.January 08
                                  FlukeDate.Create 2020 Month.January 09
                                  FlukeDate.Create 2020 Month.January 10
                                  FlukeDate.Create 2020 Month.January 11
                              ]
                          ]
                          |> Map.ofList
                          |> expectSelection peek
                  })

             ()))
