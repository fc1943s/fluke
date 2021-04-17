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
        let formTaskVisibleFlag, setFormTaskVisibleFlag = Recoil.useState Recoil.Atoms.formTaskVisibleFlag

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
                                            setFormTaskId None
                                            setFormTaskVisibleFlag true
                                        })
                        |}
                    children =
                        [
                            str "Add Task"
                        ]
                |}

            Modal.Modal
                {|
                    IsOpen = formTaskVisibleFlag
                    OnClose =
                        fun () ->
                            setFormTaskId None
                            setFormTaskVisibleFlag false
                    children =
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
                        ]
                |}
        ]
