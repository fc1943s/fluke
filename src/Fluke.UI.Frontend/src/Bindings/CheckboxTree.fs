namespace Fluke.UI.Frontend.Bindings

open Fable.React
open Fable.Core.JsInterop


module CheckboxTree =
    importAll "react-checkbox-tree/lib/react-checkbox-tree.css"

    let private checkboxTree : obj -> obj = importDefault "react-checkbox-tree"

    let render props =
        ReactBindings.React.createElement (checkboxTree, props, [])
