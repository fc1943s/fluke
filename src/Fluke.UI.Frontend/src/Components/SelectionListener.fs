namespace Fluke.UI.Frontend.Components

open System
open Fable.React
open Feliz
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks
open Fluke.Shared
open Fluke.UI.Frontend.Bindings


module SelectionListener =

    [<ReactComponent>]
    let SelectionListener () =
        Listener.useKeyPress
            [|
                "Escape"
                "R"
            |]
            (fun get set e ->
                promise {
                    let username = Atoms.getAtomValue get Atoms.username


                    match username with
                    | Some username ->
                        let cellSelectionMap = Atoms.getAtomValue get (Selectors.Session.cellSelectionMap username)

                        if e.key = "Escape" && e.``type`` = "keydown" then
                            if not cellSelectionMap.IsEmpty then
                                cellSelectionMap
                                |> Map.keys
                                |> Seq.iter
                                    (fun taskId ->
                                        Atoms.setAtomValue
                                            set
                                            (Atoms.Task.selectionSet (username, taskId))
                                            (fun _ -> Set.empty))

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

                                newMap
                                |> Map.iter
                                    (fun taskId dates ->
                                        Atoms.setAtomValue
                                            set
                                            (Atoms.Task.selectionSet (username, taskId))
                                            (fun _ -> dates |> Set.map DateId))
                    | None -> ()
                })

        nothing
