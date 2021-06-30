namespace Fluke.UI.Frontend.Tests.CellSelection

open Fable.ReactTestingLibrary
open Fable.Jester
open Feliz
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
open Fluke.UI.Frontend.State
open Fable.Core
open State


module CellSelectionSetup =
    let maxTimeout = 5 * 60 * 1000

    let getCellMap (subject: Bindings.render<_, _>) (getFn: Store.GetFn) =
        let dateSequence = Store.value getFn Selectors.dateSequence
        let sortedTaskIdList = Store.value getFn Selectors.Session.sortedTaskIdList

        printfn $"sortedTaskIdList ={sortedTaskIdList}"

        let cellMap =
            sortedTaskIdList
            |> List.collect
                (fun taskId ->
                    let taskName = Store.value getFn (Atoms.Task.name taskId)

                    dateSequence
                    |> List.map
                        (fun date ->
                            let el =
                                subject.queryByTestId
                                    $"cell-{taskId}-{(date |> FlukeDate.DateTime).ToShortDateString ()}"

                            ((taskId, taskName), date), (el |> Option.defaultValue null)))
            |> Map.ofSeq


        printfn $"cellMap=%A{cellMap |> Map.keys |> Seq.map fst |> Seq.distinct}"

        cellMap

    let expectSelection (getFn: Store.GetFn) expected =
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

            let cellSelectionMap = Store.value getFn Selectors.Session.cellSelectionMap

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
                let cellMap = cellMapGetter ()

                return
                    cellMap
                    |> Map.pick
                        (fun ((_, taskName'), date') el ->
                            if taskName = taskName' && date = date' then Some el else None)
            }

    let taskIdByName cellMapGetter taskName =
        promise {
            let cellMap = cellMapGetter ()

            return
                cellMap
                |> Map.pick
                    (fun ((taskId, TaskName taskName'), _) _ -> if taskName = taskName' then Some taskId else None)
        }

    let initialSetter (getFn: Store.GetFn) (setFn: Store.SetFn) =
        promise {
            let set atom value = Store.set setFn atom value

            let dslTemplate =
                {
                    Templates.Position =
                        FlukeDateTime.Create (
                            FlukeDate.Create 2020 Month.January 10,
                            Templates.templatesUser.DayStart,
                            Second 0
                        )
                    Templates.Tasks =
                        [
                            1 .. 4
                        ]
                        |> List.map
                            (fun n ->
                                {
                                    Task =
                                        { Task.Default with
                                            Id = TaskId.NewId ()
                                            Name = TaskName (string n)
                                        }
                                    Events = []
                                    Expected = []
                                })
                }

            let databaseName = "Test"

            printfn "initialSetter init"

            let databaseId = DatabaseId.NewId ()

            let databaseState =
                Templates.databaseStateFromDslTemplate Templates.templatesUser databaseId databaseName dslTemplate


            set Store.Atoms.username (Some Templates.templatesUser.Username)
            set Atoms.User.color (Some "#000000")
            set Atoms.User.view View.View.Priority
            set Atoms.User.daysBefore 2
            set Atoms.User.daysAfter 2
            set Atoms.position (Some dslTemplate.Position)

            do! Hydrate.hydrateDatabase getFn setFn (Store.AtomScope.ReadOnly, databaseState.Database)

            do!
                databaseState.TaskStateMap
                |> Seq.map
                    (fun (KeyValue (_taskId, taskState)) ->
                        promise {
                            do!
                                Hydrate.hydrateTaskState
                                    getFn
                                    setFn
                                    (Store.AtomScope.ReadOnly, databaseState.Database.Id, taskState)
                        })
                |> Promise.Parallel
                |> Promise.ignore

            set Atoms.User.selectedDatabaseIdSet (Set.singleton databaseId)
        }

    let getApp () =
        React.fragment [
            (React.memo
                (fun () ->
                    printfn "$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ BEFORE RENDER"

                    let gunNamespace = Store.useValue Store.Selectors.gunNamespace
                    let username, setUsername = Store.useState Store.Atoms.username

                    React.useEffect (
                        (fun () ->
                            promise {
                                if gunNamespace.__.sea.IsNone then
                                    let username = Templates.templatesUser.Username |> Username.Value
                                    let! _ = Gun.createUser gunNamespace username username
                                    let! _ = Gun.authUser gunNamespace username username

                                    RTL.act (fun () -> setUsername (Some Templates.templatesUser.Username))
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
            let! subject, (getFn, setFn) = getApp () |> Setup.render

            do! RTL.sleep 400

            let! username =
                JS.waitForSome (fun () -> async { return Store.value getFn Store.Atoms.username })
                |> Async.StartAsPromise

            printfn $"! username={username}"

            do! RTL.waitFor (initialSetter getFn setFn)

            let! sortedTaskIdList =
                JS.waitForSome
                    (fun () ->
                        async {
                            let sortedTaskIdList = Store.value getFn Selectors.Session.sortedTaskIdList
                            return if sortedTaskIdList.Length = 4 then Some sortedTaskIdList else None
                        })
                |> Async.StartAsPromise

            printfn $"! sortedTaskIdList={sortedTaskIdList}"

            let cellMapGetter = fun () -> getCellMap subject getFn

            do! RTL.sleep 1000

            return cellMapGetter, (getFn, setFn)
        }
