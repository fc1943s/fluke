namespace FsUi.Bindings


open Fable.Core
open FsUi.Bindings
open FsUi.Bindings.Vendor

module Icons =
    open ReactIcons

    [<ImportAll "react-icons/ai">]
    let ai: __ai_index.IExports = jsNative

    [<ImportAll "react-icons/bi">]
    let bi: __bi_index.IExports = jsNative

    [<ImportAll "react-icons/bs">]
    let bs: __bs_index.IExports = jsNative

    [<ImportAll "react-icons/cg">]
    let cg: __cg_index.IExports = jsNative

    [<ImportAll "react-icons/di">]
    let di: __di_index.IExports = jsNative

    [<ImportAll "react-icons/fa">]
    let fa: __fa_index.IExports = jsNative

    [<ImportAll "react-icons/fc">]
    let fc: __fc_index.IExports = jsNative

    [<ImportAll "react-icons/fi">]
    let fi: __fi_index.IExports = jsNative

    [<ImportAll "react-icons/gi">]
    let gi: __gi_index.IExports = jsNative

    [<ImportAll "react-icons/go">]
    let go: __go_index.IExports = jsNative

    [<ImportAll "react-icons/gr">]
    let gr: __gr_index.IExports = jsNative

    [<ImportAll "react-icons/hi">]
    let hi: __hi_index.IExports = jsNative

    [<ImportAll "react-icons/im">]
    let im: __im_index.IExports = jsNative

    [<ImportAll "react-icons/io">]
    let io: __io_index.IExports = jsNative

    [<ImportAll "react-icons/io5">]
    let io5: __io5_index.IExports = jsNative

    [<ImportAll "react-icons/md">]
    let md: __md_index.IExports = jsNative

    [<ImportAll "react-icons/ri">]
    let ri: __ri_index.IExports = jsNative

    [<ImportAll "react-icons/si">]
    let si: __si_index.IExports = jsNative

    [<ImportAll "react-icons/ti">]
    let ti: __ti_index.IExports = jsNative

    [<ImportAll "react-icons/vsc">]
    let vsc: __vsc_index.IExports = jsNative

    [<ImportAll "react-icons/wi">]
    let wi: __wi_index.IExports = jsNative

    let inline render cmp = React.bindComponent () [] cmp

    let inline renderWithProps (props: UI.IChakraProps -> unit) cmp =
        UI.box
            (fun x ->
                x.``as`` <- cmp
                props x)
            []
