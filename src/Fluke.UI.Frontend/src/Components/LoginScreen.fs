namespace Fluke.UI.Frontend.Components

open Browser.Types
open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks
open Fable.React
open Fable.Core.JsInterop


module LoginScreen =

    [<ReactComponent>]
    let LoginScreen () =
        let usernameField, setUsernameField = React.useState ""
        let passwordField, setPasswordField = React.useState ""
        let signIn = Auth.useSignIn ()
        let signUp = Auth.useSignUp ()

        let toast = Chakra.react.useToast ()

        let signInClick () =
            promise {
                match! signIn usernameField passwordField with
                | Ok () -> printfn "logged"
                | Error error ->
                    toast.Invoke
                        {|
                            title = "Error"
                            status = "error"
                            description = error
                            duration = 4000
                            isClosable = true
                        |}
            }

        let signUpClick () =
            promise {
                match! signUp usernameField passwordField with
                | Ok () ->
                    toast.Invoke
                        {|
                            title = "Success"
                            status = "success"
                            description = "User registered successfully"
                            duration = 4000
                            isClosable = true
                        |}
                | Error error ->
                    toast.Invoke
                        {|
                            title = "Error"
                            status = "error"
                            description = error
                            duration = 4000
                            isClosable = true
                        |}
            }

        Chakra.center
            {| flex = 1 |}
            [
                Chakra.stack
                    {|  |}
                    [
                        Input.Input (
                            jsOptions<_>
                                (fun x ->
                                    x.autoFocus <- true
                                    x.value <- Some usernameField
                                    x.placeholder <- "Username"
                                    x.onChange <- Some (fun (e: KeyboardEvent) -> promise { setUsernameField e.Value }))
                        )

                        Input.Input (
                            jsOptions<_>
                                (fun x ->
                                    x.value <- Some passwordField
                                    x.placeholder <- "Password"
                                    x.inputFormat <- Some Input.InputFormat.Password
                                    x.onChange <- Some (fun (e: KeyboardEvent) -> promise { setPasswordField e.Value })
                                    x.onEnterPress <- Some signInClick)
                        )

                        Chakra.hStack
                            {| align = "stretch" |}
                            [
                                Chakra.button
                                    {|
                                        flex = 1
                                        onClick = signInClick
                                        color = "gray"
                                    |}
                                    [
                                        str "Sign In"
                                    ]
                                Chakra.button
                                    {|
                                        flex = 1
                                        onClick = signUpClick
                                        color = "gray"
                                    |}
                                    [
                                        str "Sign Up"
                                    ]
                            ]
                    ]
            ]
