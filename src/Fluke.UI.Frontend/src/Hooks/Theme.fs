namespace Fluke.UI.Frontend.Hooks

open Fable.React.Props
open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings
open Fable.Core


module Theme =
    let private theme =
        JsInterop.toPlainJsObj
            {|
                config =
                    JsInterop.toPlainJsObj
                        {|
                            initialColorMode = "dark"
                            useSystemColorMode = false
                        |}
                breakpoints =
                    JsInterop.toPlainJsObj
                        {|
                            sm = "350px"
                            md = "750px"
                            lg = "1000px"
                            xl = "1900px"
                        |}
                colors =
                    JsInterop.toPlainJsObj
                        {|
                            gray =
                                {|
                                    ``10`` = "#1a1a1a" // grayDark
                                    ``13`` = "#212121" // grayLight
                                    ``16`` = "#292929" // grayLighter
                                    ``45`` = "#727272" // textDark
                                    ``77`` = "#b0bec5" // textLight
                                |}
                                |> JsInterop.toPlainJsObj
                        |}
                fonts = JsInterop.toPlainJsObj {| main = "'Roboto Condensed', sans-serif" |}
                fontWeights =
                    JsInterop.toPlainJsObj
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
                fontSizes = JsInterop.toPlainJsObj {| main = "12px" |}
                styles =
                    JsInterop.toPlainJsObj
                        {|
                            ``global`` =
                                fun (props: {| colorMode: string |}) ->
                                    printfn $"props on global {JS.JSON.stringify props}"

                                    JsInterop.toPlainJsObj
                                        {|
                                            html = JsInterop.toPlainJsObj {| fontSize = "main" |}
                                            body =
                                                JsInterop.toPlainJsObj
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
                                            ``*::-webkit-scrollbar`` = JsInterop.toPlainJsObj {| width = "6px" |}
                                            ``*::-webkit-scrollbar-track`` =
                                                JsInterop.toPlainJsObj {| display = "none" |}
                                            ``*::-webkit-scrollbar-thumb`` =
                                                JsInterop.toPlainJsObj {| background = "gray.45" |}
                                            ``*::-webkit-scrollbar-thumb:hover`` =
                                                JsInterop.toPlainJsObj {| background = "gray.77" |}
                                            ``*:focus`` =
                                                JsInterop.toPlainJsObj {| boxShadow = "0 0 0 1px #5ca0c1 !important" |}
                                            ``*, *::before, *::after`` =
                                                JsInterop.toPlainJsObj {| wordWrap = "break-word" |}
                                            ``.markdown-container h1`` =
                                                JsInterop.toPlainJsObj
                                                    {|
                                                        borderBottom = "1px solid #777"
                                                        marginBottom = "3px"
                                                    |}
                                            ``.markdown-container li`` =
                                                JsInterop.toPlainJsObj {| listStyleType = "square" |}
                                            ``.markdown-container ul, .tooltip-popup p`` =
                                                JsInterop.toPlainJsObj {| padding = "5px 0" |}
                                        |}
                        |}
            |}

    let useTheme () =
        React.useMemo (
            (fun () -> Chakra.react.extendTheme theme),
            [|
                theme
            |]
        )
