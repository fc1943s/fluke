namespace Fluke.UI.Frontend.Components

open Browser.Types
open Fable.React
open Feliz
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Feliz.Recoil
open Fable.Core


module Input =

    [<RequireQualifiedAccess>]
    type InputFormat =
        | Text
        | Date
        | Time
        | DateTime
        | Email
        | Password

    type IProps<'T> =
        abstract atom : RecoilValue<'T, ReadWrite> with get, set
        abstract label : string option with get, set
        abstract autoFocus : bool with get, set
        abstract placeholder : string with get, set
        abstract onFormat : ('T -> string) option with get, set
        abstract onValidate : (string -> 'T option) option with get, set
        abstract onKeyDown : (KeyboardEvent -> JS.Promise<unit>) with get, set
        abstract inputFormat : InputFormat with get, set

    [<ReactComponent>]
    let Input<'T> (input: IProps<'T>) =
        let resetAtom = Recoil.useResetState input.atom
        let atom, setAtom = Recoil.useState input.atom

        Chakra.stack
            {| spacing = "5px" |}
            [
                match input.label with
                | Some label ->
                    Chakra.box
                        {|  |}
                        [
                            str $"{label}:"
                        ]
                | None -> ()

                Chakra.input
                    {|
                        autoFocus = input.autoFocus
                        placeholder = input.placeholder
                        onKeyDown = input.onKeyDown
                        onChange =
                            match input.onValidate with
                            | Some onValidate ->
                                Some
                                    (fun (e: KeyboardEvent) ->
                                        match onValidate e.Value with
                                        | Some value -> setAtom value
                                        | None -> resetAtom ())
                            | None -> None
                        value =
                            match input.onFormat with
                            | Some onFormat -> onFormat atom
                            | None -> string atom
                        ``type`` =
                            match input.inputFormat with
                            | InputFormat.Text -> "text"
                            | InputFormat.Date -> "date"
                            | InputFormat.Time -> "time"
                            | InputFormat.DateTime -> "datetime-local"
                            | InputFormat.Email -> "email"
                            | InputFormat.Password -> "password"
                    |}
                    []
            ]
