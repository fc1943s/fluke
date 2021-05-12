namespace rec Fluke.UI.Frontend.Components

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
        let informationList = Recoil.useValue (Selectors.Session.informationList input.Username)
        let area, setArea = React.useState input.Area

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
                        Label = str "Area"
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
                                    match area.Name |> AreaName.Value with
                                    | String.ValidString name -> str name
                                    | _ -> str "Select..."
                                ]
                        Menu =
                            [
                                Chakra.box
                                    (fun x ->
                                        x.marginBottom <- "6px"
                                        x.maxHeight <- "217px"
                                        x.overflowY <- "auto"
                                        x.flexBasis <- 0)
                                    [
                                        Chakra.menuOptionGroup
                                            (fun x -> x.value <- area)
                                            [
                                                yield!
                                                    informationList
                                                    |> List.map
                                                        (function
                                                        | Area area ->
                                                            Chakra.menuItemOption
                                                                (fun x ->
                                                                    x.value <- area

                                                                    x.onClick <- fun _ -> promise { setArea area })
                                                                [
                                                                    area.Name |> AreaName.Value |> str
                                                                ]
                                                        | _ -> nothing)
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
                                                                        str "Add Area"
                                                                    ]
                                                            |}
                                                TextKey = TextKey (nameof AreaForm)
                                                TextKeyValue = None
                                            |}

                                        ModalForm.ModalForm
                                            {|
                                                Username = input.Username
                                                Content =
                                                    fun (_, onHide, _) ->
                                                        AreaForm.AreaForm
                                                            {|
                                                                Username = input.Username
                                                                Area = area
                                                                OnSave =
                                                                    fun area ->
                                                                        promise {
                                                                            input.OnSelect area
                                                                            onHide ()
                                                                        }
                                                            |}
                                                TextKey = TextKey (nameof AreaForm)
                                            |}
                                    ]
                            ]
                        MenuListProps = fun x -> x.padding <- "10px"
                    |}
            ]
