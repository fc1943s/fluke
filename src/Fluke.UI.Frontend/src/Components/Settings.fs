namespace Fluke.UI.Frontend.Components

open Fable.Core.JsInterop
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

        Chakra.stack
            {| input.Props with spacing = "10px" |}
            [
                Chakra.box
                    ()
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
                    ]

                Chakra.box
                    ()
                    [
                        Chakra.box
                            {|  |}
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
                    ]

                Input.Input (
                    jsOptions<_>
                        (fun x ->
                            x.label <- "Old API URL"
                            x.atom <- Some (Recoil.Atom Atoms.apiBaseUrl)
                            x.atomScope <- Some Recoil.AtomScope.ReadOnly)
                )

                Input.Input (
                    jsOptions<_>
                        (fun x ->
                            x.label <- "Gun peer 1"
                            x.atom <- Some (Recoil.Atom Atoms.gunPeer1)
                            x.atomScope <- Some Recoil.AtomScope.ReadOnly)
                )

                Input.Input (
                    jsOptions<_>
                        (fun x ->
                            x.label <- "Gun peer 2"
                            x.atom <- Some (Recoil.Atom Atoms.gunPeer2)
                            x.atomScope <- Some Recoil.AtomScope.ReadOnly)
                )

                Input.Input (
                    jsOptions<_>
                        (fun x ->
                            x.label <- "Gun peer 3"
                            x.atom <- Some (Recoil.Atom Atoms.gunPeer3)
                            x.atomScope <- Some Recoil.AtomScope.ReadOnly)
                )
            ]
