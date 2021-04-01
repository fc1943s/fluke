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
        let setInitialPeerSkipped = Recoil.useSetState Atoms.initialPeerSkipped

        let nextClick () = promise { setGunPeer1 gunPeer }
        let skipClick () = promise { setInitialPeerSkipped true }

        Chakra.center
            {| flex = 1 |}
            [
                Chakra.stack
                    {| spacing = "5px" |}
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
                            |}
                            []

                        Chakra.hStack
                            {| align = "stretch" |}
                            [
                                Button.Button
                                    {|
                                        Icon = None
                                        RightIcon = None
                                        props =
                                            {|
                                                marginLeft = None
                                                flex = Some 1
                                                autoFocus = Some true
                                                onClick = Some skipClick
                                                color = Some "gray"
                                            |}
                                        children =
                                            [
                                                str "Skip"
                                            ]
                                    |}
                                Chakra.button
                                    {|
                                        flex = 1
                                        onClick = nextClick
                                        color = "gray"
                                        disabled = gunPeer.Length = 0
                                    |}
                                    [
                                        str "Next"
                                    ]
                            ]
                    ]
            ]
