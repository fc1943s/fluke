namespace Fluke.UI.Frontend.Components

open Browser.Types
open Fable.Core.JsInterop
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
        let toast = Chakra.useToast ()
        let debug = Store.useValue Atoms.debug

        let onSave =
            Store.useCallback (
                (fun getter setter _ ->
                    promise {
                        let databaseName = Store.getReadWrite getter (Atoms.Database.name databaseId)
                        let username = Store.value getter Store.Atoms.username

                        match databaseName with
                        | DatabaseName String.InvalidString -> toast (fun x -> x.description <- "Invalid name")
                        | _ ->
                            let databaseIdSet = Store.value getter Atoms.databaseIdSet

                            let databaseNames =
                                databaseIdSet
                                |> Set.toList
                                |> List.filter
                                    (fun databaseId' ->
                                        databaseId <> Database.Default.Id
                                        || databaseId <> databaseId')
                                |> List.map Atoms.Database.name
                                |> List.map (Store.value getter)

                            match username with
                            | Some username ->
                                if databaseNames |> List.contains databaseName then
                                    toast (fun x -> x.description <- "Database with this name already exists")
                                else
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
                                                let database =
                                                    Store.value getter (Selectors.Database.database databaseId)

                                                return { database with Name = databaseName }
                                            }

                                    //                                let eventId = Atoms.Events.newEventId ()
                                    //                                let event = Atoms.Events.Event.AddDatabase (eventId, databaseName, dayStart)
                                    //                                setter.set (Atoms.Events.events eventId, event)
                                    //                                printfn $"event {event}"

                                    Store.readWriteReset setter (Atoms.Database.name databaseId)

                                    Store.set setter (Atoms.uiFlag Atoms.UIFlagType.Database) Atoms.UIFlag.None

                                    do! onSave database
                            | None -> ()
                    }),
                [|
                    box databaseId
                    box onSave
                    box toast
                |]
            )

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
                                str $"""{if databaseId = Database.Default.Id then "Add" else "Edit"} Database"""
                            ]

                        if not debug then
                            nothing
                        else
                            Chakra.box
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

                                        x.inputScope <- Some (Store.InputScope.ReadWrite Gun.defaultSerializer)
                                        x.onFormat <- Some (fun (DatabaseName name) -> name)
                                        x.onValidate <- Some (fst >> DatabaseName >> Some)
                                        x.onEnterPress <- Some onSave
                                Props =
                                    fun x ->
                                        x.autoFocus <- true
                                        x.label <- str "Name"
                                        x.placeholder <- $"""new-database-%s{DateTime.Now.Format "yyyy-MM-dd"}"""
                            |}

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
                                        Props = fun x -> x.onClick <- fun _ -> importDatabase files
                                        Children =
                                            [
                                                str "Confirm"
                                            ]
                                    |}
                            ]
                    ]
            ]
