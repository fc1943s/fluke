namespace Fluke.UI.Frontend.Bindings

open Fable.Core.JsInterop
open Fable.Core
open Feliz.Recoil


module rec Gun =
    importAll "gun/sea"
    importAll "gun/lib/promise"

    //    if JS.isTesting then
//        importAll "gun/lib/radix"
//
    type AppState = { a: {| b: string; c: int |} }

    type GunKeys =
        {
            pub: string
            epub: string
            priv: string
            epriv: string
        }

    type IGunUser =
        abstract create :
            alias: string * pass: string * cb: ({| err: string option; pub: string option |} -> unit) ->
            unit

        abstract auth :
            alias: string * pass: string * cb: ({| err: string option; pub: string option |} -> unit) ->
            unit

        abstract recall :
            {| sessionStorage: bool |}
            * System.Func<{| put: {| alias: string |} option
                             sea: GunKeys |}, unit> ->
            unit

        abstract is : {| alias: string option; pub: string option |}
        abstract ``_`` : {| sea: GunKeys |}
        abstract get : string -> IGunChainReference<_>
        abstract leave : unit -> unit

    type IGunChainReference<'T> =
        abstract get : string -> IGunChainReference<'U>
        abstract set : 'V -> IGunChainReference<'U>
        abstract put : 'V -> IGunChainReference<'U>
        abstract user : unit -> IGunUser
        abstract on : ('T -> unit) -> unit
        abstract off : unit -> unit
        abstract once : ('T -> unit) -> unit
        abstract on : event: string * (unit -> unit) -> unit

    type GunProps =
        {
            peers: string [] option
            radisk: bool option
            localStorage: bool option
        }

    let gun : GunProps -> IGunChainReference<AppState> = importDefault "gun/gun"

    [<ImportAll "@altrx/gundb-react-hooks">]
    let gunHooks : {| useGunKeys: obj -> (unit -> obj) -> bool -> obj
                      useGunState: IGunChainReference<obj> -> {| appKeys: GunKeys; sea: obj |} -> {| fields: obj
                                                                                                     put: obj -> JS.Promise<obj>
                                                                                                     remove: string -> JS.Promise<obj> |} |} =
        jsNative

    let createUser (user: IGunUser) username password =
        Promise.create
            (fun res err ->
                try
                    user.create (username, password, res)
                with ex ->
                    printfn $"createUser error: {ex}"
                    err ex)

    let authUser (user: IGunUser) username password =
        Promise.create
            (fun res err ->
                try
                    user.auth (username, password, res)
                with ex ->
                    printfn "authUser error: {ex}"
                    err ex)
