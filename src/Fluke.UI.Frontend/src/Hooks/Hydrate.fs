namespace Fluke.UI.Frontend.Hooks

open Feliz.Recoil
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Feliz
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings


module Hydrate =
    let hydrateDatabase (setter: CallbackMethods) atomScope (data: Database) =
        setter.scopedSet atomScope (Atoms.Database.name, (Some data.Id), data.Name)
        setter.scopedSet atomScope (Atoms.Database.owner, (Some data.Id), data.Owner)
        setter.scopedSet atomScope (Atoms.Database.sharedWith, (Some data.Id), data.SharedWith)
        setter.scopedSet atomScope (Atoms.Database.dayStart, (Some data.Id), data.DayStart)
        setter.scopedSet atomScope (Atoms.Database.position, (Some data.Id), data.Position)

    let useHydrateDatabase () = Recoil.useCallbackRef hydrateDatabase

    let hydrateTask (setter: CallbackMethods) atomScope databaseId (data: Task) =
        setter.scopedSet atomScope (Atoms.Task.name, (Some data.Id), data.Name)
        setter.scopedSet atomScope (Atoms.Task.databaseId, (Some data.Id), databaseId)
        setter.scopedSet atomScope (Atoms.Task.information, (Some data.Id), data.Information)
        setter.scopedSet atomScope (Atoms.Task.duration, (Some data.Id), data.Duration)
        setter.scopedSet atomScope (Atoms.Task.pendingAfter, (Some data.Id), data.PendingAfter)
        setter.scopedSet atomScope (Atoms.Task.missedAfter, (Some data.Id), data.MissedAfter)
        setter.scopedSet atomScope (Atoms.Task.scheduling, (Some data.Id), data.Scheduling)
        setter.scopedSet atomScope (Atoms.Task.priority, (Some data.Id), data.Priority)

    let useHydrateTask () = Recoil.useCallbackRef hydrateTask
