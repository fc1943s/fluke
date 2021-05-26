namespace Fluke.UI.Frontend.Components

open Browser.Types
open Feliz
open Fable.React
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks


module DeleteUserButton =
    [<ReactComponent>]
    let rec DeleteUserButton () =
        let deleteUser = Auth.useDeleteUser ()
        let toast = Chakra.useToast ()
        let passwordField, setPasswordField = React.useState ""

        let confirmClick (disclosure: Chakra.Disclosure) _ =
            promise {
                match! deleteUser passwordField with
                | Ok () ->
                    toast
                        (fun x ->
                            x.title <- "Success"
                            x.status <- "success"
                            x.description <- "User deleted successfully")

                    setPasswordField ""

                    disclosure.onClose ()
                | Error error -> toast (fun x -> x.description <- error)
            }

        Popover.Popover
            {|
                Trigger =
                    Button.Button
                        {|
                            Hint = None
                            Icon = Some (Icons.bs.BsTrash |> Icons.wrap, Button.IconPosition.Left)
                            Props = fun _ -> ()
                            Children =
                                [
                                    str "Delete User"
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
                                            str "Delete User"
                                        ]

                                    Input.Input
                                        (fun x ->
                                            x.value <- Some passwordField
                                            x.placeholder <- "Password"
                                            x.inputFormat <- Some Input.InputFormat.Password

                                            x.onChange <-
                                                (fun (e: KeyboardEvent) -> promise { setPasswordField e.Value })

                                            x.onEnterPress <- Some (confirmClick disclosure))

                                    Chakra.box
                                        (fun _ -> ())
                                        [
                                            Button.Button
                                                {|
                                                    Hint = None
                                                    Icon =
                                                        Some (Icons.bs.BsTrash |> Icons.wrap, Button.IconPosition.Left)
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
