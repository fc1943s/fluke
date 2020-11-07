namespace Fluke.UI.Frontend.Bindings

open Feliz.Recoil
open Fluke.UI.Frontend
open Fable.Core.JsInterop
open Feliz


module Recoil =
    type EffectProps<'T> =
        {
            node: {| key: string |}
            onSet: ('T -> 'T -> unit) -> unit
            trigger: string
            setSelf: 'T -> unit
        }

[<AutoOpen>]
module RecoilMagic =

    type AtomStateWithEffects<'T, 'U, 'V> =
        {
            State: AtomState.ReadWrite<'T, 'U, 'V>
            Effects: ((Recoil.EffectProps<'T> -> (unit -> unit)) list)
        }

    type AtomCE.AtomBuilder with
        [<CustomOperation("effects")>]
        member inline _.Effects
            (state: AtomState.ReadWrite<'T, 'U, 'V>, effects: (Recoil.EffectProps<'T> -> (unit -> unit)) list)
            : AtomStateWithEffects<'T, 'U, 'V>
            =
            { State = state; Effects = effects }

        member inline _.Run<'T, 'V> ({ Effects = effects; State = state }: AtomStateWithEffects<'T, 'T, 'V>) =
            Bindings.Recoil.atom<'T>
                ([
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
                 |> createObj)

    type AtomFamilyStateWithEffects<'T, 'U, 'V, 'P> =
        {
            State: AtomFamilyState.ReadWrite<'P -> 'T, 'U, 'V>
            Effects: 'P -> ((Recoil.EffectProps<'T> -> (unit -> unit)) list)
        }

    type AtomFamilyCE.AtomFamilyBuilder with
        [<CustomOperation("effects")>]
        member inline _.Effects
            (
                state: AtomFamilyState.ReadWrite<'P -> 'T, 'U, 'V>,
                effects: 'P -> ((Recoil.EffectProps<'T> -> (unit -> unit)) list)
            )
            : AtomFamilyStateWithEffects<'T, 'U, 'V, 'P>
            =
            { State = state; Effects = effects }


        member inline _.Run<'T, 'V, 'P>
            ({ Effects = effects; State = state }: AtomFamilyStateWithEffects<'T, 'T, 'V, 'P>)
            : 'P -> RecoilValue<'T, ReadWrite>
            =
            Bindings.Recoil.atomFamily<'T, 'P>
                ([
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
                 |> createObj)
