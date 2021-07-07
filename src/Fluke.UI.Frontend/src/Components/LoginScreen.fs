namespace Fluke.UI.Frontend.Components

open Browser.Types
open Feliz
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks
open Fable.React


module LoginScreen =
    [<ReactComponent>]
    let LoginScreen () =
        let toast = Chakra.useToast ()
        let usernameField, setUsernameField = React.useState ""
        let passwordField, setPasswordField = React.useState ""
        let password2Field, setPassword2Field = React.useState ""

        let signIn = Auth.useSignIn ()
        let signUp = Auth.useSignUp ()

        let signInClick =
            Store.useCallback (
                (fun _ _ _ ->
                    promise {
                        match! signIn (usernameField, passwordField) with
                        | Ok _ -> printfn "logged"
                        | Error error -> toast (fun x -> x.description <- error)
                    }),
                [|
                    box signIn
                    box toast
                    box usernameField
                    box passwordField
                |]
            )

        let signUpClick =
            Store.useCallback (
                (fun _ _ _ ->
                    promise {
                        if passwordField <> password2Field then
                            toast (fun x -> x.description <- "Passwords don't match")
                            return false
                        else
                            match! signUp (usernameField, passwordField) with
                            | Ok _ ->
                                toast
                                    (fun x ->
                                        x.title <- "Success"
                                        x.status <- "success"
                                        x.description <- "User registered successfully")

                                return true
                            | Error error ->
                                toast (fun x -> x.description <- error)
                                return false
                    }),
                [|
                    box signUp
                    box toast
                    box usernameField
                    box passwordField
                    box password2Field
                |]
            )

        Chakra.center
            (fun x -> x.flex <- "1")
            [
                Chakra.stack
                    (fun _ -> ())
                    [
                        Input.Input
                            {|
                                CustomProps =
                                    fun x ->
                                        x.fixedValue <- Some usernameField
                                        x.onEnterPress <- Some signInClick
                                Props =
                                    fun x ->
                                        x.autoFocus <- true
                                        x.placeholder <- "Username"
                                        x.onChange <- (fun (e: KeyboardEvent) -> promise { setUsernameField e.Value })
                            |}

                        Input.Input
                            {|
                                CustomProps =
                                    fun x ->
                                        x.fixedValue <- Some passwordField
                                        x.onEnterPress <- Some signInClick
                                        x.inputFormat <- Some Input.InputFormat.Password
                                Props =
                                    fun x ->
                                        x.placeholder <- "Password"
                                        x.onChange <- (fun (e: KeyboardEvent) -> promise { setPasswordField e.Value })
                            |}


                        Dropdown.Dropdown
                            {|
                                Tooltip = ""
                                Left = false
                                Trigger =
                                    fun visible setVisible ->
                                        Chakra.hStack
                                            (fun x -> x.alignItems <- "stretch")
                                            [
                                                Button.Button
                                                    {|
                                                        Icon =
                                                            Some (
                                                                Icons.fi.FiKey |> Icons.render,
                                                                Button.IconPosition.Left
                                                            )
                                                        Hint = None
                                                        Props =
                                                            fun x ->
                                                                x.flex <- "1"
                                                                x.onClick <- signInClick
                                                        Children =
                                                            [
                                                                str "Login"
                                                            ]
                                                    |}
                                                Button.Button
                                                    {|
                                                        Hint = None
                                                        Icon =
                                                            Some (
                                                                (if visible then
                                                                     Icons.fi.FiChevronUp
                                                                 else
                                                                     Icons.fi.FiChevronDown)
                                                                |> Icons.render,
                                                                Button.IconPosition.Right
                                                            )
                                                        Props =
                                                            fun x ->
                                                                x.onClick <-
                                                                    fun _ -> promise { setVisible (not visible) }

                                                                x.flex <- "1"
                                                        Children =
                                                            [
                                                                str "Register"
                                                            ]
                                                    |}
                                            ]
                                Body =
                                    fun onHide ->
                                        [
                                            Chakra.stack
                                                (fun x -> x.spacing <- "10px")
                                                [
                                                    Input.Input
                                                        {|
                                                            CustomProps =
                                                                fun x ->
                                                                    x.fixedValue <- Some password2Field
                                                                    x.inputFormat <- Some Input.InputFormat.Password

                                                                    x.onEnterPress <-
                                                                        Some
                                                                            (fun _ ->
                                                                                promise {
                                                                                    let! result = signUpClick ()
                                                                                    if result then onHide ()
                                                                                })
                                                            Props =
                                                                fun x ->
                                                                    x.autoFocus <- true
                                                                    x.placeholder <- "Confirm Password"

                                                                    x.onChange <-
                                                                        (fun (e: KeyboardEvent) ->
                                                                            promise { setPassword2Field e.Value })
                                                        |}

                                                    Chakra.box
                                                        (fun _ -> ())
                                                        [
                                                            Button.Button
                                                                {|
                                                                    Hint = None
                                                                    Icon =
                                                                        Some (
                                                                            Icons.fi.FiKey |> Icons.render,
                                                                            Button.IconPosition.Left
                                                                        )
                                                                    Props =
                                                                        fun x ->
                                                                            x.onClick <-
                                                                                fun _ ->
                                                                                    promise {
                                                                                        let! result = signUpClick ()
                                                                                        if result then onHide ()
                                                                                    }
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
