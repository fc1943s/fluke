namespace Fluke.UI.Frontend.Hooks

open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings
open Fable.Core


module Theme =
    let private theme =
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
                                html = {| fontSize = "main" |}
                                body =
                                    {|
                                        fontFamily = "main"
                                        color = "gray.77"
                                        backgroundColor =
                                            if props.colorMode = "dark" then
                                                "gray.13"
                                            else
                                                "gray.77"
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
                                    |}
                                ``*::-webkit-calendar-picker-indicator`` =
                                    {|
                                        filter =
                                            if props.colorMode = "dark" then
                                                "invert(1)"
                                            else
                                                ""
                                    |}
                                ``input::-ms-reveal`` =
                                    {|
                                        filter =
                                            if props.colorMode = "dark" then
                                                "invert(1)"
                                            else
                                                ""
                                    |}
                                ``input::-ms-clear`` =
                                    {|
                                        filter =
                                            if props.colorMode = "dark" then
                                                "invert(1)"
                                            else
                                                ""
                                    |}
                                ``*::-webkit-scrollbar`` = {| width = "6px" |}
                                ``*::-webkit-scrollbar-track`` = {| display = "none" |}
                                ``*::-webkit-scrollbar-thumb`` = {| background = "gray.45" |}
                                ``*::-webkit-scrollbar-thumb:hover`` = {| background = "gray.77" |}
                                ``*:focus`` =
                                    {|
                                        boxShadow = "0 0 0 1px #5ca0c1 !important"
                                    |}
                                ``*, *::before, *::after`` = {| wordWrap = "break-word" |}
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
