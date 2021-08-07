namespace FsStore

open FsCore.Model
open FsStore.Model
open Microsoft.FSharp.Core.Operators
open FsJs
open FsStore.Bindings


module Atoms =
    let rec logLevel = Store.atomWithStorage FsStore.root (nameof logLevel) DEFAULT_LOG_LEVEL
    let rec showDebug = Store.atomWithStorage FsStore.root (nameof showDebug) false
    let rec gunOptions = Store.atomWithStorage FsStore.root (nameof gunOptions) (GunOptions.Sync [||])
    let rec hubUrl = Store.atomWithStorage FsStore.root (nameof hubUrl) (None: string option)
    let rec gunTrigger = Store.atom FsStore.root (nameof gunTrigger) 0
    let rec hubTrigger = Store.atom FsStore.root (nameof hubTrigger) 0
    let rec isTesting = Store.atom FsStore.root (nameof isTesting) Dom.deviceInfo.IsTesting
    let rec username = Store.atom FsStore.root (nameof username) (None: Username option)
    let rec gunKeys = Store.atom FsStore.root (nameof gunKeys) Gun.GunKeys.Default
