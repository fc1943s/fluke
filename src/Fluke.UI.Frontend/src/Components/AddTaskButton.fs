namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend

module AddTaskButton =
    let render =
        React.memo (fun (input: {| props: {| marginLeft: string |} |}) ->
            let taskIdForm, setTaskIdForm = Recoil.useState (Recoil.Atoms.taskIdForm)

            React.fragment [
                Button.render
                    {|
                        Icon = Icons.faPlus
                        RightIcon = false
                        props =
                            {|
                                marginLeft = input.props.marginLeft
                                onClick = fun () -> setTaskIdForm (Some (Recoil.Atoms.Task.newTaskId ()))
                            |}
                        children =
                            [
                                str "Add Task"
                            ]
                    |}

                Modal.render
                    {|
                        IsOpen = taskIdForm.IsSome
                        OnClose = fun () -> setTaskIdForm None
                        children =
                            [
                                TaskForm.render ()
                            ]
                    |}
            ])
