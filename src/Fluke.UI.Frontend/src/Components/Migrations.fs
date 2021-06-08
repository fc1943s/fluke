namespace Fluke.UI.Frontend.Components

open Fluke.Shared
open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State


module Migrations =
    [<ReactComponent>]
    let Migrations (input: {| Username: Username |}) =
        Recoil.useEffect (
            (fun setter ->
                promise {
                    let setter = setter ()
                    let! joinSet = setter.snapshot.getPromise (Atoms.User.joinSet input.Username)
                    let! databaseIdSet = setter.snapshot.getPromise (Atoms.User.databaseIdSet input.Username)

                    if not joinSet.IsEmpty then
                        let databaseIdSet =
                            joinSet
                            |> Set.choose
                                (function
                                | Join.Database databaseId -> Some databaseId
                                | _ -> None)
                            |> Set.union databaseIdSet

                        let! _ =
                            databaseIdSet
                            |> Set.toArray
                            |> Array.map
                                (fun databaseId ->
                                    promise {
                                        let! taskIdSet =
                                            setter.snapshot.getPromise (
                                                Atoms.Database.taskIdSet (input.Username, databaseId)
                                            )

                                        let newTaskIdSet =
                                            joinSet
                                            |> Set.choose
                                                (function
                                                | Join.Task (databaseId', taskId) when databaseId' = databaseId ->
                                                    Some taskId
                                                | _ -> None)
                                            |> Set.union taskIdSet

                                        setter.set (Atoms.Database.taskIdSet (input.Username, databaseId), newTaskIdSet)
                                    })
                            |> Promise.Parallel

                        setter.set (Atoms.User.databaseIdSet input.Username, databaseIdSet)

                        printfn "# clearing joinSet"
                        setter.set (Atoms.User.joinSet input.Username, Set.empty)
                }),
            [|
                box input.Username
            |]
        )

        nothing
