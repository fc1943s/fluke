namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Recoil

module ModalForm =

    [<ReactComponent>]
    let ModalForm
        (input: {| Username: Username
                   TextKey: TextKey
                   Trigger: (Chakra.IChakraProps -> unit) -> ReactElement
                   Content: System.Guid option * (unit -> unit) -> ReactElement
                   Props: Chakra.IChakraProps |})
        =
        let formIdFlag, setFormIdFlag = Recoil.useState (Atoms.User.formIdFlag (input.Username, input.TextKey))

        let formVisibleFlag, setFormVisibleFlag =
            Recoil.useState (Atoms.User.formVisibleFlag (input.Username, input.TextKey))

        React.fragment [
            input.Trigger
                (fun x ->
                    x.onClick <-
                        (fun _ ->
                            promise {
                                setFormIdFlag None
                                setFormVisibleFlag true
                            }))

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
                                        input.Content (formIdFlag, (fun () -> setFormVisibleFlag false))
                                    ])
                |}
        ]
