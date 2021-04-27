namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Recoil


module AddTaskButton =
    [<ReactComponent>]
    let AddTaskButton (input: {| Props: Chakra.IChakraProps |}) =
        let username = Recoil.useValue Atoms.username

        let formIdFlag, setFormIdFlag =
            Recoil.useStateDefault
                Atoms.User.formIdFlag
                (username
                 |> Option.map (fun username -> username, TextKey (nameof TaskForm)))

        let formVisibleFlag, setFormVisibleFlag =
            Recoil.useStateDefault
                Atoms.User.formVisibleFlag
                (username
                 |> Option.map (fun username -> username, TextKey (nameof TaskForm)))

        React.fragment [
            Button.Button
                {|
                    Hint = None
                    Icon = Some (Icons.bi.BiTask |> Icons.wrap, Button.IconPosition.Left)
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
                                        str "Add Task"
                                    ])
                |}

            Modal.Modal
                {|
                    Props =
                        JS.newObj
                            (fun x ->
                                x.isOpen <- formVisibleFlag

                                x.onClose <-
                                    (fun () ->
                                        promise {
                                            setFormIdFlag None
                                            setFormVisibleFlag false
                                        })

                                x.children <-
                                    [
                                        match username with
                                        | Some username ->
                                            TaskForm.TaskForm
                                                {|
                                                    Username = username
                                                    TaskId = formIdFlag |> Option.map TaskId
                                                    OnSave = fun () -> promise { setFormVisibleFlag false }
                                                |}
                                        | _ -> ()
                                    ])
                |}
        ]
