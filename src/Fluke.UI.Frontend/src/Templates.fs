namespace Fluke.UI.Frontend

open Feliz.Recoil
open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.Bindings


module TestUser =
    let fetchTemplatesDatabaseStateMap () =
        let templates =
            Templates.getDatabaseMap Templates.templatesUser
            |> Map.toList
            |> List.map
                (fun (templateName, dslTemplate) ->
                    let databaseId =
                        templateName
                        |> Crypto.getTextGuidHash
                        |> DatabaseId

                    Templates.databaseStateFromDslTemplate Templates.templatesUser databaseId templateName dslTemplate)
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
