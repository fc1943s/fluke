namespace Fluke.UI.Frontend.Bindings

open Fable.React
open Fable.Core.JsInterop
open FsJs


module CheckboxTree =
    // TODO: remove
    match Dom.window () with
    | Some window when window?``process`` = null -> window?``process`` <- {|  |}
    | _ -> ()


    importAll "react-checkbox-tree/lib/react-checkbox-tree.css"

    let checkboxTree: obj -> obj = importDefault "react-checkbox-tree"

    let inline render props =
        ReactBindings.React.createElement (checkboxTree, props, [])
