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


module InitialPeers =
    [<ReactComponent>]
    let InitialPeers () =
        let gunPeer, setGunPeer = React.useState ""
        let setGunPeer1 = Recoil.useSetState Atoms.gunPeer1
        let setInitialPeerSkipped = Recoil.useSetState Atoms.initialPeerSkipped

        let nextClick () =
            promise {
                if gunPeer.Length > 0 then
                    setGunPeer1 gunPeer
            }

        let skipClick () = promise { setInitialPeerSkipped true }

        Chakra.center
            {| flex = 1 |}
            [
                Chakra.stack
                    {| minWidth = "200px" |}
                    [
                        Input.Input (
                            Dom.newObj
                                (fun x ->
                                    x.autoFocus <- true
                                    x.label <- "Gun peer"

                                    x.hint <-
                                        Some (
                                            ExternalLink.ExternalLink
                                                {|
                                                    isExternal = true
                                                    href =
                                                        "https://gun.eco/docs/FAQ#what-is-the-difference-between-super-peer-and-other-peers"
                                                    text = "Read documentation"
                                                |}
                                        )


                                    x.placeholder <- "https://??????.herokuapp.com/gun"
                                    x.onEnterPress <- Some nextClick
                                    x.onChange <- Some (fun (e: KeyboardEvent) -> promise { setGunPeer e.Value }))
                        )

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
