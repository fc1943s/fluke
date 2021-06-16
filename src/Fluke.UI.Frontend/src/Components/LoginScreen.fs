namespace Fluke.UI.Frontend.Components

open Browser.Types
open Feliz
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks
open Fable.React
open Fluke.UI.Frontend.State


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
            Store.useCallbackRef
                (fun _ _ ->
                    promise {
                        match! signIn (usernameField, passwordField) with
                        | Ok _ -> printfn "logged"
                        | Error error ->
                            //
                            toast (fun x -> x.description <- error)
                    })

        let signUpClick =
            Store.useCallbackRef
                (fun _ _ ->
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
                    })

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
                                x.onChange <- (fun (e: KeyboardEvent) -> promise { setUsernameField e.Value })
                                x.onEnterPress <- Some signInClick)

                        Input.Input
                            (fun x ->
                                x.value <- Some passwordField
                                x.placeholder <- "Password"
                                x.inputFormat <- Some Input.InputFormat.Password
                                x.onChange <- (fun (e: KeyboardEvent) -> promise { setPasswordField e.Value })
                                x.onEnterPress <- Some signInClick)


                        Menu.Drawer
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
                                                                Icons.fi.FiKey |> Icons.wrap,
                                                                Button.IconPosition.Left
                                                            )
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
                                                Button.Button
                                                    {|
                                                        Hint = None
                                                        Icon =
                                                            Some (
                                                                (if visible then
                                                                     Icons.fi.FiChevronUp
                                                                 else
                                                                     Icons.fi.FiChevronDown)
                                                                |> Icons.wrap,
                                                                Button.IconPosition.Right
                                                            )
                                                        Props =
                                                            fun x ->
                                                                x.onClick <-
                                                                    fun _ -> promise { setVisible (not visible) }

                                                                x.flex <- "1"
                                                                x.color <- "gray"
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
                                                    Chakra.box
                                                        (fun x ->
                                                            x.paddingBottom <- "5px"
                                                            x.fontSize <- "15px")
                                                        [
                                                            str "Register"
                                                        ]

                                                    Input.Input
                                                        (fun x ->
                                                            x.autoFocus <- true
                                                            x.value <- Some password2Field
                                                            x.placeholder <- "Confirm Password"

                                                            x.inputFormat <- Some Input.InputFormat.Password

                                                            x.onChange <-
                                                                (fun (e: KeyboardEvent) ->
                                                                    promise { setPassword2Field e.Value })

                                                            x.onEnterPress <-
                                                                Some
                                                                    (fun _ ->
                                                                        promise {
                                                                            let! result = signUpClick ()
                                                                            if result then onHide ()
                                                                        }))

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
