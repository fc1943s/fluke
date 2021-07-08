namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State
open Fluke.Shared


module DatabaseSelector =
    [<ReactComponent>]
    let rec DatabaseSelector (databaseId: DatabaseId) (taskId: TaskId) (onChange: DatabaseId -> unit) =
        let (DatabaseName databaseName) = Store.useValue (Atoms.Database.name databaseId)
        let hydrateDatabase = Hydrate.useHydrateDatabase ()

        let databaseIdAtoms = Store.useValue Selectors.asyncDatabaseIdAtoms

        let databaseIdList =
            databaseIdAtoms
            |> Store.waitForAll
            |> Store.useValue

        let filteredDatabaseIdList =
            databaseIdList
            |> Array.map Selectors.Database.isReadWrite
            |> Store.waitForAll
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
            |> Store.waitForAll
            |> Store.useValue
            |> Array.toList
            |> List.map DatabaseName.Value

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

        Chakra.box
            (fun x -> Chakra.setTestId x (nameof DatabaseSelector))
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
                                        Hint = None
                                        Icon = Some (Icons.fi.FiChevronDown |> Icons.render, Button.IconPosition.Right)
                                        Props =
                                            fun x ->
                                                x.onClick <- fun _ -> promise { setVisible (not visible) }
                                                if taskId <> Task.Default.Id then x.isDisabled <- true
                                        Children =
                                            [
                                                match databaseName with
                                                | String.ValidString name -> str name
                                                | _ -> str "Select..."
                                            ]
                                    |}
                        Body =
                            fun onHide ->
                                [
                                    Chakra.stack
                                        (fun x ->
                                            x.flex <- "1"
                                            x.spacing <- "1px"
                                            x.padding <- "1px"
                                            x.marginBottom <- "6px"
                                            x.maxHeight <- "217px"
                                            x.overflowY <- "auto"
                                            x.flexBasis <- 0)
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
                                                                                onHide ()
                                                                            }
                                                                    Checked = index = i
                                                                |}

                                                        Some (label, cmp))
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
                                                            Hint = None
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
                                                fun onHide ->
                                                    [
                                                        DatabaseForm.DatabaseForm
                                                            Database.Default.Id
                                                            (fun database ->
                                                                promise {
                                                                    do!
                                                                        hydrateDatabase (
                                                                            Store.AtomScope.Current,
                                                                            database
                                                                        )

                                                                    onHide ()
                                                                })
                                                    ]
                                        |}
                                ]
                    |}
            ]
