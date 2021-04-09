namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend


module AddTaskButton =
    [<ReactComponent>]
    let AddTaskButton (props: {| marginLeft: string |}) =
        let username = Recoil.useValue Recoil.Atoms.username
        let formTaskId, setFormTaskId = Recoil.useState Recoil.Atoms.formTaskId

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
                            onClick = Some (fun () -> promise { setFormTaskId (Some (Recoil.Atoms.Task.newTaskId ())) })
                        |}
                    children =
                        [
                            str "Add Task"
                        ]
                |}

            Modal.Modal
                {|
                    IsOpen = formTaskId.IsSome
                    OnClose = fun () -> setFormTaskId None
                    children =
                        [
                            match formTaskId, username with
                            | Some taskId, Some username ->
                                TaskForm.TaskForm
                                    {|
                                        Username = username
                                        TaskId = taskId
                                        OnSave = async { setFormTaskId None }
                                    |}
                            | _ -> ()
                        ]
                |}
        ]
