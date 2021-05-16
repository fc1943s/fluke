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
        abstract get : string -> IGunChainReference
        abstract leave : unit -> unit

    type IGunChainReference =
        abstract get : string -> IGunChainReference
        abstract set : string -> IGunChainReference
        abstract put : string -> IGunChainReference
        abstract user : unit -> IGunUser
        abstract on : ('T -> string -> unit) -> unit
        abstract on : event: string * (unit -> unit) -> unit
        abstract map : unit -> IGunChainReference
        abstract off : unit -> IGunChainReference
        abstract once : (string -> unit) -> unit

    type GunProps =
        {
            peers: string [] option
            radisk: bool option
            localStorage: bool option
        }

    let gun : GunProps -> IGunChainReference = importDefault "gun/gun"

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

    let getAtomNode (gun: IGunChainReference option) (atomPath: string) =
        (gun, atomPath.Split "/" |> Array.toList)
        ||> List.fold
                (fun result node ->
                    result
                    |> Option.map (fun result -> result.get node))


    let inline jsonEncode<'T> obj =
        Thoth.Json.Encode.Auto.toString<'T> (0, obj)

    let inline jsonDecode<'T> data =
        Thoth.Json.Decode.Auto.unsafeFromString<'T> data

    type Serializer<'T> = ('T -> string) * (string -> 'T)

    let inline defaultSerializer<'T> : Serializer<'T> = jsonEncode<'T>, jsonDecode<'T>

    let inline put (gun: IGunChainReference) (value: string) = gun.put value |> ignore
