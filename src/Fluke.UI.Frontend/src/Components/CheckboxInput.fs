namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Bindings

module CheckboxInput =
    [<ReactComponent>]
    let CheckboxInput
        (input: {| Atom: Store.Atom<_>
                   Label: string option
                   Props: UI.IChakraProps -> unit |})
        =
        let value, setValue = Store.useState input.Atom

        Checkbox.Checkbox
            input.Label
            (fun x ->
                x.isChecked <- value
                x.onChange <- fun _ -> promise { setValue (not value) }
                input.Props x)
