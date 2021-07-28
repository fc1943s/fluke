namespace Fluke.UI.Frontend.State.Atoms

open Fluke.Shared
open Fluke.UI.Frontend.State.State
open FsStore


module rec Device =
    let rec devicePing =
        Store.atomFamilyWithSync (
            State.collection,
            $"{nameof Device}/{nameof devicePing}",
            (fun (_deviceId: DeviceId) -> Ping "0"),
            (DeviceId.Value >> string >> List.singleton)
        )
