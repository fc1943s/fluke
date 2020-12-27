namespace Fluke.UI.Frontend.Components

open Fable.Core.JsInterop
open Browser.Types
open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module Settings =

    [<ReactComponent>]
    let Settings (input: {| Props: {| flex: int; overflowY: string; flexBasis: int |} |}) =
        let daysBefore, setDaysBefore = Recoil.useState (Recoil.Atoms.daysBefore)
        let daysAfter, setDaysAfter = Recoil.useState (Recoil.Atoms.daysAfter)
        let apiBaseUrl, setApiBaseUrl = Recoil.useState (Recoil.Atoms.apiBaseUrl)

        Chakra.box
            input.Props
            [
                Chakra.box
                    ()
                    [
                        str "Days Before"
                    ]
                Chakra.numberInput
                    {|
                        value = daysBefore
                        onChange = fun valueString -> setDaysBefore (int valueString)
                        min = 0
                        max = 360
                        marginTop = "5px"
                    |}
                    [
                        Chakra.numberInputField () []
                        Chakra.numberInputStepper
                            ()
                            [
                                Chakra.numberIncrementStepper () []
                                Chakra.numberDecrementStepper () []
                            ]
                    ]

                Chakra.box
                    {| marginTop = "15px" |}
                    [
                        str "Days After"
                    ]
                Chakra.numberInput
                    {|
                        value = daysAfter
                        onChange = fun valueString -> setDaysAfter (int valueString)
                        min = 0
                        max = 360
                        marginTop = "5px"
                    |}
                    [
                        Chakra.numberInputField () []
                        Chakra.numberInputStepper
                            ()
                            [
                                Chakra.numberIncrementStepper () []
                                Chakra.numberDecrementStepper () []
                            ]
                    ]


                Chakra.box
                    {| marginTop = "15px" |}
                    [
                        str "Old API URL"
                    ]
                Chakra.input
                    {|
                        value = apiBaseUrl
                        onChange = fun (e: KeyboardEvent) -> setApiBaseUrl e.target?value
                        marginTop = "5px"
                    |}
                    []
            ]
