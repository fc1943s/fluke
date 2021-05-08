namespace Fluke.UI.Frontend.Hooks

open Feliz.Recoil
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Feliz
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings


module Hydrate =
    let useHydrateDatabase () =
        Recoil.useCallbackRef
            (fun (setter: CallbackMethods) atomScope (data: Database) ->
                let set atom value =
                    setter.scopedSet atomScope (atom, (Some data.Id), value)

                set Atoms.Database.name data.Name
                set Atoms.Database.owner data.Owner
                set Atoms.Database.sharedWith data.SharedWith
                set Atoms.Database.dayStart data.DayStart
                set Atoms.Database.position data.Position)

    let useHydrateInformation () =
        Recoil.useCallbackRef
            (fun (setter: CallbackMethods) atomScope (data: Information) ->
                let informationId = data |> Information.Id

                let set atom value =
                    setter.scopedSet atomScope (atom, (Some informationId), value)

                set Atoms.Information.name (data |> Information.Name))

    let useHydrateTask () =
        let hydrateInformation = useHydrateInformation ()

        Recoil.useCallbackRef
            (fun (setter: CallbackMethods) atomScope databaseId (data: Task) ->
                let set atom value =
                    setter.scopedSet atomScope (atom, (Some data.Id), value)

                hydrateInformation atomScope data.Information

                set Atoms.Task.name data.Name
                set Atoms.Task.databaseId databaseId
                set Atoms.Task.informationId (data.Information |> Information.Id)
                set Atoms.Task.duration data.Duration
                set Atoms.Task.pendingAfter data.PendingAfter
                set Atoms.Task.missedAfter data.MissedAfter
                set Atoms.Task.scheduling data.Scheduling
                set Atoms.Task.priority data.Priority)
