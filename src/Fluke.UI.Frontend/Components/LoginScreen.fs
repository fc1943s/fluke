namespace Fluke.UI.Frontend.Components

open Browser.Types
open Fable.Core
open Feliz
open Feliz.UseListener
open Feliz.Recoil
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend
open Fable.React


module LoginScreen =
    let render =
        React.memo (fun () ->
            let username, setUsername = React.useState ""
            let password, setPassword = React.useState ""
            let gun = Recoil.useValue Recoil.Selectors.gun

            let signIn () =
                promise {
                    let user = gun.Gun.user ()
                    let! ack = Gun.authUser user username password
                    if JsInterop.isNullOrUndefined ack.err then
                        printfn "no errors found.  fluke.ack obj"
                        Bindings.Dom.set "ack" ack
                    else
                        printfn "ack error %A" ack.err
                    ()
                }

            let signUp () = ()

            Chakra.center
                {| flex = 1 |}
                [
                    Chakra.stack
                        ()
                        [
                            Chakra.input
                                {|
                                    value = username
                                    onChange = fun (e: KeyboardEvent) -> setUsername e.Value
                                    placeholder = "Username"
                                |}
                                []

                            Chakra.input
                                {|
                                    value = password
                                    onChange = fun (e: KeyboardEvent) -> setPassword e.Value
                                    placeholder = "Password"
                                |}
                                []

                            Chakra.hStack
                                {| align = "stretch" |}
                                [
                                    Chakra.button
                                        {| flex = 1; onClick = signIn |}
                                        [
                                            str "Sign In"
                                        ]
                                    Chakra.button
                                        {| onClick = signUp |}
                                        [
                                            str "Sign Up"
                                        ]
                                ]
                        ]
                ])
