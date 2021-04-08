namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend

module AddDatabaseButton =

    [<ReactComponent>]
    let AddDatabaseButton (props: {| marginLeft: string |}) =
        let username = Recoil.useValue Recoil.Atoms.username
        let formDatabaseId, setFormDatabaseId = Recoil.useState Recoil.Atoms.formDatabaseId

        React.fragment [
            Button.Button
                {|
                    Icon = Some (box Icons.faPlus)
                    RightIcon = Some false
                    props =
                        {|
                            autoFocus = None
                            color = None
                            flex = None
                            marginLeft = Some props.marginLeft
                            onClick =
                                Some
                                    (fun () ->
                                        promise { setFormDatabaseId (Some (Recoil.Atoms.Database.newDatabaseId ())) })
                        |}
                    children =
                        [
                            str "Add Database"
                        ]
                |}

            Modal.Modal
                {|
                    IsOpen = formDatabaseId.IsSome
                    OnClose = fun () -> setFormDatabaseId None
                    children =
                        [
                            match formDatabaseId, username with
                            | Some databaseId, Some username ->
                                EditDatabase.EditDatabase
                                    {|
                                        Username = username
                                        DatabaseId = databaseId
                                        OnSave = async { setFormDatabaseId None }
                                    |}
                            | _ -> ()
                        ]
                |}
        ]
