namespace Fluke.UI.Frontend.Components

open Browser.Types
open Fable.Core.JsInterop
open Fable.React
open Feliz
open System
open Fluke.Shared.Domain
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend
open Fluke.Shared
open Fluke.UI.Frontend.Bindings
open Fable.DateFunctions
open Fable.Core
open Fluke.UI.Frontend.Hooks


module DatabaseForm =
    open State

    [<ReactComponent>]
    let rec DatabaseForm
        (input: {| Username: Username
                   DatabaseId: DatabaseId
                   OnSave: Database -> JS.Promise<unit> |})
        =
        let toast = Chakra.useToast ()
        let debug = Store.useValue Atoms.debug

        let onSave =
            Store.useCallbackRef
                (fun setter _ ->
                    promise {
                        let! databaseName =
                            setter.snapshot.getReadWritePromise
                                input.Username
                                Atoms.Database.name
                                (input.Username, input.DatabaseId)

                        match databaseName with
                        | DatabaseName String.InvalidString -> toast (fun x -> x.description <- "Invalid name")
                        | _ ->
                            let! databaseIdSet = setter.snapshot.getPromise (Atoms.Session.databaseIdSet input.Username)

                            let! databaseNames =
                                databaseIdSet
                                |> Set.toList
                                |> List.filter
                                    (fun databaseId ->
                                        input.DatabaseId <> Database.Default.Id
                                        || input.DatabaseId <> databaseId)
                                |> List.map (fun databaseId -> Atoms.Database.name (input.Username, databaseId))
                                |> List.map setter.snapshot.getPromise
                                |> Promise.Parallel

                            if databaseNames |> Array.contains databaseName then
                                toast (fun x -> x.description <- "Database with this name already exists")
                            else
                                let! database =
                                    if input.DatabaseId = Database.Default.Id then
                                        {
                                            Id = DatabaseId.NewId ()
                                            Name = databaseName
                                            Owner = input.Username
                                            SharedWith = DatabaseAccess.Private []
                                            Position = None
                                        }
                                        |> Promise.lift
                                    else
                                        promise {
                                            let! database =
                                                setter.snapshot.getPromise (
                                                    Selectors.Database.database (input.Username, input.DatabaseId)
                                                )

                                            return { database with Name = databaseName }
                                        }

                                //                                let eventId = Atoms.Events.newEventId ()
//                                let event = Atoms.Events.Event.AddDatabase (eventId, databaseName, dayStart)
//                                setter.set (Atoms.Events.events eventId, event)
//                                printfn $"event {event}"

                                do!
                                    setter.readWriteReset
                                        input.Username
                                        Atoms.Database.name
                                        (input.Username, input.DatabaseId)

                                setter.set (
                                    (Atoms.User.formIdFlag (input.Username, TextKey (nameof DatabaseForm))),
                                    fun _ -> None
                                )

                                do! input.OnSave database
                    })

        let files, setFiles = React.useState (None: FileList option)

        let importDatabase = Hydrate.useImportDatabase ()

        Chakra.stack
            (fun x -> x.spacing <- "30px")
            [
                Chakra.stack
                    (fun x -> x.spacing <- "15px")
                    [

                        Chakra.box
                            (fun x -> x.fontSize <- "15px")
                            [
                                str $"""{if input.DatabaseId = Database.Default.Id then "Add" else "Edit"} Database"""
                            ]

                        if not debug then
                            nothing
                        else
                            Chakra.box
                                (fun _ -> ())
                                [
                                    str $"{input.DatabaseId}"
                                ]

                        Input.Input
                            (fun x ->
                                x.autoFocus <- true
                                x.label <- str "Name"
                                x.placeholder <- $"""new-database-%s{DateTime.Now.Format "yyyy-MM-dd"}"""

                                x.atom <-
                                    Some (
                                        Recoil.AtomFamily (
                                            input.Username,
                                            Atoms.Database.name,
                                            (input.Username, input.DatabaseId)
                                        )
                                    )

                                x.inputScope <- Some (Recoil.InputScope.ReadWrite Gun.defaultSerializer)
                                x.onFormat <- Some (fun (DatabaseName name) -> name)
                                x.onValidate <- Some (fst >> DatabaseName >> Some)
                                x.onEnterPress <- Some onSave)

                        Chakra.stack
                            (fun x ->
                                x.direction <- "row"
                                x.alignItems <- "center")
                            [
                                Chakra.box
                                    (fun _ -> ())
                                    [
                                        str "Access:"
                                    ]

                                DatabaseAccessIndicator.DatabaseAccessIndicator ()
                            ]

                        Button.Button
                            {|
                                Hint = None
                                Icon = Some (Icons.fi.FiSave |> Icons.wrap, Button.IconPosition.Left)
                                Props = fun x -> x.onClick <- onSave
                                Children =
                                    [
                                        str "Save"
                                    ]
                            |}
                    ]

                Html.hr []

                Chakra.stack
                    (fun x -> x.spacing <- "15px")
                    [
                        Chakra.box
                            (fun x -> x.fontSize <- "15px")
                            [
                                str "Import Database"
                            ]

                        Chakra.input
                            (fun x ->
                                x.``type`` <- "file"
                                x.padding <- "5px"
                                x.onChange <- fun x -> promise { x?target?files |> Option.ofObj |> setFiles })
                            []

                        Chakra.box
                            (fun _ -> ())
                            [
                                Button.Button
                                    {|
                                        Hint = None
                                        Icon = Some (Icons.bi.BiImport |> Icons.wrap, Button.IconPosition.Left)
                                        Props = fun x -> x.onClick <- fun _ -> importDatabase (input.Username, files)
                                        Children =
                                            [
                                                str "Confirm"
                                            ]
                                    |}
                            ]
                    ]
            ]
