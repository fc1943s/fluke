namespace Fluke.Shared

module RootPrivateData =
    module Shared =
        let informationComments = SharedPrivateData.Data.informationComments
        let taskComments = SharedPrivateData.Data.taskComments
        let cellComments = SharedPrivateData.Data.cellComments
        let cellStatusEntries = SharedPrivateData.Data.cellStatusEntries
        let manualTasks = SharedPrivateData.Data.manualTasks

    let informationComments = PrivateData.InformationComments.informationComments
    let taskComments = PrivateData.TaskComments.taskComments
    let cellComments =
        PrivateData.Journal.journalComments
        |> List.append PrivateData.CellComments.cellComments
    let cellStatusEntries = PrivateData.CellStatusEntries.cellStatusEntries
    let manualTasks = PrivateData.Tasks.tempManualTasks


    let taskOrderList = PrivateData.Tasks.taskOrderList
