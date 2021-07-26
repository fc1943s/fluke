namespace Fluke.Shared

open Fluke.Shared.Domain.UserInteraction

module Api =
    [<RequireQualifiedAccess>]
    type Action =
        | Connect of Username
        | Set of Username * string * string
        | Get of Username * string
        | Filter of Username * string

    [<RequireQualifiedAccess>]
    type Response =
        | ConnectResult
        | SetResult of bool
        | GetResult of string option
        | FilterResult of string []

    let endpoint = "/ws"
