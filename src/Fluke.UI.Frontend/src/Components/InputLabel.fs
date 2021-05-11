namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.UI.Frontend.Bindings


module InputLabel =
    [<ReactComponent>]
    let InputLabel
        (input: {| Label: ReactElement
                   Hint: ReactElement option
                   HintTitle: ReactElement option
                   Props: Chakra.IChakraProps -> unit |})
        =
        Chakra.flex
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
