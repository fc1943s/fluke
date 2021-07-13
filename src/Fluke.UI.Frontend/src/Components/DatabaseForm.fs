namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open System
open Fluke.Shared.Domain
open Fluke.UI.Frontend
open Fluke.Shared
open Fluke.UI.Frontend.Bindings
open Fable.DateFunctions
open Fable.Core
open Fluke.UI.Frontend.Hooks


module DatabaseForm =
    open State

    [<ReactComponent>]
    let rec DatabaseForm (databaseId: DatabaseId) (onSave: Database -> JS.Promise<unit>) =
        let toast = UI.useToast ()
        let debug = Store.useValue Atoms.debug

        let onSave =
            Store.useCallback (
                (fun getter setter _ ->
                    promise {
                        let databaseName = Store.getTempValue getter (Atoms.Database.name databaseId)
                        let username = Store.value getter Store.Atoms.username

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

                                Store.set setter (Atoms.User.uiFlag UIFlagType.Database) UIFlag.None

                                do! onSave database
                            | None -> ()
                    }),
                [|
                    box databaseId
                    box onSave
                    box toast
                |]
            )

        Accordion.Accordion
            {|
                Props =
                    fun x ->
                        x.flex <- "1"
                        x.overflowY <- "auto"
                        x.flexBasis <- 0
                Atom = Atoms.User.accordionFlag AccordionType.DatabaseForm
                Items =
                    [
                        $"""{if databaseId = Database.Default.Id then "Add" else "Edit"} Database""",
                        (UI.stack
                            (fun x -> x.spacing <- "15px")
                            [
                                if not debug then
                                    nothing
                                else
                                    UI.box
                                        (fun _ -> ())
                                        [
                                            str $"{databaseId}"
                                        ]

                                Input.Input
                                    {|
                                        CustomProps =
                                            fun x ->
                                                x.atom <-
                                                    Some (
                                                        Store.InputAtom (
                                                            Store.AtomReference.Atom (Atoms.Database.name databaseId)
                                                        )
                                                    )

                                                x.inputScope <- Some (Store.InputScope.Temp Gun.defaultSerializer)
                                                x.onFormat <- Some (fun (DatabaseName name) -> name)
                                                x.onValidate <- Some (fst >> DatabaseName >> Some)
                                                x.onEnterPress <- Some onSave
                                        Props =
                                            fun x ->
                                                x.autoFocus <- true
                                                x.label <- str "Name"

                                                x.placeholder <-
                                                    $"""new-database-%s{DateTime.Now.Format "yyyy-MM-dd"}"""
                                    |}

                                UI.stack
                                    (fun x ->
                                        x.direction <- "row"
                                        x.alignItems <- "center")
                                    [
                                        UI.box
                                            (fun _ -> ())
                                            [
                                                str "Access:"
                                            ]

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
        let hydrateDatabase = Hydrate.useHydrateDatabase ()
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
                    do! hydrateDatabase (Store.AtomScope.Current, database)
                    setRightDock None
                })
