namespace Fluke.UI.Frontend.Components

open System
open Fable.Core.JsInterop
open Fable.Core
open Fable.Extras
open Fluke.Shared
open Fluke.Shared.Domain.State
open Fluke.Shared.Domain.UserInteraction
open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State


module Databases =
    let icons =
        {|
            check =
                Chakra.icon
                    (fun x ->
                        x.``as`` <- Icons.md.MdCheckBox
                        x.marginLeft <- "-51px"
                        x.height <- "17px"
                        x.width <- "17px"
                        x.color <- "white")
                    []
            halfCheck =
                Chakra.icon
                    (fun x ->
                        x.``as`` <- Icons.md.MdIndeterminateCheckBox
                        x.marginLeft <- "-51px"
                        x.height <- "17px"
                        x.width <- "17px"
                        x.color <- "white")
                    []
            uncheck =
                Chakra.icon
                    (fun x ->
                        x.``as`` <- Icons.md.MdCheckBoxOutlineBlank
                        x.marginLeft <- "-51px"
                        x.height <- "17px"
                        x.width <- "17px"
                        x.color <- "white")
                    []
            expandOpen =
                Chakra.icon
                    (fun x ->
                        x.``as`` <- Icons.fa.FaChevronDown
                        x.transform <- Chakra.transformShiftBy (Some -10) None
                        x.color <- "white")
                    []
            expandClose =
                Chakra.icon
                    (fun x ->
                        x.``as`` <- Icons.fa.FaChevronRight
                        x.transform <- Chakra.transformShiftBy (Some -10) None
                        x.color <- "white")
                    []
            parentClose =
                Chakra.icon
                    (fun x ->
                        x.``as`` <- Icons.fi.FiDatabase
                        x.marginLeft <- "-13px"
                        x.color <- "white")
                    []
            parentOpen =
                Chakra.icon
                    (fun x ->
                        x.``as`` <- Icons.fi.FiDatabase
                        x.marginLeft <- "-13px"
                        x.color <- "white")
                    []
        |}

    type CheckboxTreeNode =
        {
            children: CheckboxTreeNode [] option
            disabled: bool
            icon: obj
            label: ReactElement
            value: string
        }

    let node
        (input: {| Username: Username
                   DatabaseId: DatabaseId option
                   Disabled: bool
                   IsTesting: bool
                   Value: string
                   IsEmptyNode: bool
                   Label: string
                   Children: CheckboxTreeNode []
                   Icon: obj |})
        =
        let disabled = input.Disabled || input.Value = ""

        let labelText =
            if input.Children = JS.undefined then
                Some ((JSe.RegExp @"^(.*? )([^ ]+)$").Match input.Label)
            else
                None

        {
            value = input.Value
            disabled = input.IsEmptyNode || disabled
            children = Some input.Children
            icon =
                if input.IsEmptyNode then
                    box (Chakra.box (fun x -> x.height <- "10px") [])
                else
                    input.Icon
            label =
                Tooltip.wrap
                    (if input.Disabled then
                         (str "There are databases selected with a different position")
                     else
                         nothing)
                    [
                        Chakra.box
                            (fun x ->
                                x.display <- "inline"
                                x.lineHeight <- "19px")
                            [
                                Chakra.box
                                    (fun x ->
                                        x.fontSize <- "main"
                                        x.paddingTop <- "1px"
                                        x.paddingBottom <- "1px"
                                        x.marginLeft <- if input.DatabaseId.IsNone then "2px" else null
                                        x.display <- "inline")
                                    [
                                        match labelText with
                                        | Some label -> str (label |> Seq.item 1)
                                        | _ -> str input.Label
                                    ]

                                match input.DatabaseId with
                                | Some databaseId ->
                                    Chakra.box
                                        (fun x ->
                                            x.display <- "inline"
                                            x.fontSize <- "main"

                                            x.visibility <-
                                                if databaseId = Database.Default.Id then "hidden" else "visible"

                                            x.whiteSpace <- "nowrap")
                                        [
                                            match labelText with
                                            | Some label ->
                                                Chakra.box
                                                    (fun x -> x.display <- "inline")
                                                    [
                                                        str (label |> Seq.item 2)
                                                    ]
                                            | None -> nothing

                                            DatabaseNodeMenu.DatabaseNodeMenu
                                                {|
                                                    Username = input.Username
                                                    DatabaseId = databaseId
                                                    Disabled = disabled
                                                |}
                                        ]
                                | _ -> nothing

                                match input.IsEmptyNode, input.DatabaseId with
                                | _, Some _
                                | true, _ -> Chakra.box (fun x -> x.height <- "14px") []
                                | _ -> nothing
                            ]
                    ]
        }

    [<RequireQualifiedAccess>]
    type NodeValue =
        | Empty
        | Parent of Guid
        | Leaf of Guid

    type Node = Node of value: NodeValue * label: string * children: Node list * index: int option

    let buildNodesFromPath (ids: Guid option list) (paths: string list) =
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
                    match list with
                    | [ String.ValidString head ] ->
                        let id =
                            match ids |> List.tryItem i with
                            | Some (Some id) -> NodeValue.Leaf id
                            | _ -> NodeValue.Empty

                        Node (id, head, [], Some i)
                    | String.ValidString head :: tail ->
                        let fullPath = nodes |> List.take depth |> String.concat "/"
                        let nodeId = fullPath |> Crypto.getTextGuidHash

                        Node (
                            NodeValue.Parent nodeId,
                            head,
                            [
                                loop (depth + 1) tail
                            ],
                            None
                        )
                    | _ -> Node (NodeValue.Empty, "", [], None)

                loop 1 nodes)
        |> groupNodes

    [<RequireQualifiedAccess>]
    type NodeType =
        | Template
        | Owned
        | Shared

    [<ReactComponent>]
    let rec Databases
        (input: {| Username: Username
                   Props: Chakra.IChakraProps -> unit |})
        =
        let isTesting = Recoil.useValue Atoms.isTesting
        let hideTemplates = Recoil.useValue (Atoms.User.hideTemplates input.Username)
        let hideTemplatesCache = React.useRef<bool option> None

        let selectedDatabaseIdSet, setSelectedDatabaseIdSet =
            Recoil.useState (Atoms.User.selectedDatabaseIdSet input.Username)

        let databaseIdSet = Recoil.useValue (Atoms.User.databaseIdSet input.Username)

        let databaseMap =
            databaseIdSet
            |> Set.toList
            |> List.map (fun databaseId -> Selectors.Database.database (input.Username, databaseId))
            |> Recoil.waitForAny
            |> Recoil.useValue
            |> List.choose
                (fun database ->
                    match database.state () with
                    | HasValue database -> Some (database.Id, database)
                    | _ -> None)
            |> Map.ofList

        let nodes =
            React.useMemo (
                (fun () ->
                    let filteredDatabaseMap =
                        databaseMap
                        |> Map.values
                        |> Seq.toList
                        |> List.map
                            (fun database ->
                                let nodeType =
                                    match database.Owner with
                                    | owner when owner = Templates.templatesUser.Username -> NodeType.Template
                                    | owner when owner = input.Username -> NodeType.Owned
                                    | _ -> NodeType.Shared

                                nodeType, database)
                        |> List.filter (fun (nodeType, _) -> nodeType <> NodeType.Template || not hideTemplates)
                        |> List.groupBy fst
                        |> Map.ofList
                        |> Map.map (fun _ v -> v |> List.map snd)

                    let nodeData =
                        [
                            if not hideTemplates then yield NodeType.Template
                            yield NodeType.Owned
                            yield NodeType.Shared
                        ]
                        |> List.collect
                            (fun nodeType ->
                                let newDatabaseNameList =
                                    filteredDatabaseMap
                                    |> Map.tryFind nodeType
                                    |> Option.map (List.map (fun database -> Some database, database.Name))
                                    |> Option.defaultValue [
                                        None, DatabaseName "None"
                                       ]

                                let prefix =
                                    match nodeType with
                                    | NodeType.Template -> "Templates\/Unit Tests"
                                    | NodeType.Owned -> "Created by me"
                                    | NodeType.Shared -> "Shared with me"

                                newDatabaseNameList
                                |> List.map
                                    (fun (database, databaseName) ->
                                        database, $"{prefix}/{databaseName |> DatabaseName.Value}")
                                |> List.sortBy snd)

                    let nodes =
                        let ids =
                            nodeData
                            |> List.map fst
                            |> List.map (Option.map (fun x -> x.Id |> DatabaseId.Value))

                        let paths = nodeData |> List.map snd
                        buildNodesFromPath ids paths

                    let rec loop nodes =
                        match nodes with
                        | Node (value, label, children, index) :: tail ->
                            let nodeChildren =
                                match children with
                                | [] -> unbox JS.undefined
                                | _ -> loop children |> List.toArray

                            let database =
                                match index with
                                | Some index -> nodeData.[index] |> fst
                                | _ -> None

                            let icon =
                                match database with
                                | Some database -> DatabaseLeafIcon.DatabaseLeafIcon input.Username database.Id
                                | _ -> JS.undefined

                            let disabled =
                                match database with
                                | Some database ->
                                    let validSelectedDatabases =
                                        selectedDatabaseIdSet
                                        |> Set.map (fun databaseId -> databaseMap |> Map.tryFind databaseId)

                                    match database.Position with
                                    | Some position ->
                                        validSelectedDatabases
                                        |> Set.exists
                                            (function
                                            | Some database ->
                                                database.Position.IsNone
                                                || database.Position <> (Some position)
                                            | None -> false)
                                    | None ->
                                        validSelectedDatabases
                                        |> Set.exists
                                            (function
                                            | Some database -> database.Position.IsSome
                                            | None -> false)
                                | _ -> false

                            let newValue =
                                database
                                |> Option.map (fun database -> database.Id |> DatabaseId.Value)
                                |> Option.defaultWith
                                    (fun () ->
                                        match value with
                                        | NodeValue.Empty -> None
                                        | NodeValue.Parent guid -> Some guid
                                        | NodeValue.Leaf guid -> Some guid
                                        |> Option.defaultWith (DatabaseId.NewId >> DatabaseId.Value))
                                |> string

                            let isEmptyNode =
                                match value with
                                | NodeValue.Empty _ -> true
                                | _ -> false

                            let newNode =
                                node
                                    {|
                                        Username = input.Username
                                        DatabaseId =
                                            database
                                            |> Option.map (fun database -> database.Id)
                                        Disabled = disabled
                                        IsTesting = isTesting
                                        Value = newValue
                                        IsEmptyNode = isEmptyNode
                                        Label = label
                                        Children = nodeChildren
                                        Icon = icon
                                    |}

                            newNode :: loop tail
                        | [] -> []

                    loop nodes |> List.toArray),
                [|
                    box databaseMap
                    box input.Username
                    box hideTemplates
                    box selectedDatabaseIdSet
                    box isTesting
                |]
            )

        let expandedDatabaseIdSet, setExpandedDatabaseIdSet =
            Recoil.useState (Atoms.User.expandedDatabaseIdSet input.Username)

        let newExpandedDatabaseIdSet =
            if expandedDatabaseIdSet.IsEmpty then
                nodes
                |> Array.map (fun node -> node.value |> Guid |> DatabaseId)
                |> Set.ofArray
            else
                expandedDatabaseIdSet

        React.useEffect (
            (fun () ->
                match hideTemplates, hideTemplatesCache.current with
                | true,
                  (None
                  | Some false) -> setExpandedDatabaseIdSet Set.empty
                | _ -> ()

                hideTemplatesCache.current <- Some hideTemplates),
            [|
                box setExpandedDatabaseIdSet
                box hideTemplates
                box hideTemplatesCache
            |]
        )

        match JS.window id with
        | Some window -> window?nodes <- nodes
        | None -> ()

        Chakra.stack
            input.Props
            [
                Chakra.flex
                    (fun x ->
                        x.marginLeft <- "6px"
                        x.flex <- "1")
                    [
                        CheckboxTree.render
                            {|
                                ``checked`` =
                                    selectedDatabaseIdSet
                                    |> Set.toArray
                                    |> Array.map DatabaseId.Value
                                expanded =
                                    newExpandedDatabaseIdSet
                                    |> Set.toArray
                                    |> Array.map DatabaseId.Value
                                onCheck =
                                    (fun (x: string []) ->
                                        x
                                        |> Array.map (Guid >> DatabaseId)
                                        |> Set.ofArray
                                        |> setSelectedDatabaseIdSet)
                                onExpand =
                                    (fun (x: string []) ->
                                        x
                                        |> Array.map (Guid >> DatabaseId)
                                        |> Set.ofArray
                                        |> setExpandedDatabaseIdSet)
                                expandOnClick = true
                                onlyLeafCheckboxes = true
                                nodes = nodes
                                icons = icons
                            |}
                    ]
            ]
