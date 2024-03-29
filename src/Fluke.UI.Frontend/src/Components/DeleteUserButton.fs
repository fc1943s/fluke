namespace Fluke.UI.Frontend.Components

open Browser.Types
open Feliz
open Fable.React
open FsStore.Hooks
open FsUi.Bindings
open FsUi.Components
open FsUi.Hooks


module DeleteUserButton =
    [<ReactComponent>]
    let rec DeleteUserButton () =
        let toast = Ui.useToast ()
        let passwordField, setPasswordField = React.useState ""

        let confirmClick =
            Store.useCallbackRef
                (fun getter setter _ ->
                    promise {
                        match! Auth.deleteUser getter setter passwordField with
                        | Ok _alias ->
                            toast
                                (fun x ->
                                    x.title <- "Success"
                                    x.status <- "success"
                                    x.description <- "User deleted successfully")

                            setPasswordField ""
                            return true
                        | Error error ->
                            toast (fun x -> x.description <- error)
                            return false
                    })

        Dropdown.ConfirmDropdown
            "Delete User"
            confirmClick
            (fun onHide ->
                [
                    Input.Input
                        {|
                            CustomProps =
                                fun x ->
                                    x.fixedValue <- Some passwordField
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
                                    x.placeholder <- "Password"
                                    x.onChange <- (fun (e: KeyboardEvent) -> promise { setPasswordField e.Value })
                        |}
                ])
