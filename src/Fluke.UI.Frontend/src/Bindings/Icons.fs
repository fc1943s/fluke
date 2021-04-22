namespace Fluke.UI.Frontend.Bindings

open Fable.Core


module Icons =
    [<ImportAll "react-icons/ai">]
    let ai : {| AiFillFolder: obj
                AiFillFolderOpen: obj
                AiOutlineGithub: obj |} =
        jsNative

    [<ImportAll "react-icons/bi">]
    let bi : {| BiTask: obj |} = jsNative

    [<ImportAll "react-icons/bs">]
    let bs : {| BsGrid: obj
                BsListCheck: obj
                BsQuestionCircle: obj
                BsThreeDots: obj |} =
        jsNative

    [<ImportAll "react-icons/fa">]
    let fa : {| FaChevronDown: obj
                FaChevronRight: obj
                FaMinus: obj
                FaPlus: obj
                FaRegClock: obj
                FaRegUser: obj
                FaSortDown: obj
                FaSortNumericDownAlt: obj
                FaSortUp: obj |} =
        jsNative

    [<ImportAll "react-icons/fi">]
    let fi : {| FiDatabase: obj; FiLogOut: obj |} = jsNative

    [<ImportAll "react-icons/gi">]
    let gi : {| GiHourglass: obj |} = jsNative

    [<ImportAll "react-icons/md">]
    let md : {| MdCheckBox: obj
                MdCheckBoxOutlineBlank: obj
                MdClear: obj
                MdIndeterminateCheckBox: obj
                MdSettings: obj |} =
        jsNative

    [<ImportAll "react-icons/ti">]
    let ti : {| TiFlowChildren: obj |} = jsNative

    let wrap cmp = React.bindComponent () [] cmp
    //    let wrap cmp (props: {| fontSize: string |}) = Chakra.box {| props with ``as`` = cmp |} []

    let aiFillFolder () = wrap ai.AiFillFolder
    let aiFillFolderOpen () = wrap ai.AiFillFolderOpen
    let aiOutlineGithub () = wrap ai.AiOutlineGithub
    let biTask () = wrap bi.BiTask
    let bsGrid () = wrap bs.BsGrid
    let bsListCheck () = wrap bs.BsListCheck
    let bsQuestionCircle () = wrap bs.BsQuestionCircle
    let bsThreeDots () = wrap bs.BsThreeDots
    let faChevronDown () = wrap fa.FaChevronDown
    let faChevronRight () = wrap fa.FaChevronRight
    let faMinus () = wrap fa.FaMinus
    let faPlus () = wrap fa.FaPlus
    let faRegClock () = wrap fa.FaRegClock
    let faRegUser () = wrap fa.FaRegUser
    let faSortUp () = wrap fa.FaSortUp
    let faSortNumericDownAlt () = wrap fa.FaSortNumericDownAlt
    let faSortDown () = wrap fa.FaSortDown
    let fiDatabase () = wrap fi.FiDatabase
    let fiLogOut () = wrap fi.FiLogOut
    let giHourglass () = wrap gi.GiHourglass
    let mdCheckBox () = wrap md.MdCheckBox
    let mdCheckBoxOutlineBlank () = wrap md.MdCheckBoxOutlineBlank
    let mdClear () = wrap md.MdClear
    let mdIndeterminateCheckBox () = wrap md.MdIndeterminateCheckBox
    let mdSettings () = wrap md.MdSettings
    let tiFlowChildren () = wrap ti.TiFlowChildren
