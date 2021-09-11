namespace Fluke.UI.Frontend.Components

open Browser.Types
open Feliz
open Fable.React
open FsStore
open FsStore.Hooks
open FsUi.Bindings
open FsUi.Components
open FsUi.Hooks


module ChangeUserPasswordButton =
    [<ReactComponent>]
    let rec ChangeUserPasswordButton () =
        let toast = Ui.useToast ()
        let passwordField, setPasswordField = React.useState ""
        let newPasswordField, setNewPasswordField = React.useState ""
        let newPassword2Field, setNewPassword2Field = React.useState ""

        let confirmClick =
            Store.useCallbackRef
                (fun getter setter _ ->
                    promise {
                        if newPasswordField <> newPassword2Field then
                            toast (fun x -> x.description <- "Passwords don't match")
                            return false
                        else
                            match! Auth.changePassword getter setter (passwordField, newPasswordField) with
                            | Ok _alias ->
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

        Dropdown.ConfirmDropdown
            "Change Password"
            confirmClick
            (fun onHide ->
                [
                    Input.Input
                        {|
                            CustomProps =
                                fun x ->
                                    x.fixedValue <- Some passwordField
                                    x.inputFormat <- Some Input.InputFormat.Password
                            Props =
                                fun x ->
                                    x.autoFocus <- true
                                    x.placeholder <- "Password"
                                    x.onChange <- (fun (e: KeyboardEvent) -> promise { setPasswordField e.Value })
                        |}

                    Input.Input
                        {|
                            CustomProps =
                                fun x ->
                                    x.fixedValue <- Some newPasswordField
                                    x.inputFormat <- Some Input.InputFormat.Password
                            Props =
                                fun x ->
                                    x.placeholder <- "New Password"
                                    x.onChange <- (fun (e: KeyboardEvent) -> promise { setNewPasswordField e.Value })
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

                                    x.onChange <- (fun (e: KeyboardEvent) -> promise { setNewPassword2Field e.Value })
                        |}
                ])
