namespace Fluke.Shared

module Api =
    type IApi =
        {
            set: string * string -> Async<bool>
            get: string -> Async<string>
            filter: string -> Async<string []>
        }

    [<RequireQualifiedAccess>]
    type Action =
        | Connect
        | Set of string * string
        | Get of string
        | Filter of string

    [<RequireQualifiedAccess>]
    type Response =
        | ConnectResult
        | SetResult of bool
        | GetResult of string option
        | FilterResult of string []

    let endpoint = "/ws"
