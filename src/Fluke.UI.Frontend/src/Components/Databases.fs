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

    [<ReactComponent>]
    let LeafIcon (username: Username) (database: Database) =
        let sharedWith =
            if database.Owner = Templates.templatesUser.Username then
                [
                    username
                ]
            else
                match database.SharedWith with
                | DatabaseAccess.Public -> []
                | DatabaseAccess.Private accessList -> accessList |> List.map fst

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
                        x.marginLeft <- "-10px"
                        x.color <- "white")
                    []
            expandClose =
                Chakra.icon
                    (fun x ->
                        x.``as`` <- Icons.fa.FaChevronRight
                        x.marginLeft <- "-10px"
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

    [<ReactComponent>]
    let NodeMenu
        (input: {| Username: Username
                   Database: Database
                   Disabled: bool |})
        =
        let setTaskDatabaseId = Recoil.useSetState (Atoms.Task.databaseId None)

        let isReadWrite =
            input.Database.Owner
            <> Templates.templatesUser.Username
            && (getAccess input.Database input.Username) = Some Access.ReadWrite

        Menu.Menu
            {|
                Tooltip = ""
                Trigger =
                    InputLabelIconButton.InputLabelIconButton
                        {|
                            Props =
                                fun x ->
                                    x.``as`` <- Chakra.react.MenuButton
                                    x.icon <- Icons.bs.BsThreeDots |> Icons.render
                                    x.fontSize <- "11px"
                                    x.disabled <- input.Disabled
                                    x.marginLeft <- "6px"
                        |}
                Menu =
                    [
                        if isReadWrite then
                            ModalForm.ModalFormTrigger
                                {|
                                    Username = input.Username
                                    Trigger =
                                        fun trigger ->
                                            Chakra.menuItem
                                                (fun x ->
                                                    x.icon <-
                                                        Icons.bs.BsPlus
                                                        |> Icons.renderChakra
                                                            (fun x ->
                                                                x.fontSize <- "13px"
                                                                x.marginTop <- "-1px")

                                                    x.onClick <-
                                                        fun _ ->
                                                            promise {
                                                                setTaskDatabaseId input.Database.Id
                                                                trigger ()
                                                                //                                                                let _ = (trigger ())()
                                                                ()
                                                            })
                                                [
                                                    str "Add Task"
                                                ]
                                    TextKey = TextKey (nameof TaskForm)
                                    TextKeyValue = None
                                |}

                            ModalForm.ModalFormTrigger
                                {|
                                    Username = input.Username
                                    Trigger =
                                        fun trigger ->
                                            Chakra.menuItem
                                                (fun x ->
                                                    x.icon <-
                                                        Icons.bs.BsPen
                                                        |> Icons.renderChakra
                                                            (fun x ->
                                                                x.fontSize <- "13px"
                                                                x.marginTop <- "-1px")

                                                    x.onClick <-
                                                        fun _ ->
                                                            promise {
                                                                //                                                                                hydrateDatabase Recoil.AtomScope.ReadWrite input.Database
                                                                trigger ()
                                                                //                                                                                let! setter = (trigger ())()
                                                                ()
                                                            })
                                                [
                                                    str "Edit Database"
                                                ]
                                    TextKey = TextKey (nameof DatabaseForm)
                                    TextKeyValue = input.Database.Id |> DatabaseId.Value |> Some
                                |}

                            Chakra.menuItem
                                (fun x ->
                                    x.icon <-
                                        Icons.bs.BsTrash
                                        |> Icons.renderChakra
                                            (fun x ->
                                                x.fontSize <- "13px"
                                                x.marginTop <- "-1px")

                                    x.onClick <- fun e -> promise { e.preventDefault () })
                                [
                                    str "Delete Database"
                                ]

                        Chakra.menuItem
                            (fun x ->
                                x.icon <-
                                    Icons.fi.FiCopy
                                    |> Icons.renderChakra
                                        (fun x ->
                                            x.fontSize <- "13px"
                                            x.marginTop <- "-1px")

                                x.isDisabled <- true
                                x.onClick <- fun e -> promise { e.preventDefault () })
                            [
                                str "Clone Database"
                            ]
                    ]
                MenuListProps = fun _ -> ()
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
                   Database: Database option
                   Disabled: bool
                   IsTesting: bool
                   Value: string
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

        let isEmptyNode = input.Value = (Database.Default.Id |> DatabaseId.Value |> string)

        {
            value = input.Value
            disabled = isEmptyNode || disabled
            children = Some input.Children
            icon = if isEmptyNode then box (Chakra.box (fun _ -> ()) []) else input.Icon
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
                                        x.fontSize <- "12px"
                                        x.paddingTop <- "1px"
                                        x.paddingBottom <- "1px"
                                        x.marginLeft <- if input.Database.IsNone then "2px" else null
                                        x.display <- "inline")
                                    [
                                        match labelText with
                                        | Some label -> str (label |> Seq.item 1)
                                        | _ -> str input.Label
                                    ]

                                match input.Database with
                                | Some database ->
                                    Chakra.box
                                        (fun x ->
                                            x.display <- "inline"
                                            x.fontSize <- "initial"

                                            x.visibility <-
                                                if database.Id = Database.Default.Id then "hidden" else "visible"

                                            x.whiteSpace <- "nowrap")
                                        [
                                            match labelText with
                                            | Some label -> str (label |> Seq.item 2)
                                            | None -> ()

                                            NodeMenu
                                                {|
                                                    Username = input.Username
                                                    Database = database
                                                    Disabled = disabled
                                                |}
                                        ]

                                    Chakra.box (fun x -> x.height <- "14px") []
                                | _ -> ()

                            ]
                    ]
        }

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

    [<ReactComponent>]
    let rec Databases
        (input: {| Username: Username
                   Props: Chakra.IChakraProps -> unit |})
        =
        let isTesting = Recoil.useValue Atoms.isTesting
        //        let availableDatabaseIds = Recoil.useValue (Atoms.Session.availableDatabaseIds input.Username)
        let hideTemplates = Recoil.useValue (Atoms.User.hideTemplates input.Username)
        let hideTemplatesCache = React.useRef<bool option> None

        let expandedDatabaseIdList, setExpandedDatabaseIdList =
            Recoil.useState (Atoms.User.expandedDatabaseIdList input.Username)

        let selectedDatabaseIdList, setSelectedDatabaseIdList =
            Recoil.useState (Atoms.User.selectedDatabaseIdList input.Username)

        let databaseIdList = Recoil.useValue (Atoms.Session.databaseIdList input.Username)

        let availableDatabases =
            databaseIdList
            |> List.map Selectors.Database.database
            |> Recoil.waitForAll
            |> Recoil.useValue

        let nodes =
            React.useMemo (
                (fun () ->
                    let tasksNodeTypeMap =
                        availableDatabases
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
                        |> List.map
                            (fun nodeType ->
                                let databases =
                                    tasksNodeTypeMap
                                    |> Map.tryFind nodeType
                                    |> Option.defaultValue [
                                        { Database.Default with
                                            Name = DatabaseName "None"
                                        }
                                       ]

                                let prefix =
                                    match nodeType with
                                    | NodeType.Template -> "Templates\/Unit Tests"
                                    | NodeType.Owned -> "Created by me"
                                    | NodeType.Shared -> "Shared with me"

                                let nodeNames =
                                    databases
                                    |> List.map (fun database -> $"{prefix}/{database.Name |> DatabaseName.Value}")
                                    |> List.sort

                                databases, nodeNames)

                    let databases = nodeData |> List.collect fst
                    let nodes = nodeData |> List.collect snd |> buildNodesFromPath

                    let availableDatabasesMap =
                        databases
                        |> List.map (fun database -> database.Id, database)
                        |> Map.ofList

                    let rec loop nodes =
                        match nodes with
                        | Node (value, label, children, index) :: tail ->
                            let nodeChildren =
                                match children with
                                | [] -> unbox JS.undefined
                                | _ -> loop children |> List.toArray

                            let database =
                                match index with
                                | Some index -> Some databases.[index]
                                | _ -> None

                            let icon =
                                match database with
                                | Some database -> LeafIcon input.Username database
                                | _ -> JS.undefined

                            let disabled =
                                match database with
                                | Some database ->
                                    let validSelectedDatabases =
                                        selectedDatabaseIdList
                                        |> List.map (fun databaseId -> availableDatabasesMap |> Map.tryFind databaseId)

                                    match database.Position with
                                    | Some position ->
                                        validSelectedDatabases
                                        |> List.exists
                                            (function
                                            | Some database ->
                                                database.Position.IsNone
                                                || database.Position <> (Some position)
                                            | None -> false)
                                    | None ->
                                        validSelectedDatabases
                                        |> List.exists
                                            (function
                                            | Some { Position = Some _ } -> true
                                            | _ -> false)
                                | _ -> false

                            let newValue =
                                match database with
                                | Some database -> database.Id |> DatabaseId.Value |> string
                                | None -> value

                            let newNode =
                                node
                                    {|
                                        Username = input.Username
                                        Database = database
                                        Disabled = disabled
                                        IsTesting = isTesting
                                        Value = newValue
                                        Label = label
                                        Children = nodeChildren
                                        Icon = icon
                                    |}

                            newNode :: loop tail
                        | [] -> []

                    loop nodes |> List.toArray),
                [|
                    box input.Username
                    box hideTemplates
                    box selectedDatabaseIdList
                    box isTesting
                    box availableDatabases
                |]
            )

        React.useEffect (
            (fun () ->
                let newExpandedDatabaseIdList =
                    match hideTemplates, hideTemplatesCache.current with
                    | true,
                      (None
                      | Some false) -> []
                    | _ -> expandedDatabaseIdList

                hideTemplatesCache.current <- Some hideTemplates

                match newExpandedDatabaseIdList with
                | [] ->
                    setExpandedDatabaseIdList (
                        nodes
                        |> Array.map (fun node -> node.value |> Guid |> DatabaseId)
                        |> Array.toList
                    )
                | _ -> ()),
            [|
                box nodes
                box expandedDatabaseIdList
                box setExpandedDatabaseIdList
                box hideTemplates
                box hideTemplatesCache
            |]
        )

        Browser.Dom.window?nodes <- nodes

        Chakra.stack
            input.Props
            [
                Chakra.box
                    (fun x -> x.marginLeft <- "6px")
                    [
                        CheckboxTree.render
                            {|
                                ``checked`` =
                                    selectedDatabaseIdList
                                    |> List.map DatabaseId.Value
                                    |> List.toArray
                                expanded =
                                    expandedDatabaseIdList
                                    |> List.map DatabaseId.Value
                                    |> List.toArray
                                onCheck =
                                    (fun (x: string []) ->
                                        x
                                        |> Array.map (Guid >> DatabaseId)
                                        |> Array.toList
                                        |> setSelectedDatabaseIdList)
                                onExpand =
                                    (fun (x: string []) ->
                                        x
                                        |> Array.map (Guid >> DatabaseId)
                                        |> Array.toList
                                        |> setExpandedDatabaseIdList)
                                expandOnClick = true
                                onlyLeafCheckboxes = true
                                nodes = nodes
                                icons = icons
                            |}
                    ]
            ]
