namespace Fluke.UI.Frontend.Components

open Fable.Core.JsInterop
open Fable.Core
open Browser.Types
open Fluke.Shared
open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module Databases =

    open Domain.UserInteraction
    open Domain.State

    module MenuItem =
        [<ReactComponent>]
        let MenuItem
            (input: {| Username: Username
                       DatabaseId: DatabaseId |})
            =
            let (DatabaseId databaseId) = input.DatabaseId

            let isTesting = Recoil.useValue Recoil.Atoms.isTesting
            let selectedPosition, setSelectedPosition = Recoil.useState Recoil.Atoms.selectedPosition
            let (DatabaseName databaseName) = Recoil.useValue (Recoil.Atoms.Database.name (Some input.DatabaseId))
            let databasePosition = Recoil.useValue (Recoil.Atoms.Database.position (Some input.DatabaseId))
            let availableDatabaseIds = Recoil.useValue (Recoil.Atoms.Session.availableDatabaseIds input.Username)

            let selectedDatabaseIds, setSelectedDatabaseIds = Recoil.useState Recoil.Atoms.selectedDatabaseIds

            let availableDatabaseIdsSet = set availableDatabaseIds

            let selectedDatabaseIdsSet =
                selectedDatabaseIds
                |> set
                |> Set.intersect availableDatabaseIdsSet

            let selected = selectedDatabaseIdsSet.Contains input.DatabaseId

            let changeSelection =
                Recoil.useCallbackRef
                    (fun _setter newSelectedDatabaseIds ->
                        setSelectedDatabaseIds newSelectedDatabaseIds

                        match newSelectedDatabaseIds with
                        | [||] -> None
                        | _ -> databasePosition
                        |> setSelectedPosition)

            let onChange =
                fun (e: KeyboardEvent) ->
                    promise {
                        if JS.instanceof (e.target, nameof HTMLInputElement) then
                            let swap value set =
                                if set |> Set.contains value then
                                    set |> Set.remove value
                                else
                                    set |> Set.add value

                            let newSelectedDatabaseIds =
                                selectedDatabaseIdsSet
                                |> swap input.DatabaseId
                                |> Set.toArray

                            changeSelection newSelectedDatabaseIds
                    }

            let (|RenderCheckbox|HideCheckbox|) (selectedPosition, databasePosition) =
                match selectedPosition, databasePosition with
                | None, None -> RenderCheckbox
                | None, Some _ when selectedDatabaseIdsSet.IsEmpty -> RenderCheckbox
                | Some _, Some _ when selectedPosition = databasePosition -> RenderCheckbox
                | _ -> HideCheckbox

            let enabled =
                match selectedPosition, databasePosition with
                | RenderCheckbox -> true
                | _ -> false

            Checkbox.Checkbox
                (fun x ->
                    x?``data-testid`` <- if isTesting then $"menu-item-{databaseId.ToString ()}" else null
                    x.value <- input.DatabaseId
                    x.isChecked <- selected
                    x.isDisabled <- if enabled then false else true
                    x.onChange <- onChange)
                [
                    str databaseName
                ]

    let node value label children =
        {|
            value = value
            label =
                React.fragment [
                    Chakra.box
                        (fun x ->
                            x.fontSize <- "12px"
                            x.lineHeight <- "15px"
                            x.marginLeft <- "-6px"
                            x.display <- "inline")
                        [
                            str label
                        ]

                    Chakra.box (fun x -> x.marginTop <- "-8px") []
                ]
            children = children
        |}

    type Node = Node of value: string * label: string * children: Node list

    let buildNodesFromPath (paths: string list) =
        let rec groupNodes nodes =
            nodes
            |> List.groupBy (fun (Node (_, label, _)) -> label)
            |> List.map
                (fun (label, nodes) ->
                    let (Node (value, _, _)) = nodes.[0]

                    Node (
                        value,
                        label,
                        (nodes
                         |> List.collect (fun (Node (_, _, children)) -> children)
                         |> groupNodes)
                    ))

        paths
        |> List.map (fun path -> path.Split "/" |> Array.toList)
        |> List.map
            (fun nodes ->
                let rec loop depth list =
                    let fullPath = nodes |> List.take depth |> String.concat "/"
                    let nodeId = fullPath |> Crypto.getTextGuidHash |> string

                    match list with
                    | [ head ] -> Node (nodeId, head, [])
                    | head :: tail ->
                        Node (
                            nodeId,
                            head,
                            [
                                loop (depth + 1) tail
                            ]
                        )
                    | [] -> Node ("", "", [])

                loop 1 nodes)
        |> groupNodes

    [<ReactComponent>]
    let rec Databases
        (input: {| Username: Username
                   Props: Chakra.IChakraProps |})
        =
        let isTesting = Recoil.useValue Recoil.Atoms.isTesting
        let availableDatabaseIds = Recoil.useValue (Recoil.Atoms.Session.availableDatabaseIds input.Username)
        let availableDatabaseNames = Recoil.useValue (Recoil.Selectors.Session.availableDatabaseNames input.Username)

        let nodes =
            React.useMemo (
                (fun () ->
                    let nodes = buildNodesFromPath availableDatabaseNames

                    let rec loop nodes =
                        match nodes with
                        | Node (value, label, children) :: tail ->
                            let children =
                                match children with
                                | [] -> JS.undefined
                                | _ -> box (loop children |> List.toArray)

                            node value label children :: (loop tail)
                        | [] -> []

                    loop nodes |> List.toArray),
                [|
                    box availableDatabaseNames
                |]
            )

        Browser.Dom.window?nodes <- nodes

        printfn $"Databases(): availableDatabaseIds.Length={availableDatabaseIds.Length}"

        let ``checked``, setChecked = React.useState [||]
        let expanded, setExpanded = React.useState [||]

        printfn
            $"Databases {
                             JS.JSON.stringify
                                 {|
                                     ``checked`` = ``checked``
                                     expanded = expanded
                                 |}
            }"

        Chakra.stack
            (fun x ->
                x <+ input.Props
                x?``data-testid`` <- if isTesting then nameof Databases else null)
            [
                Chakra.box
                    (fun x -> x.margin <- "1px")
                    [

                        CheckboxTree.render
                            {|
                                ``checked`` = ``checked``
                                expanded = expanded
                                onCheck = setChecked
                                onExpand = setExpanded
                                expandOnClick = true
                                onlyLeafCheckboxes = true
                                nodes =
                                    [|
                                        node
                                            "templates"
                                            "Templates / Unit Tests"
                                            [|
                                                yield! nodes
                                            |]
                                        node
                                            "my"
                                            "Created by Me"
                                            [|
                                                node "test11" "test11" JS.undefined
                                                node "test22" "test21" JS.undefined
                                            |]
                                        node
                                            "shared"
                                            "Shared With Me"
                                            [|
                                                node "test111" "test111" JS.undefined
                                                node "test221" "test211" JS.undefined
                                            |]

                                    |]
                                icons =
                                    {|
                                        check =
                                            Chakra.box
                                                (fun x ->
                                                    x.``as`` <- Icons.md.MdCheckBox
                                                    x.marginTop <- "5px"
                                                    x.marginLeft <- "-39px"
                                                    x.height <- "17px"
                                                    x.width <- "17px"
                                                    x.color <- "white")
                                                []
                                        halfCheck =
                                            Chakra.box
                                                (fun x ->
                                                    x.``as`` <- Icons.md.MdIndeterminateCheckBox
                                                    x.marginTop <- "5px"
                                                    x.marginLeft <- "-39px"
                                                    x.height <- "17px"
                                                    x.width <- "17px"
                                                    x.color <- "white")
                                                []
                                        uncheck =
                                            Chakra.box
                                                (fun x ->
                                                    x.``as`` <- Icons.md.MdCheckBoxOutlineBlank
                                                    x.marginLeft <- "-39px"
                                                    x.height <- "17px"
                                                    x.width <- "17px"
                                                    x.color <- "white")
                                                []
                                        expandOpen =
                                            Chakra.box
                                                (fun x ->
                                                    x.``as`` <- Icons.fa.FaChevronDown
                                                    x.marginTop <- "-5px"
                                                    x.color <- "white")
                                                []
                                        expandClose =
                                            Chakra.box
                                                (fun x ->
                                                    x.``as`` <- Icons.fa.FaChevronRight
                                                    x.marginTop <- "-5px"
                                                    x.color <- "white")
                                                []
                                        parentClose =
                                            Chakra.box
                                                (fun x ->
                                                    x.``as`` <- Icons.ai.AiFillFolder
                                                    x.marginLeft <- "-3px"
                                                    x.color <- "white")
                                                []
                                        parentOpen =
                                            Chakra.box
                                                (fun x ->
                                                    x.``as`` <- Icons.ai.AiFillFolderOpen
                                                    x.marginLeft <- "-3px"
                                                    x.color <- "white")
                                                []
                                        leaf =
                                            Chakra.box
                                                (fun x ->
                                                    x.``as`` <- Icons.fi.FiDatabase
                                                    x.marginLeft <- "-3px"
                                                    x.color <- "white")
                                                []
                                    |}
                            |}
                    ]

                yield!
                    availableDatabaseIds
                    |> List.map
                        (fun databaseId ->
                            MenuItem.MenuItem
                                {|
                                    Username = input.Username
                                    DatabaseId = databaseId
                                |})
            ]
