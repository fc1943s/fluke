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

        Chakra.center
            (fun x -> x.flex <- "1")
            [
                Chakra.stack
                    (fun x -> x.minWidth <- "200px")
                    [
                        Input.Input
                            {|
                                CustomProps =
                                    fun x ->
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

                                        x.fixedValue <- Some gunPeer
                                        x.onEnterPress <- Some nextClick
                                Props =
                                    fun x ->
                                        x.label <- str "Gun peer"
                                        x.placeholder <- "https://??????.herokuapp.com/gun"
                                        x.onChange <- (fun (e: KeyboardEvent) -> promise { setGunPeer e.Value })
                            |}

                        Chakra.hStack
                            (fun x -> x.alignItems <- "stretch")
                            [
                                Button.Button
                                    {|
                                        Hint = None
                                        Icon = Some (Icons.cg.CgCornerDownRight |> Icons.wrap, Button.IconPosition.Left)
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
                                        Hint = None
                                        Icon = Some (Icons.hi.HiArrowRight |> Icons.wrap, Button.IconPosition.Right)
                                        Props =
                                            fun x ->
                                                x.flex <- "1"
                                                x.onClick <- nextClick
                                                x.color <- "gray"
                                                x.disabled <- gunPeer.Length = 0
                                        Children =
                                            [
                                                str "Connect"
                                            ]
                                    |}

                            ]
                    ]
            ]
