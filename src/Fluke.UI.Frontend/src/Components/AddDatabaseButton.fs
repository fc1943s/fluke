namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend


module AddDatabaseButton =
    [<ReactComponent>]
    let AddDatabaseButton (input: {| Props: Chakra.IChakraProps |}) =
        let username = Recoil.useValue Recoil.Atoms.username

        let formIdFlag, setFormIdFlag =
            Recoil.useStateDefault
                Recoil.Atoms.User.formIdFlag
                (username
                 |> Option.map (fun username -> username, nameof DatabaseForm))

        let formVisibleFlag, setFormVisibleFlag =
            Recoil.useStateDefault
                Recoil.Atoms.User.formVisibleFlag
                (username
                 |> Option.map (fun username -> username, nameof DatabaseForm))

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
                                            setFormIdFlag None
                                            setFormVisibleFlag true
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
                                x.isOpen <- formVisibleFlag

                                x.onClose <-
                                    fun () ->
                                        promise {
                                            setFormIdFlag None
                                            setFormVisibleFlag false
                                        }

                                x.children <-
                                    [
                                        match username with
                                        | Some username ->
                                            DatabaseForm.DatabaseForm
                                                {|
                                                    Username = username
                                                    DatabaseId = formIdFlag |> Option.map DatabaseId
                                                    OnSave = fun () -> promise { setFormVisibleFlag false }
                                                |}
                                        | _ -> ()
                                    ])
                |}
        ]
