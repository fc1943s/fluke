namespace Fluke.UI.Frontend.Components

open Browser.Types
open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings
open Feliz.Recoil
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Recoil
open Fable.React
open Fable.Core.JsInterop


module InitialPeers =
    [<ReactComponent>]
    let InitialPeers () =
        let gunPeer, setGunPeer = React.useState ""
        let setGunPeer1 = Recoil.useSetState Atoms.gunPeer1

        let nextClick () = promise { setGunPeer1 gunPeer }

        Chakra.center
            {| flex = 1 |}
            [
                Chakra.stack
                    {|  |}
                    [
                        Chakra.box
                            {| marginTop = "15px" |}
                            [
                                str "Gun peer"
                            ]
                        Chakra.input
                            {|
                                value = gunPeer
                                onChange = fun (e: KeyboardEvent) -> setGunPeer e.target?value
                                marginTop = "5px"
                            |}
                            []

                        Chakra.button
                            {|
                                onClick = nextClick
                                color = "gray"
                                disabled = gunPeer.Length = 0
                            |}
                            [
                                str "Next"
                            ]
                    ]
            ]
