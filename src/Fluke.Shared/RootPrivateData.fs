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

        let private getTreeData (moment: FlukeDateTime) =
            let users = TempData.getUsers ()

            let dslData =
                PrivateData.Tasks.getDslData moment |> fst
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
                    yield! PrivateData.TaskCommentInteractions.getTaskCommentInteractions moment
                    yield! PrivateData.CellStatusChangeInteractions.getCellStatusChangeInteractions moment
                ]

            let mergeDslDataIntoTreeState (dslData: TempData.DslData) (treeState: TreeState) =

                let diag =
                    treeState.TaskStateMap
                        |> Map.tryPick (fun k v -> if k.Name = TaskName "seethrus" then Some v else None)
                match diag with
                | Some diag -> printfn "mergeDslDataIntoTreeState A %A B %A C %A" dslData.TaskStateList.Length treeState.TaskStateMap.Count diag
                | None -> ()

                let newInformationStateMap =
                    TempData.mergeInformationStateMap treeState.InformationStateMap dslData.InformationStateMap

                let taskStateList, userInteractionsBundle = dslData.TaskStateList |> List.unzip

                let newTaskStateMap =
                    (treeState.TaskStateMap, taskStateList)
                    ||> List.fold (fun taskStateMap taskState ->
                            let oldTaskState =
                                treeState.TaskStateMap
                                |> Map.tryFind taskState.Task

                            let newTaskState =
                                match oldTaskState with
                                | Some oldTaskState -> TempData.mergeTaskState oldTaskState taskState
                                | None -> taskState

                            taskStateMap
                            |> Map.add taskState.Task newTaskState)

                let userInteractions =
                    userInteractionsBundle |> List.collect id

                let newTreeState =
                    treeStateWithInteractions userInteractions treeState

                let result =
                    { newTreeState with
                        InformationStateMap = newInformationStateMap
                        TaskStateMap = newTaskStateMap
                    }

                result

            let rec result =
                {|
                    ``fc1943s/private`` =
                        TreeState.Create
                            (id = TreeId (Guid "8FE2ECF3-0DCB-4933-86B9-13DE90D659F0"),
                             name = TreeName (nameof result.``fc1943s/private``),
                             owner = users.fc1943s)
                        |> treeStateWithInteractions privateInteractions
                        |> mergeDslDataIntoTreeState dslData
                    ``liryanne/private`` =
                        TreeState.Create
                            (id = TreeId (Guid "A92CCFC3-9BF5-4921-9B1B-4D6787BF9C60"),
                             name = TreeName (nameof result.``liryanne/private``),
                             owner = users.liryanne)
                        |> treeStateWithInteractions privateInteractions
                    ``liryanne/shared`` =
                        TreeState.Create
                            (id = TreeId (Guid "9A7A797D-0615-4CF6-B85D-86985978E251"),
                             name = TreeName (nameof result.``liryanne/shared``),
                             owner = users.liryanne,
                             sharedWith = [ TreeAccess.Admin users.fc1943s ])
                        |> treeStateWithInteractions [
                            yield! SharedPrivateData.liryanne.InformationCommentInteractions.getInformationCommentInteractions
                                       moment
                            yield! SharedPrivateData.fc1943s.InformationCommentInteractions.getInformationCommentInteractions
                                       moment
                            yield! SharedPrivateData.liryanne.CellCommentInteractions.getCellCommentInteractions moment
                            yield! SharedPrivateData.fc1943s.CellCommentInteractions.getCellCommentInteractions moment
                            yield! SharedPrivateData.liryanne.TaskCommentInteractions.getTaskCommentInteractions moment
                            yield! SharedPrivateData.fc1943s.TaskCommentInteractions.getTaskCommentInteractions moment
                            yield! SharedPrivateData.liryanne.CellStatusChangeInteractions.getCellStatusChangeInteractions
                                       moment
                            yield! SharedPrivateData.fc1943s.CellStatusChangeInteractions.getCellStatusChangeInteractions
                                       moment
                           ]
                        |> mergeDslDataIntoTreeState sharedDslData
                    ``fluke/samples/laneRendering/frequency/postponed_until/postponed_until_later`` =
                        TreeState.Create
                            (id = TreeId (Guid "84998AA3-7262-439F-8E6C-43FDD56A0DD6"),
                             name =
                                 TreeName
                                     (nameof
                                         result.``fluke/samples/laneRendering/frequency/postponed_until/postponed_until_later``),
                             owner = users.fluke)
                        |> treeStateWithInteractions []
                    ``fluke/samples/laneSorting/frequency`` =
                        TreeState.Create
                            (id = TreeId (Guid "46A344F2-2E6C-47DF-A87B-CB2DD326417B"),
                             name = TreeName (nameof result.``fluke/samples/laneSorting/frequency``),
                             owner = users.fluke)
                        |> treeStateWithInteractions []
                    ``fluke/samples/laneSorting/timeOfDay`` =
                        TreeState.Create
                            (id = TreeId (Guid "61897654-D28F-4DCA-8185-D7B9EE83284B"),
                             name = TreeName (nameof result.``fluke/samples/laneSorting/timeOfDay``),
                             owner = users.fluke)
                        |> treeStateWithInteractions []
                |}

            result



        let getState () =
            let consts =
                PrivateData.PrivateData.getPrivateConsts ()

            let user = consts.CurrentUser

            let getLivePosition () = FlukeDateTime.FromDateTime DateTime.Now
            let moment = getLivePosition ()
            let trees = getTreeData moment

            let state: TempData.State =
                let treeStateMap =
                    let privateTreeState =
                        [
                            trees.``fc1943s/private``
                            trees.``liryanne/private``
                        ]
                        |> List.find (fun treeState -> treeState.Owner = user)

                    let result =
                        [
                            privateTreeState, true
                            trees.``liryanne/shared``, true
                            trees.``fluke/samples/laneSorting/frequency``, false
                            trees.``fluke/samples/laneSorting/timeOfDay``, false
                        ]
                        |> List.map (fun (tree, selected) -> tree.Id, (tree, selected))
                        |> Map.ofList

                    result

                {
                    User = Some user
                    GetLivePosition = getLivePosition
                    TreeStateMap = treeStateMap
                }

            let diag =
                state.TreeStateMap
                |> Map.values
                |> Seq.map fst
                |> Seq.map (fun treeState ->
                    treeState.TaskStateMap
                    |> Map.tryPick (fun k v -> if k.Name = TaskName "seethrus" then Some v else None)

                    )
                |> Seq.toList

            printfn "diag1 %A" diag
            state





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
