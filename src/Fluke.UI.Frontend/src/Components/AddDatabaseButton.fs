namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend

module AddDatabaseButton =
    let render =
        React.memo (fun (input: {| props: {| marginLeft: string |} |}) ->
            let username = Recoil.useValue Recoil.Atoms.username
            let formDatabaseId, setformDatabaseId = Recoil.useState (Recoil.Atoms.formDatabaseId)

            React.fragment [
                Button.render
                    {|
                        Icon = Icons.faPlus
                        RightIcon = false
                        props =
                            {|
                                marginLeft = input.props.marginLeft
                                onClick = fun () -> setformDatabaseId (Some (Recoil.Atoms.Database.newDatabaseId ()))
                            |}
                        children =
                            [
                                str "Add Database"
                            ]
                    |}

                Modal.render
                    {|
                        IsOpen = formDatabaseId.IsSome
                        OnClose = fun () -> setformDatabaseId None
                        children =
                            [
                                match formDatabaseId, username with
                                | Some databaseId, Some username ->
                                    DatabaseForm.render {| Username = username; DatabaseId = databaseId |}
                                | _ -> ()
                            ]
                    |}
            ])
