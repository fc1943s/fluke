namespace rec Fluke.UI.Frontend.Components

open Fable.Core.JsInterop
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
        (input: {| Username: UserInteraction.Username
                   DisableResource: bool
                   SelectionType: InformationSelectionType
                   TaskId: TaskId |})
        =
        let informationFieldOptions =
            Recoil.useAtomFieldOptions
                (Some (Recoil.AtomFamily (input.Username, Atoms.Task.information, (input.Username, input.TaskId))))
                (Some (Recoil.InputScope.ReadWrite Gun.defaultSerializer))

        let informationName, informationSelected =
            React.useMemo (
                (fun () ->
                    informationFieldOptions.AtomValue
                    |> Information.Name
                    |> InformationName.Value,

                    informationFieldOptions.AtomValue
                    |> Information.toString),
                [|
                    box informationFieldOptions.AtomValue
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

        let informationSet = Recoil.useValueLoadableDefault (Selectors.Session.informationSet input.Username) Set.empty

        let sortedInformationList =
            React.useMemo (
                (fun () ->
                    informationSet
                    |> Set.add informationFieldOptions.AtomValue
                    |> Set.filter (isVisibleInformation informationSelected)
                    |> Set.toList),
                [|
                    box informationSelected
                    box informationSet
                    box informationFieldOptions.AtomValue
                |]
            )

        let index =
            React.useMemo (
                (fun () ->
                    sortedInformationList
                    |> List.sort
                    |> List.tryFindIndex ((=) informationFieldOptions.AtomValue)
                    |> Option.defaultValue -1),
                [|
                    box sortedInformationList
                    box informationFieldOptions.AtomValue
                |]
            )

        let isTesting = Recoil.useValue Atoms.isTesting

        Chakra.box
            (fun x ->
                x.display <- "inline"
                if isTesting then x?``data-testid`` <- nameof InformationSelector)
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

                Menu.Drawer
                    {|
                        Tooltip = ""
                        Trigger =
                            fun visible setVisible ->
                                Button.Button
                                    {|
                                        Hint = None
                                        Icon =
                                            Some (
                                                (if visible then Icons.fi.FiChevronUp else Icons.fi.FiChevronDown)
                                                |> Icons.wrap,
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
                                                            if informationFieldOptions.AtomValue
                                                               |> isVisibleInformation radioValueSelected
                                                               |> not then
                                                                match radioValueSelected with
                                                                | nameof Project ->
                                                                    informationFieldOptions.SetAtomValue (
                                                                        Project Project.Default
                                                                    )
                                                                | nameof Area ->
                                                                    informationFieldOptions.SetAtomValue (
                                                                        Area Area.Default
                                                                    )
                                                                | nameof Resource ->
                                                                    informationFieldOptions.SetAtomValue (
                                                                        Resource Resource.Default
                                                                    )
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

                                    match selected, informationSelected with
                                    | false, _ -> None
                                    | _, nameof Project -> Some (TextKey (nameof ProjectForm))
                                    | _, nameof Area -> Some (TextKey (nameof AreaForm))
                                    | _, nameof Resource -> Some (TextKey (nameof AreaForm))
                                    | _ -> None
                                    |> function
                                    | Some formTextKey ->
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
                                                                                     Icons.fi.FiCheck |> Icons.wrap
                                                                                 else
                                                                                     fun () ->
                                                                                         (Chakra.box
                                                                                             (fun x ->
                                                                                                 x.width <- "11px")
                                                                                             [])),
                                                                                Button.IconPosition.Left
                                                                            )
                                                                        Props =
                                                                            fun x ->
                                                                                x.onClick <-
                                                                                    fun _ ->
                                                                                        promise {
                                                                                            informationFieldOptions.SetAtomValue
                                                                                                information

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

                                            Chakra.box
                                                (fun x -> x.textAlign <- "center")
                                                [
                                                    Menu.Drawer
                                                        {|
                                                            Tooltip = ""
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
                                                                                    |> Icons.wrap,
                                                                                    Button.IconPosition.Right
                                                                                )
                                                                            Props =
                                                                                fun x ->
                                                                                    x.onClick <-
                                                                                        fun _ ->
                                                                                            promise {
                                                                                                setVisible (not visible)
                                                                                            //                                                                    TextKey = formTextKey
//                                                                    TextKeyValue = input.TaskId |> TaskId.Value |> Some
                                                                                            }
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
                                                                                {|
                                                                                    Username = input.Username
                                                                                    Project =
                                                                                        match informationFieldOptions.AtomValue with
                                                                                        | Project project -> project
                                                                                        | _ -> Project.Default
                                                                                    OnSave =
                                                                                        fun project ->
                                                                                            promise {
                                                                                                informationFieldOptions.SetAtomValue (
                                                                                                    Project project
                                                                                                )

                                                                                                onHide ()
                                                                                                onHide2 ()
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
                                                                                                onHide2 ()
                                                                                            }
                                                                                |}
                                                                        | nameof Resource -> nothing
                                                                        | _ -> nothing
                                                                    ]
                                                        |}
                                                ]

                                        ]
                                    | _ -> nothing
                                ]
                    |}
            ]
