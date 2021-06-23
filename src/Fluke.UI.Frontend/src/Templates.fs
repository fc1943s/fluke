namespace Fluke.UI.Frontend

open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain.State


module TestUser =
    let fetchTemplatesDatabaseStateMap () =
        let templates =
            Templates.getDatabaseMap Templates.templatesUser
            |> Map.toList
            |> List.map
                (fun (templateName, dslTemplate) ->
                    let newDslTemplate =
                        { dslTemplate with
                            Tasks =
                                dslTemplate.Tasks
                                |> List.map
                                    (fun taskTemplate ->
                                        { taskTemplate with
                                            Task =
                                                { taskTemplate.Task with
                                                    Id = TaskId.NewId ()
                                                    Information = Area { Name = AreaName "temp" }
                                                }
                                        })
                        }

                    Templates.databaseStateFromDslTemplate
                        Templates.templatesUser
                        (DatabaseId.NewId ())
                        templateName
                        newDslTemplate)

        let databaseStateMap =
            templates
            |> List.map (fun databaseState -> databaseState.Database.Id, databaseState)
            |> Map.ofSeq

        databaseStateMap
