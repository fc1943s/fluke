namespace Fluke.UI.Frontend.Bindings

open Fable.Core.JsInterop


module Electron =
    let electron : {| ipcRenderer: {| send: string -> unit |} |} =
        if not JS.deviceInfo.IsTesting && JS.deviceInfo.IsElectron then
            unbox importDynamic "electron"
        else
            unbox null
