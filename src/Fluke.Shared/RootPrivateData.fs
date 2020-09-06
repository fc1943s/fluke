namespace Fluke.Shared


open System
open FSharpPlus
open Fluke.Shared
//open Fluke.Shared.PrivateData
open Suigetsu.Core


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

    module TreeData =
        open Model
        open Model.State

        let dslData = PrivateData.Tasks.dslData
        let sharedDslData = SharedPrivateData.SharedTasks.dslData

        let privateInteractions =
            [
                yield! dslData.TaskStateList |> List.collect snd
                yield! PrivateData.InformationCommentInteractions.informationCommentInteractions
                yield! PrivateData.CellCommentInteractions.cellCommentInteractions
                yield! PrivateData.Journal.cellCommentInteractions
                yield! PrivateData.TaskCommentInteractions.taskCommentInteractions
                yield! PrivateData.CellStatusChangeInteractions.cellStatusChangeInteractions
            ]

        let rec ``fc1943s/private`` =
            TreeState.Create
                (TreeId (Guid "8FE2ECF3-0DCB-4933-86B9-13DE90D659F0"),
                 TreeName (nameof ``fc1943s/private``),
                 TempData.Users.fc1943s)
            |> treeStateWithInteractions privateInteractions

        let rec ``liryanne/private`` =
            TreeState.Create
                (TreeId (Guid "A92CCFC3-9BF5-4921-9B1B-4D6787BF9C60"),
                 TreeName (nameof ``liryanne/private``),
                 TempData.Users.liryanne)
            |> treeStateWithInteractions privateInteractions

        let rec ``liryanne/shared`` =
            TreeState.Create
                (TreeId (Guid "9A7A797D-0615-4CF6-B85D-86985978E251"),
                 TreeName (nameof ``liryanne/shared``),
                 TempData.Users.liryanne)
            |> treeStateWithInteractions
                [
                    yield! sharedDslData.TaskStateList |> List.collect snd

                    yield! SharedPrivateData.liryanne.InformationCommentInteractions.informationCommentInteractions
                    yield! SharedPrivateData.fc1943s.InformationCommentInteractions.informationCommentInteractions
                    yield! SharedPrivateData.liryanne.CellCommentInteractions.cellCommentInteractions
                    yield! SharedPrivateData.fc1943s.CellCommentInteractions.cellCommentInteractions
                    yield! SharedPrivateData.liryanne.TaskCommentInteractions.taskCommentInteractions
                    yield! SharedPrivateData.fc1943s.TaskCommentInteractions.taskCommentInteractions
                    yield! SharedPrivateData.liryanne.CellStatusChangeInteractions.cellStatusChangeInteractions
                    yield! SharedPrivateData.fc1943s.CellStatusChangeInteractions.cellStatusChangeInteractions
                ]

        let rec ``fluke/samples/laneRendering/frequency/postponed_until/postponed_until_later`` =
            TreeState.Create
                (id = TreeId (Guid "84998AA3-7262-439F-8E6C-43FDD56A0DD6"),
                 name =
                     TreeName (nameof ``fluke/samples/laneRendering/frequency/postponed_until/postponed_until_later``),
                 owner = TempData.Users.fluke)
            |> treeStateWithInteractions []

        let rec ``fluke/samples/laneSorting/frequency`` =
            TreeState.Create
                (id = TreeId (Guid "46A344F2-2E6C-47DF-A87B-CB2DD326417B"),
                 name = TreeName (nameof ``fluke/samples/laneSorting/frequency``),
                 owner = TempData.Users.fluke)
            |> treeStateWithInteractions []

        let rec ``fluke/samples/laneSorting/timeOfDay`` =
            TreeState.Create
                (id = TreeId (Guid "61897654-D28F-4DCA-8185-D7B9EE83284B"),
                 name = TreeName (nameof ``fluke/samples/laneSorting/timeOfDay``),
                 owner = TempData.Users.fluke)
            |> treeStateWithInteractions []


        let state:TempData.State =
            let privateTreeState =
                [
                    ``fc1943s/private``
                    ``liryanne/private``
                ]
                |> List.find (fun treeState -> treeState.Owner = PrivateData.PrivateData.Consts.currentUser)

            let treeStateMap =
                [
                    privateTreeState, true
                    ``liryanne/shared``, true
                    ``fluke/samples/laneSorting/frequency``, false
                    ``fluke/samples/laneSorting/timeOfDay``, false
                ]
                |> List.map (fun (tree, selected) -> tree.Id, (tree, selected))
                |> Map.ofList

            {
                User = Some PrivateData.PrivateData.Consts.currentUser
                GetLivePosition = TempData.getLivePosition
                TreeMap = treeStateMap
            }



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
//
//
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
