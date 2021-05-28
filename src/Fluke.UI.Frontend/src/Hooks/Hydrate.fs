namespace Fluke.UI.Frontend.Hooks

open Feliz.Recoil
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Feliz
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings


module Hydrate =
    let hydrateDatabase (setter: CallbackMethods) username atomScope (data: Database) =
        setter.scopedSet username atomScope (Atoms.Database.name, (username, data.Id), data.Name)
        setter.scopedSet username atomScope (Atoms.Database.owner, (username, data.Id), data.Owner)
        setter.scopedSet username atomScope (Atoms.Database.sharedWith, (username, data.Id), data.SharedWith)
        setter.scopedSet username atomScope (Atoms.Database.dayStart, (username, data.Id), data.DayStart)
        setter.scopedSet username atomScope (Atoms.Database.position, (username, data.Id), data.Position)

    let useHydrateDatabase () = Recoil.useCallbackRef hydrateDatabase

    let hydrateTask (setter: CallbackMethods) username atomScope databaseId (data: Task) =
        setter.scopedSet username atomScope (Atoms.Task.name, (username, data.Id), data.Name)
        setter.scopedSet username atomScope (Atoms.Task.databaseId, (username, data.Id), databaseId)
        setter.scopedSet username atomScope (Atoms.Task.information, (username, data.Id), data.Information)
        setter.scopedSet username atomScope (Atoms.Task.duration, (username, data.Id), data.Duration)
        setter.scopedSet username atomScope (Atoms.Task.pendingAfter, (username, data.Id), data.PendingAfter)
        setter.scopedSet username atomScope (Atoms.Task.missedAfter, (username, data.Id), data.MissedAfter)
        setter.scopedSet username atomScope (Atoms.Task.scheduling, (username, data.Id), data.Scheduling)
        setter.scopedSet username atomScope (Atoms.Task.priority, (username, data.Id), data.Priority)

    let useHydrateTask () = Recoil.useCallbackRef hydrateTask
