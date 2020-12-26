namespace Fluke.UI.Frontend.Components

open Fluke.Shared
open Feliz
open Fable.React
open Feliz.Recoil
open FSharpPlus
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module Databases =

    open Domain.UserInteraction
    open Domain.State

    module MenuItem =
        [<ReactComponent>]
        let MenuItem (input: {| Username: Username; DatabaseId: DatabaseId |}) =
            let (DatabaseId databaseId) = input.DatabaseId

            let isTesting = Recoil.useValue Recoil.Atoms.isTesting
            let selectedPosition, setSelectedPosition = Recoil.useState (Recoil.Atoms.selectedPosition)
            let (DatabaseName databaseName) = Recoil.useValue (Recoil.Atoms.Database.name input.DatabaseId)
            let databasePosition = Recoil.useValue (Recoil.Atoms.Database.position input.DatabaseId)
            let availableDatabaseIds = Recoil.useValue (Recoil.Atoms.Session.availableDatabaseIds input.Username)

            let selectedDatabaseIds, setSelectedDatabaseIds = Recoil.useState Recoil.Atoms.selectedDatabaseIds

            let availableDatabaseIdsSet = set availableDatabaseIds

            let selectedDatabaseIdsSet =
                selectedDatabaseIds
                |> set
                |> Set.intersect availableDatabaseIdsSet

            let selected = selectedDatabaseIdsSet.Contains input.DatabaseId

            let changeSelection =
                Recoil.useCallbackRef (fun _setter newSelectedDatabaseIds ->
                    setSelectedDatabaseIds newSelectedDatabaseIds

                    match newSelectedDatabaseIds with
                    | [||] -> None
                    | _ -> databasePosition
                    |> setSelectedPosition)

            let onChange = fun (_e: {| target: obj |}) -> ()

            let onClick =
                fun (e: {| target: Browser.Types.HTMLElement |}) ->
                    if JS.instanceof (e.target, nameof Browser.Types.HTMLInputElement) then
                        let swap value set =
                            if set |> Set.contains value then
                                set |> Set.remove value
                            else
                                set |> Set.add value

                        let newSelectedDatabaseIds =
                            selectedDatabaseIdsSet
                            |> swap input.DatabaseId
                            |> Set.toArray

                        changeSelection newSelectedDatabaseIds

            let (|RenderCheckbox|HideCheckbox|) (selectedPosition, databasePosition) =
                match selectedPosition, databasePosition with
                | None, None -> RenderCheckbox
                | None, Some _ when selectedDatabaseIdsSet.IsEmpty -> RenderCheckbox
                | Some _, Some _ when selectedPosition = databasePosition -> RenderCheckbox
                | _ -> HideCheckbox

            let enabled =
                match selectedPosition, databasePosition with
                | RenderCheckbox -> true
                | _ -> false

            Chakra.checkbox
                {|
                    ``data-testid`` =
                        if isTesting then
                            Some $"menu-item-{databaseId.ToString ()}"
                        else
                            None
                    value = input.DatabaseId
                    isChecked = selected
                    isDisabled =
                        if enabled then
                            None
                        else
                            Some true
                    onClick = onClick
                    onChange = onChange
                |}
                [
                    str databaseName
                ]



    [<ReactComponent>]
    let Databases (username: Username) (props: {| flex: int; overflowY: string; flexBasis: int |}) =
        let availableDatabaseIds = Recoil.useValue (Recoil.Atoms.Session.availableDatabaseIds username)

        Chakra.stack
            props
            [
                yield!
                    availableDatabaseIds
                    |> List.map (fun databaseId -> MenuItem.MenuItem {| Username = username; DatabaseId = databaseId |})
            ]
