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
    let Settings (input: {| Props: {| flex: int; overflowY: string; flexBasis: int |} |}) =
        let daysBefore, setDaysBefore = Recoil.useState Atoms.daysBefore
        let daysAfter, setDaysAfter = Recoil.useState Atoms.daysAfter
        let apiBaseUrl, setApiBaseUrl = Recoil.useState Atoms.apiBaseUrl
        let gunPeer1, setGunPeer1 = Recoil.useState Atoms.gunPeer1
        let gunPeer2, setGunPeer2 = Recoil.useState Atoms.gunPeer2
        let gunPeer3, setGunPeer3 = Recoil.useState Atoms.gunPeer3


        let gunNamespace = Recoil.useValue Selectors.gunNamespace

        let rendered, setRendered = React.useState false

        React.useEffect (
            (fun () ->
                promise {
                    let daysBeforeNode =
                        gunNamespace
                            .ref
                            .get("fluke")
                            .get("settings")
                            .get ("daysBefore")

                    let id = System.Guid.NewGuid ()
                    printfn $"before the ON! {id} daysBeforeNode={daysBeforeNode}"

                    daysBeforeNode.on
                        (fun (data: int option) ->
                            match data with
                            | Some data ->
                                printfn $"Settings effect. daysbeforenode. ON DATA: {data}"
                                setDaysBefore data
                            | None -> ())

                    setRendered true

                    return fun () -> printfn $"rendering off {id}"
                //                            daysBeforeNode.off ()
                }
                |> Promise.start),
            [|
                box gunNamespace.ref
            |]
        )


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
                                if rendered then
                                    printfn $"onChange input. setting atom and put on gun {valueString}"
                                    let value = int valueString
                                    setDaysBefore value

                                    gunNamespace
                                        .ref
                                        .get("fluke")
                                        .get("settings")
                                        .put {| daysBefore = value |}
                                    |> ignore
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
