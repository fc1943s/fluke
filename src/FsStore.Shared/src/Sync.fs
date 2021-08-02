namespace FsStore.Shared


module rec Sync =
    [<RequireQualifiedAccess>]
    type Request =
        | Connect of username: string
        | Set of username: string * atomPath: string * value: string
        | Get of username: string * atomPath: string
        | Filter of username: string * collection: string

    [<RequireQualifiedAccess>]
    type Response =
        | ConnectResult
        | SetResult of ok: bool
        | GetResult of atomPath: string * value: string option
        | FilterResult of atomPathArray: string []

    let endpoint = $"/{nameof Sync}"
