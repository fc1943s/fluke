namespace Fluke.UI.Frontend.Hooks

open Feliz.Recoil
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Feliz
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings


module Hydrate =
    let hydrateDatabase (setter: CallbackMethods) username atomScope (database: Database) =
        setter.set (Atoms.User.databaseIdSet username, Set.add database.Id)

        setter.scopedSet username atomScope (Atoms.Database.name, (username, database.Id), database.Name)
        setter.scopedSet username atomScope (Atoms.Database.owner, (username, database.Id), database.Owner)
        setter.scopedSet username atomScope (Atoms.Database.sharedWith, (username, database.Id), database.SharedWith)
        setter.scopedSet username atomScope (Atoms.Database.dayStart, (username, database.Id), database.DayStart)
        setter.scopedSet username atomScope (Atoms.Database.position, (username, database.Id), database.Position)

    let useHydrateDatabase () = Recoil.useCallbackRef hydrateDatabase

    let hydrateTask (setter: CallbackMethods) username atomScope (task: Task) =
        setter.scopedSet username atomScope (Atoms.Task.name, (username, task.Id), task.Name)
        setter.scopedSet username atomScope (Atoms.Task.information, (username, task.Id), task.Information)
        setter.scopedSet username atomScope (Atoms.Task.duration, (username, task.Id), task.Duration)
        setter.scopedSet username atomScope (Atoms.Task.pendingAfter, (username, task.Id), task.PendingAfter)
        setter.scopedSet username atomScope (Atoms.Task.missedAfter, (username, task.Id), task.MissedAfter)
        setter.scopedSet username atomScope (Atoms.Task.scheduling, (username, task.Id), task.Scheduling)
        setter.scopedSet username atomScope (Atoms.Task.priority, (username, task.Id), task.Priority)

    let useHydrateTask () = Recoil.useCallbackRef hydrateTask
