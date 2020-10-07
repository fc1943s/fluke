namespace Fluke.Shared


open System
open System.Security.Cryptography
open System.Text
open FSharpPlus
open Fluke.Shared


module RootPrivateData =
    //    type TempDataType =
//        | TempPrivate
//        | TempPublic
//        | Test

    //    let tempDataType = TempPrivate
    //    let tempDataType = Test
//    let tempDataType = TempPublic

    //    let testData =
//        TempData.Testing.tempData.RenderLaneTests
    //    let testData = TempData.Testing.tempData.SortLanesTests

    //    let getLivePosition =
//        match tempDataType with
//        | TempPrivate -> TempData.getLivePosition
//        | TempPublic -> TempData.getLivePosition
//        | Test -> testData.GetLivePosition

    //    let dayStart =
//        match tempDataType with
//        | TempPrivate -> TempData.Consts.dayStart
//        | TempPublic -> TempData.Consts.dayStart
//        | Test -> TempData.Testing.Consts.testDayStart
//
//    let weekStart =
//        match tempDataType with
//        | TempPrivate -> PrivateData.Consts.weekStart
//        | TempPublic -> PrivateData.Consts.weekStart
//        | Test -> PrivateData.Consts.weekStart

    module private Private =

        //        let informationCommentInteractions =
//            match tempDataType with
//            | TempPrivate ->
//                SharedPrivateData.Data.informationCommentInteractions
//                @ InformationCommentInteractions.informationCommentInteractions
//            | TempPublic -> []
//            | Test -> []

        //        let cellCommentInteractions =
//            match tempDataType with
//            | TempPrivate ->
//                SharedPrivateData.Data.cellCommentInteractions
//                @ PrivateData.CellCommentInteractions.cellCommentInteractions
//                @ PrivateData.Journal.cellCommentInteractions
//            | TempPublic -> []
//            | Test -> []

        //        let taskCommentInteractions =
//            match tempDataType with
//            | TempPrivate -> PrivateData.TaskCommentInteractions.taskCommentInteractions
//            | TempPublic -> []
//            | Test -> []
//
//        let sharedTaskCommentInteractions =
//            match tempDataType with
//            | TempPrivate -> SharedPrivateData.Data.taskCommentInteractions
//            | TempPublic -> []
//            | Test -> []

        //        let cellStatusChangeInteractions =
//            match tempDataType with
//            | TempPrivate -> PrivateData.CellStatusChangeInteractions.cellStatusChangeInteractions
//            | TempPublic -> []
//            | Test -> []

        //        let sharedCellStatusChangeInteractions =
//            match tempDataType with
//            | TempPrivate -> SharedPrivateData.Data.cellStatusChangeInteractions
//            | TempPublic -> []
//            | Test -> []


        //    let userInteractions =
//        [
//            yield! Private.informationCommentInteractions
//            yield! Private.cellCommentInteractions
//            yield! Private.taskCommentInteractions
//            yield! Private.sharedTaskCommentInteractions
//            yield! Private.cellStatusChangeInteractions
//            yield! Private.sharedCellStatusChangeInteractions
//        ]


        //    let treeData =
//        match tempDataType with
//        | TempPrivate -> PrivateData.Tasks.treeData
//        | TempPublic -> TempData.Testing.tempData.ManualTasks
//        | Test -> testData

        //    let sharedTreeData =
//        match tempDataType with
//        | TempPrivate -> SharedPrivateData.SharedTasks.treeData
//        | TempPublic ->
//            { treeData with
//                TaskStateList = []
//                InformationList = []
//                TaskOrderList = []
//            }
//        | Test ->
//            { treeData with
//                TaskStateList = []
//                InformationList = []
//                TaskOrderList = []
//            }

        //    let currentUser =
