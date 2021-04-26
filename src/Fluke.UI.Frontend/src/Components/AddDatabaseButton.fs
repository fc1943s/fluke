namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend


module AddDatabaseButton =
    [<ReactComponent>]
    let AddDatabaseButton (input: {| Props: Chakra.IChakraProps |}) =
        let username = Recoil.useValue Recoil.Atoms.username
        let formDatabaseId, setFormDatabaseId = Recoil.useStateDefault Recoil.Atoms.User.formDatabaseId username

        let formDatabaseVisibleFlag, setFormDatabaseVisibleFlag =
            Recoil.useStateDefault Recoil.Atoms.User.formDatabaseVisibleFlag username

        React.fragment [
            Button.Button
                {|
                    Hint = None
                    Icon = Some (Icons.fi.FiDatabase |> Icons.wrap, Button.IconPosition.Left)
                    Props =
                        JS.newObj
                            (fun x ->
                                x <+ input.Props

                                x.onClick <-
                                    (fun _ ->
                                        promise {
                                            setFormDatabaseId None
                                            setFormDatabaseVisibleFlag true
                                        })

                                x.children <-
                                    [
                                        str "Add Database"
                                    ])
                |}


            Modal.Modal
                {|
                    Props =
                        JS.newObj
                            (fun x ->
                                x.isOpen <- formDatabaseVisibleFlag

                                x.onClose <-
                                    fun () ->
                                        promise {
                                            setFormDatabaseId None
                                            setFormDatabaseVisibleFlag false
                                        }

                                x.children <-
                                    [
                                        match username with
                                        | Some username ->
                                            DatabaseForm.DatabaseForm
                                                {|
                                                    Username = username
                                                    DatabaseId = formDatabaseId
                                                    OnSave = fun () -> promise { setFormDatabaseVisibleFlag false }
                                                |}
                                        | _ -> ()
                                    ])
                |}
        ]
