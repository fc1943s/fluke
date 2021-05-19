namespace Fluke.UI.Frontend.Hooks

open Feliz.Recoil
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Feliz
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings


module Hydrate =
    let hydrateDatabase (setter: CallbackMethods) atomScope (data: Database) =
        setter.scopedSet atomScope (Atoms.Database.name, data.Id, data.Name)
        setter.scopedSet atomScope (Atoms.Database.owner, data.Id, data.Owner)
        setter.scopedSet atomScope (Atoms.Database.sharedWith, data.Id, data.SharedWith)
        setter.scopedSet atomScope (Atoms.Database.dayStart, data.Id, data.DayStart)
        setter.scopedSet atomScope (Atoms.Database.position, data.Id, data.Position)

    let useHydrateDatabase () = Recoil.useCallbackRef hydrateDatabase

    let hydrateTask (setter: CallbackMethods) atomScope databaseId (data: Task) =
        setter.scopedSet atomScope (Atoms.Task.name, data.Id, data.Name)
        setter.scopedSet atomScope (Atoms.Task.databaseId, data.Id, databaseId)
        setter.scopedSet atomScope (Atoms.Task.information, data.Id, data.Information)
        setter.scopedSet atomScope (Atoms.Task.duration, data.Id, data.Duration)
        setter.scopedSet atomScope (Atoms.Task.pendingAfter, data.Id, data.PendingAfter)
        setter.scopedSet atomScope (Atoms.Task.missedAfter, data.Id, data.MissedAfter)
        setter.scopedSet atomScope (Atoms.Task.scheduling, data.Id, data.Scheduling)
        setter.scopedSet atomScope (Atoms.Task.priority, data.Id, data.Priority)

    let useHydrateTask () = Recoil.useCallbackRef hydrateTask
