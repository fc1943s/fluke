namespace Fluke.UI.Frontend.Components

open Fable.Core.JsInterop
open Browser.Types
open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module Settings =
    open Fluke.UI.Frontend.Recoil

    [<ReactComponent>]
    let rec Settings
        (input: {| Props: {| flex: int
                             overflowY: string
                             flexBasis: int |} |})
        =
        let daysBefore, setDaysBefore = Recoil.useState Atoms.daysBefore
        let daysAfter, setDaysAfter = Recoil.useState Atoms.daysAfter
        let apiBaseUrl, setApiBaseUrl = Recoil.useState Atoms.apiBaseUrl
        let gunPeer1, setGunPeer1 = Recoil.useState Atoms.gunPeer1
        let gunPeer2, setGunPeer2 = Recoil.useState Atoms.gunPeer2
        let gunPeer3, setGunPeer3 = Recoil.useState Atoms.gunPeer3

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
                        onChange =
                            fun valueString ->
                                let value = int valueString
                                setDaysBefore value
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

                Chakra.box
                    {| marginTop = "15px" |}
                    [
                        str "Gun peer 1"
                    ]
                Chakra.input
                    {|
                        value = gunPeer1
                        onChange = fun (e: KeyboardEvent) -> setGunPeer1 e.target?value
                        marginTop = "5px"
                    |}
                    []

                Chakra.box
                    {| marginTop = "15px" |}
                    [
                        str "Gun peer 2"
                    ]
                Chakra.input
                    {|
                        value = gunPeer2
                        onChange = fun (e: KeyboardEvent) -> setGunPeer2 e.target?value
                        marginTop = "5px"
                    |}
                    []

                Chakra.box
                    {| marginTop = "15px" |}
                    [
                        str "Gun peer 3"
                    ]
                Chakra.input
                    {|
                        value = gunPeer3
                        onChange = fun (e: KeyboardEvent) -> setGunPeer3 e.target?value
                        marginTop = "5px"
                    |}
                    []
            ]
