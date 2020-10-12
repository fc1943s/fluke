namespace Fluke.UI.Frontend.Hooks

open Browser
open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks
open Fable.React
open Fable.Core

module Theme =
    let private theme =
        {|
            config =
                {|
                    initialColorMode = "dark"
                    useSystemColorMode = false
                |}
                |> JsInterop.toPlainJsObj
            breakpoints =
                {|
                    sm = "350px"
                    md = "750px"
                    lg = "1000px"
                    xl = "1900px"
                |}
                |> JsInterop.toPlainJsObj
            colors =
                {|
                    gray =
                        {|
                            ``10%`` = "#1a1a1a" // grayDark
                            ``13%`` = "#212121" // grayLight
                            ``16%`` = "#292929" // grayLighter
                            ``77%`` = "#b0bec5" // textLight
                            ``45%`` = "#727272" // textDark
                        |}
                        |> JsInterop.toPlainJsObj
                |}
                |> JsInterop.toPlainJsObj
            fonts = {| main = "'Roboto Condensed', sans-serif" |}
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
                |> JsInterop.toPlainJsObj
            styles =
                {|
                    ``global`` =
                        fun props ->
                            {|
                                html = {| fontSize = "12px" |} |> JsInterop.toPlainJsObj
                                body =
                                    {|
                                        fontFamily = "main"
                                        color = "gray.77%"
                                        backgroundColor = Chakra.theme.mode ("white", "gray.13%") props
                                        fontWeight = "light"
                                        letterSpacing = 0
                                        lineHeight = "12px"
                                        fontFeatureSettings = "pnum"
                                        fontVariantNumeric = "proportional-nums"
                                    |}
                                    |> JsInterop.toPlainJsObj
                                ``*::-webkit-scrollbar`` = {| width = "6px" |} |> JsInterop.toPlainJsObj
                                ``*::-webkit-scrollbar-track`` = {| display = "none" |} |> JsInterop.toPlainJsObj
                                ``*::-webkit-scrollbar-thumb`` =
                                    {| background = "gray.45%" |}
                                    |> JsInterop.toPlainJsObj
                                ``*::-webkit-scrollbar-thumb:hover`` =
                                    {| background = "gray.77%" |}
                                    |> JsInterop.toPlainJsObj
                                ``*::focus`` =
                                    {| boxShadow = "none !important" |}
                                    |> JsInterop.toPlainJsObj
                                ``*, *::before, *::after`` =
                                    {| wordWrap = "break-word" |}
                                    |> JsInterop.toPlainJsObj
                            |}
                            |> JsInterop.toPlainJsObj
                |}
                |> JsInterop.toPlainJsObj
        |}
        |> JsInterop.toPlainJsObj

    let useTheme () = React.useMemo ((fun () -> Chakra.core.extendTheme theme), [||])
