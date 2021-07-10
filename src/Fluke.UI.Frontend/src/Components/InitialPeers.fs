namespace Fluke.UI.Frontend.Components

open Browser.Types
open Feliz
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State
open Fluke.Shared
open Fable.React


module InitialPeers =
    [<ReactComponent>]
    let InitialPeers () =
        let gunPeer, setGunPeer = React.useState "https://flukegunpeer-test.herokuapp.com/gun"
        let setGunPeers = Store.useSetState Store.Atoms.gunPeers
        let setInitialPeerSkipped = Store.useSetState Atoms.initialPeerSkipped

        let nextClick _ =
            promise {
                match gunPeer with
                | String.ValidString _ -> setGunPeers [| gunPeer |]
                | _ -> ()
            }

        let skipClick _ = promise { setInitialPeerSkipped true }

        UI.center
            (fun x -> x.flex <- "1")
            [
                UI.stack
                    (fun x -> x.minWidth <- "200px")
                    [
                        Input.Input
                            {|
                                CustomProps =
                                    fun x ->
                                        x.hint <-
                                            Some (
                                                UI.box
                                                    (fun _ -> ())
                                                    [
                                                        UI.box
                                                            (fun _ -> ())
                                                            [
                                                                str "Add a relay peer to sync data between devices"
                                                            ]

                                                        br []

                                                        ExternalLink.ExternalLink
                                                            {|
                                                                Link = str "Read documentation"
                                                                Href =
                                                                    "https://gun.eco/docs/FAQ#what-is-the-difference-between-super-peer-and-other-peers"
                                                                Props = fun _ -> ()
                                                            |}
                                                    ]
                                            )

                                        x.fixedValue <- Some gunPeer
                                        x.onEnterPress <- Some nextClick
                                Props =
                                    fun x ->
                                        x.label <- str "Relay peer"
                                        x.placeholder <- "https://??????.herokuapp.com/gun"
                                        x.onChange <- (fun (e: KeyboardEvent) -> promise { setGunPeer e.Value })
                            |}

                        UI.hStack
                            (fun x -> x.alignItems <- "stretch")
                            [
                                Button.Button
                                    {|
                                        Hint = None
                                        Icon =
                                            Some (Icons.cg.CgCornerDownRight |> Icons.render, Button.IconPosition.Left)
                                        Props =
                                            fun x ->
                                                x.flex <- "1"
                                                x.autoFocus <- true
                                                x.onClick <- skipClick
                                        Children =
                                            [
                                                str "Skip"
                                            ]
                                    |}

                                Button.Button
                                    {|
                                        Hint = None
                                        Icon = Some (Icons.hi.HiArrowRight |> Icons.render, Button.IconPosition.Right)
                                        Props =
                                            fun x ->
                                                x.flex <- "1"
                                                x.onClick <- nextClick
                                                x.disabled <- gunPeer.Length = 0
                                        Children =
                                            [
                                                str "Connect"
                                            ]
                                    |}

                            ]
                    ]
            ]
