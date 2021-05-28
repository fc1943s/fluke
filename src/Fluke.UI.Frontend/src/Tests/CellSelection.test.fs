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
open Fluke.UI.Frontend.Tests.Core
open Fluke.Shared
open Fable.React
open Fluke.UI.Frontend.Hooks
open Microsoft.FSharp.Core.Operators
open System
open Fluke.UI.Frontend.State
open Fable.Core
open State


module CellSelection =

    let getCellMap (subject: Bindings.render<_, _>) (setter: unit -> CallbackMethods) =
        promise {
            let setter = setter ()
            let! dateSequence = setter.snapshot.getPromise Selectors.dateSequence
            let! username = setter.snapshot.getPromise Atoms.username

            match username with
            | Some username ->

                let! filteredTaskIdList = setter.snapshot.getPromise (Selectors.Session.filteredTaskIdList username)

                printfn $"filteredTaskIdList={filteredTaskIdList}"

                let! cellList =
                    filteredTaskIdList
                    |> List.map
                        (fun taskId ->
                            promise {
                                let! taskName = setter.snapshot.getPromise (Atoms.Task.name (username, taskId))

                                return
                                    dateSequence
                                    |> List.toArray
                                    |> Array.map
                                        (fun date ->
                                            ((taskId, taskName), date),
                                            subject.queryByTestId
                                                $"cell-{taskId}-{(date |> FlukeDate.DateTime).ToShortDateString ()}")
                            })
                    |> Promise.Parallel

                let cellMap = cellList |> Array.collect id |> Map.ofArray

                printfn $"cellMap=%A{cellMap |> Map.keys |> Seq.map fst |> Seq.distinct}"

                return cellMap
            | None -> return failwith $"Invalid username: {username}"
        }

    Jest.describe (
        "cell selection",
        (fun () ->
            let dslTemplate =
                {
                    Templates.Position =
                        {
                            Date = FlukeDate.Create 2020 Month.January 10
                            Time = Templates.templatesUser.DayStart
                        }
                    Templates.Tasks =
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

            let databaseName = "Test"

            let initialSetter (setter: CallbackMethods) =
                promise {
                    let databaseId = DatabaseId.NewId ()

                    let databaseState =
                        Templates.databaseStateFromDslTemplate
                            Templates.templatesUser
                            databaseId
                            databaseName
                            dslTemplate

                    setter.set (Atoms.username, Some Templates.templatesUser.Username)
                    setter.set (Atoms.User.view Templates.templatesUser.Username, View.View.Priority)
                    setter.set (Atoms.User.daysBefore Templates.templatesUser.Username, 2)
                    setter.set (Atoms.User.daysAfter Templates.templatesUser.Username, 2)
                    setter.set (Atoms.gunHash, Guid.NewGuid().ToString ())
                    setter.set (Atoms.position, Some dslTemplate.Position)

                    Hydrate.hydrateDatabase
                        setter
                        Templates.templatesUser.Username
                        Recoil.AtomScope.ReadOnly
                        databaseState.Database

                    databaseState.TaskStateMap
                    |> Map.keys
                    |> Seq.iter
                        (fun task ->
                            Hydrate.hydrateTask
                                setter
                                Templates.templatesUser.Username
                                Recoil.AtomScope.ReadOnly
                                databaseId
                                { task with Id = TaskId.NewId () })

                    setter.set (
                        Atoms.User.selectedDatabaseIdList Templates.templatesUser.Username,
                        [
                            databaseId
                        ]
                    )
                }

            let getApp () =
                React.fragment [
                    (React.memo
                        (fun () ->
                            printfn "$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ BEFORE RENDER"

                            let _ = Recoil.useValue Selectors.gun
                            let init, setInit = React.useState false

                            React.useEffect (
                                (fun () ->
                                    async {
                                        let! gun = Recoil.getGun ()
                                        let user = gun.user ()
                                        printfn $"1. user.is={user.is} user.__.sea={user.__.sea}"

                                        if user.__.sea.IsSome then
                                            setInit true
                                        else
                                            let username = Templates.templatesUser.Username |> Username.Value

                                            printfn $"1## {JS.JSON.stringify user.is}"

                                            let! ack1 =
                                                Gun.createUser user username username
                                                |> Async.AwaitPromise

                                            printfn $"2## ack1={JS.JSON.stringify ack1}"
                                            //                                                do! Async.Sleep 100 |> Async.StartAsPromise
//                                                let! ack2 = Gun.authUser user username username |> Async.AwaitPromise
                                            let! ack2 = Gun.authUser user username username |> Async.AwaitPromise
                                            printfn $"3## ack2={JS.JSON.stringify ack2}"
                                            printfn "do! Async.Sleep 100 |> Async.StartAsPromise"
                                            //                                                do! Async.Sleep 100 |> Async.StartAsPromise
                                            printfn "setInit true"
                                            setInit true

                                    //                                                Gun.createUser user username username |> Promise.start
//                                                do! Async.Sleep 100 |> Async.StartAsPromise
//                                                printfn "2##"
//                                                let! ack2 = Gun.authUser user username username
//                                                printfn $"## ack2={JS.JSON.stringify ack2}"
//                                                if ack2.wait then
//                                                    do! Async.Sleep 1 |> Async.StartAsPromise
//                                                    let! ack3 = Gun.authUser user username username
//                                                    printfn $"## ack3={JS.JSON.stringify ack3}"
//                                                    ()
                                    }
                                    |> Async.StartAsPromise
                                    |> Promise.start),
                                [|
                                    box setInit
                                |]
                            )

//                            let setter = Recoil.useCallbackRef id

//                            React.useEffect (
//                                (fun () ->
//                                    promise { if init then do! initialSetter (setter ()) }
//                                    |> Promise.start),
//                                [|
//                                    box init
//                                    box setter
//                                |]
//                            )

                            nothing)
                        ())
                    App.App false
                    (React.memo
                        (fun () ->
                            printfn "$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ AFTER RENDER"

                            nothing)
                        ())
                ]

            let expectSelection (setter: CallbackMethods) expected =
                promise {
                    let toString map =
                        map
                        |> Map.toList
                        |> List.map
                            (fun (taskId, dates) ->
                                taskId |> TaskId.Value,
                                dates
                                |> Set.toList
                                |> List.map FlukeDate.Stringify)
                        |> string

                    do! RTL.waitFor id

                    let! cellSelectionMap =
                        setter.snapshot.getPromise (Selectors.Session.cellSelectionMap Templates.templatesUser.Username)

                    Jest
                        .expect(toString cellSelectionMap)
                        .toEqual (toString expected)
                }

            let click el =
                promise {
                    RTL.act (fun () -> RTL.fireEvent.click el)
                    do! RTL.waitFor id
                }

            let getCell (cellMap, taskName, date) =
                cellMap
                |> Map.pick
                    (fun ((_, taskName'), date') el -> if taskName = taskName' && date = date' then Some el else None)
                |> Option.get

            let taskIdByName cellMap taskName =
                cellMap
                |> Map.pick
                    (fun ((taskId, TaskName taskName'), _) _ -> if taskName = taskName' then Some taskId else None)

            Jest.beforeEach (
                promise {
                    Browser.Dom.window.localStorage.clear ()
                    do! RTL.waitFor id
                }
            )

            Jest.afterEach (
                promise {
                    Browser.Dom.window.localStorage.clear ()
                    do! RTL.waitFor id
                }
            )

            Jest.test (
                "single cell toggle",
                promise {
                    let! subject, setter = getApp () |> Setup.render

                    printfn "before sleep"

                    do!
                        RTL.waitFor (
                            async {
                                let! keys =
                                    JS.waitForSome
                                        (fun () ->
                                            async {
                                                let! gun = Recoil.getGun ()
                                                let user = gun.user ()
                                                return user.__.sea
                                            })

                                printfn $"after sleep keys={JS.JSON.stringify keys}"
                            }
                        )

                    do! RTL.waitFor (initialSetter (setter ()))

                    do!
                        RTL.waitFor (
                            async {
                                let! taskIdSet =
                                    JS.waitForSome
                                        (fun () ->
                                            async {
                                                let! taskIdSet =
                                                    setter()
                                                        .snapshot
                                                        .getAsync (
                                                            Atoms.Session.taskIdSet Templates.templatesUser.Username
                                                        )
                                                printfn $"!!! taskIdSet={taskIdSet}"

                                                return if taskIdSet.IsEmpty then None else Some taskIdSet
                                            })

                                printfn $"! taskIdSet={taskIdSet}"

                            }
                        )

                    //                    do! Async.Sleep 1000 |> Async.StartAsPromise
//                    do! RTL.waitFor id

                    let! cellMap = getCellMap subject setter

                    RTL.act (fun () -> setter().set (Atoms.ctrlPressed, true))

                    do! click (getCell (cellMap, TaskName "2", FlukeDate.Create 2020 Month.January 9))

                    do!
                        [
                            taskIdByName cellMap "2",
                            set [
                                FlukeDate.Create 2020 Month.January 9
                            ]
                        ]
                        |> Map.ofList
                        |> expectSelection (setter ())

                    do! click (getCell (cellMap, TaskName "2", FlukeDate.Create 2020 Month.January 9))

                    do! [] |> Map.ofList |> expectSelection (setter ())

                    do! click (getCell (cellMap, TaskName "2", FlukeDate.Create 2020 Month.January 11))

                    do!
                        [
                            taskIdByName cellMap "2",
                            set [
                                FlukeDate.Create 2020 Month.January 11
                            ]
                        ]
                        |> Map.ofList
                        |> expectSelection (setter ())

                }
            )

            //            Jest.test (
//                "ctrl pressed",
//                promise {
//                    let! subject, peek = getApp () |> Setup.render
//                    let! cellMap = getCellMap subject peek
//
//                    do! peek (fun setter -> promise { setter.set (Atoms.ctrlPressed, true) })
//
//                    do! click (getCell (cellMap, TaskName "2", FlukeDate.Create 2020 Month.January 9))
//
//
//                    do! click (getCell (cellMap, TaskName "2", FlukeDate.Create 2020 Month.January 11))
//
//
//                    do!
//                        [
//                            taskIdByName cellMap "2",
//                            set [
//                                FlukeDate.Create 2020 Month.January 9
//                                FlukeDate.Create 2020 Month.January 11
//                            ]
//                        ]
//                        |> Map.ofList
//                        |> expectSelection peek
//                }
//            )

            //            Jest.test (
//                "horizontal shift pressed",
//                promise {
//                    let! subject, peek = getApp () |> Setup.render
//                    let! cellMap = getCellMap subject peek
//
//                    do! peek (fun setter -> promise { setter.set (Atoms.shiftPressed, true) })
//
//                    do! click (getCell (cellMap, TaskName "2", FlukeDate.Create 2020 Month.January 9))
//
//
//                    do! click (getCell (cellMap, TaskName "2", FlukeDate.Create 2020 Month.January 11))
//
//
//                    do!
//                        [
//                            taskIdByName cellMap "2",
//                            set [
//                                FlukeDate.Create 2020 Month.January 9
//                                FlukeDate.Create 2020 Month.January 10
//                                FlukeDate.Create 2020 Month.January 11
//                            ]
//                        ]
//                        |> Map.ofList
//                        |> expectSelection peek
//                }
//            )

            //            Jest.test (
//                "vertical shift pressed",
//                promise {
//                    let! subject, peek = getApp () |> Setup.render
//                    let! cellMap = getCellMap subject peek
//
//                    do! peek (fun setter -> promise { setter.set (Atoms.shiftPressed, true) })
//
//                    do! click (getCell (cellMap, TaskName "2", FlukeDate.Create 2020 Month.January 9))
//
//                    do! click (getCell (cellMap, TaskName "3", FlukeDate.Create 2020 Month.January 9))
//
//                    do!
//                        [
//                            taskIdByName cellMap "2",
//                            set [
//                                FlukeDate.Create 2020 Month.January 9
//                            ]
//
//                            taskIdByName cellMap "3",
//                            set [
//                                FlukeDate.Create 2020 Month.January 9
//                            ]
//                        ]
//                        |> Map.ofList
//                        |> expectSelection peek
//                }
//            )

            //            Jest.test (
//                "box selection",
//                promise {
//                    let! subject, peek = getApp () |> Setup.render
//                    let! cellMap = getCellMap subject peek
//
//                    do! peek (fun setter -> promise { setter.set (Atoms.shiftPressed, true) })
//
//
//                    do! click (getCell (cellMap, TaskName "2", FlukeDate.Create 2020 Month.January 9))
//                    do! click (getCell (cellMap, TaskName "3", FlukeDate.Create 2020 Month.January 10))
//
//                    do!
//                        [
//                            taskIdByName cellMap "2",
//                            set [
//                                FlukeDate.Create 2020 Month.January 9
//                                FlukeDate.Create 2020 Month.January 10
//                            ]
//
//                            taskIdByName cellMap "3",
//                            set [
//                                FlukeDate.Create 2020 Month.January 9
//                                FlukeDate.Create 2020 Month.January 10
//                            ]
//                        ]
//                        |> Map.ofList
//                        |> expectSelection peek
//
//
//                    do! click (getCell (cellMap, TaskName "4", FlukeDate.Create 2020 Month.January 11))
//
//                    do!
//                        [
//                            taskIdByName cellMap "2",
//                            set [
//                                FlukeDate.Create 2020 Month.January 9
//                                FlukeDate.Create 2020 Month.January 10
//                                FlukeDate.Create 2020 Month.January 11
//                            ]
//
//                            taskIdByName cellMap "3",
//                            set [
//                                FlukeDate.Create 2020 Month.January 9
//                                FlukeDate.Create 2020 Month.January 10
//                                FlukeDate.Create 2020 Month.January 11
//                            ]
//
//                            taskIdByName cellMap "4",
//                            set [
//                                FlukeDate.Create 2020 Month.January 9
//                                FlukeDate.Create 2020 Month.January 10
//                                FlukeDate.Create 2020 Month.January 11
//                            ]
//                        ]
//                        |> Map.ofList
//                        |> expectSelection peek
//
//                    do! click (getCell (cellMap, TaskName "1", FlukeDate.Create 2020 Month.January 8))
//
//                    do!
//                        [
//                            taskIdByName cellMap "1",
//                            set [
//                                FlukeDate.Create 2020 Month.January 8
//                                FlukeDate.Create 2020 Month.January 9
//                                FlukeDate.Create 2020 Month.January 10
//                                FlukeDate.Create 2020 Month.January 11
//                            ]
//
//                            taskIdByName cellMap "2",
//                            set [
//                                FlukeDate.Create 2020 Month.January 8
//                                FlukeDate.Create 2020 Month.January 9
//                                FlukeDate.Create 2020 Month.January 10
//                                FlukeDate.Create 2020 Month.January 11
//                            ]
//
//                            taskIdByName cellMap "3",
//                            set [
//                                FlukeDate.Create 2020 Month.January 8
//                                FlukeDate.Create 2020 Month.January 9
//                                FlukeDate.Create 2020 Month.January 10
//                                FlukeDate.Create 2020 Month.January 11
//                            ]
//
//                            taskIdByName cellMap "4",
//                            set [
//                                FlukeDate.Create 2020 Month.January 8
//                                FlukeDate.Create 2020 Month.January 9
//                                FlukeDate.Create 2020 Month.January 10
//                                FlukeDate.Create 2020 Month.January 11
//                            ]
//                        ]
//                        |> Map.ofList
//                        |> expectSelection peek
//                }
//            )
            )
    )
