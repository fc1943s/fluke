namespace Fluke.UI.Frontend.Tests.CellSelection

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


module CellSelectionSetup =
    let maxTimeout = 5 * 60 * 1000

    let getCellMap (subject: Bindings.render<_, _>) (setter: IRefValue<unit -> CallbackMethods>) =
        promise {
            let! dateSequence =
                setter
                    .current()
                    .snapshot.getPromise Selectors.dateSequence

            let! username =
                setter
                    .current()
                    .snapshot.getPromise Atoms.username

            match username with
            | Some username ->

                let! filteredTaskIdList =
                    setter
                        .current()
                        .snapshot.getPromise (Selectors.Session.filteredTaskIdList username)

                printfn $"filteredTaskIdList={filteredTaskIdList}"

                let! cellList =
                    filteredTaskIdList
                    |> List.map
                        (fun taskId ->
                            promise {
                                let! taskName =
                                    setter
                                        .current()
                                        .snapshot.getPromise (Atoms.Task.name (username, taskId))

                                return
                                    dateSequence
                                    |> List.toArray
                                    |> Array.map
                                        (fun date ->
                                            let el =
                                                subject.queryByTestId
                                                    $"cell-{taskId}-{(date |> FlukeDate.DateTime).ToShortDateString ()}"

                                            ((taskId, taskName), date), (el |> Option.defaultValue (unbox null)))
                            })
                    |> Promise.Parallel

                let cellMap = cellList |> Array.collect id |> Map.ofArray

                printfn $"cellMap=%A{cellMap |> Map.keys |> Seq.map fst |> Seq.distinct}"

                return cellMap
            | None -> return failwith $"Invalid username: {username}"
        }

    let expectSelection (setter: IRefValue<unit -> CallbackMethods>) expected =
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

        promise {
            do! RTL.waitFor id

            let! cellSelectionMap =
                setter
                    .current()
                    .snapshot.getPromise (Selectors.Session.cellSelectionMap Templates.templatesUser.Username)

            Jest
                .expect(toString cellSelectionMap)
                .toEqual (toString expected)
        }

    let inline click elGetter =
        promise {
            do! RTL.waitFor id
            let! el = elGetter ()
            do! RTL.waitFor (fun () -> RTL.fireEvent.click el)
        }

    let getCell (cellMapGetter, taskName, date) =
        fun () ->
            promise {
                let! cellMap = cellMapGetter ()

                return
                    cellMap
                    |> Map.pick
                        (fun ((_, taskName'), date') el ->
                            if taskName = taskName' && date = date' then Some el else None)
            }

    let taskIdByName cellMapGetter taskName =
        promise {
            let! cellMap = cellMapGetter ()

            return
                cellMap
                |> Map.pick
                    (fun ((taskId, TaskName taskName'), _) _ -> if taskName = taskName' then Some taskId else None)
        }

    let initialSetter (setter: CallbackMethods) =
        let dslTemplate =
            {
                Templates.Position =
                    FlukeDateTime.Create (FlukeDate.Create 2020 Month.January 10, Templates.templatesUser.DayStart)
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

        promise {
            printfn "initialSetter init"

            let databaseId = DatabaseId.NewId ()

            let databaseState =
                Templates.databaseStateFromDslTemplate Templates.templatesUser databaseId databaseName dslTemplate

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

                    let gunNamespace = Recoil.useValue Selectors.gunNamespace
                    let username, setUsername = Recoil.useState Atoms.username

                    React.useEffect (
                        (fun () ->
                            promise {
                                let user = gunNamespace.``#``

                                if user.__.sea.IsNone then
                                    let username = Templates.templatesUser.Username |> Username.Value
                                    let! _ = Gun.createUser user username username
                                    let! _ = Gun.authUser gunNamespace.``#`` username username

                                    RTL.waitFor (fun () -> setUsername (Some Templates.templatesUser.Username))
                                    |> ignore
                            }
                            |> Promise.start),
                        [|
                            box username
                            box gunNamespace
                        |]
                    )

                    nothing)
                ())
            App.App false
            (React.memo
                (fun () ->
                    printfn "$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ AFTER RENDER"

                    nothing)
                ())
        ]

    let initialize () =
        promise {
            let subject, setter = getApp () |> Setup.render

            do!
                RTL.waitFor (
                    promise {
                        printfn "wait 1"
                        do! Promise.sleep 400
                    }
                )

            let! username =
                JS.waitForSome
                    (fun () ->
                        async {
                            match box setter with
                            | null -> return None
                            | _ -> return! setter.current().snapshot.getAsync Atoms.username
                        })
                |> Async.StartAsPromise

            printfn $"! username={username}"

            do! RTL.waitFor (initialSetter (setter.current ()))

            do!
                RTL.waitFor (
                    promise {
                        printfn "wait 2"
                        do! Promise.sleep 400
                    }
                )

            do!
                RTL.waitFor (
                    promise {
                        printfn "wait 3"
                        do! Promise.sleep 400
                    }
                )

            do!
                RTL.waitFor (
                    promise {
                        printfn "wait 4"
                        do! Promise.sleep 400
                    }
                )

            do!
                RTL.waitFor (
                    promise {
                        printfn "wait 5"
                        do! Promise.sleep 400
                    }
                )

            let! filteredTaskIdList =
                JS.waitForSome
                    (fun () ->
                        async {
                            let! filteredTaskIdList =
                                setter
                                    .current()
                                    .snapshot
                                    .getAsync (
                                        Selectors.Session.filteredTaskIdList Templates.templatesUser.Username
                                    )

                            return if filteredTaskIdList.IsEmpty then None else Some filteredTaskIdList
                        })
                |> Async.StartAsPromise

            printfn $"! filteredTaskIdList={filteredTaskIdList}"

            let cellMapGetter = fun () -> getCellMap subject setter

            return cellMapGetter, setter
        }
