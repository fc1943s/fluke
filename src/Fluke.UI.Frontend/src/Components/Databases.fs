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
    let LeafIcon (username: Username) (databaseId: DatabaseId) =
        let owner = Recoil.useValue (Atoms.Database.owner (username, databaseId))
        let sharedWith = Recoil.useValue (Atoms.Database.sharedWith (username, databaseId))
        let position = Recoil.useValue (Atoms.Database.position (username, databaseId))

        let newSharedWith =
            if owner = Templates.templatesUser.Username then
                [
                    username
                ]
            else
                match sharedWith with
                | DatabaseAccess.Public -> []
                | DatabaseAccess.Private accessList -> accessList |> List.map fst

        let isPrivate =
            match sharedWith with
            | DatabaseAccess.Public -> false
            | _ ->
                newSharedWith
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
                                str $"Owner: {owner |> Username.Value}"
                                br []
                                if not newSharedWith.IsEmpty then
                                    str
                                        $"""Shared with: {
                                                              newSharedWith
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

                match position with
                | Some position ->
                    Tooltip.wrap
                        (str $"Database paused at position {position |> FlukeDateTime.Stringify}")
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

    [<ReactComponent>]
    let NodeMenu
        (input: {| Username: Username
                   DatabaseId: DatabaseId
                   Disabled: bool |})
        =
        let isReadWrite = Recoil.useValueLoadableDefault (Selectors.Database.isReadWrite input.DatabaseId) false

        let exportDatabase =
            Recoil.useCallbackRef
                (fun setter ->
                    promise {
                        let! database =
                            setter.snapshot.getPromise (Selectors.Database.database (input.Username, input.DatabaseId))

                        let! dateSequence = setter.snapshot.getPromise Selectors.dateSequence
                        let! taskMetadata = setter.snapshot.getPromise (Selectors.Session.taskMetadata input.Username)

                        let databaseMap =
                            taskMetadata
                            |> Seq.map (fun (KeyValue (taskId, metadata)) -> metadata.DatabaseId, taskId)
                            |> Seq.groupBy fst
                            |> Map.ofSeq
                            |> Map.mapValues (Seq.map snd >> Seq.toList)

                        let dateTaskPairs =
                            dateSequence
                            |> List.map DateId
                            |> List.collect
                                (fun dateId ->
                                    databaseMap.[database.Id]
                                    |> List.map (fun taskId -> dateId, taskId))

                        let! statusList =
                            dateTaskPairs
                            |> List.map (fun (dateId, taskId) -> Selectors.Cell.status (input.Username, taskId, dateId))
                            |> Recoil.waitForAll
                            |> setter.snapshot.getPromise

                        let taskCellStateMap =
                            dateTaskPairs
                            |> List.zip statusList
                            |> List.filter
                                (function
                                | UserStatus _, _ -> true
                                | _ -> false)
                            |> List.map
                                (fun (status, (dateId, taskId)) ->
                                    taskId,
                                    (dateId,
                                     {
                                         Status = status
                                         Attachments = []
                                         Sessions = []
                                     }))
                            |> List.groupBy fst
                            |> Map.ofList
                            |> Map.mapValues (List.map snd >> Map.ofList)

                        let! taskList =
                            databaseMap.[database.Id]
                            |> List.map (fun taskId -> Selectors.Task.task (input.Username, taskId))
                            |> Recoil.waitForAll
                            |> setter.snapshot.getPromise

                        let databaseState =
                            {
                                Database = database
                                InformationStateMap =
                                    taskList
                                    |> List.map
                                        (fun task ->
                                            task.Information,
                                            {
                                                Information = task.Information
                                                Attachments = []
                                                SortList = []
                                            })
                                    |> Map.ofList
                                TaskStateMap =
                                    taskList
                                    |> List.map
                                        (fun task ->
                                            task,
                                            {
                                                Task = task
                                                Sessions = []
                                                Attachments = []
                                                SortList = []
                                                CellStateMap =
                                                    taskCellStateMap
                                                    |> Map.tryFind task.Id
                                                    |> Option.defaultValue Map.empty
                                            })
                                    |> Map.ofList
                            }

                        let json = databaseState |> Gun.jsonEncode

                        let timestamp =
                            (FlukeDateTime.FromDateTime DateTime.Now)
                            |> FlukeDateTime.Stringify

                        JS.download json $"{database.Name |> DatabaseName.Value}-{timestamp}.json" "application/json"
                    })

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
                            TaskFormTrigger.TaskFormTrigger
                                {|
                                    Username = input.Username
                                    DatabaseId = input.DatabaseId
                                    TaskId = None
                                    Trigger =
                                        fun trigger _setter ->
                                            Chakra.menuItem
                                                (fun x ->
                                                    x.icon <-
                                                        Icons.bs.BsPlus
                                                        |> Icons.renderChakra
                                                            (fun x ->
                                                                x.fontSize <- "13px"
                                                                x.marginTop <- "-1px")

                                                    x.onClick <- fun _ -> promise { trigger () })
                                                [
                                                    str "Add Task"
                                                ]
                                |}

                            DatabaseFormTrigger.DatabaseFormTrigger
                                {|
                                    Username = input.Username
                                    DatabaseId = Some input.DatabaseId
                                    Trigger =
                                        fun trigger _setter ->
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
                                                                trigger ()
                                                                ()
                                                            })
                                                [
                                                    str "Edit Database"
                                                ]
                                |}

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

                        Chakra.menuItem
                            (fun x ->
                                x.icon <-
                                    Icons.bi.BiExport
                                    |> Icons.renderChakra
                                        (fun x ->
                                            x.fontSize <- "13px"
                                            x.marginTop <- "-1px")

                                x.onClick <- fun _ -> exportDatabase ())
                            [
                                str "Export Database"
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
                   DatabaseId: DatabaseId option
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
            icon =
                if isEmptyNode then
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

                                            NodeMenu
                                                {|
                                                    Username = input.Username
                                                    DatabaseId = databaseId
                                                    Disabled = disabled
                                                |}
                                        ]
                                | _ -> nothing

                                match isEmptyNode, input.DatabaseId with
                                | _, Some _
                                | true, _ -> Chakra.box (fun x -> x.height <- "14px") []
                                | _ -> nothing
                            ]
                    ]
        }

    type Node = Node of value: string * label: string * children: Node list * index: int option

    let buildNodesFromPath (ids: Guid list) (paths: string list) =
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
                    | [ head ] -> Node (ids.[i] |> string, head, [], Some i)
                    | head :: tail ->
                        let fullPath = nodes |> List.take depth |> String.concat "/"
                        let nodeId = fullPath |> Crypto.getTextGuidHash |> string

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
        let hideTemplates = Recoil.useValue (Atoms.User.hideTemplates input.Username)
        let hideTemplatesCache = React.useRef<bool option> None

        let selectedDatabaseIdList, setSelectedDatabaseIdList =
            Recoil.useState (Atoms.User.selectedDatabaseIdList input.Username)

        let databaseIdSet = Recoil.useValueLoadable (Selectors.Session.databaseIdSet input.Username)

        let databaseIdList =
            databaseIdSet
            |> Recoil.loadableDefault Set.empty
            |> Set.toList

        let databaseNameList =
            databaseIdList
            |> List.map (fun databaseId -> Atoms.Database.name (input.Username, databaseId))
            |> Recoil.waitForAll
            |> Recoil.useValue

        let databaseOwnerList =
            databaseIdList
            |> List.map (fun databaseId -> Atoms.Database.owner (input.Username, databaseId))
            |> Recoil.waitForAll
            |> Recoil.useValue

        let databasePositionList =
            databaseIdList
            |> List.map (fun databaseId -> Atoms.Database.position (input.Username, databaseId))
            |> Recoil.waitForAll
            |> Recoil.useValue

        let nodes =
            React.useMemo (
                (fun () ->
                    let databaseIndexMap =
                        databaseOwnerList
                        |> List.mapi
                            (fun i owner ->
                                let nodeType =
                                    match owner with
                                    | owner when owner = Templates.templatesUser.Username -> NodeType.Template
                                    | owner when owner = input.Username -> NodeType.Owned
                                    | _ -> NodeType.Shared

                                nodeType, i)
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
                                    databaseIndexMap
                                    |> Map.tryFind nodeType
                                    |> Option.map (List.map (fun i -> Some i, databaseNameList.[i]))
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
                                    (fun (i, databaseName) -> i, $"{prefix}/{databaseName |> DatabaseName.Value}")
                                |> List.sortBy snd)

                    let databaseIndexList = nodeData |> List.map fst

                    let databaseIdFromIndex databaseIndex =
                        match databaseIndex with
                        | Some i -> databaseIdList.[i]
                        | None -> Database.Default.Id

                    let databasePositionFromIndex databaseIndex =
                        match databaseIndex with
                        | Some i -> databasePositionList.[i]
                        | _ -> Database.Default.Position

                    let nodes =
                        let ids =
                            nodeData
                            |> List.map fst
                            |> List.map (databaseIdFromIndex >> DatabaseId.Value)

                        let paths = nodeData |> List.map snd
                        buildNodesFromPath ids paths

                    let databaseIndexMap =
                        databaseIndexList
                        |> List.map (fun databaseIndex -> databaseIdFromIndex databaseIndex, databaseIndex)
                        |> Map.ofList

                    let rec loop nodes =
                        match nodes with
                        | Node (value, label, children, index) :: tail ->
                            let nodeChildren =
                                match children with
                                | [] -> unbox JS.undefined
                                | _ -> loop children |> List.toArray

                            let nodeIndex =
                                match index with
                                | Some index -> nodeData.[index] |> fst
                                | _ -> None

                            let databaseId =
                                match nodeIndex with
                                | Some nodeIndex -> databaseIdList |> List.tryItem nodeIndex
                                | _ -> None

                            let icon =
                                match databaseId with
                                | Some databaseId -> LeafIcon input.Username databaseId
                                | _ -> JS.undefined

                            let disabled =
                                match nodeIndex with
                                | Some nodeIndex ->
                                    let validSelectedDatabaseIndexes =
                                        selectedDatabaseIdList
                                        |> List.map (fun databaseId -> databaseIndexMap |> Map.tryFind databaseId)

                                    match databasePositionList.[nodeIndex] with
                                    | Some position ->
                                        validSelectedDatabaseIndexes
                                        |> List.exists
                                            (function
                                            | Some databaseIndex ->
                                                let newPosition = databasePositionFromIndex databaseIndex

                                                newPosition.IsNone
                                                || newPosition <> (Some position)
                                            | None -> false)
                                    | None ->
                                        validSelectedDatabaseIndexes
                                        |> List.exists
                                            (function
                                            | Some databaseIndex -> (databasePositionFromIndex databaseIndex).IsSome
                                            | None -> false)
                                | _ -> false

                            let newValue =
                                match databaseId with
                                | Some databaseId -> databaseId |> DatabaseId.Value |> string
                                | None -> value

                            let newNode =
                                node
                                    {|
                                        Username = input.Username
                                        DatabaseId = databaseId
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
                    box databaseIdList
                    box databaseNameList
                    box databaseOwnerList
                    box databasePositionList
                    box input.Username
                    box hideTemplates
                    box selectedDatabaseIdList
                    box isTesting
                |]
            )

        let expandedDatabaseIdList, setExpandedDatabaseIdList =
            Recoil.useState (Atoms.User.expandedDatabaseIdList input.Username)

        let newExpandedDatabaseIdList =
            match expandedDatabaseIdList with
            | [] ->
                nodes
                |> Array.map (fun node -> node.value |> Guid |> DatabaseId)
                |> Array.toList
            | _ -> expandedDatabaseIdList

        React.useEffect (
            (fun () ->
                match hideTemplates, hideTemplatesCache.current with
                | true,
                  (None
                  | Some false) -> setExpandedDatabaseIdList []
                | _ -> ()

                hideTemplatesCache.current <- Some hideTemplates),
            [|
                box setExpandedDatabaseIdList
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
                        match databaseIdSet.state () with
                        | HasValue _ ->
                            CheckboxTree.render
                                {|
                                    ``checked`` =
                                        selectedDatabaseIdList
                                        |> List.map DatabaseId.Value
                                        |> List.toArray
                                    expanded =
                                        newExpandedDatabaseIdList
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

                        | _ -> LoadingSpinner.LoadingSpinner ()
                    ]
            ]
