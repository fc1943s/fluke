namespace Fluke.UI.Frontend.State.Selectors

open FsStore.State
open FsCore.BaseModel
open FsJs
open FsStore

open System
open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain.State

#nowarn "40"


module rec Selectors =
    let interval = 750

    let readSelector name getFn =
        Store.readSelector Fluke.root name getFn

    let selectAtomSyncKeys name atom =
        Store.selectAtomSyncKeys Fluke.root name atom Database.Default.Id (Guid >> DatabaseId)

    let rec dateIdArray =
        readSelector
            (nameof dateIdArray)
            (fun getter ->
                let position = Store.value getter Atoms.Session.position

                match position with
                | Some position ->
                    let daysBefore = Store.value getter Atoms.User.daysBefore
                    let daysAfter = Store.value getter Atoms.User.daysAfter
                    let dayStart = Store.value getter Atoms.User.dayStart
                    let dateId = dateId dayStart position
                    let (DateId referenceDay) = dateId

                    referenceDay
                    |> List.singleton
                    |> Rendering.getDateSequence (daysBefore, daysAfter)
                    |> List.map DateId
                    |> List.toArray
                | _ -> [||])


    let rec dateIdAtoms =
        readSelector
            (nameof dateIdAtoms)
            (fun getter ->
                dateIdArray
                |> Store.splitAtom
                |> Store.value getter)


    let rec dateIdAtomsByMonth =
        readSelector
            (nameof dateIdAtomsByMonth)
            (fun getter ->
                let dateIdAtoms = Store.value getter dateIdAtoms

                dateIdAtoms
                |> Store.waitForAll
                |> Store.value getter
                |> Array.indexed
                |> Array.groupBy
                    (fun (_, dateId) ->
                        dateId
                        |> DateId.Value
                        |> Option.map (fun date -> date.Month))
                |> Array.map (fun (_, dates) -> dates |> Array.map (fun (i, _) -> dateIdAtoms.[i])))


    let rec asyncDatabaseIdAtoms =
        Store.selectAtomSyncKeys
            Fluke.root
            (nameof asyncDatabaseIdAtoms)
            Atoms.Database.name
            Database.Default.Id
            (Guid >> DatabaseId)

    let rec asyncTaskIdAtoms =
        Store.selectAtomSyncKeys
            Fluke.root
            (nameof asyncTaskIdAtoms)
            Atoms.Task.databaseId
            Task.Default.Id
            (Guid >> TaskId)

    let rec asyncAttachmentIdAtoms =
        Store.selectAtomSyncKeys
            Fluke.root
            (nameof asyncAttachmentIdAtoms)
            Atoms.Attachment.parent
            AttachmentId.Default
            (Guid >> AttachmentId)

    let rec asyncDeviceIdAtoms =
        Store.selectAtomSyncKeys
            Fluke.root
            (nameof asyncDeviceIdAtoms)
            Atoms.Device.devicePing
            Dom.deviceInfo.DeviceId
            (Guid >> DeviceId)
