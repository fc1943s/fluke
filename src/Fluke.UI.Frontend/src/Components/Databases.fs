namespace Fluke.UI.Frontend.Components

open Fable.Core.JsInterop
open Fable.Core
open Fable.Extras
open Fluke.Shared
open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Bindings


module Databases =

    open Domain.UserInteraction
    open Domain.State

    [<ReactComponent>]
    let LeafIcon (username: Username) (database: Database) =
        let sharedWith =
            if database.Owner = TempData.testUser.Username then
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
                   Database: Database |})
        =
        let setDatabaseId = Recoil.useSetState (Atoms.Task.databaseId None)

        let isReadWrite =
            input.Database.Owner <> TempData.testUser.Username
            && (getAccess input.Database input.Username) = Some Access.ReadWrite

        Menu.Menu
            {|
                Title = ""
                Trigger =
                    React.fragment [
                        InputLabelIconButton.InputLabelIconButton
                            {|
                                Props =
                                    JS.newObj
                                        (fun x ->
                                            x.``as`` <- Chakra.react.MenuButton
                                            x.icon <- Icons.bs.BsThreeDots |> Icons.render
                                            x.fontSize <- "10px"
                                            x.marginLeft <- "6px")
                            |}
                    ]
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
                                                                setDatabaseId (Some input.Database.Id)
                                                                trigger ()
                                                            })
                                                [
                                                    str "Add Task"
                                                ]
                                    TextKey = TextKey (nameof TaskForm)
                                    TextKeyValue = None
                                |}

                            Chakra.menuItem
                                (fun x ->

                                    x.icon <-
                                        Icons.bs.BsPen
                                        |> Icons.renderChakra
                                            (fun x ->
                                                x.fontSize <- "13px"
                                                x.marginTop <- "-1px")

                                    x.onClick <- fun e -> promise { e.preventDefault () })
                                [
                                    str "Edit Database"
                                ]

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

                                x.onClick <- fun e -> promise { e.preventDefault () })
                            [
                                str "Clone Database"
                            ]

                    ]
            |}

    let node
        (input: {| Username: Username
                   Database: Database option
                   Disabled: bool
                   IsTesting: bool
                   Value: string
                   Label: string
                   Children: obj
                   Icon: obj |})
        =
        {|
            value = input.Value
            showCheckbox = input.Value <> ""
            disabled = input.Disabled || input.Value = ""
            label =
                React.fragment [
                    let label =
                        if input.Children = JS.undefined then
                            Some ((JSe.RegExp @"^(.*? )([^ ]+)$").Match input.Label)
                        else
                            None

                    Chakra.box
                        (fun x ->
                            x.fontSize <- "12px"
                            x.paddingTop <- "1px"
                            x.paddingBottom <- "1px"
                            x.lineHeight <- "19px"
                            x.marginLeft <- "-6px"
                            x.display <- "inline")
                        [
                            match label with
                            | Some label -> str (label |> Seq.item 1)
                            | None -> str input.Label
                        ]

                    match input.Database with
                    | Some database ->
                        Chakra.box
                            (fun x ->
                                x.display <- "inline"
                                x.fontSize <- "initial"
                                x.whiteSpace <- "nowrap")
                            [
                                match label with
                                | Some label -> str (label |> Seq.item 2)
                                | None -> ()

                                NodeMenu
                                    {|
                                        Username = input.Username
                                        Database = database
                                    |}
                            ]
                    | _ -> ()
                ]
            children = input.Children
            icon = input.Icon
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

        let expandedDatabaseIds, setExpandedDatabaseIds =
            Recoil.useState (Atoms.User.expandedDatabaseIds input.Username)

        let selectedDatabaseIds, setSelectedDatabaseIds =
            Recoil.useState (Atoms.User.selectedDatabaseIds input.Username)

        let availableDatabases =
            availableDatabaseIds
            |> List.map Selectors.Database.database
            |> Recoil.waitForAll
            |> Recoil.useValue

        let nodes =
            React.useMemo (
                (fun () ->
                    let databases =
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

                    let availableDatabasesMap =
                        databases
                        |> List.map (fun (_, database) -> database.Id, database)
                        |> Map.ofList

                    let nodes =
                        databases
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

                            let database =
                                match index with
                                | Some index -> Some (databases.[index] |> snd)
                                | _ -> None

                            let icon =
                                match database with
                                | Some database -> LeafIcon input.Username database
                                | _ -> JS.undefined

                            let disabled =
                                match database with
                                | Some database ->
                                    let validSelectedDatabases =
                                        selectedDatabaseIds
                                        |> Array.map (fun databaseId -> availableDatabasesMap |> Map.tryFind databaseId)

                                    match database.Position with
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
                    box selectedDatabaseIds
                    box isTesting
                    box availableDatabases
                |]
            )

        Browser.Dom.window?nodes <- nodes

        Chakra.stack
            (fun x -> x <+ input.Props)
            [
                Chakra.box
                    (fun x -> x.marginLeft <- "6px")
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
