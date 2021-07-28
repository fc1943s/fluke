namespace FsUi.Bindings

open Fable.Core.JsInterop
open Browser.Types


module Rooks =
    type IRooks =
        abstract useKey : string [] -> (KeyboardEvent -> unit) -> {| eventTypes: string [] |} -> unit

    let Rooks: IRooks = importAll "rooks"

[<AutoOpen>]
module RooksMagic =
    let Rooks = Rooks.Rooks
