namespace rec Fluke.UI.Frontend.Components

open System
open Fable.React
open Feliz
open Fluke.Shared.Domain.Model
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open Fluke.Shared


module AreaSelector =

    [<ReactComponent>]
    let AreaSelector (area: Area) (onSelect: Area -> unit) =
        let informationSet = Store.useValue Selectors.Session.informationSet

        let sortedAreaList =
            React.useMemo (
                (fun () ->
                    informationSet
                    |> Set.addIf
                        (Area area)
                        (area.Name
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
                    box area
                |]
            )

        let index =
            React.useMemo (
                (fun () ->
                    sortedAreaList
                    |> List.sort
                    |> List.tryFindIndex ((=) area)
                    |> Option.defaultValue -1),
                [|
                    box sortedAreaList
                    box area
                |]
            )

        Chakra.box
            (fun x ->
                Chakra.setTestId x (nameof AreaSelector)
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
                        Label = str "Area"
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
                                                match area.Name |> AreaName.Value with
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
                                                                             Icons.fi.FiCheck
                                                                             |> Icons.renderChakra
                                                                                 (fun x -> x.marginTop <- "3px")
                                                                         else
                                                                             Chakra.box (fun x -> x.width <- "11px") []),
                                                                        Button.IconPosition.Left
                                                                    )
                                                                Props =
                                                                    fun x ->
                                                                        x.onClick <-
                                                                            fun _ ->
                                                                                promise {
                                                                                    onSelect area
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
                                                            area
                                                            (fun area ->
                                                                promise {
                                                                    onSelect area
                                                                    onHide ()
                                                                    onHide2 ()
                                                                })
                                                    ]
                                        |}
                                ]
                    |}
            ]
