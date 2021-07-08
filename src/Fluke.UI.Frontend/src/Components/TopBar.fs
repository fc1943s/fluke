namespace Fluke.UI.Frontend.Components

open Feliz
open Fable.Core.JsInterop
open Fable.React
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks
open Fluke.UI.Frontend.Bindings


module TopBar =
    [<ReactComponent>]
    let TopBar () =
        let deviceInfo = Store.useValue Selectors.deviceInfo
        let logout = Auth.useLogout ()

        let onLogoClick =
            Store.useCallback (
                (fun _ setter _ ->
                    promise {
                        Store.set setter Atoms.User.leftDock None
                        Store.set setter Atoms.User.rightDock None
                        Store.set setter Atoms.User.view UserState.Default.View
                    }),
                [||]
            )

        let onRandom =
            Store.useCallback (
                (fun getter setter _ ->
                    promise {

                        let selectedDatabaseIdSet = Store.value getter Atoms.User.selectedDatabaseIdSet

                        let databaseId = selectedDatabaseIdSet |> JS.randomSeq

                        let informationSet = Store.value getter Selectors.Session.informationSet

                        let information = informationSet |> JS.randomSeq

                        let attachmentIdMap = Store.value getter (Selectors.Information.attachmentIdMap information)

//                        let attachmentId = attachmentIdSet |> JS.randomSeq

//                        let attachment = Store.value getter (Selectors.Attachment.attachment attachmentId)

                        let taskIdAtoms = Store.value getter (Selectors.Database.taskIdAtoms databaseId)

                        ()

                    //                    let taskIdAtoms = Store.value getter (Selectors.Database.taskIdAtoms databaseId)
//
//                    let taskStateList =
//                        taskIdAtoms
//                        |> Array.toList
//                        |> List.map (Store.value getter)
//                        |> List.map Selectors.Task.taskState
//                        |> List.map (Store.value getter)
//
//                    let fileIdList =
//                        taskStateList
//                        |> List.collect
//                            (fun taskState ->
//                                taskState.Attachments
//                                |> List.choose
//                                    (fun (_, attachment) ->
//                                        match attachment with
//                                        | Attachment.Image fileId -> Some fileId
//                                        | _ -> None))
//
//                    let hexStringList =
//                        fileIdList
//                        |> List.map Selectors.File.hexString
//                        |> List.toArray
//                        |> Store.waitForAll
//                        |> Store.value getter
//
//                    if hexStringList |> Array.contains None then
//                        toast (fun x -> x.description <- "Invalid files present")
//                    else
//                        let fileMap =
//                            fileIdList
//                            |> List.mapi (fun i fileId -> fileId, hexStringList.[i].Value)
//                            |> Map.ofList
//
//                        let informationSet = Store.value getter Selectors.Session.informationSet
//
//                        let informationStateMap =
//                            informationSet
//                            |> Set.toList
//                            |> List.map
//                                (fun information ->
//                                    let attachmentIdSet =
//                                        Store.value getter (Selectors.Information.attachmentIdSet information)
//
//                                    let attachments =
//                                        attachmentIdSet
//                                        |> Set.toArray
//                                        |> Array.map Selectors.Attachment.attachment
//                                        |> Store.waitForAll
//                                        |> Store.value getter
//                                        |> Array.toList
//                                        |> List.choose id
//
//                                    information,
//                                    {
//                                        Information = information
//                                        Attachments = attachments
//                                        SortList = []
//                                    })
//                            |> Map.ofSeq
//
//                        let taskStateMap =
//                            taskStateList
//                            |> List.map (fun taskState -> taskState.Task.Id, taskState)
//                            |> Map.ofSeq

                    //                        let databaseState =
//                            {
//                                Database = database
//                                InformationStateMap = informationStateMap
//                                TaskStateMap = taskStateMap
//                                FileMap = fileMap
//                            }
                    }),
                [||]
            )

        UI.flex
            (fun x ->
                x.height <- "29px"
                x.alignItems <- "center"
                x.backgroundColor <- "gray.10"
                //                x.paddingTop <- "7px"
//                x.paddingRight <- "1px"
//                x.paddingBottom <- "8px"
//                x.paddingLeft <- "7px"
                )
            [

                UI.flex
                    (fun x ->
                        x.cursor <- "pointer"
                        x.paddingLeft <- "7px"
                        x.paddingTop <- "6px"
                        x.paddingBottom <- "7px"
                        x.alignItems <- "center"
                        x.onClick <- onLogoClick)
                    [
                        Logo.Logo ()

                        UI.box
                            (fun x -> x.marginLeft <- "5px")
                            [
                                str "Fluke"
                            ]
                    ]

                UI.spacer (fun x -> x.style <- JS.newObj (fun x -> x.WebkitAppRegion <- "drag")) []

                UI.stack
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
                                                                match JS.window id with
                                                                | Some window -> window?api?send "minimize"
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
                                                                match JS.window id with
                                                                | Some window ->
                                                                    window.location.href <- window.location.href
                                                                | None -> ()
                                                            }
                                        |}
                                ]

                        Tooltip.wrap
                            (str "Randomize")
                            [
                                TransparentIconButton.TransparentIconButton
                                    {|
                                        Props =
                                            fun x ->
                                                x.icon <- Icons.bi.BiShuffle |> Icons.render
                                                x.height <- "27px"
                                                x.fontSize <- "17px"

                                                x.onClick <- fun _ -> onRandom ()
                                    |}
                            ]

                        Tooltip.wrap
                            (React.fragment [
                                str "GitHub repository"
                                ExternalLink.externalLinkIcon
                             ])
                            [
                                UI.link
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
                                                                match JS.window id with
                                                                | Some window ->
                                                                    if deviceInfo.IsElectron then
                                                                        window?api?send "close"
                                                                    else
                                                                        window?close "" "_parent" ""
                                                                | None -> ()
                                                            }
                                        |}
                                ]
                    ]
            ]
