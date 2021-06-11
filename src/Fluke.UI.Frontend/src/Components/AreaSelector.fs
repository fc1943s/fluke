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


module AreaSelector =

    [<ReactComponent>]
    let AreaSelector
        (input: {| Username: UserInteraction.Username
                   Area: Area
                   OnSelect: Area -> unit |})
        =
        let informationSet = Recoil.useValueLoadableDefault (Selectors.Session.informationSet input.Username) Set.empty

        let sortedAreaList =
            React.useMemo (
                (fun () ->
                    informationSet
                    |> Set.addIf
                        (Area input.Area)
                        (input.Area.Name
                         |> AreaName.Value
                         |> String.IsNullOrWhiteSpace
                         |> not)
                    |> Set.toList
                    |> List.choose
                        (function
                        | Area area -> Some area
                        | _ -> None)
                    |> List.sortBy (fun area -> area.Name |> AreaName.Value)),
                [|
                    box informationSet
                    box input.Area
                |]
            )

        let index =
            React.useMemo (
                (fun () ->
                    sortedAreaList
                    |> List.sort
                    |> List.tryFindIndex ((=) input.Area)
                    |> Option.defaultValue -1),
                [|
                    box sortedAreaList
                    box input.Area
                |]
            )

        let isTesting = Recoil.useValue Atoms.isTesting

        Chakra.box
            (fun x ->
                x.display <- "inline"
                if isTesting then x?``data-testid`` <- nameof AreaSelector)
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
                        Label = str "Area"
                        Props = fun x -> x.marginBottom <- "5px"
                    |}

                Menu.Drawer
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
                                                |> Icons.wrap,
                                                Button.IconPosition.Right
                                            )
                                        Props = fun x -> x.onClick <- fun _ -> promise { setVisible (not visible) }
                                        Children =
                                            [
                                                match input.Area.Name |> AreaName.Value with
                                                | String.ValidString name -> str name
                                                | _ -> str "Select..."
                                            ]
                                    |}
                        Body =
                            fun onHide ->
                                [
                                    Chakra.stack
                                        (fun x ->
                                            x.flex <- "1"
                                            x.spacing <- "1px"
                                            x.padding <- "1px"
                                            x.flexDirection <- "column"
                                            x.alignItems <- "stretch"
                                            x.marginBottom <- "6px"
                                            x.maxHeight <- "217px"
                                            x.overflowY <- "auto"
                                            x.flexBasis <- 0)
                                        [
                                            yield!
                                                sortedAreaList
                                                |> List.mapi
                                                    (fun i area ->

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
                                                                                     (fun x -> x.width <- "11px")
                                                                                     [])),
                                                                        Button.IconPosition.Left
                                                                    )
                                                                Props =
                                                                    fun x ->
                                                                        x.onClick <-
                                                                            fun _ ->
                                                                                promise {
                                                                                    input.OnSelect area
                                                                                    onHide ()
                                                                                }

                                                                        x.alignSelf <- "stretch"

                                                                        x.backgroundColor <- "whiteAlpha.100"

                                                                        x.borderRadius <- "2px"
                                                                Children =
                                                                    [
                                                                        area.Name |> AreaName.Value |> str
                                                                    ]
                                                            |})
                                        ]

                                    Menu.Drawer
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
                                                                    |> Icons.wrap,
                                                                    Button.IconPosition.Right
                                                                )
                                                            Props =
                                                                fun x ->
                                                                    x.onClick <-
                                                                        fun _ -> promise { setVisible (not visible) }
                                                            Children =
                                                                [
                                                                    str "Add Area"
                                                                ]
                                                        |}
                                            Body =
                                                fun onHide2 ->
                                                    [
                                                        AreaForm.AreaForm
                                                            {|
                                                                Username = input.Username
                                                                Area = input.Area
                                                                OnSave =
                                                                    fun area ->
                                                                        promise {
                                                                            input.OnSelect area
                                                                            onHide ()
                                                                            onHide2 ()
                                                                        }
                                                            |}
                                                    ]
                                        |}
                                ]
                    |}
            ]
