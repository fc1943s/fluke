namespace Fluke.UI.Frontend.Components

open Fluke.Shared
open Feliz
open Fable.React
open Feliz.Recoil
open FSharpPlus
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module Settings =
    open Domain.Model
    open Domain.UserInteraction
    open Domain.State

    let render =
        React.memo (fun (input: {| Props: {| flex: int; overflowY: string; flexBasis: int |} |}) ->
            let daysBefore, setDaysBefore = Recoil.useState (Recoil.Atoms.daysBefore)
            let daysAfter, setDaysAfter = Recoil.useState (Recoil.Atoms.daysAfter)

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
                ])
