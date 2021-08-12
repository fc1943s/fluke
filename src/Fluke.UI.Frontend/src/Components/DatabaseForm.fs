namespace Fluke.UI.Frontend.Components

open FsCore
open Fable.React
open Feliz
open System
open Fluke.Shared.Domain
open Fluke.UI.Frontend
open Fluke.Shared
open FsStore
open FsStore.Bindings
open FsStore.Model
open FsUi.Bindings
open Fable.Core
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State.State
open Fluke.UI.Frontend.State
open FsUi.Components
open FsJs


module DatabaseForm =
    open State

    [<ReactComponent>]
    let rec DatabaseForm (databaseId: DatabaseId) (onSave: Database -> JS.Promise<unit>) =
        let toast = Ui.useToast ()
        let logLevel = Store.useValue Atoms.logLevel

        let onSave =
            Store.useCallbackRef
                (fun getter setter _ ->
                    promise {
                        let databaseName = Store.getTempValue getter (Atoms.Database.name databaseId)
                        let username = Store.value getter Atoms.username

                        match databaseName with
                        | DatabaseName String.InvalidString -> toast (fun x -> x.description <- "Invalid name")
                        | _ ->
                            match username with
                            | Some username ->
                                let! database =
                                    if databaseId = Database.Default.Id then
                                        {
                                            Id = DatabaseId.NewId ()
                                            Name = databaseName
                                            Owner = username
                                            SharedWith = DatabaseAccess.Private []
                                            Position = None
                                        }
                                        |> Promise.lift
                                    else
                                        promise {
                                            let database = Store.value getter (Selectors.Database.database databaseId)

                                            return { database with Name = databaseName }
                                        }

                                //                                let eventId = Atoms.Events.newEventId ()
                                //                                let event = Atoms.Events.Event.AddDatabase (eventId, databaseName, dayStart)
                                //                                setter.set (Atoms.Events.events eventId, event)
                                //                                printfn $"event {event}"

                                Store.resetTempValue setter (Atoms.Database.name databaseId)

                                do! onSave database
                            | None -> ()
                    })

        Accordion.Accordion
            {|
                Props = fun x -> x.flex <- "1"
                Atom = Atoms.User.accordionHiddenFlag AccordionType.DatabaseForm
                Items =
                    [
                        str $"""{if databaseId = Database.Default.Id then "Add" else "Edit"} Database""",
                        (Ui.stack
                            (fun x -> x.spacing <- "15px")
                            [
                                if logLevel <= LogLevel.Debug then Ui.str $"{databaseId}" else nothing

                                Input.Input
                                    {|
                                        CustomProps =
                                            fun x ->
                                                x.atom <-
                                                    Some (
                                                        InputAtom (AtomReference.Atom (Atoms.Database.name databaseId))
                                                    )

                                                x.inputScope <- Some (InputScope.Temp Gun.defaultSerializer)
                                                x.onFormat <- Some (fun (DatabaseName name) -> name)
                                                x.onValidate <- Some (fst >> DatabaseName >> Some)
                                                x.onEnterPress <- Some onSave
                                        Props =
                                            fun x ->
                                                x.autoFocus <- true
                                                x.label <- str "Name"

                                                x.placeholder <-
                                                    $"""new-database-%s{DateTime.Now |> DateTime.format "yyyy-MM-dd"}"""
                                    |}

                                Ui.stack
                                    (fun x ->
                                        x.direction <- "row"
                                        x.alignItems <- "center")
                                    [
                                        Ui.str "Access:"

                                        DatabaseAccessIndicator.DatabaseAccessIndicator ()
                                    ]

                                Button.Button
                                    {|
                                        Hint = None
                                        Icon = Some (Icons.fi.FiSave |> Icons.render, Button.IconPosition.Left)
                                        Props = fun x -> x.onClick <- onSave
                                        Children =
                                            [
                                                str "Save"
                                            ]
                                    |}
                            ])
                    ]
            |}


    [<ReactComponent>]
    let DatabaseFormWrapper () =
        let hydrateDatabase = Store.useCallbackRef Hydrate.hydrateDatabase
        let setRightDock = Store.useSetState Atoms.User.rightDock

        let databaseUIFlag = Store.useValue (Atoms.User.uiFlag UIFlagType.Database)

        let databaseId =
            match databaseUIFlag with
            | UIFlag.Database databaseId -> databaseId
            | _ -> Database.Default.Id

        DatabaseForm
            databaseId
            (fun database ->
                promise {
                    do! hydrateDatabase (AtomScope.Current, database)
                    setRightDock None
                })
