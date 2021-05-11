namespace Fluke.UI.Frontend.Components

open Fable.React
open Feliz
open Fluke.Shared.Domain
open Feliz.Recoil
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.Bindings
open Fable.Core
open Fluke.UI.Frontend.State
open Fluke.Shared


module AreaForm =

    [<ReactComponent>]
    let AreaForm
        (input: {| Username: UserInteraction.Username
                   TaskId: TaskId option
                   OnSave: Area -> JS.Promise<unit> |})
        =
        let toast = Chakra.useToast ()

        let onSave =
            Recoil.useCallbackRef
                (fun (setter: CallbackMethods) _ ->
                    promise {
                        let! information = setter.snapshot.getReadWritePromise Atoms.Task.information input.TaskId

                        let areaName =
                            information
                            |> Information.Name
                            |> InformationName.Value

                        match areaName with
                        | String.NullString
                        | String.WhitespaceStr -> toast (fun x -> x.description <- "Invalid name")
                        | _ ->
                            let area =
                                { Area.Default with
                                    Name = AreaName areaName
                                }

                            do! setter.readWriteReset Atoms.Task.information input.TaskId

                            do! input.OnSave area
                    })

        Chakra.stack
            (fun x -> x.spacing <- "25px")
            [
                Chakra.box
                    (fun x -> x.fontSize <- "15px")
                    [
                        str "Add Area"
                    ]

                Chakra.stack
                    (fun x -> x.spacing <- "15px")
                    [
                        Input.Input
                            (fun x ->
                                x.autoFocus <- true
                                x.label <- str "Name"
                                x.placeholder <- "e.g. chores"
                                x.atom <- Some (Recoil.AtomFamily (Atoms.Task.information, input.TaskId))
                                x.onFormat <- Some (Information.Name >> InformationName.Value)
                                x.onValidate <- Some (fun name -> Some (Area { Name = AreaName name }))
                                x.onEnterPress <- Some onSave)
                    ]

                Chakra.button
                    (fun x -> x.onClick <- onSave)
                    [
                        str "Save"
                    ]
            ]