//        PrivateData.PrivateData.Consts.currentUser
        ()

    module State =
        open Domain.Information
        open Domain.UserInteraction
        open Domain.State

        let getTreeStateList (moment: FlukeDateTime) =
            let users = TempData.getUsers ()

            let dslData = PrivateData.Tasks.getDslData moment |> fst
            //                |> TempData.Testing.dslDataToTreeState PrivateData.PrivateData.Consts.currentUser

            let sharedDslData =
                SharedPrivateData.SharedTasks.getDslData moment
                |> fst
            //                |> TempData.Testing.dslDataToTreeState PrivateData.PrivateData.Consts.currentUser

            let privateInteractions =
                [
                    yield! PrivateData.InformationCommentInteractions.getInformationCommentInteractions moment
                    yield! PrivateData.CellCommentInteractions.getCellCommentInteractions moment
                    yield! PrivateData.Journal.getCellCommentInteractions moment
                    yield! PrivateData.CellStatusChangeInteractions.getCellStatusChangeInteractions moment
                ]


            let treeStateList =
                [
                    TreeState.Create (name = TreeName ("fc1943s/private"), owner = users.fc1943s)
                    |> treeStateWithInteractions privateInteractions
                    |> TempData.mergeDslDataIntoTreeState dslData
                    TreeState.Create (name = TreeName ("liryanne/private"), owner = users.liryanne)
                    |> treeStateWithInteractions privateInteractions
                    TreeState.Create
                        (name = TreeName ("liryanne/shared"),
                         owner = users.liryanne,
                         sharedWith =
                             TreeAccess.Private
                                 [
                                     TreeAccessItem.Admin users.fc1943s
                                 ])
                    |> treeStateWithInteractions [
                        yield! SharedPrivateData.liryanne.InformationCommentInteractions.getInformationCommentInteractions
                                   moment
                        yield! SharedPrivateData.fc1943s.InformationCommentInteractions.getInformationCommentInteractions
                                   moment
                        yield! SharedPrivateData.liryanne.CellCommentInteractions.getCellCommentInteractions moment
                        yield! SharedPrivateData.fc1943s.CellCommentInteractions.getCellCommentInteractions moment
                        yield! SharedPrivateData.liryanne.CellStatusChangeInteractions.getCellStatusChangeInteractions
                                   moment
                        yield! SharedPrivateData.fc1943s.CellStatusChangeInteractions.getCellStatusChangeInteractions
                                   moment
                       ]
                    |> TempData.mergeDslDataIntoTreeState sharedDslData

                    yield! Templates.getTreeMap users.fluke
                           |> Map.toList
                           |> List.map (fun (templateName, dslTemplate) ->

                               let dslDataList =
                                   dslTemplate.Tasks
                                   |> List.map (fun templateTask ->
                                       TempData.Testing.createLaneRenderingDslData
                                           {|
                                               User = users.fluke
                                               Position = dslTemplate.Position
                                               Task = templateTask.Task
                                               Events = templateTask.Events
                                           |})

                               let treeState =
                                   TreeState.Create
                                       (name = TreeName templateName,
                                        owner = users.fluke,
                                        position = dslTemplate.Position,
                                        sharedWith = TreeAccess.Public)

                               let newTreeState =
                                   (treeState, dslDataList)
                                   ||> List.fold (fun treeState dslData ->
                                           treeState
                                           |> TempData.mergeDslDataIntoTreeState dslData)

                               newTreeState)
                ]

            let consts = PrivateData.PrivateData.getPrivateConsts ()
            let user = consts.CurrentUser

            let treesWithAccess =
                treeStateList
                |> List.filter (fun treeState ->
                    match treeState with
                    | { Owner = owner } when owner = user -> true
                    | { SharedWith = TreeAccess.Public } -> true
                    | { SharedWith = TreeAccess.Private accessList } ->
                        accessList
                        |> List.exists (function
                            | (TreeAccessItem.Admin user'
                            | TreeAccessItem.ReadOnly user') when user' = user -> true
                            | _ -> false)
                    | _ -> false)

            //            let treeSelection =
//                treesWithAccess
//                |> List.filter (fun treeState ->
//                    treeState.SharedWith <> TreeAccess.Public
//                )
//                |> set

            let result = user, treesWithAccess
            result









//    module Tmp =
//        let informationList =
//            match tempDataType with
//            | TempPrivate -> treeData.InformationList
//            | TempPublic  -> TempData.Testing.tempData.ManualTasks.InformationList
//            | Test        -> []
//
//        let taskOrderList =
//            match tempDataType with
//            | TempPrivate -> treeData.TaskOrderList
//            | TempPublic  -> TempData.Testing.tempData.ManualTasks.TaskOrderList
//            | Test        -> testData.TaskOrderList
//
//        let informationCommentsMap =
//            match tempDataType with
//            | TempPrivate ->
//                informationComments
//                |> List.groupBy (fun x -> x.Information)
//                |> Map.ofList
//                |> Map.mapValues (List.map (fun x -> x.Comment))
//            | TempPublic  -> Map.empty
//            | Test        -> Map.empty



//
//    open Model
//    let tree =
//        let getInformationCommentsMap comments =
//            comments
//            |> List.groupBy (fun x -> x.Information)
//            |> Map.ofList
//            |> Map.mapValues (List.map (fun x -> x.Comment))
//
//        {|
//            Id = TreeId "fc1943s/tree/default"
//            Access = [ TreeAccess.Owner TempData.Users.fc1943s ]
//            InformationList = [
//                let commentsMap =
//                    PrivateData.InformationComments.informationComments
//                    |> List.groupBy (fun x -> x.Information)
//                    |> Map.ofList
//                    |> Map.mapValues (List.map (fun x -> x.Comment))
//                PrivateData.Tasks.treeData.InformationList
//                |> List.map (fun information ->
//                    {|
//                        Information = information
//                        Comments =
//                            commentsMap
//                            |> Map.tryFind information
//                            |> Option.defaultValue []
//                    |}
//                )
//            ]
//            Tasks = [
//                let tmp =
//                    let treeData = PrivateData.Tasks.treeData
//                    let sharedTreeData = SharedPrivateData.Data.treeData
//
//                    let applyState statusEntries comments (taskState: TaskState) =
//                        { taskState with
//                            StatusEntries =
//                                statusEntries
//                                |> createTaskStatusEntries taskState.Task
//                                |> List.prepend taskState.StatusEntries
//                            Comments =
//                                comments
//                                |> List.filter (fun (TaskComment (task, _)) -> task = taskState.Task)
//                                |> List.map (ofTaskComment >> snd)
//                                |> List.prepend taskState.Comments
//                            CellCommentsMap =
//                                cellComments
//                                |> List.filter (fun (CellComment (address, _)) -> address.Task = taskState.Task)
//                                |> List.map (fun (CellComment (address, comment)) -> address.Date, comment)
//                                |> List.groupBy fst
//                                |> Map.ofList
//                                |> Map.mapValues (List.map snd)
//                                |> Map.union taskState.CellCommentsMap }
//
//                    let taskStateList =
//                        treeData.TaskStateList
//                        |> List.map (applyState
//                                         cellStatusEntries
//                                         taskComments)
//
//                    let sharedTaskStateList =
//                        sharedTreeData.TaskStateList
//                        |> List.map (applyState
//                                         Shared.cellStatusEntries
//                                         Shared.taskComments)
//
//                    taskStateList |> List.append sharedTaskStateList
//                {|
//                    Name = "task1"
//                    Information = Area { Name = "area1" }
//                    Scheduling = Recurrency (Offset (Days 1))
//                    PendingAfter = None
//                    MissedAfter = None
//                    Priority = Critical10
//                    Duration = Some 30
//                    Sessions = [
//                        flukeDateTime 2020 Month.May 20 02 05
//                        flukeDateTime 2020 Month.May 31 00 09
//                        flukeDateTime 2020 Month.May 31 23 21
//                        flukeDateTime 2020 Month.June 04 01 16
//                        flukeDateTime 2020 Month.June 20 15 53
//                    ]
//                    Lane = [
//                        (FlukeDate.Create 2020 Month.June 13), Completed
//                    ]
//                    Comments = [
//                        Comment (TempData.Users.fc1943s, "fc1943s: task1 comment")
//                        Comment (TempData.Users.liryanne, "liryanne: task1 comment")
//                    ]
//                |}
////
////                {|
////                    Name = "task2"
////                    Information = Area { Name = "area2" }
////                    Scheduling = Recurrency (Offset (Days 1))
////                    PendingAfter = None
////                    MissedAfter = FlukeTime.Create 09 00 |> Some
////                    Priority = Critical10
////                    Duration = Some 5
////                    Sessions = []
////                    Lane = []
////                    Comments = []
////                |}
//            ]
//        |}
//
////        {|
////            Id = TreeId "liryanne/shared"
////            Access = [ TreeAccess.Owner TempData.Users.liryanne
////                       TreeAccess.Admin TempData.Users.fc1943s ]
////            InformationList = []
////            Tasks = []
////        |}
////
////        {|
////            Id = TreeId "fluke/samples/laneSorting/frequency"
////            Access = [ TreeAccess.ReadOnly TempData.Users.liryanne
////                       TreeAccess.ReadOnly TempData.Users.fc1943s ]
////            InformationList = []
////            Tasks = []
////        |}
////
////        {|
////            Id = TreeId "fluke/samples/laneSorting/timeOfDay"
////            Access = [ TreeAccess.ReadOnly TempData.Users.liryanne
////                       TreeAccess.ReadOnly TempData.Users.fc1943s ]
////            InformationList = []
////            Tasks = []
////        |}














/// Tasks.fs

//    let private manualTaskOrderList : (Information * string) list = []
//
//    let treeData, tasks =
//        let treeData =
//            rawTreeData
//            |> treeDataWithUser currentUser
//            |> transformTreeData Consts.dayStart
////        let taskList = treeData.TaskStateList |> List.map (fun x -> x.Task)
//        let taskList = treeData.TaskList
//
//        let duplicated =
//            taskList
//            |> List.map (fun x -> x.Name)
//            |> List.groupBy id
//            |> List.filter (snd >> List.length >> fun n -> n > 1)
//            |> List.map fst
//
//        if not duplicated.IsEmpty then
//            failwithf "Duplicated task names: %A" duplicated
//
//        let taskMap =
//            taskList
//            |> List.map (fun x -> x.Name, x)
//            |> Map.ofList
//
//        let getTask name =
//            taskMap.[name]
//
//        let tasks = getTaskLinks getTask
//
//        let taskOrderList = getTaskOrderList [] taskList manualTaskOrderList
//
//        let newTreeData = {| treeData with TaskOrderList = taskOrderList |}
//
//        newTreeData, tasks
//
//
//

/// InformationComments.fs
//        |> List.map (fun (information, comment) ->
//            { Information = information
//              Comment = UserComment (currentUser, comment) }
//        )


/// TaskComments.fs
//        |> List.map (fun (task, comment) -> TaskComment (task, UserComment (currentUser, comment)))

/// CellComments.fs
//    let comment comment task date =
//        createCellComment task { Date = date; Time = TempData.Consts.dayStart } PrivateData.currentUser comment

/// CellStatusEntries.fs
//    let entry date task manualCellStatus =
//        CellStatusEntry (PrivateData.currentUser, task, { Date = date; Time = TempData.Consts.dayStart }, manualCellStatus)
//
//    let createEvents oldCellStatusEntries entries =
//        let oldEvents, newEvents = TempData.Events.eventsFromStatusEntries PrivateData.currentUser entries
//        oldEvents |> List.append oldCellStatusEntries, newEvents
