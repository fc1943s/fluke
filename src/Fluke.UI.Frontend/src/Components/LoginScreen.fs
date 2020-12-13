namespace Fluke.UI.Frontend.Components

open Browser.Types
open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks
open Fable.React


module LoginScreen =

    [<ReactComponent>]
    let LoginScreen () =
        let usernameField, setUsernameField = React.useState "fc1943s"
        let passwordField, setPasswordField = React.useState "123456"
        let signIn = Auth.useSignIn ()
        let signUp = Auth.useSignUp ()

        let toast = Chakra.core.useToast ()

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
                    setUsernameField ""
                    setPasswordField ""

                    toast.Invoke
                        {|
                            title = "Success"
                            status = "success"
                            description = "User registered successfully! Please log in"
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
                        Chakra.input
                            {|
                                value = usernameField
                                onChange = fun (e: KeyboardEvent) -> setUsernameField e.Value
                                placeholder = "Username"
                            |}
                            []

                        Chakra.input
                            {|
                                value = passwordField
                                onChange = fun (e: KeyboardEvent) -> setPasswordField e.Value
                                placeholder = "Password"
                            |}
                            []

                        Chakra.hStack
                            {| align = "stretch" |}
                            [
                                Chakra.button
                                    {| flex = 1; onClick = signInClick |}
                                    [
                                        str "Sign In"
                                    ]
                                Chakra.button
                                    {| onClick = signUpClick |}
                                    [
                                        str "Sign Up"
                                    ]
                            ]
                    ]
            ]
