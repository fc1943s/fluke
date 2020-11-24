namespace Fluke.UI.Frontend.Bindings

open Fable.Core.JsInterop
open Feliz.Recoil


module Gun =
    importAll "gun/sea"
    importAll "gun/lib/promise"

    type AppState = { a: {| b: string; c: int |} }

    type IGunUser =
        abstract create: alias:string * pass:string * cb:({| err: string option; pub: string option |} -> unit) -> unit
        abstract auth: alias:string * pass:string * cb:({| err: string option; pub: string option |} -> unit) -> unit
        abstract recall: {| sessionStorage: bool |} * System.Func<{| put: {| alias: string |} option |}, unit> -> unit
        abstract is: {| alias: string option; pub: string option |}
        abstract leave: unit -> unit

    type IGunChainReference<'T> =
        abstract get: string -> IGunChainReference<'U>
        abstract set: 'V -> IGunChainReference<'U>
        abstract put: 'V -> IGunChainReference<'U>
        abstract user: unit -> IGunUser
        abstract on: ('T -> unit) -> unit
        abstract off: unit -> unit
        abstract once: ('T -> unit) -> unit
        abstract on: event:string * (unit -> unit) -> unit

    let gun: {| peers: string []; radisk: bool |} -> IGunChainReference<AppState> = importDefault "gun"

    let createUser (user: IGunUser) username password =
        Promise.create (fun res err ->
            try
                user.create (username, password, res)
            with ex ->
                printfn $"createUser error: {ex}"
                err ex)

    let authUser (user: IGunUser) username password =
        Promise.create (fun res err ->
            try
                user.auth (username, password, res)
            with ex ->
                printfn "authUser error: {ex}"
                err ex)
