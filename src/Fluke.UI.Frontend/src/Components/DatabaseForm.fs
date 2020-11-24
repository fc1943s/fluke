namespace Fluke.UI.Frontend.Components

open System.Globalization
open Fable.React
open Feliz
open System
open Feliz.Recoil
open Fluke.Shared.Domain
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fable.DateFunctions
open Feliz.UseListener

module DatabaseForm =
    open State
    open Model

    module DatabaseAccessIndicator =
        let render () =
            Chakra.stack
                {| direction = "row"; spacing = "15px" |}
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
                            width = "22px"
                            height = "15px"
                            onClick = fun () -> ()
                        |}
                        []

                ]

    module Fields =
        let render databaseId =
            Chakra.stack
                {| spacing = "15px" |}
                [
                    Input.render
                        {|
                            Label = Some "Name"
                            Placeholder = sprintf "new-database-%s" (DateTime.Now.Format "yyyy-MM-dd")
                            Atom = Recoil.Atoms.Database.name databaseId
                            InputFormat = Input.InputFormat.Text
                            OnFormat = fun (DatabaseName name) -> name
                            OnValidate = DatabaseName >> Some
                        |}

                    Input.render
                        {|
                            Label = Some "Day starts at"
                            Placeholder = "00:00"
                            Atom = Recoil.Atoms.Database.dayStart databaseId
                            InputFormat = Input.InputFormat.Time
                            OnFormat = fun time -> time.Stringify ()
                            OnValidate = DateTime.Parse >> FlukeTime.FromDateTime >> Some
                        |}

                    Chakra.stack
                        {| direction = "row"; align = "center" |}
                        [
                            Chakra.box
                                {|  |}
                                [
                                    str "Access:"
                                ]

                            DatabaseAccessIndicator.render ()
                        ]
                ]

    let render =
        React.memo (fun (input: {| Username: UserInteraction.Username
                                   DatabaseId: State.DatabaseId |}) ->

            let availableDatabaseIds, setAvailableDatabaseIds =
                Recoil.useState (Recoil.Atoms.Session.availableDatabaseIds input.Username)

            Chakra.stack
                {| spacing = "25px" |}
                [
                    Chakra.box
                        {| fontSize = "15px" |}
                        [
                            str "Add Database"
                        ]

                    Fields.render input.DatabaseId

                    Chakra.button
                        {|  |}
                        [
                            str "Save"
                        ]
                ])
