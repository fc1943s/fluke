namespace FsCore


module Model =
    type Username = Username of username: string

    and Username with
        static member inline Value value =
            try
                match value with
                | Username username -> Some username
            with
            | ex ->
                eprintfn $"Username.Value error value={value} ex={ex}"
                None

        static member inline ValueOrDefault = Username.Value >> Option.defaultValue ""

    and Color = Color of hex: string

    type StoreRoot = StoreRoot of name: string

    and StoreRoot with
        static member inline Value (StoreRoot name) = name

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
