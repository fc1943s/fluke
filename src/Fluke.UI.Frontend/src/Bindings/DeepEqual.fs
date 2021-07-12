namespace Fluke.UI.Frontend.Bindings

open System
open Fable.Core
open Fable.Core.JsInterop
open Fluke.Shared
open System
open Fable.Core.JsInterop
open System.Collections.Generic
open FSharp.Data.UnitSystems.SI.UnitSymbols

module DeepEqual =
    let private fastDeepEqual<'T> (_a: 'T) (_b: 'T) : bool = importDefault "fast-deep-equal/react"

    let inline getCompareFnKey obj : string option =
        //        match obj with
//        Fable.
//        | :? IComparable<'T> -> ()
//        | _ -> ()

        let fn = obj?CompareTo
        if unbox fn <> null then Some "CompareTo" else None

    let compare<'T> (a: 'T) (b: 'T) : bool =
        //        if unbox a <> null && a?toString <> null && jsTypeof a <> "boolean" then
//            a?toString <- emitJsExpr () "Object.prototype.toString"
//            b?toString <- emitJsExpr () "Object.prototype.toString"

        //        Fable.Core.JS.undefined
        match a, b with
        | a, b when unbox a <> null && unbox b <> null -> (compare (unbox a) (unbox b)) = 0
        //            match getCompareFnKey a with
//            | Some fnKey ->
//                Browser.Dom.window?compare <- {|
//                                                  CompareTo = a?CompareTo
//                                                  a = a
//                                                  b = b
//                                                  prot = a |> getPrototypeOf
//                                                  prot2 = a |> getPrototypeOf |> getPrototypeOf
//                                                  keys = a |> JS.Constructors.Object.keys
//                                                  emptySet = Set.empty
//                                                  seqCompareWith = Seq.compareWith
//                                              |}
//
//                emitJsExpr (a, b, fnKey) "$0[$2]($1)" = 0
//            | None -> fastDeepEqual a b
        | _ -> fastDeepEqual a b
