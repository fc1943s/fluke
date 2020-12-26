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
        let taskIdForm, setTaskIdForm = Recoil.useState (Recoil.Atoms.taskIdForm)

        React.fragment [
            Button.Button
                {|
                    Icon = Icons.faPlus
                    RightIcon = false
                    props =
                        {|
                            marginLeft = props.marginLeft
                            onClick = fun () -> setTaskIdForm (Some (Recoil.Atoms.Task.newTaskId ()))
                        |}
                    children =
                        [
                            str "Add Task"
                        ]
                |}

            Modal.Modal
                {|
                    IsOpen = taskIdForm.IsSome
                    OnClose = fun () -> setTaskIdForm None
                    children =
                        [
                            TaskForm.TaskForm ()
                        ]
                |}
        ]
