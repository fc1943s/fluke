namespace Fluke.UI.Frontend.Bindings

open Fable.Core.JsInterop


module DeepEqual =
    let private fastDeepEqual<'T> (_a: 'T) (_b: 'T) : bool = importDefault "fast-deep-equal/react"

    let compare<'T> (a: 'T) (b: 'T) : bool =
        //        if unbox a <> null && a?toString <> null && jsTypeof a <> "boolean" then
//            a?toString <- emitJsExpr () "Object.prototype.toString"
//            b?toString <- emitJsExpr () "Object.prototype.toString"

//        Fable.Core.JS.undefined
        if unbox a <> null
           && unbox b <> null
           && a?CompareTo <> null then
            (a?CompareTo b) = 0
        else
            fastDeepEqual a b
