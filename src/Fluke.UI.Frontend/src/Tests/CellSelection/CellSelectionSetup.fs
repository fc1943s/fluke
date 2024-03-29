namespace Fluke.UI.Frontend.Tests.CellSelection

open FsCore
open Fable.ReactTestingLibrary
open Fable.Jester
open Feliz
open FsCore.BaseModel
open FsJs
open FsStore
open FsStore.Hooks
open FsStore.Bindings
open FsStore.Model
open FsUi.Bindings
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

    let inline getCellMap (subject: Bindings.render<_, _>) (getFn: Getter<_>) =
        let dateArray = Atom.get getFn Selectors.Selectors.dateArray
        let sortedTaskIdArray = Atom.get getFn Selectors.Session.sortedTaskIdArray

        printfn $"sortedTaskIdArray={sortedTaskIdArray}"

        let cellMap =
            sortedTaskIdArray
            |> Array.collect
                (fun taskId ->
                    let taskName = Atom.get getFn (Atoms.Task.name taskId)

                    dateArray
                    |> Array.map
                        (fun date ->
                            let el =
                                subject.queryByTestId
                                    $"cell-{taskId}-{(date |> FlukeDate.DateTime).ToShortDateString ()}"

                            ((taskId, taskName), date), (el |> Option.defaultValue null)))
            |> Map.ofSeq


        printfn $"cellMap=%A{cellMap |> Map.keys |> Seq.map fst |> Seq.distinct}"

        cellMap

    let inline expectSelection (getFn: Getter<_>) expected =
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

            let cellSelectionMap = Atom.get getFn Selectors.Session.visibleTaskSelectedDateMap

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

    let inline getCell (cellMapGetter, taskName, date) =
        fun () ->
            promise {
                let cellMap = cellMapGetter ()

                return
                    cellMap
                    |> Map.pick
                        (fun ((_, taskName'), date') el ->
                            if taskName = taskName' && date = date' then Some el else None)
            }

    let inline taskIdByName cellMapGetter taskName =
        promise {
            let cellMap = cellMapGetter ()

            return
                cellMap
                |> Map.pick
                    (fun ((taskId, TaskName taskName'), _) _ -> if taskName = taskName' then Some taskId else None)
        }

    let inline initialSetter (getter: Getter<_>) (setter: Setter<_>) =
        promise {
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

            do!
                Hydrate.hydrateUserState
                    getter
                    setter
                    { UserState.Default with
                        DaysBefore = 2
                        DaysAfter = 2
                        View = View.View.Priority
                        UserColor = Some Color.Default
                        Archive = Some false
                    }

            let databaseId = DatabaseId.NewId ()

            let databaseState =
                Templates.databaseStateFromDslTemplate Templates.templatesUser databaseId databaseName dslTemplate

            do! Hydrate.hydrateDatabase getter setter (AtomScope.Current, databaseState.Database)

            do!
                databaseState.TaskStateMap
                |> Seq.map
                    (fun (KeyValue (_taskId, taskState)) ->
                        promise {
                            do!
                                Hydrate.hydrateTaskState
                                    getter
                                    setter
                                    (AtomScope.Current, databaseState.Database.Id, taskState)
                        })
                |> Promise.all
                |> Promise.ignore

            Atom.set setter Atoms.User.selectedDatabaseIdSet (Set.singleton databaseId)
            Atom.set setter Atoms.Session.position (Some dslTemplate.Position)
        }

    let inline getApp () =
        React.fragment [
            (React.memo
                (fun () ->
                    printfn "$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ BEFORE RENDER"

                    let gun = Store.useValue Selectors.Gun.gun
                    //                    let alias, setAlias = Store.useState Selectors.Gun.alias

                    React.useEffect (
                        (fun () ->
                            promise {
                                let user = gun.user ()

                                if user.__.sea.IsNone then
                                    let alias = Templates.templatesUser.Username |> Username.Value

                                    let! _ = Gun.createUser user (Gun.Alias alias) (Gun.Pass alias)
                                    let! _ = Gun.authUser user (Gun.Alias alias) (Gun.Pass alias)

                                    //                                    RTL.act (fun () -> setAlias (Some Templates.templatesUser.Username))
                                    ()
                            }
                            |> Promise.start),
                        [|
                            //                            box alias
                            box gun
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

    let inline initialize () =
        promise {
            let! subject, (getFn, setFn) = getApp () |> Setup.render

            do! RTL.sleep 400

            let! alias =
                Dom.waitForSome (fun () -> async { return Atom.get getFn Selectors.Gun.alias })
                |> Async.StartAsPromise

            printfn $"! alias={alias}"

            do! RTL.waitFor (initialSetter getFn setFn)

            let! sortedTaskIdArray =
                Dom.waitForSome
                    (fun () ->
                        async {
                            let sortedTaskIdArray = Atom.get getFn Selectors.Session.sortedTaskIdArray
                            return if sortedTaskIdArray.Length = 4 then Some sortedTaskIdArray else None
                        })
                |> Async.StartAsPromise

            printfn $"! sortedTaskIdArray={sortedTaskIdArray}"

            let cellMapGetter = fun () -> getCellMap subject getFn

            do! RTL.sleep 1000

            return cellMapGetter, (getFn, setFn)
        }
