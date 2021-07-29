namespace FsUi.Components

open Fable.React
open FsUi.Bindings


module Checkbox =
    let inline Checkbox (label: string option) (props: UI.IChakraProps -> unit) =
        UI.checkbox
            (fun x ->
                x.colorScheme <- "purple"
                x.borderColor <- "gray.30"
                x.size <- "lg"
                x.alignSelf <- "flex-start"
                props x)
            [
                match label with
                | Some label ->
                    UI.box
                        (fun x -> x.fontSize <- "main")
                        [
                            str label
                        ]
                | _ -> nothing
            ]
