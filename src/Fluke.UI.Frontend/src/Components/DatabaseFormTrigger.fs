namespace Fluke.UI.Frontend.Components

open Feliz
open Feliz.Recoil
open Feliz.UseListener
open Fluke.Shared.Domain.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State


module DatabaseFormTrigger =

    [<ReactComponent>]
    let DatabaseFormTrigger
        (input: {| Username: Username
                   DatabaseId: DatabaseId option
                   Trigger: (unit -> unit) -> (unit -> CallbackMethods) -> ReactElement |})
        =
        let hydrateDatabase = Hydrate.useHydrateDatabase ()

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
                                        fun (formIdFlag, onHide, setter) ->
                                            let databaseId =
                                                formIdFlag
                                                |> Option.map DatabaseId
                                                |> Option.defaultValue Database.Default.Id

                                            DatabaseForm.DatabaseForm
                                                {|
                                                    Username = input.Username
                                                    DatabaseId = databaseId
                                                    OnSave =
                                                        fun database ->
                                                            promise {
                                                                hydrateDatabase
                                                                    input.Username
                                                                    Recoil.AtomScope.ReadOnly
                                                                    database

                                                                setter()
                                                                    .set (
                                                                        Atoms.User.databaseIdSet input.Username,
                                                                        Set.add database.Id
                                                                    )

                                                                onHide ()
                                                            }
                                                |}
                                    TextKey = TextKey (nameof DatabaseForm)
                                |}
                        ]
                TextKey = TextKey (nameof DatabaseForm)
                TextKeyValue = input.DatabaseId |> Option.map DatabaseId.Value
            |}
