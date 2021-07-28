namespace FsUi.Components

open Fable.React
open Feliz
open FsUi.Bindings


module InputLabel =
    let inline InputLabel
        (input: {| Label: ReactElement
                   Hint: ReactElement option
                   HintTitle: ReactElement option
                   Props: UI.IChakraProps -> unit |})
        =
        UI.flex
            input.Props
            [
                input.Label

                match input.Hint with
                | Some hint ->
                    Hint.Hint
                        (fun x ->
                            x.hint <- hint

                            x.hintTitle <-
                                Some (
                                    match input.HintTitle with
                                    | Some hintTitle -> hintTitle
                                    | None -> input.Label
                                ))
                | None -> nothing
            ]
