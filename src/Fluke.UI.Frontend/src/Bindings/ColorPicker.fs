namespace Fluke.UI.Frontend.Bindings

open Fable.React
open Fable.Core.JsInterop

module ColorPicker =
    let private sketchPicker : obj -> obj = import "SketchPicker" "react-color"

    let render
        (props: {| color: string
                   onChange: {| hex: string |} -> unit |})
        =
        ReactBindings.React.createElement (sketchPicker, {| props with width = "210px" |}, [])
