namespace Fluke.UI.Frontend.Components

open Fluke.Shared
open Fluke.Shared.Domain.UserInteraction
open FsCore.Model
open FsUi.Components
open FsUi.Hooks
open Browser.Types
open Feliz
open System
open FsStore
open Fluke.UI.Frontend.State.State
open FsUi.Bindings
open Fluke.UI.Frontend.Hooks
open Fable.React
open FsUi.Model


module LoginScreen =
    [<ReactComponent>]
    let LoginScreen () =
        let toast = Ui.useToast ()
        let usernameField, setUsernameField = React.useState ""
        let passwordField, setPasswordField = React.useState ""
        let password2Field, setPassword2Field = React.useState ""

        let signIn = Auth.useSignIn ()
        let signUp = Auth.useSignUp ()

        let signInClick =
            Store.useCallbackRef
                (fun _ _ _ ->
                    promise {
                        match! signIn (usernameField, passwordField) with
                        | Ok _ -> printfn "logged"
                        | Error error -> toast (fun x -> x.description <- error)
                    })

        let signUpClick =
            Store.useCallbackRef
                (fun getter setter _ ->
                    promise {
                        if passwordField <> password2Field then
                            toast (fun x -> x.description <- "Passwords don't match")
                            return false
                        elif Templates.templatesUser.Username
                             |> Username.ValueOrDefault = usernameField then
                            toast (fun x -> x.description <- "Invalid username")
                            return false
                        else
                            match! signUp (usernameField, passwordField) with
                            | Ok (_username, _keys) ->
                                do! Hydrate.hydrateTemplates getter setter

                                do! Hydrate.hydrateUiState getter setter UiState.Default

                                do!
                                    Hydrate.hydrateUserState
                                        getter
                                        setter
                                        { UserState.Default with
                                            Archive = Some false
                                            HideTemplates = Some false
                                            UserColor =
                                                String.Format ("#{0:X6}", Random().Next 0x1000000)
                                                |> Color
                                                |> Some
                                        }

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

        Ui.center
            (fun x -> x.flex <- "1")
            [
                Ui.stack
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
                                        x.placeholder <- "Email"
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

                        Dropdown.CustomConfirmDropdown
                            false
                            signUpClick
                            (fun visible setVisible ->
                                Ui.hStack
                                    (fun x -> x.alignItems <- "stretch")
                                    [
                                        Button.Button
                                            {|
                                                Icon = Some (Icons.fi.FiKey |> Icons.render, Button.IconPosition.Left)
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
                                                        x.onClick <- fun _ -> promise { setVisible (not visible) }

                                                        x.flex <- "1"
                                                Children =
                                                    [
                                                        str "Register"
                                                    ]
                                            |}
                                    ])
                            (fun onHide ->
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
                                ])
                    ]
            ]
