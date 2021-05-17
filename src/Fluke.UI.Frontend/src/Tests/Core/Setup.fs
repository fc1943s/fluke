namespace Fluke.UI.Frontend.Tests.Core

open Fable.Core.JsInterop
open Fable.ReactTestingLibrary
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


module Setup =
    open State

    import "jest" "@jest/globals"

    let rootWrapper cmp =
        React.memo
            (fun () ->
                React.strictMode [
                    Recoil.root [ root.children [ cmp ] ]
                ]
                |> ReactErrorBoundary.renderCatchFn
                    (fun (error, info) -> printfn $"ReactErrorBoundary Error: {info.componentStack} {error}")
                    (str "error"))

    let handlePromise promise = promise
    //        |> Promise.catch (fun ex -> Fable.Core.JS.console.error (box ex))

    let render (cmp: ReactElement) =
        promise {
            let mutable peekFn : (CallbackMethods -> Fable.Core.JS.Promise<unit>) -> Fable.Core.JS.Promise<unit> =
                fun _ -> failwith "called empty callback"

            let cmpWrapper =
                React.memo
                    (fun () ->
                        peekFn <-
                            Recoil.useCallbackRef
                                (fun (setter: CallbackMethods) (fn: CallbackMethods -> Fable.Core.JS.Promise<unit>) ->
                                    RTL.waitFor (fn setter |> handlePromise))

                        cmp)

            let subject = RTL.render ((rootWrapper (cmpWrapper ())) ())
            do! RTL.waitFor id
            return subject, peekFn
        }

    let baseInitializeSessionData username (setter: CallbackMethods) sessionData =
        Profiling.addTimestamp "state.set[1]"

        sessionData.TaskList
        |> List.map (fun task -> sessionData.TaskStateMap.[task])
        |> List.iter
            (fun taskState ->
                let taskId = Some taskState.Task.Id
                setter.set (Atoms.Task.task taskId, taskState.Task)
                setter.set (Atoms.Task.name taskId, taskState.Task.Name)
                setter.set (Atoms.Task.information taskId, taskState.Task.Information)
                setter.set (Atoms.Task.pendingAfter taskId, taskState.Task.PendingAfter)
                setter.set (Atoms.Task.missedAfter taskId, taskState.Task.MissedAfter)
                setter.set (Atoms.Task.scheduling taskId, taskState.Task.Scheduling)
                setter.set (Atoms.Task.priority taskId, taskState.Task.Priority)
                setter.set (Atoms.Task.attachments taskId, taskState.Attachments)
                setter.set (Atoms.Task.duration taskId, taskState.Task.Duration)

                taskState.CellStateMap
                |> Map.filter
                    (fun _dateId cellState ->
                        (<>) cellState.Status Disabled
                        || not cellState.Attachments.IsEmpty
                        || not cellState.Sessions.IsEmpty)
                |> Map.iter
                    (fun dateId cellState ->
                        setter.set (Atoms.Cell.status (taskState.Task.Id, dateId), cellState.Status)
                        setter.set (Atoms.Cell.attachments (taskState.Task.Id, dateId), cellState.Attachments)
                        setter.set (Atoms.Cell.sessions (taskState.Task.Id, dateId), cellState.Sessions)
                        //                setter.set (Atoms.Cell.selected (taskId, dateId), false)
                        ))

        let taskIdList =
            sessionData.TaskList
            |> List.choose
                (fun task ->
                    sessionData.TaskStateMap
                    |> Map.tryFind task
                    |> Option.map (fun x -> x.Task.Id))

        setter.set (Selectors.Session.selectedTaskIdSet username, taskIdList |> Set.ofList)


    let initializeSessionData user peek =
        peek
            (fun (setter: CallbackMethods) ->
                promise {
                    let! _sessionData = setter.snapshot.getPromise (Selectors.Session.sessionData user.Username)
                    //                    initializeSessionData user.Username setter sessionData
                    ()
                })

    let getCellMap (subject: Bindings.render<_, _>) peek =
        promise {
            let mutable cellMap = Map.empty

            do!
                peek
                    (fun (setter: CallbackMethods) ->
                        promise {
                            let! dateSequence = setter.snapshot.getPromise Selectors.dateSequence
                            let! username = setter.snapshot.getPromise Atoms.username

                            let! selectedTaskIdSet =
                                setter.snapshot.getPromise (Selectors.Session.selectedTaskIdSet username.Value)

                            let! cellList =
                                selectedTaskIdSet
                                |> Set.toArray
                                |> Array.map
                                    (fun taskId ->
                                        promise {
                                            let! taskName = setter.snapshot.getPromise (Atoms.Task.name (Some taskId))

                                            return
                                                dateSequence
                                                |> List.toArray
                                                |> Array.map
                                                    (fun date ->
                                                        ((taskId, taskName), date),
                                                        subject.queryByTestId
                                                            $"cell-{taskId}-{
                                                                                 (date |> FlukeDate.DateTime)
                                                                                     .ToShortDateString ()
                                                            }")
                                        })
                                |> Promise.Parallel

                            cellMap <- cellList |> Array.collect id |> Map.ofArray
                        })

            printfn $"cellMap.Count={cellMap.Count}"
            return cellMap
        }
