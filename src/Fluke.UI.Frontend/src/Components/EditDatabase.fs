namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open System
open Feliz.Recoil
open Fluke.Shared.Domain
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fable.DateFunctions


module EditDatabase =
    open State
    open Model

    module DatabaseAccessIndicator =
        [<ReactComponent>]
        let DatabaseAccessIndicator () =
            Chakra.stack
                {|
                    direction = "row"
                    spacing = "15px"
                |}
                [
                    Chakra.stack
                        {|
                            direction = "row"
                            spacing = "4px"
                            align = "center"
                        |}
                        [
                            Chakra.circle
                                {|
                                    width = "10px"
                                    height = "10px"
                                    backgroundColor = "#0f0"
                                |}
                                []

                            Chakra.box
                                {|  |}
                                [
                                    str "Private"
                                ]

                        ]
                    Chakra.iconButton
                        {|
                            icon = Icons.bsThreeDots ()
                            disabled = true
                            width = "22px"
                            height = "15px"
                            onClick = fun () -> ()
                        |}
                        []

                ]


    [<ReactComponent>]
    let EditDatabase
        (input: {| Username: UserInteraction.Username
                   DatabaseId: State.DatabaseId
                   OnSave: Async<unit> |})
        =
        let onSave =
            Recoil.useCallbackRef
                (fun (setter: CallbackMethods) ->
                    async {
                        let eventId =
                            Recoil.Atoms.Events.EventId (Fable.Core.JS.Constructors.Date.now (), Guid.NewGuid ())

                        let! name = setter.snapshot.getAsync (Recoil.Atoms.Database.name input.DatabaseId)
                        let! dayStart = setter.snapshot.getAsync (Recoil.Atoms.Database.dayStart input.DatabaseId)

                        let! availableDatabaseIds =
                            setter.snapshot.getAsync (Recoil.Atoms.Session.availableDatabaseIds input.Username)

                        let event = Recoil.Atoms.Events.Event.AddDatabase (eventId, name, dayStart)

                        setter.set (Recoil.Atoms.Events.events eventId, event)

                        let! databaseStateMapCache =
                            setter.snapshot.getAsync (Recoil.Atoms.Session.databaseStateMapCache input.Username)

                        let newDatabaseStateMapCache =
                            databaseStateMapCache
                            |> Map.add
                                input.DatabaseId
                                {
                                    Database =
                                        {
                                            Id = input.DatabaseId
                                            Name = name
                                            Owner = input.Username
                                            SharedWith =
                                                DatabaseAccess.Private [
                                                    DatabaseAccessItem.Admin input.Username
                                                ]
                                            Position = None
                                            DayStart = dayStart
                                        }
                                    InformationStateMap = Map.empty
                                    TaskStateMap = Map.empty
                                }

                        setter.set (Recoil.Atoms.Session.databaseStateMapCache input.Username, newDatabaseStateMapCache)

                        setter.set (
                            Recoil.Atoms.Session.availableDatabaseIds input.Username,
                            (input.DatabaseId :: availableDatabaseIds)
                        )


                        printfn $"event {event}"
                        do! input.OnSave
                    }
                    |> Async.StartImmediate)

        Chakra.stack
            {| spacing = "25px" |}
            [
                Chakra.box
                    {| fontSize = "15px" |}
                    [
                        str "Add Database"
                    ]

                Chakra.stack
                    {| spacing = "15px" |}
                    [
                        Input.Input
                            {|
                                Label = Some "Name"
                                Placeholder = sprintf "new-database-%s" (DateTime.Now.Format "yyyy-MM-dd")
                                Atom = Recoil.Atoms.Database.name input.DatabaseId
                                InputFormat = Input.InputFormat.Text
                                OnFormat = fun (DatabaseName name) -> name
                                OnValidate = DatabaseName >> Some
                            |}

                        Input.Input
                            {|
                                Label = Some "Day starts at"
                                Placeholder = "00:00"
                                Atom = Recoil.Atoms.Database.dayStart input.DatabaseId
                                InputFormat = Input.InputFormat.Time
                                OnFormat = fun time -> time.Stringify ()
                                OnValidate = DateTime.Parse >> FlukeTime.FromDateTime >> Some
                            |}

                        Chakra.stack
                            {|
                                direction = "row"
                                align = "center"
                            |}
                            [
                                Chakra.box
                                    {|  |}
                                    [
                                        str "Access:"
                                    ]

                                DatabaseAccessIndicator.DatabaseAccessIndicator ()
                            ]
                    ]

                Chakra.button
                    {| onClick = onSave |}
                    [
                        str "Save"
                    ]
            ]
