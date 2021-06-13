namespace Fluke.UI.Frontend.Components

open Fable.Core.JsInterop
open Fable.React
open Feliz
open System
open Feliz.Recoil
open Fluke.Shared.Domain
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend
open Fluke.Shared
open Fluke.UI.Frontend.Bindings
open Fable.DateFunctions
open Fable.Core


module DatabaseForm =
    open State

    [<ReactComponent>]
    let DatabaseForm
        (input: {| Username: Username
                   DatabaseId: DatabaseId
                   OnSave: Database -> JS.Promise<unit> |})
        =
        let toast = Chakra.useToast ()

        let onSave =
            Recoil.useCallbackRef
                (fun (setter: CallbackMethods) _ ->
                    promise {
                        let! databaseName =
                            setter.snapshot.getReadWritePromise
                                input.Username
                                Atoms.Database.name
                                (input.Username, input.DatabaseId)

                        match databaseName with
                        | DatabaseName String.InvalidString -> toast (fun x -> x.description <- "Invalid name")
                        | _ ->
                            let! databaseIdSet = setter.snapshot.getPromise (Atoms.User.databaseIdSet input.Username)

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

                                do! input.OnSave database
                    })

        let importDatabase =
            Recoil.useCallbackRef
                (fun setter (files: string []) ->
                    promise {
                        JS.consoleLog ("files", files)
                        //                        let obj = Gun.jsonDecode<DatabaseState> text
//
//                        let! database =
//                            setter.snapshot.getPromise (Selectors.Database.database (input.Username, input.DatabaseId))
//
//                        let! taskIdSet =
//                            setter.snapshot.getPromise (Atoms.Database.taskIdSet (input.Username, input.DatabaseId))
//
//                        let! taskStateArray =
//                            taskIdSet
//                            |> Set.toList
//                            |> List.map (fun taskId -> Selectors.Task.taskState (input.Username, taskId))
//                            |> List.map setter.snapshot.getPromise
//                            |> Promise.Parallel
//
//                        let! informationStateList =
//                            setter.snapshot.getPromise (Selectors.Session.informationStateList input.Username)
//
//                        let databaseState =
//                            {
//                                Database = database
//                                InformationStateMap =
//                                    informationStateList
//                                    |> List.map (fun informationState -> informationState.Information, informationState)
//                                    |> Map.ofList
//                                TaskStateMap =
//                                    taskStateArray
//                                    |> Array.map (fun taskState -> taskState.Task.Id, taskState)
//                                    |> Map.ofArray
//                            }
//
//                        let json = databaseState |> Gun.jsonEncode
//
//                        let timestamp =
//                            (FlukeDateTime.FromDateTime DateTime.Now)
//                            |> FlukeDateTime.Stringify
//
//                        JS.download json $"{database.Name |> DatabaseName.Value}-{timestamp}.json" "application/json"
                        ()
                    })

        Chakra.stack
            (fun x -> x.spacing <- "35px")
            [

                Chakra.stack
                    (fun x -> x.spacing <- "15px")
                    [

                        Chakra.box
                            (fun x -> x.fontSize <- "15px")
                            [
                                str $"""{if input.DatabaseId = Database.Default.Id then "Add" else "Edit"} Database"""
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
                                x.onChange <- fun x -> promise { JS.consoleLog ("files", x?target?files) })
                            []

                        Chakra.box
                            (fun _ -> ())
                            [
                                Button.Button
                                    {|
                                        Hint = None
                                        Icon = Some (Icons.bi.BiImport |> Icons.wrap, Button.IconPosition.Left)
                                        Props = fun x -> x.onClick <- fun x -> importDatabase x?target?files
                                        Children =
                                            [
                                                str "Confirm"
                                            ]
                                    |}
                            ]
                    ]
            ]
