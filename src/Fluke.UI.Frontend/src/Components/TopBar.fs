namespace Fluke.UI.Frontend.Components

open FsCore
open Feliz
open Fable.Core.JsInterop
open Fable.React
open Fluke.Shared.Domain
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks
open Fluke.Shared
open FsJs
open FsStore
open FsUi.Bindings
open Fluke.UI.Frontend.State.State
open FsUi.Components
open FsUi.Hooks


module TopBar =
    [<RequireQualifiedAccess>]
    type RandomType =
        | Project
        | ProjectAttachment
        | Area
        | AreaAttachment
        | Resource
        | ResourceAttachment
        | ProjectTask
        | ProjectTaskAttachment
        | AreaTask
        | AreaTaskAttachment
        | CellAttachment

    [<ReactComponent>]
    let RandomizeButton () =
        let toast = Ui.useToast ()
        let randomizeProject, setRandomizeProject = Store.useState Atoms.User.randomizeProject

        let randomizeProjectAttachment, setRandomizeProjectAttachment =
            Store.useState Atoms.User.randomizeProjectAttachment

        let randomizeArea, setRandomizeArea = Store.useState Atoms.User.randomizeArea
        let randomizeAreaAttachment, setRandomizeAreaAttachment = Store.useState Atoms.User.randomizeAreaAttachment
        let randomizeResource, setRandomizeResource = Store.useState Atoms.User.randomizeResource

        let randomizeResourceAttachment, setRandomizeResourceAttachment =
            Store.useState Atoms.User.randomizeResourceAttachment

        let randomizeProjectTask, setRandomizeProjectTask = Store.useState Atoms.User.randomizeProjectTask
        let randomizeAreaTask, setRandomizeAreaTask = Store.useState Atoms.User.randomizeAreaTask

        let randomizeProjectTaskAttachment, setRandomizeProjectTaskAttachment =
            Store.useState Atoms.User.randomizeProjectTaskAttachment

        let randomizeAreaTaskAttachment, setRandomizeAreaTaskAttachment =
            Store.useState Atoms.User.randomizeAreaTaskAttachment

        let randomizeCellAttachment, setRandomizeCellAttachment = Store.useState Atoms.User.randomizeCellAttachment

        let lastResult, setLastResult = React.useState None

        let onRandom =
            Store.useCallbackRef
                (fun getter setter _ ->
                    promise {
                        let selectedRandomTypeArray =
                            [|
                                if randomizeProject then yield RandomType.Project
                                if randomizeProjectAttachment then yield RandomType.ProjectAttachment
                                if randomizeArea then yield RandomType.Area
                                if randomizeAreaAttachment then yield RandomType.AreaAttachment
                                if randomizeResource then yield RandomType.Resource
                                if randomizeResourceAttachment then
                                    yield RandomType.ResourceAttachment
                                if randomizeProjectTask then yield RandomType.ProjectTask
                                if randomizeAreaTask then yield RandomType.AreaTask
                                if randomizeProjectTaskAttachment then
                                    yield RandomType.ProjectTaskAttachment
                                if randomizeAreaTaskAttachment then
                                    yield RandomType.AreaTaskAttachment
                                if randomizeCellAttachment then yield RandomType.CellAttachment
                            |]

                        let selectedDatabaseIdSet = Store.value getter Atoms.User.selectedDatabaseIdSet

                        let selectedDatabaseIdArray = selectedDatabaseIdSet |> Set.toArray

                        let informationSet = Store.value getter Selectors.Session.informationSet

                        let informationArray = informationSet |> Set.toArray

                        let informationAttachmentIdMapByArchiveArray =
                            selectedDatabaseIdArray
                            |> Array.map Selectors.Database.informationAttachmentIdMapByArchive
                            |> Store.waitForAll
                            |> Store.value getter

                        let databaseTaskIdArray =
                            selectedDatabaseIdArray
                            |> Array.map Selectors.Database.taskIdAtomsByArchive
                            |> Store.waitForAll
                            |> Store.value getter
                            |> Array.map Store.waitForAll
                            |> Store.waitForAll
                            |> Store.value getter

                        let databaseTaskInformationArray =
                            databaseTaskIdArray
                            |> Array.map
                                (fun taskIdArray ->
                                    taskIdArray
                                    |> Array.map Atoms.Task.information
                                    |> Store.waitForAll
                                    |> Store.value getter)

                        let databaseTaskAttachmentIdSetArray =
                            databaseTaskIdArray
                            |> Array.map
                                (fun taskIdArray ->
                                    taskIdArray
                                    |> Array.map Selectors.Task.attachmentIdSet
                                    |> Store.waitForAll
                                    |> Store.value getter)

                        let databaseCellAttachmentIdMapArray =
                            databaseTaskIdArray
                            |> Array.map
                                (fun taskIdArray ->
                                    taskIdArray
                                    |> Array.map Selectors.Task.cellAttachmentIdMap
                                    |> Store.waitForAll
                                    |> Store.value getter)

                        let anchorArray =
                            selectedDatabaseIdArray
                            |> Array.mapi
                                (fun i databaseId ->
                                    let informationAttachmentIdMapByArchive =
                                        informationAttachmentIdMapByArchiveArray.[i]

                                    let getInformationAttachmentArray filterFn =
                                        informationArray
                                        |> Array.filter filterFn
                                        |> Array.collect
                                            (fun information ->
                                                informationAttachmentIdMapByArchive
                                                |> Map.tryFind information
                                                |> Option.defaultValue Set.empty
                                                |> Set.toArray
                                                |> Array.map
                                                    (fun attachmentId ->
                                                        Navigate.Anchor.InformationAttachment (
                                                            information,
                                                            attachmentId
                                                        )))

                                    let taskIdArray = databaseTaskIdArray.[i]
                                    let taskInformationArray = databaseTaskInformationArray.[i]
                                    let taskAttachmentIdSetArray = databaseTaskAttachmentIdSetArray.[i]
                                    let cellAttachmentIdMapArray = databaseCellAttachmentIdMapArray.[i]

                                    let getTaskAttachmentArray filterFn =
                                        taskIdArray
                                        |> Array.indexed
                                        |> Array.filter (fun (i, _) -> taskInformationArray.[i] |> filterFn)
                                        |> Array.map
                                            (fun (i, taskId) ->
                                                taskAttachmentIdSetArray.[i]
                                                |> Set.toArray
                                                |> Array.map
                                                    (fun attachmentId ->
                                                        Navigate.Anchor.TaskAttachment (
                                                            databaseId,
                                                            taskId,
                                                            attachmentId
                                                        )))
                                        |> Array.collect id

                                    let getTaskArray filterFn =
                                        taskIdArray
                                        |> Array.indexed
                                        |> Array.filter (fun (i, _) -> taskInformationArray.[i] |> filterFn)
                                        |> Array.map (fun (_, taskId) -> Navigate.Anchor.Task (databaseId, taskId))

                                    selectedRandomTypeArray
                                    |> Array.collect
                                        (fun randomType ->
                                            match randomType with
                                            | RandomType.Project ->
                                                informationArray
                                                |> Array.filter Information.isProject
                                                |> Array.map Navigate.Anchor.Information
                                            | RandomType.ProjectAttachment ->
                                                getInformationAttachmentArray Information.isProject
                                            | RandomType.Area ->
                                                informationArray
                                                |> Array.filter Information.isArea
                                                |> Array.map Navigate.Anchor.Information
                                            | RandomType.AreaAttachment ->
                                                getInformationAttachmentArray Information.isArea
                                            | RandomType.Resource ->
                                                informationArray
                                                |> Array.filter Information.isResource
                                                |> Array.map Navigate.Anchor.Information
                                            | RandomType.ResourceAttachment ->
                                                getInformationAttachmentArray Information.isResource
                                            | RandomType.ProjectTask -> getTaskArray Information.isProject
                                            | RandomType.AreaTask -> getTaskArray Information.isArea
                                            | RandomType.ProjectTaskAttachment ->
                                                getTaskAttachmentArray Information.isProject
                                            | RandomType.AreaTaskAttachment -> getTaskAttachmentArray Information.isArea
                                            | RandomType.CellAttachment ->
                                                taskIdArray
                                                |> Array.mapi
                                                    (fun i taskId ->
                                                        cellAttachmentIdMapArray.[i]
                                                        |> Map.toArray
                                                        |> Array.collect
                                                            (fun (dateId, attachmentIdSet) ->
                                                                attachmentIdSet
                                                                |> Set.toArray
                                                                |> Array.map
                                                                    (fun attachmentId ->
                                                                        Navigate.Anchor.CellAttachment (
                                                                            taskId,
                                                                            dateId,
                                                                            attachmentId
                                                                        ))))
                                                |> Array.collect id))
                            |> Array.collect id

                        if anchorArray.Length = 0 then
                            toast (fun x -> x.description <- "No data found")
                            return false
                        else
                            let anchor = anchorArray |> Seq.random
                            do! Navigate.navigateAnchor getter setter anchor

                            let title, description = anchor |> Navigate.Anchor.Stringify getter

                            setLastResult (Some (anchorArray.Length, title, description))

                            toast
                                (fun x ->
                                    x.title <- title
                                    x.status <- "success"
                                    x.description <- description)

                            return true
                    })

        Popover.ConfirmPopover
            (Tooltip.wrap
                (str "Randomize")
                [
                    TransparentIconButton.TransparentIconButton
                        {|
                            Props =
                                fun x ->
                                    x.icon <- Icons.bi.BiShuffle |> Icons.render
                                    x.height <- "27px"
                                    x.fontSize <- "17px"
                        |}
                ])
            onRandom
            (fun (_disclosure, _fetchInitialFocusRef) ->
                [
                    Ui.stack
                        (fun x -> x.spacing <- "10px")
                        [
                            Checkbox.Checkbox
                                (Some "Project")
                                (fun x ->
                                    x.isChecked <- randomizeProject
                                    x.onChange <- fun _ -> promise { setRandomizeProject (not randomizeProject) })

                            Checkbox.Checkbox
                                (Some "Project Attachment")
                                (fun x ->
                                    x.isChecked <- randomizeProjectAttachment

                                    x.onChange <-
                                        fun _ ->
                                            promise { setRandomizeProjectAttachment (not randomizeProjectAttachment) })

                            Checkbox.Checkbox
                                (Some "Area")
                                (fun x ->
                                    x.isChecked <- randomizeArea
                                    x.onChange <- fun _ -> promise { setRandomizeArea (not randomizeArea) })

                            Checkbox.Checkbox
                                (Some "Area Attachment")
                                (fun x ->
                                    x.isChecked <- randomizeAreaAttachment

                                    x.onChange <-
                                        fun _ -> promise { setRandomizeAreaAttachment (not randomizeAreaAttachment) })

                            Checkbox.Checkbox
                                (Some "Resource")
                                (fun x ->
                                    x.isChecked <- randomizeResource
                                    x.onChange <- fun _ -> promise { setRandomizeResource (not randomizeResource) })

                            Checkbox.Checkbox
                                (Some "Resource Attachment")
                                (fun x ->
                                    x.isChecked <- randomizeResourceAttachment

                                    x.onChange <-
                                        fun _ ->
                                            promise { setRandomizeResourceAttachment (not randomizeResourceAttachment) })

                            Checkbox.Checkbox
                                (Some "Project Task")
                                (fun x ->
                                    x.isChecked <- randomizeProjectTask

                                    x.onChange <-
                                        fun _ -> promise { setRandomizeProjectTask (not randomizeProjectTask) })

                            Checkbox.Checkbox
                                (Some "Area Task")
                                (fun x ->
                                    x.isChecked <- randomizeAreaTask
                                    x.onChange <- fun _ -> promise { setRandomizeAreaTask (not randomizeAreaTask) })

                            Checkbox.Checkbox
                                (Some "Project Task Attachment")
                                (fun x ->
                                    x.isChecked <- randomizeProjectTaskAttachment

                                    x.onChange <-
                                        fun _ ->
                                            promise {
                                                setRandomizeProjectTaskAttachment (not randomizeProjectTaskAttachment) })

                            Checkbox.Checkbox
                                (Some "Area Task Attachment")
                                (fun x ->
                                    x.isChecked <- randomizeAreaTaskAttachment

                                    x.onChange <-
                                        fun _ ->
                                            promise { setRandomizeAreaTaskAttachment (not randomizeAreaTaskAttachment) })

                            Checkbox.Checkbox
                                (Some "Cell Attachment")
                                (fun x ->
                                    x.isChecked <- randomizeCellAttachment

                                    x.onChange <-
                                        fun _ -> promise { setRandomizeCellAttachment (not randomizeCellAttachment) })

                            match lastResult with
                            | Some (length, title, description) ->
                                Ui.box
                                    (fun _ -> ())
                                    [
                                        str $"Last result (sample size: {length}):"
                                        br []
                                        str $"\t{title}: {description}"
                                    ]
                            | None -> nothing
                        ]
                ])

    [<ReactComponent>]
    let TopBar () =
        let deviceInfo = Store.useValue Selectors.deviceInfo
        let logout = Auth.useLogout ()
        let username = Store.useValue Atoms.username

        let onLogoClick =
            Store.useCallbackRef
                (fun _ setter _ ->
                    promise {
                        Store.set setter Atoms.User.leftDock None
                        Store.set setter Atoms.User.rightDock None
                        Store.set setter Atoms.User.view UserState.Default.View
                    })

        let archive, setArchive = Store.useState Atoms.User.archive

        Ui.flex
            (fun x ->
                x.height <- "29px"
                x.alignItems <- "center"
                x.backgroundColor <- "gray.10")
            [
                Ui.flex
                    (fun x ->
                        x.cursor <- "pointer"
                        x.paddingLeft <- "7px"
                        x.paddingTop <- "6px"
                        x.paddingBottom <- "7px"
                        x.alignItems <- "center"
                        x.onClick <- onLogoClick)
                    [
                        Logo.Logo ()

                        Ui.box
                            (fun x -> x.marginLeft <- "5px")
                            [
                                str "Fluke"
                            ]
                    ]

                Ui.spacer (fun x -> x.style <- Js.newObj (fun x -> x.WebkitAppRegion <- "drag")) []

                Ui.stack
                    (fun x ->
                        x.margin <- "1px"
                        x.spacing <- "1px"
                        x.alignItems <- "center"
                        x.direction <- "row")
                    [
                        if deviceInfo.IsElectron then
                            Tooltip.wrap
                                (str "Minimize")
                                [
                                    TransparentIconButton.TransparentIconButton
                                        {|
                                            Props =
                                                fun x ->
                                                    x.icon <- Icons.fi.FiMinus |> Icons.render
                                                    x.height <- "27px"
                                                    x.fontSize <- "17px"

                                                    x.onClick <-
                                                        fun _ ->
                                                            promise {
                                                                match Dom.window () with
                                                                | Some window -> window?electronApi?send "minimize"
                                                                | None -> ()
                                                            }
                                        |}
                                ]

                            Tooltip.wrap
                                (str "Refresh")
                                [
                                    TransparentIconButton.TransparentIconButton
                                        {|
                                            Props =
                                                fun x ->
                                                    x.icon <- Icons.vsc.VscRefresh |> Icons.render
                                                    x.height <- "27px"
                                                    x.fontSize <- "17px"

                                                    x.onClick <-
                                                        fun _ ->
                                                            promise {
                                                                match Dom.window () with
                                                                | Some window ->
                                                                    window.location.href <- window.location.href
                                                                | None -> ()
                                                            }
                                        |}
                                ]

                        match username with
                        | Some _ -> RandomizeButton ()
                        | _ -> nothing

                        match username with
                        | Some _ ->
                            Tooltip.wrap
                                (str "Toggle Archive")
                                [
                                    TransparentIconButton.TransparentIconButton
                                        {|
                                            Props =
                                                fun x ->
                                                    x.icon <- Icons.ri.RiArchiveLine |> Icons.render
                                                    x.height <- "27px"
                                                    x.fontSize <- "17px"

                                                    if archive = Some true then
                                                        x._active <- Js.newObj (fun x -> x.backgroundColor <- "gray.30")
                                                        x.backgroundColor <- "gray.30"

                                                    x.onClick <-
                                                        fun _ ->
                                                            promise {
                                                                setArchive (
                                                                    match archive with
                                                                    | Some archive -> Some (not archive)
                                                                    | None -> Some false
                                                                )
                                                            }
                                        |}
                                ]
                        | _ -> nothing

                        Tooltip.wrap
                            (React.fragment [
                                str "LessPass"
                                ExternalLink.externalLinkIcon
                             ])
                            [
                                Ui.link
                                    (fun x ->
                                        x.href <- "https://lesspass.com/"
                                        x.isExternal <- true
                                        x.display <- "block")
                                    [
                                        TransparentIconButton.TransparentIconButton
                                            {|
                                                Props =
                                                    fun x ->
                                                        x.tabIndex <- -1
                                                        x.icon <- Icons.cg.CgKeyhole |> Icons.render
                                                        x.fontSize <- "17px"
                                                        x.height <- "27px"
                                            |}
                                    ]
                            ]

                        Tooltip.wrap
                            (React.fragment [
                                str "GitHub repository"
                                ExternalLink.externalLinkIcon
                             ])
                            [
                                Ui.link
                                    (fun x ->
                                        x.href <- "https://github.com/fc1943s/fluke"
                                        x.isExternal <- true
                                        x.display <- "block")
                                    [
                                        TransparentIconButton.TransparentIconButton
                                            {|
                                                Props =
                                                    fun x ->
                                                        x.tabIndex <- -1
                                                        x.icon <- Icons.ai.AiOutlineGithub |> Icons.render
                                                        x.fontSize <- "17px"
                                                        x.height <- "27px"
                                            |}
                                    ]
                            ]

                        if username.IsSome then
                            Tooltip.wrap
                                (str "Logout")
                                [
                                    TransparentIconButton.TransparentIconButton
                                        {|
                                            Props =
                                                fun x ->
                                                    x.icon <- Icons.hi.HiLogout |> Icons.render
                                                    x.fontSize <- "17px"
                                                    x.height <- "27px"
                                                    x.onClick <- fun _ -> promise { do! logout () }
                                        |}
                                ]

                        if deviceInfo.IsElectron then
                            Tooltip.wrap
                                (str "Close")
                                [
                                    TransparentIconButton.TransparentIconButton
                                        {|
                                            Props =
                                                fun x ->
                                                    x.icon <- Icons.vsc.VscChromeClose |> Icons.render
                                                    x.height <- "27px"
                                                    x.fontSize <- "17px"

                                                    x.onClick <-
                                                        fun _ ->
                                                            promise {
                                                                match Dom.window () with
                                                                | Some window ->
                                                                    if deviceInfo.IsElectron then
                                                                        window?electronApi?send "close"
                                                                    else
                                                                        window?close "" "_parent" ""
                                                                | None -> ()
                                                            }
                                        |}
                                ]
                    ]
            ]
