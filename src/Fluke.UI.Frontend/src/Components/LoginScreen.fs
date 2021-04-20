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
        let usernameField, setUsernameField = React.useState ""
        let passwordField, setPasswordField = React.useState ""
        let signIn = Auth.useSignIn ()
        let signUp = Auth.useSignUp ()

        let toast = Chakra.react.useToast ()

        let signInClick _ =
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

        let signUpClick _ =
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
            (fun x -> x.flex <- 1)
            [
                Chakra.stack
                    (fun _ -> ())
                    [
                        Input.Input (
                            JS.newObj
                                (fun x ->
                                    x.autoFocus <- true
                                    x.value <- Some usernameField
                                    x.placeholder <- "Username"

                                    x.onChange <- (fun (e: KeyboardEvent) -> promise { setUsernameField e.Value }))
                        )

                        Input.Input (
                            JS.newObj
                                (fun x ->
                                    x.value <- Some passwordField
                                    x.placeholder <- "Password"
                                    x.inputFormat <- Some Input.InputFormat.Password

                                    x.onChange <- (fun (e: KeyboardEvent) -> promise { setPasswordField e.Value })

                                    x.onEnterPress <- Some signInClick)
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
                                                    x.onClick <- signInClick
                                                    x.color <- "gray"

                                                    x.children <-
                                                        [
                                                            str "Login"
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
                                                    x.onClick <- signUpClick
                                                    x.color <- "gray"

                                                    x.children <-
                                                        [
                                                            str "Register"
                                                        ])
                                    |}
                            ]
                    ]
            ]
