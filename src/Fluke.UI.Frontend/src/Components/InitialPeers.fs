namespace Fluke.UI.Frontend.Components

open Browser.Types
open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings
open Feliz.Recoil
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State
open Fluke.Shared
open Fable.React


module InitialPeers =
    [<ReactComponent>]
    let InitialPeers () =
        let gunPeer, setGunPeer = React.useState "https://flukegunpeer-test.herokuapp.com/gun"
        let setGunPeers = Recoil.useSetState Atoms.gunPeers
        let setInitialPeerSkipped = Recoil.useSetState Atoms.initialPeerSkipped

        let nextClick _ =
            promise {
                match gunPeer with
                | String.ValidString _ -> setGunPeers [ gunPeer ]
                | _ -> ()
            }

        let skipClick _ = promise { setInitialPeerSkipped true }

        Chakra.center
            (fun x -> x.flex <- "1")
            [
                Chakra.stack
                    (fun x -> x.minWidth <- "200px")
                    [
                        Input.Input
                            (fun x ->
                                x.autoFocus <- true
                                x.label <- str "Gun peer"

                                x.hint <-
                                    Some (
                                        ExternalLink.ExternalLink
                                            {|
                                                Link = str "Read documentation"
                                                Href =
                                                    "https://gun.eco/docs/FAQ#what-is-the-difference-between-super-peer-and-other-peers"
                                                Props = fun _ -> ()
                                            |}
                                    )


                                x.value <- Some gunPeer
                                x.placeholder <- "https://??????.herokuapp.com/gun"
                                x.onEnterPress <- Some nextClick
                                x.onChange <- (fun (e: KeyboardEvent) -> promise { setGunPeer e.Value }))

                        Chakra.hStack
                            (fun x -> x.align <- "stretch")
                            [
                                Button.Button
                                    {|
                                        Icon = None
                                        Hint = None
                                        Props =
                                            fun x ->
                                                x.flex <- "1"
                                                x.autoFocus <- true
                                                x.onClick <- skipClick
                                                x.color <- "gray"
                                        Children =
                                            [
                                                str "Skip"
                                            ]
                                    |}

                                Button.Button
                                    {|
                                        Icon = None
                                        Hint = None
                                        Props =
                                            fun x ->
                                                x.flex <- "1"
                                                x.onClick <- nextClick
                                                x.color <- "gray"
                                                x.disabled <- gunPeer.Length = 0
                                        Children =
                                            [
                                                str "Next"
                                            ]
                                    |}

                            ]
                    ]
            ]
