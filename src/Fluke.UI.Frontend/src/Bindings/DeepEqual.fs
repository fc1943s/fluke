namespace Fluke.UI.Frontend.Bindings

module DeepEqual =
    let inline compare<'T> (a: 'T) (b: 'T) : bool =
//        match a, b with
//        | a, b when unbox a <> null && unbox b <> null -> (compare (unbox a) (unbox b)) = 0
//        | _ ->
            (unbox a) = (unbox b)
