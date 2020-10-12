namespace Fluke.UI.Frontend.Bindings

open Fable.Core


module Icons =
    [<ImportAll "react-icons/fi">]
    let fi: {| FiDatabase: obj |} = jsNative

    [<ImportAll "react-icons/md">]
    let md: {| MdSettings: obj |} = jsNative

    let wrap cmp = React.wrap cmp () []
    //    let wrap cmp (props: {| fontSize: string |}) = Chakra.box {| props with ``as`` = cmp |} []

    let fiDatabase () = wrap fi.FiDatabase

    let mdSettings () = wrap md.MdSettings
