namespace Fluke.UI.Frontend.Hooks

#nowarn "40"

open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings
open Fable.Core


module Theme =
    let rec private theme =
        {|
            config =
                {|
                    initialColorMode = "dark"
                    useSystemColorMode = false
                |}
            breakpoints =
                {|
                    sm = "350px"
                    md = "750px"
                    lg = "1000px"
                    xl = "1900px"
                |}
            colors =
                {|
                    heliotrope = "#b586ff"
                    gray =
                        {|
                            ``10`` = "#1a1a1a" // grayDark
                            ``13`` = "#212121" // grayLight
                            ``16`` = "#292929" // grayLighter
                            ``45`` = "#727272" // textDark
                            ``77`` = "#b0bec5" // textLight
                        |}
                |}
            fonts =
                {|
                    main = "'Roboto Condensed', sans-serif"
                |}
            fontWeights =
                {|
                    hairline = 100
                    thin = 200
                    light = 300
                    normal = 400
                    medium = 500
                    semibold = 600
                    bold = 700
                    extrabold = 800
                    black = 900
                |}
            fontSizes = {| main = "12px" |}
            styles =
                {|
                    ``global`` =
                        fun (props: {| colorMode: string |}) ->
                            {|
                                ``*, *::before, *::after`` = {| wordWrap = "break-word" |}
                                html = {| fontSize = "main" |}
                                body =
                                    {|
                                        fontFamily = "main"
                                        color = "gray.77"
                                        backgroundColor = if props.colorMode = "dark" then "gray.13" else "gray.77"
                                        fontWeight = "light"
                                        letterSpacing = 0
                                        lineHeight = "12px"
                                        fontFeatureSettings = "pnum"
                                        fontVariantNumeric = "proportional-nums"
                                        margin = 0
                                        padding = 0
                                        boxSizing = "border-box"
                                        fontSize = "12px"
                                        color = "#ddd"
                                        userSelect = "none"
                                        touchAction = "pan-x pan-y"
                                    |}
                                ``input::-ms-reveal`` =
                                    {|
                                        filter = if props.colorMode = "dark" then "invert(1)" else ""
                                    |}
                                ``input::-ms-clear`` =
                                    {|
                                        filter = if props.colorMode = "dark" then "invert(1)" else ""
                                    |}
                                ``*::-webkit-calendar-picker-indicator`` =
                                    {|
                                        filter = if props.colorMode = "dark" then "invert(1)" else ""
                                    |}
                                ``*::-webkit-scrollbar`` = {| width = "9px" |}
                                ``*::-webkit-scrollbar:horizontal`` = {| height = "6px" |}
                                ``*::-webkit-scrollbar-track`` = {| display = "none" |}
                                ``*::-webkit-scrollbar-corner`` = {| display = "none" |}
                                ``*::-webkit-scrollbar-thumb`` =
                                    {|
                                        background = "gray.45"
                                        opacity = 0.8
                                        backgroundClip = "content-box"
                                        borderLeft = "3px solid transparent"
                                    |}
                                ``*::-webkit-scrollbar-thumb:hover`` =
                                    {|
                                        background = "gray.77"
                                        backgroundClip = "content-box"
                                        borderLeft = "3px solid transparent"
                                    |}
                                ``*:focus`` =
                                    {|
                                        boxShadow = $"0 0 0 1px {theme.colors.heliotrope} !important"
                                    |}
                                ``.rct-collapse-btn:focus`` =
                                    {|
                                        boxShadow = "none !important"
                                    |}
                                ``.rct-disabled svg`` =
                                    {|
                                        opacity = "0"
                                    |}
                                ``.rct-disabled .rct-title`` =
                                    {|
                                        marginLeft = "-14px"
                                    |}
                                ``.react-checkbox-tree label:hover`` =
                                    {|
                                        background = "none"
                                    |}
                                ``.markdown-container h1`` =
                                    {|
                                        borderBottom = "1px solid #777"
                                        marginBottom = "3px"
                                    |}
                                ``.markdown-container li`` = {| listStyleType = "square" |}
                                ``.markdown-container ul, .tooltip-popup p`` = {| padding = "5px 0" |}
                            |}
                |}
        |}

    let useTheme () =
        React.useMemo (fun () -> Chakra.react.extendTheme (JsInterop.toPlainJsObj theme))
