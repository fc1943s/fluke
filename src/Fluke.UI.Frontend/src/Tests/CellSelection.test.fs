namespace Fluke.UI.Frontend.Tests

open Fable.ReactTestingLibrary
open Fable.Jester
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend
open Fluke.Shared.Domain
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Tests.Core
open Fluke.UI.Frontend.Recoil
open Fluke.Shared
open Fable.React
open Fluke.UI.Frontend.Hooks


module CellSelection =
    open Sync
    open Templates
    open TempData
    open State


    Jest.describe (
        "cell selection",
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
                        |> List.map
                            (fun n ->
                                {
                                    Task =
                                        { Task.Default with
                                            Name = TaskName (string n)
                                        }
                                    Events = []
                                    Expected = []
                                })
                }

            let databaseId = DatabaseId.NewId ()
            let databaseName = "Test"
            let databaseState = databaseStateFromDslTemplate testUser databaseId databaseName dslTemplate

            let initialSetter (setter: CallbackMethods) =
                promise {
                    setter.set (
                        Atoms.api,
                        Some
                            {
                                currentUser = async { return testUser }
                                databaseStateList =
                                    fun _username _moment ->
                                        async {
                                            return
                                                [
                                                    databaseState
                                                ]
                                        }
                            }
                    )

                    setter.set (Atoms.username, Some testUser.Username)
                    setter.set (Atoms.view, View.View.Priority)
                    setter.set (Atoms.daysBefore, 2)
                    setter.set (Atoms.daysAfter, 2)
                    setter.set (Atoms.selectedPosition, Some dslTemplate.Position)

                    setter.set (
                        Atoms.selectedDatabaseIds,
                        [|
                            SessionDataLoader.getDatabaseIdFromName databaseName
                        |]
                    )
                }

            let getApp () =

                React.fragment [
                    (React.memo
                        (fun () ->
                            printfn "$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ BEFORE RENDER"

                            let initialSetterCallback =
                                Recoil.useCallbackRef (fun setter -> promise { do! initialSetter setter })

                            React.useEffect ((fun () -> initialSetterCallback () |> Promise.start), [||])

                            nothing)
                        ())
                    App.App false
                    (React.memo
                        (fun () ->
                            printfn "$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ AFTER RENDER"

                            nothing)
                        ())
                ]

            let expectSelection peek expected =
                promise {
                    let toString map =
                        map
                        |> Map.toList
                        |> List.map
                            (fun ((Atoms.Task.TaskId (_, TaskName taskName)), dates) ->
                                taskName,
                                dates
                                |> Set.toList
                                |> List.map (fun (date: FlukeDate) -> date.Stringify ()))
                        |> string

                    //                    do! peek (fun _ -> promise { () })
                    do! RTL.waitFor id

                    do!
                        peek
                            (fun (setter: CallbackMethods) ->
                                promise {
                                    let! cellSelectionMap = setter.snapshot.getPromise Selectors.cellSelectionMap

                                    Jest
                                        .expect(toString cellSelectionMap)
                                        .toEqual (toString expected)
                                })
                }

            Jest.test (
                "single cell toggle",
                promise {
                    let! subject, peek = getApp () |> Setup.render
                    let! cellMap = Setup.getCellMap subject peek

                    do! peek (fun setter -> promise { setter.set (Atoms.ctrlPressed, true) })

                    RTL.fireEvent.click
                        cellMap.[TaskName "2", FlukeDate.Create 2020 Month.January 9]
                            .Value

                    do!
                        [
                            databaseState |> Setup.taskIdByName "2",
                            set [
                                FlukeDate.Create 2020 Month.January 9
                            ]
                        ]
                        |> Map.ofList
                        |> expectSelection peek

                    RTL.fireEvent.click
                        cellMap.[TaskName "2", FlukeDate.Create 2020 Month.January 9]
                            .Value

                    do! [] |> Map.ofList |> expectSelection peek

                    RTL.fireEvent.click
                        cellMap.[TaskName "2", FlukeDate.Create 2020 Month.January 11]
                            .Value

                    do!
                        [
                            databaseState |> Setup.taskIdByName "2",
                            set [
                                FlukeDate.Create 2020 Month.January 11
                            ]
                        ]
                        |> Map.ofList
                        |> expectSelection peek
                }
            )

            Jest.test (
                "ctrl pressed",
                promise {
                    let! subject, peek = getApp () |> Setup.render
                    let! cellMap = Setup.getCellMap subject peek

                    do! peek (fun setter -> promise { setter.set (Atoms.ctrlPressed, true) })

                    RTL.fireEvent.click
                        cellMap.[TaskName "2", FlukeDate.Create 2020 Month.January 9]
                            .Value

                    RTL.fireEvent.click
                        cellMap.[TaskName "2", FlukeDate.Create 2020 Month.January 11]
                            .Value

                    do!
                        [
                            databaseState |> Setup.taskIdByName "2",
                            set [
                                FlukeDate.Create 2020 Month.January 9
                                FlukeDate.Create 2020 Month.January 11
                            ]
                        ]
                        |> Map.ofList
                        |> expectSelection peek
                }
            )

            Jest.test (
                "horizontal shift pressed",
                promise {
                    let! subject, peek = getApp () |> Setup.render
                    let! cellMap = Setup.getCellMap subject peek

                    do! peek (fun setter -> promise { setter.set (Atoms.shiftPressed, true) })

                    RTL.fireEvent.click
                        cellMap.[TaskName "2", FlukeDate.Create 2020 Month.January 9]
                            .Value

                    RTL.fireEvent.click
                        cellMap.[TaskName "2", FlukeDate.Create 2020 Month.January 11]
                            .Value

                    do!
                        [
                            databaseState |> Setup.taskIdByName "2",
                            set [
                                FlukeDate.Create 2020 Month.January 9
                                FlukeDate.Create 2020 Month.January 10
                                FlukeDate.Create 2020 Month.January 11
                            ]
                        ]
                        |> Map.ofList
                        |> expectSelection peek
                }
            )

            Jest.test (
                "vertical shift pressed",
                promise {
                    let! subject, peek = getApp () |> Setup.render
                    let! cellMap = Setup.getCellMap subject peek

                    do! peek (fun setter -> promise { setter.set (Atoms.shiftPressed, true) })

                    RTL.fireEvent.click
                        cellMap.[TaskName "2", FlukeDate.Create 2020 Month.January 9]
                            .Value

                    RTL.fireEvent.click
                        cellMap.[TaskName "3", FlukeDate.Create 2020 Month.January 9]
                            .Value

                    do!
                        [
                            databaseState |> Setup.taskIdByName "2",
                            set [
                                FlukeDate.Create 2020 Month.January 9
                            ]

                            databaseState |> Setup.taskIdByName "3",
                            set [
                                FlukeDate.Create 2020 Month.January 9
                            ]
                        ]
                        |> Map.ofList
                        |> expectSelection peek
                }
            )

            Jest.test (
                "box selection",
                promise {
                    let! subject, peek = getApp () |> Setup.render
                    let! cellMap = Setup.getCellMap subject peek

                    do! peek (fun setter -> promise { setter.set (Atoms.shiftPressed, true) })

                    RTL.fireEvent.click
                        cellMap.[TaskName "2", FlukeDate.Create 2020 Month.January 9]
                            .Value

                    RTL.fireEvent.click
                        cellMap.[TaskName "3", FlukeDate.Create 2020 Month.January 10]
                            .Value

                    do!
                        [
                            databaseState |> Setup.taskIdByName "2",
                            set [
                                FlukeDate.Create 2020 Month.January 9
                                FlukeDate.Create 2020 Month.January 10
                            ]

                            databaseState |> Setup.taskIdByName "3",
                            set [
                                FlukeDate.Create 2020 Month.January 9
                                FlukeDate.Create 2020 Month.January 10
                            ]
                        ]
                        |> Map.ofList
                        |> expectSelection peek

                    RTL.fireEvent.click
                        cellMap.[TaskName "4", FlukeDate.Create 2020 Month.January 11]
                            .Value

                    do!
                        [
                            databaseState |> Setup.taskIdByName "2",
                            set [
                                FlukeDate.Create 2020 Month.January 9
                                FlukeDate.Create 2020 Month.January 10
                                FlukeDate.Create 2020 Month.January 11
                            ]

                            databaseState |> Setup.taskIdByName "3",
                            set [
                                FlukeDate.Create 2020 Month.January 9
                                FlukeDate.Create 2020 Month.January 10
                                FlukeDate.Create 2020 Month.January 11
                            ]

                            databaseState |> Setup.taskIdByName "4",
                            set [
                                FlukeDate.Create 2020 Month.January 9
                                FlukeDate.Create 2020 Month.January 10
                                FlukeDate.Create 2020 Month.January 11
                            ]
                        ]
                        |> Map.ofList
                        |> expectSelection peek

                    RTL.fireEvent.click
                        cellMap.[TaskName "1", FlukeDate.Create 2020 Month.January 8]
                            .Value

                    do!
                        [
                            databaseState |> Setup.taskIdByName "1",
                            set [
                                FlukeDate.Create 2020 Month.January 8
                                FlukeDate.Create 2020 Month.January 9
                                FlukeDate.Create 2020 Month.January 10
                                FlukeDate.Create 2020 Month.January 11
                            ]

                            databaseState |> Setup.taskIdByName "2",
                            set [
                                FlukeDate.Create 2020 Month.January 8
                                FlukeDate.Create 2020 Month.January 9
                                FlukeDate.Create 2020 Month.January 10
                                FlukeDate.Create 2020 Month.January 11
                            ]

                            databaseState |> Setup.taskIdByName "3",
                            set [
                                FlukeDate.Create 2020 Month.January 8
                                FlukeDate.Create 2020 Month.January 9
                                FlukeDate.Create 2020 Month.January 10
                                FlukeDate.Create 2020 Month.January 11
                            ]

                            databaseState |> Setup.taskIdByName "4",
                            set [
                                FlukeDate.Create 2020 Month.January 8
                                FlukeDate.Create 2020 Month.January 9
                                FlukeDate.Create 2020 Month.January 10
                                FlukeDate.Create 2020 Month.January 11
                            ]
                        ]
                        |> Map.ofList
                        |> expectSelection peek
                }
            ))
    )
