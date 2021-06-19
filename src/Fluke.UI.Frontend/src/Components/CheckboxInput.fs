namespace Fluke.UI.Frontend.Components

open Feliz
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings

module CheckboxInput =
    [<ReactComponent>]
    let CheckboxInput
        (input: {| Atom: Recoil.RecoilValue<_, _>
                   Label: string option
                   Props: Chakra.IChakraProps -> unit |})
        =
        let value, setValue = Store.useState input.Atom

        Checkbox.Checkbox
            {|
                Label = input.Label
                Props =
                    fun x ->
                        x.isChecked <- value
                        x.onChange <- fun _ -> promise { setValue (not value) }
                        input.Props x
            |}