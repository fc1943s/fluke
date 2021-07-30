namespace Fluke.UI.Frontend.State.Selectors

open Fluke.Shared
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.UserInteraction
open FsStore


module rec DateId =
    let isToday =
        Store.readSelectorFamily
            $"{nameof DateId}/{nameof isToday}"
            (fun (dateId: DateId) getter ->
                let position = Store.value getter Atoms.Session.position

                match position with
                | Some position ->
                    let dayStart = Store.value getter Atoms.User.dayStart

                    Domain.UserInteraction.isToday dayStart position dateId
                | _ -> false)

    let rec hasCellSelection =
        Store.readSelectorFamily
            $"{nameof DateId}/{nameof hasCellSelection}"
            (fun (dateId: DateId) getter ->
                let visibleTaskSelectedDateIdMap = Store.value getter Session.visibleTaskSelectedDateIdMap

                visibleTaskSelectedDateIdMap
                |> Map.values
                |> Seq.exists (Set.contains dateId))
