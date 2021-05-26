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

        let confirmClick (disclosure: Chakra.Disclosure) _ =
            promise {
                if newPasswordField <> newPassword2Field then
                    toast (fun x -> x.description <- "Passwords don't match")
                else
                    match! changePassword passwordField newPasswordField with
                    | Ok () ->
                        toast
                            (fun x ->
                                x.title <- "Success"
                                x.status <- "success"
                                x.description <- "Password changed successfully")

                        setPasswordField ""
                        setNewPasswordField ""
                        setNewPassword2Field ""

                        disclosure.onClose ()
                    | Error error -> toast (fun x -> x.description <- error)
            }

        Popover.Popover
            {|
                Trigger =
                    Button.Button
                        {|
                            Hint = None
                            Icon = Some (Icons.fi.FiKey |> Icons.wrap, Button.IconPosition.Left)
                            Props = fun _ -> ()
                            Children =
                                [
                                    str "Change Password"
                                ]
                        |}
                Body =
                    fun (disclosure, _initialFocusRef) ->
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
                                        (fun x ->
                                            x.value <- Some passwordField
                                            x.placeholder <- "Password"
                                            x.inputFormat <- Some Input.InputFormat.Password

                                            x.onChange <-
                                                (fun (e: KeyboardEvent) -> promise { setPasswordField e.Value }))

                                    Input.Input
                                        (fun x ->
                                            x.value <- Some newPasswordField
                                            x.placeholder <- "New Password"
                                            x.inputFormat <- Some Input.InputFormat.Password

                                            x.onChange <-
                                                (fun (e: KeyboardEvent) -> promise { setNewPasswordField e.Value }))

                                    Input.Input
                                        (fun x ->
                                            x.value <- Some newPassword2Field
                                            x.placeholder <- "Confirm New Password"
                                            x.inputFormat <- Some Input.InputFormat.Password

                                            x.onChange <-
                                                (fun (e: KeyboardEvent) -> promise { setNewPassword2Field e.Value })

                                            x.onEnterPress <- Some (confirmClick disclosure))

                                    Chakra.box
                                        (fun _ -> ())
                                        [
                                            Button.Button
                                                {|
                                                    Hint = None
                                                    Icon = Some (Icons.fi.FiKey |> Icons.wrap, Button.IconPosition.Left)
                                                    Props = fun x -> x.onClick <- confirmClick disclosure
                                                    Children =
                                                        [
                                                            str "Confirm"
                                                        ]
                                                |}
                                        ]
                                ]
                        ]
            |}
