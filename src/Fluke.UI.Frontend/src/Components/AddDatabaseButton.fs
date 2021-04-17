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
        let formDatabaseVisibleFlag, setFormDatabaseVisibleFlag = Recoil.useState Recoil.Atoms.formDatabaseVisibleFlag

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
                                        promise {
                                            setFormDatabaseId None
                                            setFormDatabaseVisibleFlag true
                                        })
                        |}
                    children =
                        [
                            str "Add Database"
                        ]
                |}

            Modal.Modal
                {|
                    IsOpen = formDatabaseVisibleFlag
                    OnClose =
                        fun () ->
                            setFormDatabaseId None
                            setFormDatabaseVisibleFlag false
                    children =
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
                        ]
                |}
        ]
