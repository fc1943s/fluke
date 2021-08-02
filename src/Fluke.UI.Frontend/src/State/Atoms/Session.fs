namespace Fluke.UI.Frontend.State.Atoms

open Fluke.Shared.Domain.UserInteraction
open FsStore
open Fluke.UI.Frontend.State


module Session =


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
//                Store.atomFamilyWithSync (
//                    $"{nameof Events}/{nameof events}",
//                    (fun (_eventId: EventId) -> Event.NoOp)
//                )

    let rec sessionRestored = Store.atom Fluke.root (nameof sessionRestored) false
    let rec position = Store.atom Fluke.root (nameof position) (None: FlukeDateTime option)
    let rec ctrlPressed = Store.atom Fluke.root (nameof ctrlPressed) false
    let rec shiftPressed = Store.atom Fluke.root (nameof shiftPressed) false
