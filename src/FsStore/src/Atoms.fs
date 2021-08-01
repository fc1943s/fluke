namespace FsStore

open FsCore.Model
open FsStore.Model
open Microsoft.FSharp.Core.Operators
open FsJs
open FsStore.Bindings


module Atoms =
    let rec logLevel = Store.atomWithStorage FsStore.collection $"{nameof logLevel}" DEFAULT_LOG_LEVEL
    let rec gunOptions = Store.atomWithStorage FsStore.collection $"{nameof gunOptions}" (GunOptions.Sync [||])
    let rec hubUrl = Store.atomWithStorage FsStore.collection $"{nameof hubUrl}" (None: string option)
    let rec gunTrigger = Store.atom $"{nameof gunTrigger}" 0
    let rec hubTrigger = Store.atom $"{nameof hubTrigger}" 0
    let rec isTesting = Store.atom $"{nameof isTesting}" Dom.deviceInfo.IsTesting
    let rec username = Store.atom $"{nameof username}" (None: Username option)
    let rec gunKeys = Store.atom $"{nameof gunKeys}" Gun.GunKeys.Default
