namespace FsStore.Shared

open FsCore.Model


module rec Sync =
    [<RequireQualifiedAccess>]
    type Request =
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

    let endpoint = nameof Sync
