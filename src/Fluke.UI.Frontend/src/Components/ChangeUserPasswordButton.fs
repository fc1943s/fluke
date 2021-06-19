namespace Fluke.UI.Frontend.Components

open Browser.Types
open Feliz
open Fable.React
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks


module ChangeUserPasswordButton =
    [<ReactComponent>]
    let rec ChangeUserPasswordButton () =
        let changePassword = Auth.useChangePassword ()
        let toast = Chakra.useToast ()
        let passwordField, setPasswordField = React.useState ""
        let newPasswordField, setNewPasswordField = React.useState ""
        let newPassword2Field, setNewPassword2Field = React.useState ""

        let confirmClick =
            Store.useCallbackRef
                (fun _ _ ->
                    promise {
                        if newPasswordField <> newPassword2Field then
                            toast (fun x -> x.description <- "Passwords don't match")
                            return false
                        else
                            match! changePassword (passwordField, newPasswordField) with
                            | Ok () ->
                                toast
                                    (fun x ->
                                        x.title <- "Success"
                                        x.status <- "success"
                                        x.description <- "Password changed successfully")

                                setPasswordField ""
                                setNewPasswordField ""
                                setNewPassword2Field ""

                                return true
                            | Error error ->
                                toast (fun x -> x.description <- error)
                                return false
                    })

        Dropdown.Dropdown
            {|
                Tooltip = ""
                Left = true
                Trigger =
                    fun visible setVisible ->
                        Button.Button
                            {|
                                Hint = None
                                Icon =
                                    Some (
                                        (if visible then Icons.fi.FiChevronUp else Icons.fi.FiChevronDown)
                                        |> Icons.wrap,
                                        Button.IconPosition.Right
                                    )
                                Props = fun x -> x.onClick <- fun _ -> promise { setVisible (not visible) }
                                Children =
                                    [
                                        str "Change Password"
                                    ]
                            |}
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
                                            str "Change Password"
                                        ]

                                    Input.Input
                                        {|
                                            CustomProps = fun x ->
                                                x.fixedValue <- Some passwordField
                                                x.inputFormat <- Some Input.InputFormat.Password
                                            Props =
                                                fun x ->
                                                    x.autoFocus <- true
                                                    x.placeholder <- "Password"

                                                    x.onChange <-
                                                        (fun (e: KeyboardEvent) -> promise { setPasswordField e.Value })
                                        |}

                                    Input.Input
                                        {|
                                            CustomProps = fun x ->
                                                x.fixedValue <- Some newPasswordField
                                                x.inputFormat <- Some Input.InputFormat.Password
                                            Props =
                                                fun x ->
                                                    x.placeholder <- "New Password"

                                                    x.onChange <-
                                                        (fun (e: KeyboardEvent) ->
                                                            promise { setNewPasswordField e.Value })
                                        |}

                                    Input.Input
                                        {|
                                            CustomProps =
                                                fun x ->
                                                    x.fixedValue <- Some newPassword2Field
                                                    x.inputFormat <- Some Input.InputFormat.Password
                                                    x.onEnterPress <-
                                                        Some
                                                            (fun _ ->
                                                                promise {
                                                                    let! result = confirmClick ()
                                                                    if result then onHide ()
                                                                })
                                            Props =
                                                fun x ->
                                                    x.placeholder <- "Confirm New Password"
                                                    x.onChange <-
                                                        (fun (e: KeyboardEvent) ->
                                                            promise { setNewPassword2Field e.Value })
                                        |}

                                    Chakra.box
                                        (fun _ -> ())
                                        [
                                            Button.Button
                                                {|
                                                    Hint = None
                                                    Icon = Some (Icons.fi.FiKey |> Icons.wrap, Button.IconPosition.Left)
                                                    Props =
                                                        fun x ->
                                                            x.onClick <-
                                                                fun _ ->
                                                                    promise {
                                                                        let! result = confirmClick ()
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
