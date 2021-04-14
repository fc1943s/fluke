namespace Fluke.UI.Frontend.Bindings

open Fable.React
open Feliz.Recoil
open Fluke.UI.Frontend
open Fable.Core.JsInterop
open Feliz
open React


module Recoil =
    type EffectProps<'T> =
        {
            node: {| key: string |}
            onSet: ('T -> 'T -> unit) -> unit
            trigger: string
            setSelf: 'T -> unit
        }

    module Atoms =
        module rec Form =
            let rec fieldValue =
                atomFamily {
                    key $"{nameof atomFamily}/{nameof Form}/{nameof fieldValue}"
                    def (fun (_key: obj) -> null: obj)
                }

            let rec fieldValueMounted =
                atomFamily {
                    key $"{nameof atomFamily}/{nameof Form}/{nameof fieldValueMounted}"
                    def (fun (_key: obj) -> false)
                }

    let wrapAtomField<'TValue, 'TKey> (atom: RecoilValue<'TValue, ReadWrite>) =
        {|
            ReadOnly = atom
            ReadWrite = Atoms.Form.fieldValue {| key = atom.key |} :> obj :?> RecoilValue<'TValue, ReadWrite>
        |}

    [<RequireQualifiedAccess>]
    type AtomScope =
        | ReadOnly
        | ReadWrite

    type InputAtom<'TValue, 'TKey> =
        | Atom of RecoilValue<'TValue, ReadWrite>
        | AtomFamily of ('TKey -> RecoilValue<'TValue, ReadWrite>) * 'TKey

    let getAtomField<'TValue, 'TKey> atom =
        let flatAtom =
            match atom with
            | Some (Atom atom) -> atom
            | Some (AtomFamily (atom, key: 'TKey)) -> atom key
            | _ -> (box (RecoilValue.lift null)) :?> RecoilValue<'TValue, ReadWrite>

        wrapAtomField<'TValue, 'TKey> flatAtom

    let useAtomField<'TValue, 'TKey when 'TValue: equality> atom =
        let atomField = getAtomField<'TValue, 'TKey> atom

        let readOnlyValue, setReadOnlyValue = Recoil.useState atomField.ReadOnly
        let readWriteValue, setReadWriteValue = Recoil.useState atomField.ReadWrite

        let fieldValueMounted, setFieldValueMounted =
            Recoil.useState (Atoms.Form.fieldValueMounted {| key = atomField.ReadOnly.key |})

        React.useEffect (
            (fun () ->
                if not fieldValueMounted
                   && readOnlyValue <> readWriteValue then
                    setReadWriteValue readOnlyValue
                    setFieldValueMounted true

                ),
            [|
                box fieldValueMounted
                box setFieldValueMounted
                box setReadWriteValue
                box readOnlyValue
                box readWriteValue
            |]
        )

        let atomFieldOptions =
            React.useMemo (
                (fun () ->
                    {|
                        ReadWriteValue =
                            if not fieldValueMounted then
                                readOnlyValue
                            else
                                readWriteValue
                        SetReadWriteValue =
                            if atom.IsSome then
                                setReadWriteValue
                            else
                                (fun _ -> ())
                        ReadOnlyValue = readOnlyValue
                        SetReadOnlyValue =
                            if atom.IsSome then
                                setReadOnlyValue
                            else
                                (fun _ -> ())
                        AtomField = atomField
                    |}),
                [|
                    box fieldValueMounted
                    box atom
                    box atomField
                    box readOnlyValue
                    box readWriteValue
                    box setReadOnlyValue
                    box setReadWriteValue
                |]
            )

        atomFieldOptions

module Recoilize =
    let recoilizeDebugger<'T> =
        //         importDefault "recoilize"
        nothing |> composeComponent

[<AutoOpen>]
module RecoilMagic =

    type Snapshot with
        member this.getReadWritePromise atom key =
            this.getPromise
                (Recoil.getAtomField (Some (Recoil.AtomFamily (atom, key))))
                    .ReadWrite


    type AtomStateWithEffects<'T, 'U, 'V> =
        {
            State: AtomState.ReadWrite<'T, 'U, 'V>
            Effects: (Recoil.EffectProps<'T> -> unit -> unit) list
        }

    type AtomCE.AtomBuilder with
        [<CustomOperation("effects")>]
        member inline _.Effects
            (
                state: AtomState.ReadWrite<'T, 'U, 'V>,
                effects: (Recoil.EffectProps<'T> -> unit -> unit) list
            ) : AtomStateWithEffects<'T, 'U, 'V> =
            { State = state; Effects = effects }

        member inline _.Run<'T, 'V> ({ Effects = effects; State = state }: AtomStateWithEffects<'T, 'T, 'V>) =
            Bindings.Recoil.atom<'T> (
                [
                    "key" ==> state.Key
                    "default" ==> state.Def
                    "effects_UNSTABLE" ==> effects
                    match state.Persist with
                    | Some persist ->
                        "persistence_UNSTABLE"
                        ==> PersistenceSettings.CreateObj persist
                    | None -> ()
                    match state.DangerouslyAllowMutability with
                    | Some dangerouslyAllowMutability ->
                        "dangerouslyAllowMutability"
                        ==> dangerouslyAllowMutability
                    | None -> ()
                ]
                |> createObj
            )

    type AtomFamilyStateWithEffects<'T, 'U, 'V, 'P> =
        {
            State: AtomFamilyState.ReadWrite<'P -> 'U, 'U, 'V, 'P>
            Effects: 'P -> (Recoil.EffectProps<'T> -> unit -> unit) list
        }

    type AtomFamilyCE.AtomFamilyBuilder with
        [<CustomOperation("effects")>]
        member inline _.Effects
            (
                state: AtomFamilyState.ReadWrite<'P -> 'U, 'U, 'V, 'P>,
                effects: 'P -> (Recoil.EffectProps<'T> -> unit -> unit) list
            ) : AtomFamilyStateWithEffects<'T, 'U, 'V, 'P> =
            { State = state; Effects = effects }


        member inline _.Run<'U, 'V, 'P>
            ({ Effects = effects; State = state }: AtomFamilyStateWithEffects<'U, 'U, 'V, 'P>)
            : 'P -> RecoilValue<'U, ReadWrite> =
            Bindings.Recoil.atomFamily<'U, 'P> (
                [
                    "key" ==> state.Key
                    "default" ==> state.Def
                    "effects_UNSTABLE" ==> effects
                    match state.Persist with
                    | Some persist ->
                        "persistence_UNSTABLE"
                        ==> PersistenceSettings.CreateObj persist
                    | None -> ()
                    match state.DangerouslyAllowMutability with
                    | Some dangerouslyAllowMutability ->
                        "dangerouslyAllowMutability"
                        ==> dangerouslyAllowMutability
                    | None -> ()
                ]
                |> createObj
            )

//    module Effects =
//        type Wrapper = { Value: string }
//
//        let localStorage<'T> =
//            (fun ({ node = node; onSet = onSet; setSelf = setSelf }: Recoil.EffectProps<'T>) ->
//                let storageJson =
//                    Browser.Dom.window.localStorage.getItem node.key
//                    |> Option.ofObj
//
//                let value =
//                    //                    let parsed1 = Fable.Core.JS.JSON.parse storageJson :?> Wrapper<'T> option
//                    let parsed2 =
//                        match storageJson with
//                        | Some json ->
//                            try
//                                Json.parseAs<Wrapper> json
//                            with ex ->
//                                printfn "simplejson error: %A" ex
//                                Fable.Core.JS.JSON.parse json :?> Wrapper
//                        | None -> { Value = "" }
//                    //                        | Ok wrapper -> wrapper.Value
////                        | Error error ->
////                            printfn "Internal Specific Json parse error (input: %A): %A" storageJson error
////                            None
//
//                    //                    let parsed3 =
////                        let decoder : Decoder<Wrapper<'T>> =
////                            Decode.object
////                                (fun get -> { Value = get.Required.Field "value" Decode.string })
////                        Thoth.Json.Decode.fromString decoder storageJson
////                        |> function
////                            | Ok x -> Some x
////                            | Error error ->
////                                printfn "Internal Specific Json parse error (input: %A): %A" storageJson error
////                                None
//
//                    //                    match Thoth.Json.Decode.Auto.fromString<Wrapper<'T>> storageJson with
//                    match parsed2 with
//                    //                    | Ok wrapper -> Some wrapper.Value
////                    | Error error ->
////                        printfn "json parse error (input: %A): %A" storageJson error
////                        None
//                    | { Value = value } -> setSelf (unbox value)
////                    | _ -> printfn "json parse error (key: %A; input: %A)" node.key storageJson
//
//
//                onSet (fun value _oldValue ->
//                    printfn "onSet. oldValue: %Avalue: %A" _oldValue value
//
//                    let valueJson = Json.serialize value
//                    let wrapper = { Value = valueJson }
//                    let json = Json.serialize wrapper
//                    //                    let json = Thoth.Json.Encode.Auto.toString (0, wrapper)
////                    let json = Fable.Core.JS.JSON.stringify wrapper
//                    Browser.Dom.window.localStorage.setItem (node.key, json))
//
//                //    // Subscribe to storage updates
//                //    storage.subscribe(value => setSelf(value));
//
//                fun () -> printfn "> unsubscribe")
