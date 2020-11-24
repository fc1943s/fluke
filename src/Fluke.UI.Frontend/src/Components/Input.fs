namespace Fluke.UI.Frontend.Components

open Browser.Types
open Fable.React
open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Feliz.Recoil


module Input =
    [<RequireQualifiedAccess>]
    type InputFormat =
        | Text
        | Date
        | Time
        | DateTime
        | Email
        | Password

    let render<'T> =
        React.memo (fun (input: {| Label: string option
                                   Placeholder: string
                                   InputFormat: InputFormat
                                   OnFormat: 'T -> string
                                   OnValidate: string -> 'T option
                                   Atom: RecoilValue<'T, ReadWrite> |}) ->
            let atom, setAtom = Recoil.useState input.Atom
            let resetAtom = Recoil.useResetState input.Atom

            Chakra.stack
                {| spacing = "5px" |}
                [
                    match input.Label with
                    | Some label ->
                        Chakra.box
                            {|  |}
                            [
                                str $"{label}:"
                            ]
                    | None -> ()

                    Chakra.input
                        {|
                            placeholder = input.Placeholder
                            onChange =
                                fun (e: KeyboardEvent) ->
                                    match input.OnValidate e.Value with
                                    | Some value -> setAtom value
                                    | None -> resetAtom ()
                            value = input.OnFormat atom
                            ``type`` =
                                match input.InputFormat with
                                | InputFormat.Text -> "text"
                                | InputFormat.Date -> "date"
                                | InputFormat.Time -> "time"
                                | InputFormat.DateTime -> "datetime-local"
                                | InputFormat.Email -> "email"
                                | InputFormat.Password -> "password"
                        |}
                        []

                ])
