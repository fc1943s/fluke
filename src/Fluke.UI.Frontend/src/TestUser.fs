namespace Fluke.UI.Frontend

open System
open Feliz.Recoil
open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.Shared.Domain


module TestUser =
    let rec testUser =
        {
            Username = Username "Fluke"
            Color = UserColor.Black
            WeekStart = DayOfWeek.Sunday
            DayStart = FlukeTime.Create 12 0
            SessionLength = Minute 25.
            SessionBreakLength = Minute 5.
        }

    let fetchTemplatesDatabaseStateMap () =
        let templates =
            Templates.getDatabaseMap testUser
            |> Map.toList
            |> List.map
                (fun (templateName, dslTemplate) ->
                    let databaseId =
                        templateName
                        |> Crypto.getTextGuidHash
                        |> DatabaseId

                    Templates.databaseStateFromDslTemplate testUser databaseId templateName dslTemplate)
            |> List.map
                (fun databaseState ->
                    { databaseState with
                        TaskStateMap =
                            databaseState.TaskStateMap
                            |> Map.map
                                (fun { Name = TaskName taskName } taskState ->
                                    { taskState with
                                        TaskId = taskName |> Crypto.getTextGuidHash |> TaskId
                                    })
                    })

        let databaseStateMap =
            templates
            |> List.map (fun databaseState -> databaseState.Database.Id, databaseState)
            |> Map.ofList

        databaseStateMap
