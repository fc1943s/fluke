namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Feliz.Recoil
open Fluke.Shared.Domain
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.Bindings
open Fable.Core
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.State
open Fluke.Shared
open Fable.Core.JsInterop


module DatabaseSelector =
    [<ReactComponent>]
    let rec DatabaseSelector
        (input: {| Username: UserInteraction.Username
                   TaskId: TaskId |})
        =
        let databaseId, setDatabaseId =
            Store.useStateLoadableDefault (Selectors.Task.databaseId (input.Username, input.TaskId)) Database.Default.Id

        let (DatabaseName databaseName) = Store.useValue (Atoms.Database.name (input.Username, databaseId))

        let databaseIdSet = Store.useValue (Atoms.Session.databaseIdSet input.Username)

        let setDatabaseIdSet = Store.useSetStatePrev (Atoms.Session.databaseIdSet input.Username)

        let hydrateDatabase = Hydrate.useHydrateDatabase ()

        let databaseIdList =
            React.useMemo (
                (fun () -> databaseIdSet |> Set.toList),
                [|
                    databaseIdSet
                |]
            )

        let filteredDatabaseIdList =
            databaseIdList
            |> List.map Selectors.Database.isReadWrite
            |> Recoil.waitForNone
            |> Store.useValue
            |> List.mapi
                (fun i isReadWrite ->
                    match isReadWrite.valueMaybe () with
                    //                    match Some isReadWrite with
                    | Some true -> Some databaseIdList.[i]
                    | _ -> None)
            |> List.choose id

        let databaseNameList =
            filteredDatabaseIdList
            |> List.map (fun databaseId -> Atoms.Database.name (input.Username, databaseId))
            |> Recoil.waitForNone
            |> Store.useValue
            |> List.map
                (fun name ->
                    name.valueMaybe ()
                    |> Option.map DatabaseName.Value
                    |> Option.defaultValue "")

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

        let isTesting = Store.useValue Atoms.isTesting

        Chakra.box
            (fun x -> if isTesting then x?``data-testid`` <- nameof DatabaseSelector)
            [
                InputLabel.InputLabel
                    {|
                        Hint = None
                        HintTitle = None
                        Label = str "Database"
                        Props = fun x -> x.marginBottom <- "5px"
                    |}
                Menu.Drawer
                    {|
                        Tooltip = ""
                        Left = true
                        Trigger =
                            fun visible setVisible ->
                                Button.Button
                                    {|
                                        Hint = None
                                        Icon = Some (Icons.fi.FiChevronDown |> Icons.wrap, Button.IconPosition.Right)
                                        Props =
                                            fun x ->
                                                x.onClick <- fun _ -> promise { setVisible (not visible) }
                                                if input.TaskId <> Task.Default.Id then x.isDisabled <- true
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
                                                            Menu.DrawerMenuButton
                                                                {|
                                                                    Label = label
                                                                    OnClick =
                                                                        fun () ->
                                                                            promise {
                                                                                setDatabaseId databaseId

                                                                                onHide ()
                                                                            }
                                                                    Checked = index = i
                                                                |}

                                                        Some (label, cmp))
                                                |> List.sortBy (Option.map fst)
                                                |> List.map (Option.map snd)
                                                |> List.map (Option.defaultValue nothing)
                                        ]

                                    Menu.Drawer
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
                                                                    |> Icons.wrap,
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
                                                            {|
                                                                Username = input.Username
                                                                DatabaseId = Database.Default.Id
                                                                OnSave =
                                                                    fun database ->
                                                                        promise {
                                                                            do!
                                                                                hydrateDatabase (
                                                                                    input.Username,
                                                                                    Recoil.AtomScope.ReadOnly,
                                                                                    database
                                                                                )

                                                                            JS.setTimeout
                                                                                (fun () ->
                                                                                    setDatabaseIdSet (
                                                                                        Set.add database.Id
                                                                                    ))
                                                                                0
                                                                            |> ignore

                                                                            onHide ()
                                                                        }
                                                            |}
                                                    ]
                                        |}
                                ]
                    |}
            ]
