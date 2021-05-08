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

                    let newDslTemplate =
                        { dslTemplate with
                            Tasks =
                                dslTemplate.Tasks
                                |> List.map
                                    (fun taskTemplate ->
                                        { taskTemplate with
                                            Task =
                                                { taskTemplate.Task with
                                                    Id =
                                                        taskTemplate.Task.Name
                                                        |> TaskName.Value
                                                        |> Crypto.getTextGuidHash
                                                        |> TaskId
                                                }
                                        })
                        }

                    Templates.databaseStateFromDslTemplate
                        Templates.templatesUser
                        databaseId
                        templateName
                        newDslTemplate)

        let databaseStateMap =
            templates
            |> List.map (fun databaseState -> databaseState.Database.Id, databaseState)
            |> Map.ofList

        databaseStateMap
