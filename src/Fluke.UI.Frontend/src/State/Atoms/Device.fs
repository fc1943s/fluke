namespace Fluke.UI.Frontend.State.Atoms

open Fluke.UI.Frontend.Bindings
open Fluke.Shared
open Fluke.UI.Frontend.State.State


module rec Device =
    let rec devicePing =
        Store.atomFamilyWithSync (
            $"{nameof Device}/{nameof devicePing}",
            (fun (_deviceId: DeviceId) -> Ping "0"),
            (DeviceId.Value >> string >> List.singleton)
        )
