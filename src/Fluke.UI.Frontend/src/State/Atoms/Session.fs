namespace Fluke.UI.Frontend.State.Atoms

open Fluke.Shared.Domain.UserInteraction
open FsCore.BaseModel
open FsStore
open Fluke.UI.Frontend.State
open FsStore.Model


module Session =
    let collection = Collection (nameof Session)


    //        module rec Events =
//            type EventId = EventId of position: float * guid: Guid
//
//            let newEventId () =
//                EventId (JS.Constructors.Date.now (), Guid.NewTicksGuid ())
//
//            [<RequireQualifiedAccess>]
//            type Event =
//                | AddDatabase of id: EventId * name: DatabaseName * dayStart: FlukeTime
//                | AddTask of id: EventId * name: TaskName
//                | NoOp
//
//            let rec events =
//                Store.createFamilyWithSubscription (
//                    $"{nameof Events}/{nameof events}",
//                    (fun (_eventId: EventId) -> Event.NoOp)
//                )

    let rec position =
        Atom.create
            (StoreAtomPath.IndexedAtomPath (Fluke.root, collection, [], AtomName (nameof position)))
            (AtomType.Atom (None: FlukeDateTime option))

    let rec shiftPressed =
        Atom.create
            (StoreAtomPath.IndexedAtomPath (Fluke.root, collection, [], AtomName (nameof shiftPressed)))
            (AtomType.Atom false)

    let rec ctrlPressed =
        Atom.create
            (StoreAtomPath.IndexedAtomPath (Fluke.root, collection, [], AtomName (nameof ctrlPressed)))
            (AtomType.Atom false)

    let rec hydrateTemplatesPending =
        Atom.create
            (StoreAtomPath.IndexedAtomPath (Fluke.root, collection, [], AtomName (nameof hydrateTemplatesPending)))
            (AtomType.Atom false)
