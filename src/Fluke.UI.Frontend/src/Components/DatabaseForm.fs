namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open System
open Feliz.Recoil
open Fluke.Shared.Domain
open Fluke.UI.Frontend
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
                            x.icon <- Icons.bsThreeDots ()
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
                   OnSave: unit -> JS.Promise<unit> |})
        =
        let onSave =
            Recoil.useCallbackRef
                (fun (setter: CallbackMethods) _ ->
                    promise {
                        let eventId = Recoil.Atoms.Events.newEventId ()
                        let! name = setter.snapshot.getReadWritePromise Recoil.Atoms.Database.name input.DatabaseId

                        let! dayStart =
                            setter.snapshot.getReadWritePromise Recoil.Atoms.Database.dayStart input.DatabaseId

                        let! availableDatabaseIds =
                            setter.snapshot.getPromise (Recoil.Atoms.Session.availableDatabaseIds input.Username)

                        let event = Recoil.Atoms.Events.Event.AddDatabase (eventId, name, dayStart)

                        setter.set (Recoil.Atoms.Events.events eventId, event)

                        let! databaseStateMapCache =
                            setter.snapshot.getPromise (Recoil.Atoms.Session.databaseStateMapCache input.Username)

                        let databaseId =
                            input.DatabaseId
                            |> Option.defaultValue (DatabaseId.NewId ())

                        let database =
                            databaseStateMapCache
                            |> Map.tryFind databaseId
                            |> Option.defaultValue (
                                DatabaseState.Create (
                                    name = name,
                                    owner = input.Username,
                                    dayStart = dayStart,
                                    id = databaseId
                                )
                            )

                        let newDatabaseStateMapCache =
                            databaseStateMapCache
                            |> Map.add
                                databaseId
                                { database with
                                    Database =
                                        { database.Database with
                                            Name = name
                                            DayStart = dayStart
                                        }
                                }

                        printfn
                            $"DatabaseForm():
                        databaseStateMapCache.Count={databaseStateMapCache.Count}
                        newDatabaseStateMapCache.Count={newDatabaseStateMapCache.Count}"

                        setter.set (Recoil.Atoms.Session.databaseStateMapCache input.Username, newDatabaseStateMapCache)

                        setter.set (
                            Recoil.Atoms.Session.availableDatabaseIds input.Username,
                            (availableDatabaseIds
                             @ [
                                 databaseId
                             ])
                        )

                        do! setter.readWriteReset Recoil.Atoms.Database.name input.DatabaseId
                        do! setter.readWriteReset Recoil.Atoms.Database.dayStart input.DatabaseId

                        printfn $"event {event}"
                        do! input.OnSave ()

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
                        Input.Input (
                            JS.newObj
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
                                    x.atom <- Some (Recoil.AtomFamily (Recoil.Atoms.Database.name, input.DatabaseId))
                                    x.onFormat <- Some (fun (DatabaseName name) -> name)
                                    x.onValidate <- Some (DatabaseName >> Some)
                                    x.onEnterPress <- Some onSave)
                        )

                        Input.Input (
                            JS.newObj
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

                                    x.atom <-
                                        Some (Recoil.AtomFamily (Recoil.Atoms.Database.dayStart, input.DatabaseId))

                                    x.inputFormat <- Some Input.InputFormat.Time
                                    x.onFormat <- Some (fun time -> time.Stringify ())
                                    x.onValidate <- Some (DateTime.Parse >> FlukeTime.FromDateTime >> Some))
                        )

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
