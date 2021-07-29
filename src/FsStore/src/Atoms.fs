namespace FsStore

open FsCore.Model
open Microsoft.FSharp.Core.Operators
open FsJs
open FsStore.Bindings


module Atoms =
    let rec logLevel =
        Store.atomWithStorage (
            FsStore.collection,
            nameof logLevel,
            (if Dom.isDebug () then
                 Model.LogLevel.Debug
             else
                 Model.LogLevel.Information)
        )

    let rec gunPeers = Store.atomWithStorage (FsStore.collection, $"{nameof gunPeers}", (Some [||]: string [] option))
    let rec hubUrl = Store.atomWithStorage (FsStore.collection, $"{nameof hubUrl}", (None: string option))
    let rec gunTrigger = Store.atom ($"{nameof gunTrigger}", 0)
    let rec hubTrigger = Store.atom ($"{nameof hubTrigger}", 0)
    let rec isTesting = Store.atom ($"{nameof isTesting}", Dom.deviceInfo.IsTesting)
    let rec username = Store.atom ($"{nameof username}", (None: Username option))
    let rec gunKeys = Store.atom ($"{nameof gunKeys}", Gun.GunKeys.Default)
