namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State


module AreaFormTrigger =
    [<ReactComponent>]
    let AreaFormTrigger
        (input: {| Username: Username
                   Area: Area
                   OnSelect: Area -> unit
                   Trigger: (unit -> unit) -> (unit -> CallbackMethods) -> ReactElement |})
        =
        ModalFormTrigger.ModalFormTrigger
            {|
                Username = input.Username
                Trigger =
                    fun trigger setter ->
                        React.fragment [
                            input.Trigger trigger setter

                            ModalForm.ModalForm
                                {|
                                    Username = input.Username
                                    Content =
                                        fun (_, onHide, _) ->
                                            AreaForm.AreaForm
                                                {|
                                                    Username = input.Username
                                                    Area = input.Area
                                                    OnSave =
                                                        fun area ->
                                                            promise {
                                                                input.OnSelect area
                                                                onHide ()
                                                            }
                                                |}
                                    TextKey = TextKey (nameof AreaForm)
                                |}
                        ]
                TextKey = TextKey (nameof AreaForm)
                TextKeyValue = None
            |}
