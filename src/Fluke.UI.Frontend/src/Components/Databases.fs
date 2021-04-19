namespace Fluke.UI.Frontend.Components

open Browser.Types
open Fluke.Shared
open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module Databases =

    open Domain.UserInteraction
    open Domain.State

    module MenuItem =
        [<ReactComponent>]
        let MenuItem
            (input: {| Username: Username
                       DatabaseId: DatabaseId |})
            =
            let (DatabaseId databaseId) = input.DatabaseId

            let isTesting = Recoil.useValue Recoil.Atoms.isTesting
            let selectedPosition, setSelectedPosition = Recoil.useState Recoil.Atoms.selectedPosition
            let (DatabaseName databaseName) = Recoil.useValue (Recoil.Atoms.Database.name (Some input.DatabaseId))
            let databasePosition = Recoil.useValue (Recoil.Atoms.Database.position (Some input.DatabaseId))
            let availableDatabaseIds = Recoil.useValue (Recoil.Atoms.Session.availableDatabaseIds input.Username)

            let selectedDatabaseIds, setSelectedDatabaseIds = Recoil.useState Recoil.Atoms.selectedDatabaseIds

            let availableDatabaseIdsSet = set availableDatabaseIds

            let selectedDatabaseIdsSet =
                selectedDatabaseIds
                |> set
                |> Set.intersect availableDatabaseIdsSet

            let selected = selectedDatabaseIdsSet.Contains input.DatabaseId

            let changeSelection =
                Recoil.useCallbackRef
                    (fun _setter newSelectedDatabaseIds ->
                        setSelectedDatabaseIds newSelectedDatabaseIds

                        match newSelectedDatabaseIds with
                        | [||] -> None
                        | _ -> databasePosition
                        |> setSelectedPosition)

            let onChange =
                fun (e: KeyboardEvent) ->
                    promise {
                        if JS.instanceof (e.target, nameof HTMLInputElement) then
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
                    }

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

            Checkbox.Checkbox
                (fun x ->
                    x.``data-testid`` <- if isTesting then $"menu-item-{databaseId.ToString ()}" else null
                    x.value <- input.DatabaseId
                    x.isChecked <- selected
                    x.isDisabled <- if enabled then false else true
                    x.onChange <- onChange)
                [
                    str databaseName
                ]



    [<ReactComponent>]
    let rec Databases
        (input: {| Username: Username
                   Props: Chakra.IChakraProps |})
        =
        let isTesting = Recoil.useValue Recoil.Atoms.isTesting
        let availableDatabaseIds = Recoil.useValue (Recoil.Atoms.Session.availableDatabaseIds input.Username)

        printfn $"Databases(): availableDatabaseIds.Length={availableDatabaseIds.Length}"

        Chakra.stack
            (fun x ->
                x <+ input.Props
                x.``data-testid`` <- if isTesting then nameof Databases else null)
            [
                yield!
                    availableDatabaseIds
                    |> List.map
                        (fun databaseId ->
                            MenuItem.MenuItem
                                {|
                                    Username = input.Username
                                    DatabaseId = databaseId
                                |})
            ]
