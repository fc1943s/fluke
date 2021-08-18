namespace Fluke.UI.Frontend.State.Selectors

open FsCore
open Fluke.Shared
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.UserInteraction
open FsCore.BaseModel
open FsStore
open FsStore.Hooks


module rec DateId =
    let collection = Collection (nameof DateId)

    let isToday =
        Store.readSelectorFamily
            Fluke.root
            (nameof isToday)
            (fun (dateId: DateId) getter ->
                let position = Store.value getter Atoms.Session.position

                match position with
                | Some position ->
                    let dayStart = Store.value getter Atoms.User.dayStart

                    Domain.UserInteraction.isToday dayStart position dateId
                | _ -> false)

    let rec hasCellSelection =
        Store.readSelectorFamily
            Fluke.root
            (nameof hasCellSelection)
            (fun (dateId: DateId) getter ->
                let visibleTaskSelectedDateIdMap = Store.value getter Session.visibleTaskSelectedDateIdMap

                visibleTaskSelectedDateIdMap
                |> Map.values
                |> Seq.exists (Set.contains dateId))
