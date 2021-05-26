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
        let password2Field, setPassword2Field = React.useState ""
        let signIn = Auth.useSignIn ()
        let signUp = Auth.useSignUp ()
        let toast = Chakra.useToast ()

        let signInClick _ =
            promise {
                match! signIn usernameField passwordField with
                | Ok () -> printfn "logged"
                | Error error -> toast (fun x -> x.description <- error)
            }

        let signUpClick _ =
            promise {
                if passwordField <> password2Field then
                    toast (fun x -> x.description <- "Passwords don't match")
                else
                    match! signUp usernameField passwordField with
                    | Ok () ->
                        toast
                            (fun x ->
                                x.title <- "Success"
                                x.status <- "success"
                                x.description <- "User registered successfully")
                    | Error error -> toast (fun x -> x.description <- error)
            }

        Chakra.center
            (fun x -> x.flex <- "1")
            [
                Chakra.stack
                    (fun _ -> ())
                    [
                        Input.Input
                            (fun x ->
                                x.autoFocus <- true
                                x.value <- Some usernameField
                                x.placeholder <- "Username"
                                x.onChange <- (fun (e: KeyboardEvent) -> promise { setUsernameField e.Value }))

                        Input.Input
                            (fun x ->
                                x.value <- Some passwordField
                                x.placeholder <- "Password"
                                x.inputFormat <- Some Input.InputFormat.Password
                                x.onChange <- (fun (e: KeyboardEvent) -> promise { setPasswordField e.Value })
                                x.onEnterPress <- Some signInClick)

                        Chakra.hStack
                            (fun x -> x.alignItems <- "stretch")
                            [
                                Button.Button
                                    {|
                                        Icon = None
                                        Hint = None
                                        Props =
                                            fun x ->
                                                x.flex <- "1"
                                                x.onClick <- signInClick
                                                x.color <- "gray"
                                        Children =
                                            [
                                                str "Login"
                                            ]
                                    |}

                                Popover.Popover
                                    {|
                                        Trigger =
                                            Button.Button
                                                {|
                                                    Icon = None
                                                    Hint = None
                                                    Props =
                                                        fun x ->
                                                            x.flex <- "1"
                                                            x.color <- "gray"
                                                    Children =
                                                        [
                                                            str "Register"
                                                        ]
                                                |}
                                        Body =
                                            fun _disclosure ->
                                                [
                                                    Chakra.stack
                                                        (fun x -> x.spacing <- "10px")
                                                        [
                                                            Chakra.box
                                                                (fun x ->
                                                                    x.paddingBottom <- "5px"
                                                                    x.fontSize <- "15px")
                                                                [
                                                                    str "Register"
                                                                ]

                                                            Input.Input
                                                                (fun x ->
                                                                    x.value <- Some password2Field
                                                                    x.placeholder <- "Confirm Password"
                                                                    x.inputFormat <- Some Input.InputFormat.Password

                                                                    x.onChange <-
                                                                        (fun (e: KeyboardEvent) ->
                                                                            promise { setPassword2Field e.Value })

                                                                    x.onEnterPress <- Some signUpClick)

                                                            Chakra.box
                                                                (fun _ -> ())
                                                                [
                                                                    Button.Button
                                                                        {|
                                                                            Hint = None
                                                                            Icon =
                                                                                Some (
                                                                                    Icons.fi.FiKey |> Icons.wrap,
                                                                                    Button.IconPosition.Left
                                                                                )
                                                                            Props = fun x -> x.onClick <- signUpClick
                                                                            Children =
                                                                                [
                                                                                    str "Confirm"
                                                                                ]
                                                                        |}
                                                                ]
                                                        ]
                                                ]
                                    |}
                            ]
                    ]
            ]
