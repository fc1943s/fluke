namespace Fluke.UI.Frontend.Bindings

open Fable.Core.JsInterop
open Feliz.Recoil
open Thoth.Json


module rec Gun =
    importAll "gun/sea"
    importAll "gun/lib/promise"

    //    if JS.isTesting then
//        importAll "gun/lib/radix"
//
    type GunKeys =
        {
            pub: string
            epub: string
            priv: string
            epriv: string
        }

    type IGunUser =
        abstract create :
            alias: string
            * pass: string
            * cb: ({| err: string option
                      pub: string option |} -> unit) ->
            unit

        abstract auth :
            alias: string
            * pass: string
            * cb: ({| err: string option
                      pub: string option |} -> unit) ->
            unit

        abstract recall :
            {| sessionStorage: bool |}
            * System.Func<{| put: {| alias: string |} option
                             sea: GunKeys |}, unit> ->
            unit

        abstract is :
            {| alias: string option
               pub: string option |}

        abstract ``_`` : {| sea: GunKeys |}
        abstract get : string -> IGunChainReference<_>
        abstract leave : unit -> unit

    type IGunChainReference<'T> =
        abstract get : string -> IGunChainReference<'U>
        abstract set : 'V -> IGunChainReference<'U>
        abstract put : 'V -> IGunChainReference<'U>
        abstract user : unit -> IGunUser
        abstract on : ('T -> unit) -> unit
        abstract off : unit -> IGunChainReference<'T>
        abstract once : ('T -> unit) -> unit
        abstract on : event: string * (unit -> unit) -> unit

    type GunProps =
        {
            peers: string [] option
            radisk: bool option
            localStorage: bool option
        }

    let gun : GunProps -> IGunChainReference<obj> = importDefault "gun/gun"

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

    let getGunAtomNode<'T> (gun: IGunChainReference<obj>) (gunAtomKey: string) =
        (gun, gunAtomKey.Split "/" |> Array.toList)
        ||> List.fold (fun result -> result.get)
        :?> IGunChainReference<'T>

    let inline putGunAtomNode<'T> (gun: IGunChainReference<obj>) (value: 'T) =
        gun.put (Encode.Auto.toString (0, value))
        |> ignore

    let inline deserializeGunAtomNode (data: obj) =
        match data :?> string option with
        | Some data ->

            let newValue = Decode.Auto.fromString<'T> data

            match newValue with
            | Ok newValue -> Some newValue
            | Error error ->
                Browser.Dom.console.error error
                failwithf "Deserialize error"
                None

        | None -> None
