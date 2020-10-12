namespace Fluke.UI.Frontend.Hooks

open FSharpPlus
open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.Shared


module UserLoader =
    let hook =
        React.memo (fun () ->
            let username = Recoil.useValue Recoil.Atoms.username
            let treeStateMap = Recoil.useValue Recoil.Selectors.treeStateMap

            let loadUser =
                Recoil.useCallbackRef (fun setter ->
                    async {
                        //                            let! treeStateMap = setter.snapshot.getAsync Recoil.Selectors.treeStateMap

                        match treeStateMap with
                        | Some (user, treeStateMap) ->
                            let availableTreeIds =
                                treeStateMap
                                |> Map.toList
                                |> List.sortBy (fun (id, treeState) -> treeState.Name)
                                |> List.map fst

                            setter.set (Recoil.Atoms.username, Some user.Username)
                            setter.set (Recoil.Atoms.Session.user user.Username, Some user)
                            setter.set (Recoil.Atoms.Session.availableTreeIds user.Username, availableTreeIds)
                            setter.set (Recoil.Atoms.treeStateMap, treeStateMap)
                        | None -> ()
                    }
                    |> Async.StartImmediate)

            React.useEffect
                ((fun () ->
                    match username with
                    | Some _ -> ()
                    | None -> loadUser ()),
                 [|
                     username :> obj
                 |])

            nothing)
