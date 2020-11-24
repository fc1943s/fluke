namespace Fluke.UI.Frontend.Tests.Core

open Fable.ReactTestingLibrary
open Fable.React
open Feliz
open Feliz.Recoil
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.Shared.Domain
open FSharpPlus
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Hooks


module Setup =
    open State

    let rootWrapper cmp =
        React.memo (fun () ->
            React.strictMode [
                Recoil.root [ root.children [ cmp ] ]
            ]
            |> ReactErrorBoundary.renderCatchFn (fun (error, info) ->
                printfn $"ReactErrorBoundary Error: {info.componentStack} {error}") (str "error"))

    let handlePromise promise =
        promise
        |> Promise.catch (fun ex -> Fable.Core.JS.console.error (box ex))

    let render (cmp: ReactElement) =
        promise {
            let mutable peekFn: (CallbackMethods -> Fable.Core.JS.Promise<unit>) -> Fable.Core.JS.Promise<unit> =
                fun _ -> failwith "called empty callback"

            let cmpWrapper =
                React.memo (fun () ->
                    peekFn <-
                        Recoil.useCallbackRef (fun (setter: CallbackMethods) (fn: CallbackMethods -> Fable.Core.JS.Promise<unit>) ->
                            RTL.waitFor (fn setter |> handlePromise))

                    cmp)

            let subject = RTL.render ((rootWrapper (cmpWrapper ())) ())
            do! RTL.waitFor id
            return subject, peekFn
        }

    let initializeSessionData user peek =
        peek (fun (setter: CallbackMethods) ->
            promise {
                let! sessionData = setter.snapshot.getPromise (Recoil.Selectors.Session.sessionData user.Username)

                match sessionData with
                | Some sessionData -> SessionDataLoader.initializeSessionData user.Username setter sessionData
                | None -> ()
            })

    let taskIdByName name databaseState =
        databaseState.TaskStateMap
        |> Map.pick (fun task _ ->
            match task with
            | { Name = TaskName taskName } when taskName = name -> Some (Recoil.Atoms.Task.taskId task)
            | _ -> None)

    let getCellMap (subject: Bindings.render<_, _>) peek =
        promise {
            let mutable cellMap = Map.empty

            do! peek (fun (setter: CallbackMethods) ->
                    promise {
                        let! dateSequence = setter.snapshot.getPromise Recoil.Selectors.dateSequence
                        let! username = setter.snapshot.getPromise Recoil.Atoms.username
                        let! taskIdList = setter.snapshot.getPromise (Recoil.Atoms.Session.taskIdList username.Value)

                        let! cellList =
                            taskIdList
                            |> List.toArray
                            |> Array.map (fun taskId ->
                                promise {
                                    let! name = setter.snapshot.getPromise (Recoil.Atoms.Task.name taskId)

                                    return
                                        dateSequence
                                        |> List.toArray
                                        |> Array.map (fun date ->
                                            (name, date),
                                            subject.queryByTestId $"cell-{taskId}-{date.DateTime.ToShortDateString ()}")
                                })
                            |> Promise.Parallel

                        cellMap <- cellList |> Array.collect id |> Map.ofArray
                    })

            return cellMap
        }
