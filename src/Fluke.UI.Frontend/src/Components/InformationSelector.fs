namespace rec Fluke.UI.Frontend.Components

open System
open Fable.React
open Feliz
open Fluke.Shared.Domain
open Feliz.Recoil
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open Fluke.Shared


module InformationSelector =
    [<RequireQualifiedAccess>]
    type InformationSelectionType =
        | Information
        | Project
        | Area
        | Resource

    [<ReactComponent>]
    let InformationSelector
        (input: {| Username: UserInteraction.Username
                   DisableResource: bool
                   SelectionType: InformationSelectionType
                   TaskId: TaskId option |})
        =
        let informationList = Recoil.useValue (Selectors.Session.informationList input.Username)
        let newInformation = Recoil.useValue (Atoms.Task.information None)

        let informationFieldOptions =
            Recoil.useAtomField
                (Some (Recoil.AtomFamily (Atoms.Task.information, input.TaskId)))
                (Some (
                    if input.TaskId.IsNone then
                        Recoil.AtomScope.ReadOnly
                    else
                        Recoil.AtomScope.ReadWrite
                ))

        let radioValue, setRadioValue =
            React.useState (
                if informationFieldOptions.AtomValue
                   |> Information.Name
                   |> InformationName.Value
                   |> String.IsNullOrWhiteSpace then
                    ""
                else
                    informationFieldOptions.AtomValue
                    |> Information.toString
            )

        let isVisibleInformation information =
            match information with
            | information when
                information |> Information.isProject
                && radioValue = nameof Project -> true
            | information when
                information |> Information.isArea
                && radioValue = nameof Area -> true
            | information when
                information |> Information.isResource
                && radioValue = nameof Resource -> true
            | _ -> false

        let informationName =
            if isVisibleInformation informationFieldOptions.AtomValue then
                informationFieldOptions.AtomValue
                |> Information.Name
                |> InformationName.Value
            else
                ""

        let sortedInformationList =
            (informationList
             @ [
                 newInformation
             ])
            |> List.filter isVisibleInformation
            |> List.sort

        let formParams =
            match radioValue with
            | nameof Project -> Some (TextKey (nameof ProjectForm), "Add Project")
            | nameof Area -> Some (TextKey (nameof AreaForm), "Add Area")
            | nameof Resource -> Some (TextKey (nameof AreaForm), "Add Resource")
            | _ -> None

        Chakra.stack
            (fun x ->
                x.spacing <- "5px"
                x.display <- "inline")
            [
                InputLabel.InputLabel
                    {|
                        Hint =
                            Some (
                                ExternalLink.ExternalLink
                                    {|
                                        Link = str "Read documentation"
                                        Href = "https://fortelabs.co/blog/para/"
                                        Props = fun _ -> ()
                                    |}
                            )
                        HintTitle = None
                        Label = str "Information"
                        Props = fun _ -> ()
                    |}
                Menu.Menu
                    {|
                        Tooltip = ""
                        Trigger =
                            Chakra.menuButton
                                (fun x ->
                                    x.``as`` <- Chakra.react.Button
                                    x.rightIcon <- Chakra.Icons.chevronDownIcon (fun _ -> ()) [])
                                [
                                    match informationName with
                                    | String.ValidString _ -> str $"{radioValue}: {informationName}"
                                    | _ -> str "Select..."
                                ]
                        Menu =
                            [
                                match input.SelectionType with
                                | InformationSelectionType.Information ->
                                    Chakra.radioGroup
                                        (fun x ->
                                            x.onChange <- fun (selected: string) -> promise { setRadioValue selected }

                                            x.value <- radioValue)
                                        [
                                            Chakra.stack
                                                (fun x ->
                                                    x.spacing <- "15px"
                                                    x.direction <- "row")
                                                [
                                                    let label text =
                                                        Chakra.box
                                                            (fun x -> x.marginBottom <- "-2px")
                                                            [
                                                                str text
                                                            ]

                                                    Chakra.radio
                                                        (fun x ->
                                                            x.colorScheme <- "purple"
                                                            x.value <- nameof Project)
                                                        [
                                                            label "Project"
                                                        ]

                                                    Chakra.radio
                                                        (fun x ->
                                                            x.colorScheme <- "purple"
                                                            x.value <- nameof Area)
                                                        [
                                                            label "Area"
                                                        ]

                                                    Chakra.radio
                                                        (fun x ->
                                                            x.colorScheme <- "purple"
                                                            x.isDisabled <- input.DisableResource
                                                            x.value <- nameof Resource)
                                                        [
                                                            Tooltip.wrap
                                                                (if input.DisableResource then
                                                                     str "Tasks can't be assigned to Resources"
                                                                 else
                                                                     nothing)
                                                                [
                                                                    label "Resource"
                                                                ]
                                                        ]
                                                ]
                                        ]
                                | _ -> nothing

                                match formParams with
                                | Some (formTextKey, addButtonLabel) ->
                                    Chakra.box
                                        (fun x ->
                                            x.marginBottom <- "6px"
                                            x.marginTop <- "10px"
                                            x.maxHeight <- "217px"
                                            x.overflowY <- "auto"
                                            x.flexBasis <- 0)
                                        [
                                            Chakra.menuOptionGroup
                                                (fun x -> x.value <- informationFieldOptions.AtomValue)
                                                [
                                                    yield!
                                                        sortedInformationList
                                                        |> List.map
                                                            (fun information ->
                                                                Chakra.menuItemOption
                                                                    (fun x ->
                                                                        x.value <- information

                                                                        x.onClick <-
                                                                            fun _ ->
                                                                                promise {
                                                                                    informationFieldOptions.SetAtomValue
                                                                                        information
                                                                                })
                                                                    [
                                                                        information
                                                                        |> Information.Name
                                                                        |> InformationName.Value
                                                                        |> str
                                                                    ])
                                                ]
                                        ]

                                    Chakra.box
                                        (fun x -> x.textAlign <- "center")
                                        [
                                            ModalForm.ModalFormTrigger
                                                {|
                                                    Username = input.Username
                                                    Trigger =
                                                        fun trigger ->

                                                            Button.Button
                                                                {|
                                                                    Hint = None
                                                                    Icon =
                                                                        Some (
                                                                            Icons.bs.BsPlus |> Icons.wrap,
                                                                            Button.IconPosition.Left
                                                                        )
                                                                    Props =
                                                                        fun x ->
                                                                            x.onClick <- fun _ -> promise { trigger () }
                                                                    Children =
                                                                        [
                                                                            str addButtonLabel
                                                                        ]
                                                                |}
                                                    TextKey = formTextKey
                                                    TextKeyValue = input.TaskId |> Option.map TaskId.Value
                                                |}

                                            ModalForm.ModalForm
                                                {|
                                                    Username = input.Username
                                                    Content =
                                                        fun (formIdFlag, onHide, _) ->
                                                            let taskId = formIdFlag |> Option.map TaskId


                                                            match radioValue with
                                                            | nameof Project ->
                                                                ProjectForm.ProjectForm
                                                                    {|
                                                                        Username = input.Username
                                                                        TaskId = taskId
                                                                        OnSave =
                                                                            fun project ->
                                                                                promise {
                                                                                    informationFieldOptions.SetAtomValue (
                                                                                        Project project
                                                                                    )

                                                                                    onHide ()
                                                                                }
                                                                    |}
                                                            | nameof Area ->
                                                                AreaForm.AreaForm
                                                                    {|
                                                                        Username = input.Username
                                                                        Area =
                                                                            match informationFieldOptions.AtomValue with
                                                                            | Area area -> area
                                                                            | _ -> Area.Default
                                                                        OnSave =
                                                                            fun area ->
                                                                                promise {
                                                                                    informationFieldOptions.SetAtomValue (
                                                                                        Area area
                                                                                    )

                                                                                    onHide ()
                                                                                }
                                                                    |}
                                                            | nameof Resource -> nothing
                                                            | _ -> nothing
                                                    TextKey = formTextKey
                                                |}
                                        ]
                                | _ -> nothing
                            ]
                        MenuListProps = fun x -> x.padding <- "10px"
                    |}
            ]
