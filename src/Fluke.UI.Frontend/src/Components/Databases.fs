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

    let leafIcon locked paused =
        Chakra.stack
            (fun x ->
                x.display <- "inline"
                x.spacing <- "1px"
                x.direction <- "row")
            [
                if locked then
                    Tooltip.wrap
                        (str "Private")
                        [
                            Chakra.box
                                (fun _ -> ())
                                [
                                    Chakra.icon
                                        (fun x ->
                                            x.``as`` <- Icons.fa.FaUserShield
                                            x.color <- "#a4ff8d"
                                            x.marginLeft <- "-3px")
                                        []
                                ]
                        ]
                else
                    Tooltip.wrap
                        (Chakra.box
                            (fun _ -> ())
                            [
                                str "Owner: ?"
                                br []
                                str "Shared with: ?"
                            ])
                        [
                            Chakra.box
                                (fun _ -> ())
                                [
                                    Chakra.icon
                                        (fun x ->
                                            x.``as`` <- Icons.hi.HiUsers
                                            x.color <- "#ffb836"
                                            x.marginLeft <- "-3px")
                                        []
                                ]
                        ]
                if paused then
                    Tooltip.wrap
                        (str "Database paused at position XX")
                        [
                            Chakra.box
                                (fun _ -> ())
                                [
                                    Chakra.icon
                                        (fun x ->
                                            x.``as`` <- Icons.bs.BsPauseFill
                                            x.color <- "#ffb836"
                                            x.marginLeft <- "-3px")
                                        []
                                ]
                        ]
                else
                    Tooltip.wrap
                        (str "Live Database")
                        [
                            Chakra.box
                                (fun _ -> ())
                                [
                                    Chakra.icon
                                        (fun x ->
                                            x.``as`` <- Icons.bs.BsPlayFill
                                            x.color <- "#a4ff8d"
                                            x.marginLeft <- "-3px")
                                        []
                                ]
                        ]
            ]

    let icons =
        {|
            check =
                Chakra.icon
                    (fun x ->
                        x.``as`` <- Icons.md.MdCheckBox
                        x.marginLeft <- "-39px"
                        x.marginTop <- "-1px"
                        x.height <- "17px"
                        x.width <- "17px"
                        x.color <- "white")
                    []
            halfCheck =
                Chakra.icon
                    (fun x ->
                        x.``as`` <- Icons.md.MdIndeterminateCheckBox
                        x.marginLeft <- "-39px"
                        x.marginTop <- "-1px"
                        x.height <- "17px"
                        x.width <- "17px"
                        x.color <- "white")
                    []
            uncheck =
                Chakra.icon
                    (fun x ->
                        x.``as`` <- Icons.md.MdCheckBoxOutlineBlank
                        x.marginLeft <- "-39px"
                        x.marginTop <- "-1px"
                        x.height <- "17px"
                        x.width <- "17px"
                        x.color <- "white")
                    []
            expandOpen =
                Chakra.icon
                    (fun x ->
                        x.``as`` <- Icons.fa.FaChevronDown
                        x.marginTop <- "-5px"
                        x.color <- "white")
                    []
            expandClose =
                Chakra.icon
                    (fun x ->
                        x.``as`` <- Icons.fa.FaChevronRight
                        x.marginTop <- "-5px"
                        x.color <- "white")
                    []
            parentClose =
                Chakra.icon
                    (fun x ->
                        x.``as`` <- Icons.fi.FiDatabase
                        x.marginLeft <- "-3px"
                        x.color <- "white")
                    []
            parentOpen =
                Chakra.icon
                    (fun x ->
                        x.``as`` <- Icons.fi.FiDatabase
                        x.marginLeft <- "-3px"
                        x.color <- "white")
                    []
        |}

    let node value label children icon =
        {|
            value = value
            showCheckbox = value <> ""
            disabled = value = ""
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
            icon = icon
        |}

    type Node = Node of value: string * label: string * children: Node list * index: int option

    let buildNodesFromPath (paths: string list) =
        let rec groupNodes nodes =
            nodes
            |> List.groupBy (fun (Node (_, label, _, _)) -> label)
            |> List.map
                (fun (label, nodes) ->
                    let (Node (value, _, _, index)) = nodes.[0]

                    Node (
                        value,
                        label,
                        (nodes
                         |> List.collect (fun (Node (_, _, children, _)) -> children)
                         |> groupNodes),
                        index
                    ))

        paths
        |> List.map (fun path -> path.Split "/" |> Array.toList)
        |> List.mapi
            (fun i nodes ->
                let rec loop depth list =
                    let fullPath = nodes |> List.take depth |> String.concat "/"
                    let nodeId = fullPath |> Crypto.getTextGuidHash |> string

                    match list with
                    | [ head ] -> Node (nodeId, head, [], Some i)
                    | head :: tail ->
                        Node (
                            nodeId,
                            head,
                            [
                                loop (depth + 1) tail
                            ],
                            None
                        )
                    | [] -> Node ("", "", [], None)

                loop 1 nodes)
        |> groupNodes

    [<ReactComponent>]
    let rec Databases
        (input: {| Username: Username
                   Props: Chakra.IChakraProps |})
        =
        let isTesting = Recoil.useValue Recoil.Atoms.isTesting
        let availableDatabaseIds = Recoil.useValue (Recoil.Atoms.Session.availableDatabaseIds input.Username)

        let availableDatabaseNames =
            availableDatabaseIds
            |> List.map (fun databaseId -> Recoil.Atoms.Database.name (Some databaseId))
            |> Recoil.waitForAll
            |> Recoil.useValue
            |> List.map (fun (DatabaseName databaseName) -> databaseName)

        let availableDatabasePositions =
            availableDatabaseIds
            |> List.map (fun databaseId -> Recoil.Atoms.Database.position (Some databaseId))
            |> Recoil.waitForAll
            |> Recoil.useValue

        let nodes =
            React.useMemo (
                (fun () ->
                    let nodes = buildNodesFromPath availableDatabaseNames

                    let rec loop nodes =
                        match nodes with
                        | Node (value, label, children, index) :: tail ->
                            let nodeChildren =
                                match children with
                                | [] -> JS.undefined
                                | _ -> box (loop children |> List.toArray)

                            let icon =
                                match index with
                                | Some index ->
                                    match availableDatabasePositions.[index] with
                                    | Some _ -> leafIcon false true
                                    | _ -> leafIcon false false
                                | _ -> JS.undefined

                            node value label nodeChildren icon :: (loop tail)
                        | [] -> []

                    loop nodes |> List.toArray),
                [|
                    box availableDatabaseNames
                    box availableDatabasePositions
                |]
            )

        let allNodes =
            [|
                node
                    "templates"
                    "Templates / Unit Tests"
                    [|
                        yield! nodes
                    |]
                    JS.undefined
                node
                    "owned"
                    "Created by Me"
                    [|
                        node "default" "Default" JS.undefined (leafIcon true false)
                        node "fluke" "GitHub: Fluke" JS.undefined (leafIcon false false)
                    |]
                    JS.undefined
                node
                    "shared"
                    "Shared With Me"
                    [|
                        node "" "None" JS.undefined (Chakra.box (fun _ -> ()) [])
                    |]
                    JS.undefined
            |]

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

        let checkboxTreeRef = React.useRef null

        React.useEffect (
            (fun () ->
                if checkboxTreeRef.current <> null then
                    printfn $"CURR ${checkboxTreeRef.current}"
                    Browser.Dom.window?CURR <- checkboxTreeRef.current

                ()),
            [|
                box checkboxTreeRef
            |]
        )

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
                                ref = checkboxTreeRef
                                ``checked`` = ``checked``
                                expanded = expanded
                                onCheck = setChecked
                                onExpand =
                                    fun expanded targetNode ->
                                        Browser.Dom.window?targetNode <- targetNode
                                        printfn $"onExpand. expanded={expanded} targetNode={targetNode}"
                                        setExpanded expanded
                                expandOnClick = true
                                onlyLeafCheckboxes = true
                                nodes = allNodes
                                icons = icons
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
