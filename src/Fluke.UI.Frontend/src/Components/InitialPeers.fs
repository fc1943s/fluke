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
        let setGunPeers = Recoil.useSetState Atoms.gunPeers
        let setInitialPeerSkipped = Recoil.useSetState Atoms.initialPeerSkipped

        let nextClick _ =
            promise { if gunPeer.Length > 0 then setGunPeers [ gunPeer ] }

        let skipClick _ = promise { setInitialPeerSkipped true }

        Chakra.center
            (fun x -> x.flex <- 1)
            [
                Chakra.stack
                    (fun x -> x.minWidth <- "200px")
                    [
                        Input.Input (
                            JS.newObj
                                (fun x ->
                                    x.autoFocus <- true
                                    x.label <- str "Gun peer"

                                    x.hint <-
                                        Some (
                                            ExternalLink.ExternalLink
                                                {|
                                                    Text = "Read documentation"
                                                    Props =
                                                        JS.newObj
                                                            (fun x ->
                                                                x.isExternal <- true

                                                                x.href <-
                                                                    "https://gun.eco/docs/FAQ#what-is-the-difference-between-super-peer-and-other-peers")
                                                |}
                                        )


                                    x.value <- Some "https://flukegunpeer-test.herokuapp.com/gun"
                                    x.placeholder <- "https://??????.herokuapp.com/gun"
                                    x.onEnterPress <- Some nextClick
                                    x.onChange <- (fun (e: KeyboardEvent) -> promise { setGunPeer e.Value }))
                        )

                        Chakra.hStack
                            (fun x -> x.align <- "stretch")
                            [
                                Button.Button
                                    {|
                                        Icon = None
                                        Hint = None
                                        Props =
                                            JS.newObj
                                                (fun x ->
                                                    x.flex <- 1
                                                    x.autoFocus <- true
                                                    x.onClick <- skipClick
                                                    x.color <- "gray"

                                                    x.children <-
                                                        [
                                                            str "Skip"
                                                        ])
                                    |}

                                Button.Button
                                    {|
                                        Icon = None
                                        Hint = None
                                        Props =
                                            JS.newObj
                                                (fun x ->
                                                    x.flex <- 1
                                                    x.onClick <- nextClick
                                                    x.color <- "gray"
                                                    x.disabled <- gunPeer.Length = 0

                                                    x.children <-
                                                        [
                                                            str "Next"
                                                        ])
                                    |}

                            ]
                    ]
            ]
