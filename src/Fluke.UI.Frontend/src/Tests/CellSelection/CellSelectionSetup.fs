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

    let getCellMap (subject: Bindings.render<_, _>) (getFn: Jotai.GetFn) =
        let dateSequence = Atoms.getAtomValue getFn Selectors.dateSequence
        let username = Atoms.getAtomValue getFn Atoms.username

        match username with
        | Some username ->

            let sortedTaskIdList = Atoms.getAtomValue getFn (Selectors.Session.sortedTaskIdList username)

            printfn $"sortedTaskIdList ={sortedTaskIdList}"

            let cellMap =
                sortedTaskIdList
                |> List.collect
                    (fun taskId ->
                        let taskName = Atoms.getAtomValue getFn (Atoms.Task.name (username, taskId))

                        dateSequence
                        |> List.map
                            (fun date ->
                                let el =
                                    subject.queryByTestId
                                        $"cell-{taskId}-{(date |> FlukeDate.DateTime).ToShortDateString ()}"

                                ((taskId, taskName), date), (el |> Option.defaultValue null)))
                |> Map.ofList


            printfn $"cellMap=%A{cellMap |> Map.keys |> Seq.map fst |> Seq.distinct}"

            cellMap
        | None -> failwith $"Invalid username: {username}"

    let expectSelection (getFn: Jotai.GetFn) expected =
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

            let cellSelectionMap =
                Atoms.getAtomValue getFn (Selectors.Session.cellSelectionMap Templates.templatesUser.Username)

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

    let initialSetter (getFn: Jotai.GetFn) (setFn: Jotai.SetFn) =
        promise {
            let get atom = Atoms.getAtomValue getFn atom
            let set atom value = Atoms.setAtomValue setFn atom value

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


            set Atoms.username (Some Templates.templatesUser.Username)
            set (Atoms.User.view Templates.templatesUser.Username) View.View.Priority
            set (Atoms.User.daysBefore Templates.templatesUser.Username) 2
            set (Atoms.User.daysAfter Templates.templatesUser.Username) 2
            set Atoms.position (Some dslTemplate.Position)

            do!
                Hydrate.hydrateDatabase
                    getFn
                    setFn
                    (Templates.templatesUser.Username, JotaiTypes.AtomScope.ReadOnly, databaseState.Database)

            set (Atoms.Session.databaseIdSet Templates.templatesUser.Username) (Set.singleton databaseId)

            do!
                databaseState.TaskStateMap
                |> Seq.map
                    (fun (KeyValue (taskId, taskState)) ->
                        promise {
                            do!
                                Hydrate.hydrateTaskState
                                    getFn
                                    setFn
                                    (Templates.templatesUser.Username,
                                     JotaiTypes.AtomScope.ReadOnly,
                                     databaseState.Database.Id,
                                     taskState)

                            let taskIdSet =
                                get (Atoms.Database.taskIdSet (Templates.templatesUser.Username, databaseId))

                            set
                                (Atoms.Database.taskIdSet (Templates.templatesUser.Username, databaseId))
                                (taskIdSet |> Set.add taskId)
                        })
                |> Promise.Parallel
                |> Promise.ignore

            set (Atoms.User.selectedDatabaseIdSet Templates.templatesUser.Username) (Set.singleton databaseId)
        }

    let getApp () =
        React.fragment [
            (React.memo
                (fun () ->
                    printfn "$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ BEFORE RENDER"

                    let gunNamespace = Store.useValue Atoms.gunNamespace
                    let username, setUsername = Store.useState Atoms.username

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
                JS.waitForSome (fun () -> async { return Atoms.getAtomValue getFn Atoms.username })
                |> Async.StartAsPromise

            printfn $"! username={username}"

            do! RTL.waitFor (initialSetter getFn setFn)

            let! sortedTaskIdList =
                JS.waitForSome
                    (fun () ->
                        async {
                            let sortedTaskIdList =
                                Atoms.getAtomValue
                                    getFn
                                    (Selectors.Session.sortedTaskIdList Templates.templatesUser.Username)

                            return if sortedTaskIdList.Length = 4 then Some sortedTaskIdList else None
                        })
                |> Async.StartAsPromise

            printfn $"! sortedTaskIdList={sortedTaskIdList}"

            let cellMapGetter = fun () -> getCellMap subject getFn

            return cellMapGetter, (getFn, setFn)
        }
