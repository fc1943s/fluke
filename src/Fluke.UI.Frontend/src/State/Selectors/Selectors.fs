namespace Fluke.UI.Frontend.State.Selectors

open FsStore
open FsStore.Bindings
open System
open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.State.State

#nowarn "40"


module rec Selectors =
    let rec dateIdArray =
        Store.readSelector
            $"{nameof dateIdArray}"
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
        Store.readSelector
            $"{nameof dateIdAtoms}"
            (fun getter ->
                dateIdArray
                |> Jotai.jotaiUtils.splitAtom
                |> Store.value getter)


    let rec dateIdAtomsByMonth =
        Store.readSelector
            $"{nameof dateIdAtomsByMonth}"
            (fun getter ->
                let dateIdArray = Store.value getter dateIdArray
                let dateIdAtoms = Store.value getter dateIdAtoms

                dateIdArray
                |> Array.indexed
                |> Array.groupBy
                    (fun (_, dateId) ->
                        dateId
                        |> DateId.Value
                        |> Option.map (fun date -> date.Month))
                |> Array.map (fun (_, dates) -> dates |> Array.map (fun (i, _) -> dateIdAtoms.[i])))


    let rec asyncDatabaseIdAtoms =
        Store.selectAtomSyncKeys
            $"{nameof asyncDatabaseIdAtoms}"
            Atoms.Database.name
            Database.Default.Id
            (Guid >> DatabaseId)

    let rec asyncTaskIdAtoms =
        Store.selectAtomSyncKeys $"{nameof asyncTaskIdAtoms}" Atoms.Task.databaseId Task.Default.Id (Guid >> TaskId)

    let rec asyncDeviceIdAtoms =
        Store.selectAtomSyncKeys $"{nameof asyncDeviceIdAtoms}" Atoms.Device.devicePing deviceId (Guid >> DeviceId)
