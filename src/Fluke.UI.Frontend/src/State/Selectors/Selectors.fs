namespace Fluke.UI.Frontend.State.Selectors

open FsStore.Model
open FsStore.State
open FsCore.BaseModel
open FsStore
open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain.State

#nowarn "40"


module rec Selectors =
    let interval2 = 750

    let rec dateArray =
        Atom.readSelector
            (StoreAtomPath.RootAtomPath (Fluke.root, AtomName (nameof dateArray)))
            (fun getter ->
                let position = Atom.get getter Atoms.Session.position

                match position with
                | Some position ->
                    let daysBefore = Atom.get getter Atoms.User.daysBefore
                    let daysAfter = Atom.get getter Atoms.User.daysAfter
                    let dayStart = Atom.get getter Atoms.User.dayStart
                    let date = getReferenceDay dayStart position

                    date
                    |> List.singleton
                    |> Rendering.getDateSequence (daysBefore, daysAfter)
                    |> List.toArray
                | _ -> [||])


    let rec dateAtoms =
        Atom.readSelector
            (StoreAtomPath.RootAtomPath (Fluke.root, AtomName (nameof dateAtoms)))
            (fun getter -> dateArray |> Atom.split |> Atom.get getter)


    let rec dateAtomsByMonth =
        Atom.readSelector
            (StoreAtomPath.RootAtomPath (Fluke.root, AtomName (nameof dateAtomsByMonth)))
            (fun getter ->
                let dateAtoms = Atom.get getter dateAtoms

                dateAtoms
                |> Atom.waitForAll
                |> Atom.get getter
                |> Array.indexed
                |> Array.groupBy (fun (_, date) -> date.Month)
                |> Array.map (fun (_, dates) -> dates |> Array.map (fun (i, _) -> dateAtoms.[i])))


    let rec asyncDatabaseIdAtoms =
        Engine.subscribeCollection Fluke.root Atoms.Database.collection (Engine.parseGuidKey DatabaseId)

    let rec asyncTaskIdAtoms = Engine.subscribeCollection Fluke.root Atoms.Task.collection (Engine.parseGuidKey TaskId)

    let rec asyncAttachmentIdAtoms =
        Engine.subscribeCollection Fluke.root Atoms.Attachment.collection (Engine.parseGuidKey AttachmentId)

    let rec asyncDeviceIdAtoms =
        Engine.subscribeCollection Fluke.root Atoms.Device.collection (Engine.parseGuidKey DeviceId)
