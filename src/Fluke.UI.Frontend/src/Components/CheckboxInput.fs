namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.Recoil
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Bindings

module CheckboxInput =
    [<ReactComponent>]
    let CheckboxInput
        (input: {| Atom: RecoilValue<_, _>
                   Props: Chakra.IChakraProps -> unit |})
        =
        let value, setValue = Recoil.useState input.Atom

        Checkbox.Checkbox
            {|
                Props =
                    fun x ->
                        x.isChecked <- value
                        x.onChange <- fun _ -> promise { setValue (not value) }
                        input.Props x
            |}
