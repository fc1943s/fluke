namespace Fluke.UI.Frontend.Components

open System
open Fable.React
open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks
open Fluke.Shared


module SelectionListener =

    [<ReactComponent>]
    let SelectionListener () =
        Listener.useKeyPress
            (fun setter e ->
                async {
                    let! username = setter.snapshot.getAsync Atoms.username

                    match username with
                    | Some username ->
                        let! cellSelectionMap = setter.snapshot.getAsync (Selectors.Session.cellSelectionMap username)

                        if e.key = "Escape" && e.``type`` = "keydown" then
                            if not cellSelectionMap.IsEmpty then
                                setter.set (Selectors.Session.cellSelectionMap username, Map.empty)

                            setter.set (Atoms.User.cellMenuOpened username, None)

                        if e.key = "R" && e.``type`` = "keydown" then
                            if not cellSelectionMap.IsEmpty then
                                let newMap =
                                    if cellSelectionMap.Count = 1 then
                                        cellSelectionMap
                                        |> Map.toList
                                        |> List.map
                                            (fun (taskId, dates) ->
                                                let date =
                                                    dates
                                                    |> Seq.item (Random().Next (0, dates.Count - 1))

                                                taskId, Set.singleton date)
                                        |> Map.ofList
                                    else
                                        let key =
                                            cellSelectionMap
                                            |> Map.keys
                                            |> Seq.item (Random().Next (0, cellSelectionMap.Count - 1))

                                        Map.singleton key cellSelectionMap.[key]

                                setter.set (Selectors.Session.cellSelectionMap username, newMap)
                    | None -> ()
                })

        nothing
