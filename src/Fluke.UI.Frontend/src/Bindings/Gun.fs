namespace Fluke.UI.Frontend.Bindings

open Fable.Core
open Fable.Core.JsInterop


module rec Gun =
    type GunKeys =
        {
            pub: string
            epub: string
            priv: string
            epriv: string
        }
        static member inline Default =
            {
                Gun.pub = ""
                Gun.epub = ""
                Gun.priv = ""
                Gun.epriv = ""
            }

    type UserResult =
        {
            err: string option
            ok: int option
            pub: string option
            wait: bool
        }

    type IGunUserPub =
        {
            alias: string option
            pub: string option
        }

    type IGunUser =
        abstract create : alias: string * pass: string * cb: (UserResult -> unit) -> unit
        abstract delete : alias: string * pass: string * cb: (UserResult -> unit) -> unit
        abstract auth : alias: string * pass: string * cb: (UserResult -> unit) * ?opt: {| change: string |} -> unit

        [<Emit("$0._")>]
        abstract __ : {| sea: GunKeys option |}

        abstract get : string -> IGunChainReference
        abstract leave : unit -> unit

        abstract recall :
            {| sessionStorage: bool |}
            * System.Func<{| put: {| alias: string |} option
                             sea: GunKeys |}, unit> ->
            unit

        abstract is : IGunUserPub option


    type IGunChainReference =
        abstract get : string -> IGunChainReference
        abstract map : unit -> IGunChainReference
        abstract off : unit -> IGunChainReference
        abstract on : ('T -> string -> unit) -> unit
        abstract once : ('T -> string -> unit) -> unit
        abstract on : event: string * (unit -> unit) -> unit
        abstract put : string -> IGunChainReference
        abstract user : unit -> IGunUser
    //        abstract once : (string -> unit) -> unit
//        abstract set : string -> IGunChainReference


    type ISEA =
        abstract encrypt : data: string -> keys: GunKeys -> JS.Promise<string>
        abstract sign : data: string -> keys: GunKeys -> JS.Promise<string>
        abstract verify : data: string -> pub: string -> JS.Promise<obj>
        abstract decrypt : data: obj -> keys: GunKeys -> JS.Promise<string>
    //        abstract work : data:string -> keys:GunKeys -> JS.Promise<string>

    type GunProps =
        {
            peers: string [] option
            radisk: bool option
            localStorage: bool option
            multicast: bool option
        }

    //    match JS.window id with
//    | Some _ -> ()
//    | None -> importAll "gun"

    let gun : GunProps -> IGunChainReference =
        if JS.deviceInfo.IsTesting then
            importDefault "gun/gun"
        else
            importAll "gun/lib/radix"
            importAll "gun/lib/radisk"
            importAll "gun/lib/store"
            importAll "gun/lib/rindexed"

            importDefault "gun"


    importAll "gun/sea"
    importAll "gun/lib/promise"

    [<Emit "Gun.SEA">]
    let sea : ISEA = jsNative

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

    let changeUserPassword (user: IGunUser) username password newPassword =
        Promise.create
            (fun res err ->
                try
                    user.auth (username, password, res, {| change = newPassword |})
                with ex ->
                    printfn "changeUserPassword error: {ex}"
                    err ex)

    let deleteUser (user: IGunUser) username password =
        Promise.create
            (fun res err ->
                try
                    user.delete (username, password, res)
                with ex ->
                    printfn "deleteUser error: {ex}"
                    err ex)

    let inline jsonEncode<'T> obj =
        Thoth.Json.Encode.Auto.toString<'T> (0, obj)

    let inline jsonDecode<'T> data =
        Thoth.Json.Decode.Auto.unsafeFromString<'T> data

    type Serializer<'T> = ('T -> string) * (string -> 'T)

    let inline defaultSerializer<'T> : Serializer<'T> = jsonEncode<'T>, jsonDecode<'T>

    let inline put (gun: IGunChainReference) (value: string) = gun.put value |> ignore
