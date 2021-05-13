namespace Fluke.UI.Frontend.Bindings

open Fable.Core.JsInterop
open Feliz.Recoil


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
        abstract on : (string -> string -> unit) -> unit
        abstract on : event: string * (unit -> unit) -> unit
        abstract map : unit -> IGunChainReference<'U>
        abstract off : unit -> IGunChainReference<'T>
        abstract once : (string -> unit) -> unit

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

    let getGunAtomNode (gun: IGunChainReference<'U> option) (gunAtomKey: string) =
        (box gun :?> IGunChainReference<'T> option, gunAtomKey.Split "/" |> Array.toList)
        ||> List.fold
                (fun result node ->
                    result
                    |> Option.map (fun result -> result.get node))

    let inline encode text =
        Fable.SimpleJson.SimpleJson.stringify text
    //        Thoth.Json.Encode.Auto.toString (0, text)
//        ""

    let inline decode< 'T> data =
        data
        |> Fable.SimpleJson.SimpleJson.parse
        |> Fable.SimpleJson.SimpleJson.toPlainObject
        :?> 'T
        |> Some
    //        Thoth.Json.Decode.Auto.unsafeFromString data |> Some
//        None
//        None

    let inline putGunAtomNode (gun: IGunChainReference<_>) (value: string) =
        gun.put (if box value = null then null else encode value)
        |> ignore

    let inline deserializeGunAtomNode data =
        match box data :?> string option with
        | Some data -> decode data
        | None -> None
