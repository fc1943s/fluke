namespace Fluke.UI.Frontend.Tests

open System
open Fable.ReactTestingLibrary
open Fable.Jester
open Feliz.Recoil
open Fluke.Shared.Domain.Information
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Recoil
open Fluke.Shared
open FSharpPlus

module CellSelection =
    open Model
    open Domain.Information
    open Domain.UserInteraction
    open Domain.State
    open View
    open Templates

    Jest.describe
        ("cell selection",
         (fun () ->
             let user = Setup.getUser ()

             let dslTemplate =
                 {
                     Position =
                         {
                             Date = FlukeDate.Create 2020 Month.January 10
                             Time = user.DayStart
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

             let treeState = treeStateFromDslTemplate user "Test" dslTemplate

             let treeStateMap =
                 let treeId = TreeId Guid.Empty
                 [
                     treeId, treeState
                 ]
                 |> Map.ofList

             let initialSetter (setter: CallbackMethods) =
                 promise {
                     setter.set
                         (Atoms.path,
                          [
                              "view"
                              "Tasks"
                          ])
                     setter.set (Atoms.Session.user user.Username, Some user)
                     setter.set (Atoms.username, Some user.Username)
                     setter.set (Atoms.lanePaddingLeft, 2)
                     setter.set (Atoms.lanePaddingRight, 2)
                     setter.set (Atoms.selectedPosition, Some dslTemplate.Position)
                     setter.set (Atoms.treeStateMap, treeStateMap)
                     setter.set
                         (Atoms.treeSelectionIds,
                          [|
                              treeStateMap |> Map.keys |> Seq.head
                          |])
                 }

             let tasksView =
                 Chakra.box
                     ()
                     [
                         //                     MainComponent.SessionDataLoader.hook {| Username = user.Username |}
                         TasksViewComponent.render {| Username = user.Username |}
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

             Jest.test
                 ("single cell toggle",
                  promise {
                      let! subject, peek = tasksView |> Setup.render
                      do! peek initialSetter
                      do! Setup.initializeSessionData user peek
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
                      let! subject, peek = tasksView |> Setup.render
                      do! peek initialSetter
                      do! Setup.initializeSessionData user peek
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
                      let! subject, peek = tasksView |> Setup.render
                      do! peek initialSetter
                      do! Setup.initializeSessionData user peek
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
                      let! subject, peek = tasksView |> Setup.render
                      do! peek initialSetter
                      do! Setup.initializeSessionData user peek
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
                      let! subject, peek = tasksView |> Setup.render
                      do! peek initialSetter
                      do! Setup.initializeSessionData user peek
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
