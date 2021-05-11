namespace Fluke.UI.Frontend.Components

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
                   SelectionType: InformationSelectionType
                   TaskId: TaskId option |})
        =
        let informationList = Recoil.useValue (Selectors.Session.informationList input.Username)

        let informationFieldOptions =
            Recoil.useAtomField
                (Some (Recoil.AtomFamily (Atoms.Task.information, input.TaskId)))
                (Some (
                    if input.TaskId.IsNone then
                        Recoil.AtomScope.ReadOnly
                    else
                        Recoil.AtomScope.ReadWrite
                ))

        //        let a =
//            match informationFieldOptions.AtomValue |> Information.toString with
//            | Project project -> ()
//            | Area area -> ()
//            | Resource resource -> ()
//            | Archive _ -> ()
//
        let radioValue, setRadioValue =
            React.useState (
                if input.TaskId.IsNone then
                    None
                else
                    informationFieldOptions.AtomValue
                    |> Information.toString
                    |> Some
            )

        let formParams =
            match radioValue with
            | Some (nameof Project) -> Some (TextKey (nameof AreaForm), "Add Project")
            | Some (nameof Area) -> Some (TextKey (nameof AreaForm), "Add Area")
            | Some (nameof Resource) -> Some (TextKey (nameof AreaForm), "Add Resource")
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
                                    match informationFieldOptions.AtomValue
                                          |> Information.Name
                                          |> InformationName.Value with
                                    | String.ValidString name ->
                                        str
                                            $"{
                                                informationFieldOptions.AtomValue
                                                |> Information.toString
                                            }: {name}"
                                    | _ -> str "Select..."
                                ]
                        Menu =
                            [
                                match input.SelectionType with
                                | InformationSelectionType.Information ->
                                    Chakra.radioGroup
                                        (fun x ->
                                            x.onChange <-
                                                fun (selected: string) -> promise { setRadioValue (Some selected) }

                                            x.value <- radioValue)
                                        [
                                            Chakra.stack
                                                (fun x ->
                                                    x.spacing <- "15px"
                                                    x.direction <- "row")
                                                [
                                                    Chakra.radio
                                                        (fun x ->
                                                            x.colorScheme <- "purple"
                                                            x.value <- nameof Project)
                                                        [
                                                            str "Project"
                                                        ]
                                                    Chakra.radio
                                                        (fun x ->
                                                            x.colorScheme <- "purple"
                                                            x.value <- nameof Area)
                                                        [
                                                            str "Area"
                                                        ]
                                                    Chakra.radio
                                                        (fun x ->
                                                            x.colorScheme <- "purple"
                                                            x.value <- nameof Resource)
                                                        [
                                                            str "Resource"
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
                                                        informationList
                                                        |> List.filter
                                                            (function
                                                            | information when
                                                                information |> Information.isProject
                                                                && radioValue = Some (nameof Project) -> true
                                                            | information when
                                                                information |> Information.isArea
                                                                && radioValue = Some (nameof Area) -> true
                                                            | information when
                                                                information |> Information.isResource
                                                                && radioValue = Some (nameof Resource) -> true
                                                            | _ -> false)
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
                                        ]
                                | _ -> nothing
                            ]
                        MenuListProps = fun x -> x.padding <- "10px"
                    |}
            ]
