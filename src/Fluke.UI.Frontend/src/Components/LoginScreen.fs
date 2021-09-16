namespace Fluke.UI.Frontend.Components

open Fluke.Shared
open Fluke.Shared.Domain.UserInteraction
open FsCore.BaseModel
open FsStore.Bindings
open FsStore.State
open FsUi.Components
open FsUi.Hooks
open Browser.Types
open Feliz
open FsStore
open FsStore.Hooks
open Fluke.UI.Frontend.State
open FsUi.Bindings
open Fable.React


module LoginScreen =
    [<ReactComponent>]
    let LoginScreen () =
        let toast = Ui.useToast ()
        let usernameField, setUsernameField = React.useState ""
        let passwordField, setPasswordField = React.useState ""
        let password2Field, setPassword2Field = React.useState ""

        let setHydratePending = Store.useSetState Atoms.Session.hydrateTemplatesPending

        let signInClick =
            Store.useCallbackRef
                (fun getter setter _ ->
                    promise {
                        match! Auth.signIn getter setter (usernameField, passwordField) with
                        | Ok _ ->
                            let gun = Atom.get getter Selectors.Gun.gun
                            do! Gun.putPublicHash gun (Gun.Alias usernameField)
                            printfn "logged"
                        | Error error -> toast (fun x -> x.description <- error)
                    })

        let signUpClick =
            Store.useCallbackRef
                (fun getter setter _ ->
                    promise {
                        if passwordField <> password2Field then
                            toast (fun x -> x.description <- "Passwords don't match")
                        elif Templates.templatesUser.Username |> Username.Value = usernameField then
                            toast (fun x -> x.description <- "Invalid username")
                        else
                            match! Auth.signUp getter setter (usernameField, passwordField) with
                            | Ok (_alias, _keys) -> setHydratePending true
                            | Error error -> toast (fun x -> x.description <- error)
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
                            (fun () ->
                                promise {
                                    do! signUpClick ()
                                    return true
                                })
                            (fun visible setVisible ->
                                Ui.hStack
                                    (fun x -> x.alignItems <- "stretch")
                                    [
                                        Button.Button
                                            {|
                                                Icon = Some (Icons.fi.FiKey |> Icons.render, Button.IconPosition.Left)
                                                Tooltip = None
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
                                                Tooltip = None
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
                            (fun _onHide ->
                                [
                                    Input.Input
                                        {|
                                            CustomProps =
                                                fun x ->
                                                    x.fixedValue <- Some password2Field
                                                    x.inputFormat <- Some Input.InputFormat.Password

                                                    x.onEnterPress <- Some (fun _ -> signUpClick ())
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
