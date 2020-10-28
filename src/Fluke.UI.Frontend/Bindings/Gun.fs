namespace Fluke.UI.Frontend.Bindings

open Fable.React
open Fable.Core.JsInterop
open Feliz
open Feliz.Recoil


module Gun =
    importAll "gun/sea"
    importAll "gun/lib/promise"

    type AppState = { a: {| b: string; c: int |} }

    type IGunUser =
        abstract create: alias:string * pass:string * cb:({| err: string option; pub: string option |} -> unit) -> unit
        abstract auth: alias:string * pass:string * cb:({| err: string option; pub: string option |} -> unit) -> unit
        abstract recall: {| sessionStorage: bool |} * System.Func<{| put: {| alias:string |} option |}, unit> -> unit
        abstract is: {| alias: string option; pub: string option |}
        abstract leave: unit -> unit

    type IGunChainReference<'T> =
        abstract get: string -> IGunChainReference<'U>
        abstract put: 'V -> IGunChainReference<'U>
        abstract user: unit -> IGunUser
        abstract on: event:string * (unit -> unit) -> unit

    let gun: string [] -> IGunChainReference<AppState> = importDefault "gun"

    let createUser (user: IGunUser) username password =
        Promise.create (fun res _err -> user.create (username, password, res))

    let authUser (user: IGunUser) username password =
        Promise.create (fun res _err -> user.auth (username, password, res))
