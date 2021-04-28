namespace Fluke.UI.Frontend.Hooks

open Feliz.Recoil
open Fluke.Shared.Domain.State
open Feliz
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings


module HydrateDatabase =
    let useHydrateDatabase () =
        Recoil.useCallbackRef
            (fun (setter: CallbackMethods) atomScope (data: Database) ->
                let set atom value =
                    setter.scopedSet atomScope (atom, (Some data.Id), value)

                set Atoms.Database.name data.Name
                set Atoms.Database.owner (Some data.Owner)
                set Atoms.Database.sharedWith data.SharedWith
                set Atoms.Database.dayStart data.DayStart
                set Atoms.Database.position data.Position)
