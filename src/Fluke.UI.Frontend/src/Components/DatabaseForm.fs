namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open System
open Feliz.Recoil
open Fluke.Shared.Domain
open Fluke.UI.Frontend
open Fluke.Shared
open Fluke.UI.Frontend.Bindings
open Fable.DateFunctions
open Fable.Core


module DatabaseForm =
    open State
    open Model

    module DatabaseAccessIndicator =
        [<ReactComponent>]
        let DatabaseAccessIndicator () =
            Chakra.stack
                (fun x ->
                    x.direction <- "row"
                    x.spacing <- "15px")
                [
                    Chakra.stack
                        (fun x ->
                            x.direction <- "row"
                            x.spacing <- "4px"
                            x.align <- "center")
                        [
                            Chakra.circle
                                (fun x ->
                                    x.width <- "10px"
                                    x.height <- "10px"
                                    x.backgroundColor <- "#0f0")
                                []

                            Chakra.box
                                (fun _ -> ())
                                [
                                    str "Private"
                                ]

                        ]
                    Chakra.iconButton
                        (fun x ->
                            x.icon <- Icons.bs.BsThreeDots |> Icons.render
                            x.disabled <- true
                            x.width <- "22px"
                            x.height <- "15px"
                            x.onClick <- fun _ -> promise { () })
                        []

                ]

    [<ReactComponent>]
    let DatabaseForm
        (input: {| Username: UserInteraction.Username
                   DatabaseId: State.DatabaseId option
                   OnSave: Database -> JS.Promise<unit> |})
        =
        let toast = Chakra.useToast ()

        let onSave =
            Recoil.useCallbackRef
                (fun (setter: CallbackMethods) _ ->
                    promise {
                        let! databaseName = setter.snapshot.getReadWritePromise Atoms.Database.name input.DatabaseId

                        match databaseName with
                        | DatabaseName (String.NullString
                        | String.WhitespaceStr) -> toast (fun x -> x.description <- "Invalid name")
                        | _ ->
                            let! databaseIdList =
                                setter.snapshot.getPromise (Atoms.Session.databaseIdList input.Username)

                            let! databaseNames =
                                databaseIdList
                                |> List.filter
                                    (fun databaseId ->
                                        match input.DatabaseId with
                                        | Some databaseId' -> databaseId' <> databaseId
                                        | None -> true)
                                |> List.map (fun databaseId -> Atoms.Database.name (Some databaseId))
                                |> Recoil.waitForAll
                                |> setter.snapshot.getPromise

                            if databaseNames |> List.contains databaseName then
                                toast (fun x -> x.description <- "Database with this name already exists")
                            else
                                let! dayStart =
                                    setter.snapshot.getReadWritePromise Atoms.Database.dayStart input.DatabaseId

                                let! database =
                                    match input.DatabaseId with
                                    | Some databaseId ->
                                        promise {
                                            let! database =
                                                setter.snapshot.getPromise (Selectors.Database.database databaseId)

                                            return
                                                { database with
                                                    Name = databaseName
                                                    DayStart = dayStart
                                                }
                                        }
                                    | None ->
                                        {
                                            Id = DatabaseId.NewId ()
                                            Name = databaseName
                                            Owner = input.Username
                                            SharedWith = DatabaseAccess.Private []
                                            Position = None
                                            DayStart = dayStart
                                        }
                                        |> Promise.lift

                                //                                let eventId = Atoms.Events.newEventId ()
//                                let event = Atoms.Events.Event.AddDatabase (eventId, databaseName, dayStart)
//                                setter.set (Atoms.Events.events eventId, event)
//                                printfn $"event {event}"

                                do! setter.readWriteReset Atoms.Database.name input.DatabaseId

                                do! setter.readWriteReset Atoms.Database.dayStart input.DatabaseId

                                do! input.OnSave database
                    })

        Chakra.stack
            (fun x -> x.spacing <- "25px")
            [
                Chakra.box
                    (fun x -> x.fontSize <- "15px")
                    [
                        str $"""{if input.DatabaseId.IsNone then "Add" else "Edit"} Database"""
                    ]

                Chakra.stack
                    (fun x -> x.spacing <- "15px")
                    [
                        Input.Input
                            (fun x ->
                                x.autoFocus <- true
                                x.label <- str "Name"

                                x.hint <-
                                    Some (
                                        Chakra.box
                                            (fun _ -> ())
                                            [
                                                str "Documentation"
                                            ]
                                    )

                                x.placeholder <- $"""new-database-%s{DateTime.Now.Format "yyyy-MM-dd"}"""
                                x.atom <- Some (Recoil.AtomFamily (Atoms.Database.name, input.DatabaseId))
                                x.onFormat <- Some (fun (DatabaseName name) -> name)
                                x.onValidate <- Some (fst >> DatabaseName >> Some)
                                x.onEnterPress <- Some onSave)

                        Input.Input
                            (fun x ->
                                x.label <- str "Day starts at"

                                x.hint <-
                                    Some (
                                        Chakra.box
                                            (fun _ -> ())
                                            [
                                                str "Documentation"
                                            ]
                                    )

                                x.placeholder <- "00:00"

                                x.atom <- Some (Recoil.AtomFamily (Atoms.Database.dayStart, input.DatabaseId))
                                x.inputFormat <- Some Input.InputFormat.Time
                                x.onFormat <- Some (fun time -> time.Stringify ())

                                x.onValidate <-
                                    Some (
                                        fst
                                        >> DateTime.Parse
                                        >> FlukeTime.FromDateTime
                                        >> Some
                                    ))

                        Chakra.stack
                            (fun x ->
                                x.direction <- "row"
                                x.align <- "center")
                            [
                                Chakra.box
                                    (fun _ -> ())
                                    [
                                        str "Access:"
                                    ]

                                DatabaseAccessIndicator.DatabaseAccessIndicator ()
                            ]
                    ]

                Chakra.button
                    (fun x -> x.onClick <- onSave)
                    [
                        str "Save"
                    ]
            ]
