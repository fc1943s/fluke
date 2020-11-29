namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.UseListener
open Feliz.Recoil
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks
open Fluke.Shared.Domain
open Fluke.UI.Frontend
open Fable.React


module GunObserver =

    [<ReactComponent>]
    let gunObserver () =
        let gun = Recoil.useValue Recoil.Selectors.gun
        let setUsername = Recoil.useSetState Recoil.Atoms.username
        let setSessionRestored = Recoil.useSetState Recoil.Atoms.sessionRestored
        printfn "GunObserver.render: Constructor"

        React.useEffect
            ((fun () ->
                let recall = Browser.Dom.window.sessionStorage.getItem "recall"
                printfn $"recall {recall}"

                match recall with
                | null
                | "" -> setSessionRestored true
                | _ -> ()

                let user = gun.root.user ()
                printfn "before recall"

                try
                    user.recall
                        ({| sessionStorage = true |},
                         (fun ack ->
                             match ack.put with
                             | Some put -> setUsername (Some (UserInteraction.Username put.alias))
                             | None -> printfn "Empty ack"

                             setSessionRestored true

                             printfn $"ACK {ack.put}"
                             Dom.set "ack" ack))
                with ex -> printfn $"ERROR: {ex}"

                printfn "after recall"),
             [|
                 box gun
             |])

        React.useDisposableEffect
            ((fun disposed -> ()),
             //                    gun.root.on
//                        ("auth",
//                         (fun () ->
//                             match disposed.current with
//                             | false ->
//                                 let user = gun.root.user ()
//                                 match user.is.alias with
//                                 | Some username -> setUsername (Some (UserInteraction.Username username))
//                                 | None -> printfn "GunObserver.render: Auth occurred without username: %A" user.is
//                             | true -> ()))),
             [|
                 box gun
             |])

        nothing
