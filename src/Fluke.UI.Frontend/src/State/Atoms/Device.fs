namespace Fluke.UI.Frontend.State.Atoms

open Fluke.Shared
open Fluke.UI.Frontend.State.State
open Fluke.UI.Frontend.State
open FsCore.Model
open FsStore


module rec Device =
    let collection = Collection (nameof Device)

    let inline deviceIdIdentifier deviceId =
        deviceId
        |> DeviceId.Value
        |> string
        |> List.singleton

    let inline atomFamilyWithSync name defaultValueFn =
        Store.atomFamilyWithSync Fluke.root collection name defaultValueFn deviceIdIdentifier

    let rec devicePing = atomFamilyWithSync (nameof devicePing) (fun (_: DeviceId) -> Ping "0")
