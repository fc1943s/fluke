namespace rec Fluke.UI.Frontend.Components

open System
open Fable.React
open Feliz
open Fluke.Shared.Domain
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

    let isVisibleInformation informationString information =
        match information with
        | information when
            information
            |> Information.Name
            |> InformationName.Value
            |> String.IsNullOrWhiteSpace -> false
        | information when
            information |> Information.isProject
            && informationString = nameof Project -> true
        | information when
            information |> Information.isArea
            && informationString = nameof Area -> true
        | information when
            information |> Information.isResource
            && informationString = nameof Resource -> true
        | _ -> false

    [<ReactComponent>]
    let InformationSelector
        (input: {| DisableResource: bool
                   SelectionType: InformationSelectionType
                   Information: Information
                   OnSelect: Information -> unit |})
        =
        let informationName, informationSelected =
            React.useMemo (
                (fun () ->
                    input.Information
                    |> Information.Name
                    |> InformationName.Value,

                    input.Information |> Information.toString),
                [|
                    box input.Information
                |]
            )

        let selected, setSelected =
            React.useState (
                informationName
                |> String.IsNullOrWhiteSpace
                |> not
            )

        React.useEffect (
            (fun () -> if not selected && informationName.Length > 0 then setSelected true),
            [|
                box selected
                box informationName
                box setSelected
            |]
        )

        let informationSet = Store.useValue Selectors.Session.informationSet

        let sortedInformationList =
            React.useMemo (
                (fun () ->
                    informationSet
                    |> Set.add input.Information
                    |> Set.filter (isVisibleInformation informationSelected)
                    |> Set.toList),
                [|
                    box informationSelected
                    box informationSet
                    box input.Information
                |]
            )

        let index =
            React.useMemo (
                (fun () ->
                    sortedInformationList
                    |> List.sort
                    |> List.tryFindIndex ((=) input.Information)
                    |> Option.defaultValue -1),
                [|
                    box sortedInformationList
                    box input.Information
                |]
            )

        Chakra.box
            (fun x ->
                Chakra.setTestId x (nameof InformationSelector)
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
                        Props = fun x -> x.marginBottom <- "5px"
                    |}

                Dropdown.Dropdown
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
                                                (if visible then Icons.fi.FiChevronUp else Icons.fi.FiChevronDown)
                                                |> Icons.render,
                                                Button.IconPosition.Right
                                            )
                                        Props = fun x -> x.onClick <- fun _ -> promise { setVisible (not visible) }
                                        Children =
                                            [
                                                match informationName with
                                                | String.ValidString _ ->
                                                    str $"{informationSelected}: {informationName}"
                                                | _ -> str "Select..."
                                            ]
                                    |}
                        Body =
                            fun onHide ->
                                [
                                    match input.SelectionType with
                                    | InformationSelectionType.Information ->
                                        Chakra.radioGroup
                                            (fun x ->
                                                x.onChange <-
                                                    fun (radioValueSelected: string) ->
                                                        promise {
                                                            if input.Information
                                                               |> isVisibleInformation radioValueSelected
                                                               |> not then
                                                                match radioValueSelected with
                                                                | nameof Project ->
                                                                    input.OnSelect (Project Project.Default)
                                                                | nameof Area -> input.OnSelect (Area Area.Default)
                                                                | nameof Resource ->
                                                                    input.OnSelect (Resource Resource.Default)
                                                                | _ -> ()

                                                            setSelected true
                                                        }

                                                x.value <- if not selected then null else informationSelected)
                                            [
                                                Chakra.stack
                                                    (fun x ->
                                                        x.justifyContent <- "center"
                                                        x.flex <- "1"
                                                        x.spacing <- "15px"
                                                        x.direction <- "row")
                                                    [
                                                        let label text =
                                                            Chakra.box
                                                                (fun x -> x.marginBottom <- "-2px")
                                                                [
                                                                    str text
                                                                ]

                                                        Radio.Radio
                                                            (fun x -> x.value <- nameof Project)
                                                            [
                                                                label "Project"
                                                            ]

                                                        Radio.Radio
                                                            (fun x -> x.value <- nameof Area)
                                                            [
                                                                label "Area"
                                                            ]

                                                        Radio.Radio
                                                            (fun x ->
                                                                x.value <- nameof Resource
                                                                x.isDisabled <- input.DisableResource)
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

                                    match selected, informationSelected with
                                    | false, _ -> None
                                    | _, nameof Project -> Some ()
                                    | _, nameof Area -> Some ()
                                    | _, nameof Resource -> Some ()
                                    | _ -> None
                                    |> function
                                    | Some () ->
                                        React.fragment [
                                            Chakra.stack
                                                (fun x ->
                                                    x.flex <- "1"
                                                    x.spacing <- "1px"
                                                    x.padding <- "1px"
                                                    x.marginBottom <- "6px"
                                                    x.marginTop <- "10px"
                                                    x.maxHeight <- "217px"
                                                    x.overflowY <- "auto"
                                                    x.flexBasis <- 0)
                                                [
                                                    yield!
                                                        sortedInformationList
                                                        |> List.mapi
                                                            (fun i information ->

                                                                Button.Button
                                                                    {|
                                                                        Hint = None
                                                                        Icon =
                                                                            Some (
                                                                                (if index = i then
                                                                                     Icons.fi.FiCheck
                                                                                     |> Icons.renderChakra
                                                                                         (fun x -> x.marginTop <- "3px")
                                                                                 else
                                                                                     Chakra.box
                                                                                         (fun x -> x.width <- "11px")
                                                                                         []),
                                                                                Button.IconPosition.Left
                                                                            )
                                                                        Props =
                                                                            fun x ->
                                                                                x.onClick <-
                                                                                    fun _ ->
                                                                                        promise {
                                                                                            input.OnSelect information

                                                                                            onHide ()
                                                                                        }

                                                                                x.alignSelf <- "stretch"
                                                                                x.backgroundColor <- "whiteAlpha.100"
                                                                                x.borderRadius <- "2px"
                                                                        Children =
                                                                            [
                                                                                information
                                                                                |> Information.Name
                                                                                |> InformationName.Value
                                                                                |> str
                                                                            ]
                                                                    |})
                                                ]

                                            Dropdown.Dropdown
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
                                                                            |> Icons.render,
                                                                            Button.IconPosition.Right
                                                                        )
                                                                    Props =
                                                                        fun x ->
                                                                            x.onClick <-
                                                                                fun _ ->
                                                                                    promise { setVisible (not visible) }
                                                                    Children =
                                                                        [
                                                                            match informationSelected with
                                                                            | nameof Project -> "Add Project"
                                                                            | nameof Area -> "Add Area"
                                                                            | nameof Resource -> "Add Resource"
                                                                            | _ -> ""
                                                                            |> str
                                                                        ]
                                                                |}
                                                    Body =
                                                        fun onHide2 ->
                                                            [
                                                                match informationSelected with
                                                                | nameof Project ->
                                                                    ProjectForm.ProjectForm
                                                                        (match input.Information with
                                                                         | Project project -> project
                                                                         | _ -> Project.Default)
                                                                        (fun project ->
                                                                            promise {
                                                                                input.OnSelect (Project project)

                                                                                onHide ()
                                                                                onHide2 ()
                                                                            })
                                                                | nameof Area ->
                                                                    AreaForm.AreaForm
                                                                        (match input.Information with
                                                                         | Area area -> area
                                                                         | _ -> Area.Default)
                                                                        (fun area ->
                                                                            promise {
                                                                                input.OnSelect (Area area)

                                                                                onHide ()
                                                                                onHide2 ()
                                                                            })
                                                                | nameof Resource ->
                                                                    ResourceForm.ResourceForm
                                                                        (match input.Information with
                                                                         | Resource resource -> resource
                                                                         | _ -> Resource.Default)
                                                                        (fun resource ->
                                                                            promise {
                                                                                input.OnSelect (Resource resource)

                                                                                onHide ()
                                                                                onHide2 ()
                                                                            })
                                                                | _ -> nothing
                                                            ]
                                                |}
                                        ]
                                    | _ -> nothing
                                ]
                    |}
            ]
