namespace Fluke.UI.Frontend.Components

open FsCore
open System
open Fable.React
open Feliz
open Fluke.Shared.Domain.State
open FsStore
open FsStore.Hooks
open FsStore.Model
open FsUi.Bindings
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State
open Fluke.Shared
open FsUi.Components


module DatabaseSelector =
    [<ReactComponent>]
    let rec DatabaseSelector (databaseId: DatabaseId) (onChange: DatabaseId -> unit) =
        let setLastDatabaseSelected = Store.useSetState Atoms.User.lastDatabaseSelected

        let onChange =
            fun databaseId ->
                setLastDatabaseSelected (Some databaseId)
                onChange databaseId

        let (DatabaseName databaseName) = Store.useValue (Atoms.Database.name databaseId)
        let hydrateDatabase = Store.useCallbackRef Hydrate.hydrateDatabase

        let databaseIdAtoms = Store.useValue Selectors.Session.databaseIdAtoms

        let databaseIdList =
            databaseIdAtoms
            |> Atom.waitForAll
            |> Store.useValue

        let filteredDatabaseIdList =
            databaseIdList
            |> Array.map Selectors.Database.isReadWrite
            |> Atom.waitForAll
            |> Store.useValue
            |> Array.toList
            |> List.mapi
                (fun i isReadWrite ->
                    match Some isReadWrite with
                    | Some true -> Some databaseIdList.[i]
                    | _ -> None)
            |> List.choose id

        let databaseNameList =
            filteredDatabaseIdList
            |> List.map Atoms.Database.name
            |> List.toArray
            |> Atom.waitForAll
            |> Store.useValue
            |> Array.toList
            |> List.map DatabaseName.ValueOrDefault


        let index =
            React.useMemo (
                (fun () ->
                    filteredDatabaseIdList
                    |> List.sort
                    |> List.tryFindIndex ((=) databaseId)
                    |> Option.defaultValue -1),
                [|
                    box filteredDatabaseIdList
                    box databaseId
                |]
            )

        Ui.box
            (fun x -> Ui.setTestId x (nameof DatabaseSelector))
            [
                InputLabel.InputLabel
                    {|
                        Hint = None
                        HintTitle = None
                        Label = str "Database"
                        Props = fun x -> x.marginBottom <- "5px"
                    |}

                Dropdown.Dropdown
                    {|
                        Tooltip = ""
                        Left = true
                        Trigger =
                            fun visible setVisible ->
                                Button.Button
                                    {|
                                        Tooltip = None
                                        Icon = Some (Icons.fi.FiChevronDown |> Icons.render, Button.IconPosition.Right)
                                        Props = fun x -> x.onClick <- fun _ -> promise { setVisible (not visible) }
                                        Children =
                                            [
                                                match databaseName with
                                                | String.Valid name -> str name
                                                | _ -> str "Select..."
                                            ]
                                    |}
                        Body =
                            fun onHide1 ->
                                [
                                    Ui.stack
                                        (fun x ->
                                            x.flex <- "1"
                                            x.spacing <- "1px"
                                            x.padding <- "1px"
                                            x.marginBottom <- "6px"
                                            x.maxHeight <- "217px"
                                            x.overflowY <- "auto")
                                        [
                                            yield!
                                                filteredDatabaseIdList
                                                |> List.mapi
                                                    (fun i databaseId ->
                                                        let label = databaseNameList.[i]

                                                        let cmp =
                                                            DropdownMenuButton.DropdownMenuButton
                                                                {|
                                                                    Label = label
                                                                    OnClick =
                                                                        fun () ->
                                                                            promise {
                                                                                onChange databaseId
                                                                                onHide1 ()
                                                                            }
                                                                    Checked = index = i
                                                                |}

                                                        Some (label, cmp))
                                                |> List.filter (
                                                    Option.map fst
                                                    >> (Option.map (String.IsNullOrWhiteSpace >> not))
                                                    >> Option.defaultValue false
                                                )
                                                |> List.sortBy (Option.map fst)
                                                |> List.map (Option.map snd)
                                                |> List.map (Option.defaultValue nothing)
                                        ]

                                    Dropdown.Dropdown
                                        {|
                                            Tooltip = ""
                                            Left = true
                                            Trigger =
                                                fun visible setVisible ->
                                                    Button.Button
                                                        {|
                                                            Tooltip = None
                                                            Icon =
                                                                Some (
                                                                    (if visible then
                                                                         Icons.fi.FiChevronUp
                                                                     else
                                                                         Icons.fi.FiChevronDown)
                                                                    |> Icons.render,
                                                                    Button.IconPosition.Right
                                                                )
                                                            Props =
                                                                fun x ->
                                                                    x.onClick <-
                                                                        fun _ -> promise { setVisible (not visible) }
                                                            Children =
                                                                [
                                                                    str "Add Database"
                                                                ]
                                                        |}
                                            Body =
                                                fun onHide2 ->
                                                    [
                                                        DatabaseForm.DatabaseForm
                                                            Database.Default.Id
                                                            (fun database ->
                                                                promise {
                                                                    do! hydrateDatabase (AtomScope.Current, database)
                                                                    onChange database.Id
                                                                    onHide1 ()
                                                                    onHide2 ()
                                                                })
                                                    ]
                                        |}
                                ]
                    |}
            ]
