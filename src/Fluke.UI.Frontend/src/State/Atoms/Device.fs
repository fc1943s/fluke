namespace Fluke.UI.Frontend.State.Atoms

open Fluke.Shared
open Fluke.UI.Frontend.State.State
open Fluke.UI.Frontend.State
open FsStore


module rec Device =
    let inline deviceIdIdentifier deviceId =
        deviceId
        |> DeviceId.Value
        |> string
        |> List.singleton

    let inline atomFamilyWithSync atomPath defaultValueFn =
        Store.atomFamilyWithSync (Fluke.collection, atomPath, defaultValueFn, deviceIdIdentifier)

    let rec devicePing =
        atomFamilyWithSync $"{nameof Device}/{nameof devicePing}" (fun (_deviceId: DeviceId) -> Ping "0")
