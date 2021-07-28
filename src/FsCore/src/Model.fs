namespace FsCore


module Model =
    type Username = Username of username: string

    and Username with
        static member inline Value (Username username) = username

    and Color = Color of hex: string

    type Collection = Collection of collection: string

    and Collection with
        static member inline Value (Collection collection) = collection

    and Color with
        static member inline Value value =
            match value |> Option.ofObjUnbox with
            | Some (Color hex) -> Some hex
            | _ -> None

        static member inline Default = Color "#000000"

        static member inline ValueOrDefault value =
            value
            |> Color.Value
            |> Option.defaultValue (Color.Default |> Color.Value |> Option.get)
