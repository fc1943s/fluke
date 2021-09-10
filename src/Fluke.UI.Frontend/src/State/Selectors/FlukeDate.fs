namespace Fluke.UI.Frontend.State.Selectors

open FsCore
open Fluke.Shared
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.UserInteraction
open FsCore.BaseModel
open FsStore
open FsStore.Bindings.Gun
open FsStore.Model



module rec FlukeDate =
    let collection = Collection (nameof FlukeDate)

    let inline formatDate date =
        date
        |> FlukeDate.Stringify
        |> AtomKeyFragment
        |> List.singleton

    let inline readSelectorFamily name =
        Atom.readSelectorFamily
            (fun (date: FlukeDate) ->
                StoreAtomPath.ValueAtomPath (Fluke.root, collection, formatDate date, AtomName name))

    let isToday =
        readSelectorFamily
            (nameof isToday)
            (fun (date: FlukeDate) getter ->
                let position = Atom.get getter Atoms.Session.position

                match position with
                | Some position ->
                    let dayStart = Atom.get getter Atoms.User.dayStart

                    Domain.UserInteraction.isToday dayStart position date
                | _ -> false)

    let rec hasCellSelection =
        readSelectorFamily
            (nameof hasCellSelection)
            (fun (date: FlukeDate) getter ->
                let visibleTaskSelectedDateMap = Atom.get getter Session.visibleTaskSelectedDateMap

                visibleTaskSelectedDateMap
                |> Map.values
                |> Seq.exists (Set.contains date))
