namespace Fluke.UI.Frontend.Components

open Fable.Core.JsInterop
open Fable.Core
open Fluke.Shared
open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings


module Databases =

    open Domain.UserInteraction
    open Domain.State

    let leafIcon (username: Username) (database: Database) =
        let sharedWith =
            match database.SharedWith with
            | DatabaseAccess.Public -> []
            | DatabaseAccess.Private accessList -> accessList |> List.map DatabaseAccessItem.Value

        let isPrivate =
            match database.SharedWith with
            | DatabaseAccess.Public -> false
            | _ ->
                sharedWith
                |> List.exists (fun share -> share <> username)
                |> not

        Chakra.stack
            (fun x ->
                x.display <- "inline"
                x.spacing <- "4px"
                x.direction <- "row")
            [

                match isPrivate with
                | false ->
                    Tooltip.wrap
                        (Chakra.box
                            (fun _ -> ())
                            [
                                str $"Owner: {database.Owner |> Username.Value}"
                                br []
                                if not sharedWith.IsEmpty then
                                    str
                                        $"""Shared with: {
                                                              sharedWith
                                                              |> List.map Username.Value
                                                              |> String.concat ", "
                                        }"""
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
                | _ ->
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

                match database.Position with
                | Some position ->
                    Tooltip.wrap
                        (str $"Database paused at position {position.Stringify ()}")
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
                | None ->
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

    let node
        (input: {| disabled: bool
                   isTesting: bool
                   value: string
                   label: string
                   children: obj
                   icon: obj |})
        =
        {|
            value = input.value
            showCheckbox = input.value <> ""
            disabled = input.disabled || input.value = ""
            label =
                React.fragment [
                    Chakra.box
                        (fun x ->
                            x?``data-testid`` <- if input.isTesting then $"menu-item-{input.value}" else null
                            x.fontSize <- "12px"
                            x.lineHeight <- "15px"
                            x.marginLeft <- "-6px"
                            x.display <- "inline")
                        [
                            str input.label
                        ]

                    Chakra.box (fun x -> x.marginTop <- "-8px") []
                ]
            children = input.children
            icon = input.icon
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
        |> List.map
            (fun path ->
                path.Replace("\/", "|||").Split "/"
                |> Array.map (fun str -> str.Replace ("|||", "/"))
                |> Array.toList)
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

    [<RequireQualifiedAccess>]
    type NodeType =
        | Template
        | Owned
        | Shared
        | Legacy

    [<ReactComponent>]
    let rec Databases
        (input: {| Username: Username
                   Props: Chakra.IChakraProps |})
        =
        let isTesting = Recoil.useValue Atoms.isTesting
        let availableDatabaseIds = Recoil.useValue (Atoms.Session.availableDatabaseIds input.Username)
        let hideTemplates = Recoil.useValue (Atoms.User.hideTemplates input.Username)

        let selectedDatabaseIds, setSelectedDatabaseIds = Recoil.useState Atoms.selectedDatabaseIds
        let expandedDatabaseIds, setExpandedDatabaseIds = Recoil.useState Atoms.expandedDatabaseIds

        let availableDatabases =
            availableDatabaseIds
            |> List.map Selectors.Database.database
            |> Recoil.waitForAll
            |> Recoil.useValue

        let nodes =
            React.useMemo (
                (fun () ->
                    let availableDatabasesMap =
                        availableDatabases
                        |> List.map (fun database -> database.Id, database)
                        |> Map.ofList

                    let nodes =
                        availableDatabases
                        |> List.map
                            (fun database ->
                                let nodeType =
                                    match database.Owner with
                                    | owner when owner = TempData.testUser.Username -> NodeType.Template
                                    | owner when owner = input.Username -> NodeType.Owned
                                    | _ -> NodeType.Shared

                                nodeType, database)
                        |> List.filter (fun (nodeType, _) -> nodeType <> NodeType.Template || not hideTemplates)
                        |> List.map
                            (fun (nodeType, database) ->
                                let prefix =
                                    match nodeType with
                                    | NodeType.Template -> "Templates\/Unit Tests"
                                    | NodeType.Owned -> "Created by me"
                                    | NodeType.Shared -> "Shared with me"
                                    | NodeType.Legacy -> "Legacy"

                                $"{prefix}/{database.Name |> DatabaseName.Value}")
                        |> buildNodesFromPath

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
                                    let database = availableDatabases.[index]
                                    leafIcon input.Username database
                                | _ -> JS.undefined

                            let disabled =
                                match index with
                                | Some index ->
                                    let validSelectedDatabases =
                                        selectedDatabaseIds
                                        |> Array.map (fun databaseId -> availableDatabasesMap |> Map.tryFind databaseId)

                                    match availableDatabases.[index].Position with
                                    | Some position ->
                                        validSelectedDatabases
                                        |> Array.exists
                                            (function
                                            | Some database ->
                                                database.Position.IsNone
                                                || database.Position <> (Some position)
                                            | None -> false)
                                    | None ->
                                        validSelectedDatabases
                                        |> Array.exists
                                            (function
                                            | Some { Position = Some _ } -> false
                                            | _ -> true)
                                | _ -> false

                            let newValue =
                                match index with
                                | Some index ->
                                    availableDatabases.[index].Id
                                    |> DatabaseId.Value
                                    |> string
                                | None -> value

                            let newNode =
                                node
                                    {|
                                        disabled = disabled
                                        isTesting = isTesting
                                        value = newValue
                                        label = label
                                        children = nodeChildren
                                        icon = icon
                                    |}

                            newNode :: loop tail
                        | [] -> []

                    loop nodes |> List.toArray),
                [|
                    box input.Username
                    box hideTemplates
                    box selectedDatabaseIds
                    box isTesting
                    box availableDatabases
                |]
            )

        Browser.Dom.window?nodes <- nodes

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
                                ``checked`` = selectedDatabaseIds |> Array.map DatabaseId.Value
                                expanded = expandedDatabaseIds |> Array.map DatabaseId.Value
                                onCheck = Array.map DatabaseId >> setSelectedDatabaseIds
                                onExpand = Array.map DatabaseId >> setExpandedDatabaseIds
                                expandOnClick = true
                                onlyLeafCheckboxes = true
                                nodes = nodes
                                icons = icons
                            |}
                    ]
            ]
