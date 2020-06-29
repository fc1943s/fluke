namespace Fluke.Shared


open FSharpPlus
open Fluke.Shared
open Suigetsu.Core


module RootPrivateData =
    let informationComments =
        PrivateData.InformationComments.informationComments
        |> List.append SharedPrivateData.Data.informationComments

    let cellComments =
        PrivateData.Journal.journalComments
        |> List.append PrivateData.CellComments.cellComments
        |> List.append SharedPrivateData.Data.cellComments

    let taskComments = PrivateData.TaskComments.taskComments
    let sharedTaskComments = SharedPrivateData.Data.taskComments

    let cellStatusEntries = PrivateData.CellStatusEntries.cellStatusEntries
    let sharedCellStatusEntries = SharedPrivateData.Data.cellStatusEntries

    let treeData = PrivateData.Tasks.treeData
    let sharedTreeData = SharedPrivateData.SharedTasks.treeData

    let currentUser = PrivateData.PrivateData.currentUser

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
//                        (flukeDate 2020 Month.June 13), Completed
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
////                    MissedAfter = flukeTime 09 00 |> Some
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
