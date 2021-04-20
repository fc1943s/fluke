namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Components


module AddTaskButton =
    [<ReactComponent>]
    let AddTaskButton (input: {| Props: Chakra.IChakraProps |}) =
        let username = Recoil.useValue Recoil.Atoms.username
        let selectedPosition = Recoil.useValue Recoil.Atoms.selectedPosition
        let selectedDatabaseIds = Recoil.useValue Recoil.Atoms.selectedDatabaseIds
        let formTaskId, setFormTaskId = Recoil.useStateDefault Recoil.Atoms.User.formTaskId username

        let formTaskVisibleFlag, setFormTaskVisibleFlag =
            Recoil.useStateDefault Recoil.Atoms.User.formTaskVisibleFlag username

        let hintText =
            match selectedDatabaseIds, selectedPosition with
            | [| _ |], None -> None
            | _ -> Some (str "Select at least one live database")

        React.fragment [
            Button.Button
                {|
                    Hint = hintText
                    Icon = Some (Icons.biTask, Button.IconPosition.Left)
                    Props =
                        JS.newObj
                            (fun x ->
                                x <+ input.Props
                                x.cursor <- if hintText.IsNone then "pointer" else "default"
                                x.opacity <- if hintText.IsNone then 1. else 0.5

                                x.onClick <-
                                    (fun _ ->
                                        promise {
                                            if hintText.IsNone then
                                                setFormTaskId None
                                                setFormTaskVisibleFlag true
                                        })

                                x.children <-
                                    [
                                        str "Add Task"
                                    ])
                |}

            Modal.Modal
                {|
                    Props =
                        JS.newObj
                            (fun x ->
                                x.isOpen <- formTaskVisibleFlag

                                x.onClose <-
                                    (fun () ->
                                        promise {
                                            setFormTaskId None
                                            setFormTaskVisibleFlag false
                                        })

                                x.children <-
                                    [
                                        match username with
                                        | Some username ->
                                            TaskForm.TaskForm
                                                {|
                                                    Username = username
                                                    TaskId = formTaskId
                                                    OnSave = fun () -> promise { setFormTaskVisibleFlag false }
                                                |}
                                        | _ -> ()
                                    ])
                |}
        ]
