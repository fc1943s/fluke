namespace Fluke.UI.Frontend

open Fable.Remoting.Client
open Fluke.Shared

module Sync =
    open Sync

    let api =
        Remoting.createApi ()
        |> Remoting.withBinarySerialization
        |> Remoting.buildProxy<Api>
