namespace FsStore.Bindings

open FsCore
open FsJs
open Fable.SignalR
open Fable.Core
open Fable.Core.JsInterop
open System


module Gun =
    type GunKeys =
        {
            pub: string option
            epub: string option
            priv: string option
            epriv: string option
        }
        static member inline Default =
            {
                pub = None
                epub = None
                priv = None
                epriv = None
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
            alias: GunUserAlias option
            pub: string option
        }

    and [<Erase; RequireQualifiedAccess>] GunUserAlias =
        | Alias of username: string
        | GunKeys of keys: GunKeys

    module rec Types =
        type IGunNode =
            abstract get : string -> IGunChainReference

        module GunOps =
            let inline private (|HasAuth|) x = (^a:(member A:string) x)

        type IGunUser =
            inherit IGunNode
            abstract create : alias: string * pass: string * cb: (UserResult -> unit) -> unit
            abstract delete : alias: string * pass: string * cb: (UserResult -> unit) -> unit
            abstract auth : alias: string * pass: string * cb: (UserResult -> unit) * ?opt: {| change: string |} -> unit
            abstract auth : keys: GunKeys * cb: (UserResult -> unit) * ?opt: {| change: string |} -> unit

            [<Emit("$0._")>]
            abstract __ : {| sea: GunKeys option |}

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
            inherit IGunNode
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

        abstract work :
            data: string ->
            keys: GunKeys option ->
            x: unit option ->
            crypto: {| name: string option |} option ->
            JS.Promise<string>

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
        if Dom.deviceInfo.IsTesting then
            importDefault "gun/gun"
        else
            importAll "gun/lib/radix"
            importAll "gun/lib/radisk"
            importAll "gun/lib/store"
            importAll "gun/lib/rindexed"

            importDefault "gun"


    importAll "gun/sea"
    importAll "gun/lib/promise"

    let sea: ISEA = emitJsExpr () "Gun.SEA"

    let inline createUser (user: IGunUser) username password =
        Promise.create
            (fun res err ->
                try
                    user.create (username, password, res)
                with
                | ex ->
                    printfn $"createUser error: {ex}"
                    err ex)

    let inline authUser (user: IGunUser) username password =
        Promise.create
            (fun res err ->
                try
                    user.auth (username, password, res)
                with
                | ex ->
                    printfn "authUser error: {ex}"
                    err ex)

    let inline authKeys (user: IGunUser) keys =
        Promise.create
            (fun res err ->
                try
                    user.auth (keys, res)
                with
                | ex ->
                    printfn "authKeys error: {ex}"
                    err ex)

    let inline changeUserPassword (user: IGunUser) username password newPassword =
        Promise.create
            (fun res err ->
                try
                    user.auth (username, password, res, {| change = newPassword |})
                with
                | ex ->
                    printfn "changeUserPassword error: {ex}"
                    err ex)

    let inline deleteUser (user: IGunUser) username password =
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
                    match keys with
                    | { pub = Some pub } ->
                        try
                            let! verified = sea.verify data pub

                            if verified |> Option.ofObjUnbox |> Option.isNone then
                                return JS.undefined
                            else
                                let! decrypted = sea.decrypt verified keys
                                return decrypted
                        with
                        | ex ->
                            Dom.consoleError ("userDecode decrypt exception", ex, data)
                            return null
                    | _ -> return null
                }

            let decoded =
                try
                    if decrypted |> Option.ofObjUnbox |> Option.isNone then
                        Dom.log (fun () -> $"userDecode decrypt empty. decrypted={decrypted} data={data}")
                        JS.undefined
                    else
                        decrypted |> Json.decode<'TValue option>
                with
                | ex ->
                    Dom.consoleError ("userDecode decode error. ex=", ex, "data=", data, "decrypted=", decrypted)
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
                    //                    Dom.log (fun () -> $"userEncode. json={json} encrypted={encrypted} signed={signed}")
                    return signed
                | None -> return failwith $"No keys found for user {user.is}"
            with
            | ex ->
                Dom.consoleError ("[exception4]", ex, value)
                return raise ex
        }

    type Serializer<'T> = ('T -> string) * (string -> 'T)

    let inline defaultSerializer<'T> : Serializer<'T> = Json.encode<'T>, Json.decode<'T>

    let inline put (gun: IGunChainReference) (value: string) =
        Promise.create
            (fun res _err ->
                let newValue = value
                //                let newValue = if value = Dom.undefined && not Dom.jestWorkerId then null else value
                gun.put
                    newValue
                    (fun ack _node ->
                        if ack.ok = Some 1 && ack.err.IsNone then
                            res true
                        else

                            match Dom.window () with
                            | Some window ->
                                if window?Cypress = null then
                                    Dom.consoleError $"Gun.put error. newValue={newValue} ack={JS.JSON.stringify ack} "
                            | None -> ()

                            res false)
                |> ignore)

    let inline subscribe (gun: IGunChainReference) fn =
        gun.on
            (fun data _key ->
                Dom.log
                    (fun () ->
                        if _key = "devicePing" then
                            null
                        else
                            $"subscribe.on() data. batching...data={data} key={_key}")

                fn data)

        Object.newDisposable
            (fun () ->
                printfn "subscribe.on() data. Dispose promise observable."
                gun.off () |> ignore)
        |> Promise.lift


    let inline batchData<'T> (fn: int64 * 'T -> JS.Promise<IDisposable>) (data: 'T) =
        Batcher.batch (Batcher.BatchType.Data (data, DateTime.Now.Ticks, fn))

    let inline batchKeys map fn data =
        let fn = map >> fn
        Batcher.batch (Batcher.BatchType.KeysFromServer (data, DateTime.Now.Ticks, fn))

    let inline batchSubscribe gunAtomNode fn =
        let fn () = subscribe gunAtomNode (batchData fn)
        Batcher.batch (Batcher.BatchType.Subscribe fn)

    let inline batchSet gunAtomNode fn =
        let fn () = subscribe gunAtomNode (batchData fn)
        Batcher.batch (Batcher.BatchType.Subscribe fn)

    let inline hubSubscribe<'A, 'R> (hub: HubConnection<'A, 'A, _, 'R, 'R>) action fn onError =
        promise {
            let! stream = hub.streamFrom action |> Async.StartAsPromise

            let subscription =
                stream.subscribe
                    {
                        next = fun (msg: 'R) -> fn msg
                        complete =
                            fun () ->
                                Dom.log
                                    (fun () -> $"[hubSubscribe.complete() HUB stream subscription] action={action} ")
                        error =
                            fun err ->
                                Dom.consoleError (
                                    $"[hubSubscribe.error() HUB stream subscription] action={action} ",
                                    err
                                )

                                onError err
                    }

            return subscription
        }

    let inline batchHubSubscribe (hub: HubConnection<'A, 'A, _, 'R, 'R>) action fn onError =
        let fn () =
            hubSubscribe hub action (batchData fn) onError

        Batcher.batch (Batcher.BatchType.Subscribe fn)