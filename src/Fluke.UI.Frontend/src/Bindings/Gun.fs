namespace Fluke.UI.Frontend.Bindings

#nowarn "40"


open System
open Fable.Core
open Fable.Core.JsInterop
open Fluke.UI.Frontend.Bindings
open Fluke.Shared


module Gun =
    type GunKeys =
        {
            pub: string
            epub: string
            priv: string
            epriv: string
        }
        static member inline Default =
            {
                pub = ""
                epub = ""
                priv = ""
                epriv = ""
            }

    type UserResult =
        {

            err: string option
            ok: int option
            pub: string option
            ``#``: string option
            [<Emit("@")>]
            at: string option
            wait: bool
        }

    type IGunUserPub =
        {
            alias: string option
            pub: string option
        }

    module rec Types =
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

        type PutAck =
            {
                err: int option
                ok: int option
                ``#``: string option
                [<Emit("@")>]
                at: string option
            }

        type PutNode =
            {
                err: int option
                off: (unit -> unit) option
            }

        type IGunChainReference =
            abstract get : string -> IGunChainReference
            abstract map : unit -> IGunChainReference
            abstract off : unit -> IGunChainReference
            abstract back : unit -> IGunChainReference
            abstract on : ('T -> string -> unit) -> unit
            abstract once : ('T -> string -> unit) -> unit
            abstract on : event: string * (unit -> unit) -> unit
            abstract put : string -> (PutAck -> PutNode -> unit) -> IGunChainReference
            abstract user : unit -> IGunUser
    //        abstract once : (string -> unit) -> unit
    //        abstract set : string -> IGunChainReference
    open Types


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

    let gun: GunProps -> IGunChainReference =
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
    let sea: ISEA = jsNative

    let createUser (user: IGunUser) username password =
        Promise.create
            (fun res err ->
                try
                    user.create (username, password, res)
                with
                | ex ->
                    printfn $"createUser error: {ex}"
                    err ex)

    let authUser (user: IGunUser) username password =
        Promise.create
            (fun res err ->
                try
                    user.auth (username, password, res)
                with
                | ex ->
                    printfn "authUser error: {ex}"
                    err ex)

    let changeUserPassword (user: IGunUser) username password newPassword =
        Promise.create
            (fun res err ->
                try
                    user.auth (username, password, res, {| change = newPassword |})
                with
                | ex ->
                    printfn "changeUserPassword error: {ex}"
                    err ex)

    let deleteUser (user: IGunUser) username password =
        Promise.create
            (fun res err ->
                try
                    user.delete (username, password, res)
                with
                | ex ->
                    printfn "deleteUser error: {ex}"
                    err ex)


    let inline userDecode<'TValue> (keys: GunKeys) data =
        promise {
            let! decrypted =
                promise {
                    try
                        let! verified = sea.verify data keys.pub
                        let! decrypted = sea.decrypt verified keys
                        return decrypted
                    with
                    | ex ->
                        JS.consoleError ("userDecode decrypt exception", ex, data)
                        return null
                }

            let decoded =
                try
                    decrypted |> Json.decode<'TValue option>
                with
                | ex ->
                    JS.consoleError ("userDecode decode error. ex=", ex, "data=", data, "decrypted=", decrypted)
                    None

            return decoded
        }

    let inline userEncode<'TValue> (gun: IGunChainReference) (value: 'TValue) =
        promise {
            try
                let user = gun.user ()
                let keys = user.__.sea

                match keys with
                | Some keys ->
                    let json =
                        value
                        |> Json.encode<'TValue>
                        |> Json.encode<string>

                    //                    printfn $"userEncode value={value} json={json}"
//
                    let! encrypted = sea.encrypt json keys

                    let! signed = sea.sign encrypted keys
                    //                    JS.log (fun () -> $"userEncode. json={json} encrypted={encrypted} signed={signed}")
                    return signed
                | None -> return failwith $"No keys found for user {user.is}"
            with
            | ex ->
                JS.consoleError ("[exception4]", ex, value)
                return raise ex
        }

    type Serializer<'T> = ('T -> string) * (string -> 'T)

    let inline defaultSerializer<'T> : Serializer<'T> = Json.encode<'T>, Json.decode<'T>

    let inline put (gun: IGunChainReference) (value: string) =
        Promise.create
            (fun res _err ->
                let newValue = value
                //                let newValue = if value = JS.undefined && not JS.jestWorkerId then null else value
                gun.put
                    newValue
                    (fun ack _node ->
                        if ack.ok = Some 1 && ack.err.IsNone then
                            res true
                        else

                            match JS.window id with
                            | Some window ->
                                if window?Cypress = null then
                                    JS.consoleError $"Gun.put error. newValue={newValue} ack={JS.JSON.stringify ack} "
                            | None -> ()

                            res false)
                |> ignore)


    let batchData<'T> =
        let fn
            (item: {| Fn: int64 * 'T -> JS.Promise<unit>
                      Timestamp: int64
                      Data: 'T |})
            =
            //                JS.consoleLog("batchData", item)
            item.Fn (item.Timestamp, item.Data)

        //        fn >> ignore
        Batcher.batcher (Array.map fn >> Promise.Parallel >> Promise.start) {| interval = 500 |}

    let inline subscribe (gun: IGunChainReference) fn =
        gun.on
            (fun data _key ->
                JS.log
                    (fun () ->
                        if _key = "devicePing" then
                            null
                        else
                            $"subscribe.on() data. batching...data={data} key={_key}")

                fn data)


    let batchSubscribe<'T> =
        let fn
            (item: {| GunAtomNode: IGunChainReference
                      Fn: int64 * 'T -> JS.Promise<unit> |})
            =
            promise {
                subscribe
                    item.GunAtomNode
                    (fun data ->
                        batchData
                            {|
                                Timestamp = DateTime.Now.Ticks
                                Data = data
                                Fn = item.Fn
                            |})
            }

        //        fn >> Promise.start
        Batcher.batcher (Array.map fn >> Promise.Parallel >> Promise.start) {| interval = 500 |}

    let inline wrapAtomPath (atomPath: string) =
        let header = $"{nameof Fluke}/"
        let header = if atomPath.StartsWith header then "" else header
        $"{header}{atomPath}"

    let inline getGunNodePath (atomPath: string) (keyIdentifier: string list) =
        let newAtomPath =
            match keyIdentifier with
            | [] -> atomPath
            | keyIdentifier when keyIdentifier |> List.head |> Guid.TryParse |> fst ->
                [
                    match atomPath |> String.split "/" with
                    | [| node |] ->
                        yield node
                        yield! keyIdentifier
                    | nodes ->
                        yield! nodes |> Array.take (nodes.Length - 2)

                        let secondLast = nodes.[nodes.Length - 2]

                        if secondLast |> Guid.TryParse |> fst then
                            yield! keyIdentifier
                            yield secondLast
                        else
                            yield secondLast
                            yield! keyIdentifier

                        yield nodes.[nodes.Length - 1]
                ]
                |> String.concat "/"
            | keyIdentifier ->
                ([
                    atomPath
                 ]
                 @ keyIdentifier)
                |> String.concat "/"

        wrapAtomPath newAtomPath
