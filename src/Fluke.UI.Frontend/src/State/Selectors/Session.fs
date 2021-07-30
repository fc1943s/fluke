namespace Fluke.UI.Frontend.State.Selectors

open FsCore
open System
open Fluke.Shared
open Fluke.Shared.View
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend
open Fluke.UI.Frontend.State.State
open FsCore.Model
open FsJs
open FsStore
open FsStore.Bindings

#nowarn "40"


module rec Session =
    let rec devicePingList =
        Store.readSelector
            $"{nameof Session}/{nameof devicePingList}"
            (fun getter ->
                let deviceIdArray =
                    Selectors.asyncDeviceIdAtoms
                    |> Store.value getter
                    |> Store.waitForAll
                    |> Store.value getter

                let pingArray =
                    deviceIdArray
                    |> Array.map Atoms.Device.devicePing
                    |> Store.waitForAll
                    |> Store.value getter

                deviceIdArray
                |> Array.toList
                |> List.mapi (fun i deviceId -> deviceId, pingArray.[i]))


    let rec databaseIdAtoms =
        Store.readSelector
            $"{nameof Session}/{nameof databaseIdAtoms}"
            (fun getter ->
                let asyncDatabaseIdAtoms = Store.value getter Selectors.asyncDatabaseIdAtoms
                let hideTemplates = Store.value getter Atoms.User.hideTemplates

                asyncDatabaseIdAtoms
                |> Array.filter
                    (fun databaseIdAtom ->
                        let databaseId = Store.value getter databaseIdAtom
                        let database = Store.value getter (Database.database databaseId)

                        let valid =
                            database.Name
                            |> DatabaseName.ValueOrDefault
                            |> String.IsNullOrWhiteSpace
                            |> not
                            && database.Owner
                               |> Username.ValueOrDefault
                               |> String.IsNullOrWhiteSpace
                               |> not

                        if not valid then
                            false
                        else
                            let nodeType = Store.value getter (Database.nodeType databaseId)

                            nodeType <> DatabaseNodeType.Template
                            || hideTemplates = Some false))


    let rec selectedTaskIdAtoms =
        Store.readSelector
            $"{nameof Session}/{nameof selectedTaskIdAtoms}"
            (fun getter ->
                let selectedDatabaseIdSet = Store.value getter Atoms.User.selectedDatabaseIdSet

                selectedDatabaseIdSet
                |> Set.toArray
                |> Array.map Database.taskIdAtoms
                |> Store.waitForAll
                |> Store.value getter
                |> Array.collect id)


    let rec selectedTaskIdListByArchive =
        Store.readSelector
            $"{nameof Session}/{nameof selectedTaskIdListByArchive}"
            (fun getter ->
                let selectedDatabaseIdSet = Store.value getter Atoms.User.selectedDatabaseIdSet

                selectedDatabaseIdSet
                |> Set.toArray
                |> Array.map Database.taskIdAtomsByArchive
                |> Store.waitForAll
                |> Store.value getter
                |> Array.collect id
                |> Store.waitForAll
                |> Store.value getter
                |> Array.toList)


    let rec informationSet =
        Store.readSelector
            $"{nameof Session}/{nameof informationSet}"
            (fun getter ->
                let selectedDatabaseIdSet = Store.value getter Atoms.User.selectedDatabaseIdSet

                let informationAttachmentIdMapArray =
                    selectedDatabaseIdSet
                    |> Set.toArray
                    |> Array.map Atoms.Database.informationAttachmentIdMap
                    |> Store.waitForAll
                    |> Store.value getter
                    |> Array.collect (
                        Map.filter (fun _ attachmentIdSet -> attachmentIdSet |> Set.isEmpty |> not)
                        >> Map.keys
                        >> Seq.toArray
                    )

                let selectedTaskIdAtoms = Store.value getter Session.selectedTaskIdAtoms

                let taskInformationArray =
                    selectedTaskIdAtoms
                    |> Array.map (Store.value getter)
                    |> Array.map Atoms.Task.information
                    |> Store.waitForAll
                    |> Store.value getter

                let informationArray =
                    taskInformationArray
                    |> Array.append informationAttachmentIdMapArray

                let projectAreas =
                    informationArray
                    |> Array.choose
                        (fun information ->
                            match information with
                            | Project project -> Some (Area project.Area)
                            | _ -> None)

                informationArray
                |> Array.append projectAreas
                |> Array.append informationAttachmentIdMapArray
                |> Array.filter
                    (fun information ->
                        information
                        |> Information.Name
                        |> InformationName.Value
                        |> String.IsNullOrWhiteSpace
                        |> not)
                |> Set.ofSeq)


    let rec activeSessions =
        Store.readSelector
            $"{nameof Session}/{nameof activeSessions}"
            (fun getter ->
                let selectedTaskIdArray =
                    Store.value getter selectedTaskIdListByArchive
                    |> List.toArray

                let durationArray =
                    selectedTaskIdArray
                    |> Array.map Task.activeSession
                    |> Store.waitForAll
                    |> Store.value getter

                let nameArray =
                    selectedTaskIdArray
                    |> Array.map Atoms.Task.name
                    |> Store.waitForAll
                    |> Store.value getter

                durationArray
                |> Array.toList
                |> List.indexed
                |> List.sortBy snd
                |> List.choose
                    (fun (i, duration) ->
                        duration
                        |> Option.map
                            (fun duration -> TempUI.ActiveSession (TaskName.Value nameArray.[i], Minute duration))))

    let rec filteredTaskIdSet =
        Store.readSelectorInterval
            1000
            Set.empty
            $"{nameof Session}/{nameof filteredTaskIdSet}"
            (fun getter ->
                let filter = Store.value getter Atoms.User.filter

                let selectedTaskIdListByArchive =
                    Store.value getter selectedTaskIdListByArchive
                    |> List.toArray

                let selectedTaskStateArray =
                    selectedTaskIdListByArchive
                    |> Array.map Task.taskState
                    |> Store.waitForAll
                    |> Store.value getter

                let checkText (text: string) = text.IndexOf filter.Filter >= 0

                let filteredTaskIdArray =
                    selectedTaskStateArray
                    |> Array.filter
                        (fun taskState ->
                            let text =
                                (taskState.Task.Name |> TaskName.Value |> checkText)
                                || (taskState.Task.Information
                                    |> Information.Name
                                    |> InformationName.Value
                                    |> checkText)

                            let information =
                                filter.Information.Information
                                |> Information.Name
                                |> InformationName.Value
                                |> String.IsNullOrWhiteSpace
                                || filter.Information.Information = taskState.Task.Information

                            let scheduling =
                                filter.Scheduling.IsNone
                                || filter.Scheduling = Some taskState.Task.Scheduling

                            let priority =
                                filter.Task.Priority = Task.Default.Priority
                                || filter.Task.Priority = taskState.Task.Priority

                            let duration =
                                filter.Task.Duration = Task.Default.Duration
                                || filter.Task.Duration = taskState.Task.Duration

                            let pendingAfter =
                                filter.Task.PendingAfter = Task.Default.PendingAfter
                                || filter.Task.PendingAfter = taskState.Task.PendingAfter

                            let missedAfter =
                                filter.Task.MissedAfter = Task.Default.MissedAfter
                                || filter.Task.MissedAfter = taskState.Task.MissedAfter

                            text
                            && information
                            && scheduling
                            && priority
                            && duration
                            && pendingAfter
                            && missedAfter)
                    |> Array.map (fun taskState -> taskState.Task.Id)


                Dom.log
                    (fun () ->
                        $"filteredTaskIdArray.Length={filteredTaskIdArray.Length}
                        selectedTaskStateArray.Length={selectedTaskStateArray.Length}")

                filteredTaskIdArray |> Set.ofSeq)


    let rec filteredTaskIdCount =
        Store.readSelector
            $"{nameof Session}/{nameof filteredTaskIdCount}"
            (fun getter ->
                let filteredTaskIdSet = Store.value getter filteredTaskIdSet
                filteredTaskIdSet.Count)


    let rec sortedTaskIdArray =
        Store.readSelector
            $"{nameof Session}/{nameof sortedTaskIdArray}"
            (fun getter ->
                let position = Store.value getter Atoms.Session.position

                match position with
                | Some position ->
                    let filteredTaskIdSet = Store.value getter filteredTaskIdSet

                    Dom.log (fun () -> $"sortedTaskIdArray. filteredTaskIdSet.Count={filteredTaskIdSet.Count}")

                    let filteredTaskIdArray = filteredTaskIdSet |> Set.toArray

                    let statusMapArray =
                        filteredTaskIdArray
                        |> Array.map Task.cellStatusMap
                        |> Store.waitForAll
                        |> Store.value getter

                    let taskStateArray =
                        filteredTaskIdArray
                        |> Array.map Task.taskState
                        |> Store.waitForAll
                        |> Store.value getter

                    let lanes =
                        statusMapArray
                        |> Array.zip taskStateArray
                        |> Array.toList

                    let view = Store.value getter Atoms.User.view
                    let dayStart = Store.value getter Atoms.User.dayStart
                    let informationSet = Store.value getter Session.informationSet

                    let result =
                        sortLanes
                            {|
                                View = view
                                DayStart = dayStart
                                Position = position
                                InformationSet = informationSet
                                Lanes = lanes
                            |}

                    Dom.log (fun () -> $"sortedTaskIdArray. result.Length={result.Length}")

                    result
                    |> List.map (fun (taskState, _) -> taskState.Task.Id)
                    |> List.toArray
                | _ -> [||])


    let rec sortedTaskIdAtoms =
        Store.readSelector
            $"{nameof Session}/{nameof sortedTaskIdAtoms}"
            (fun getter ->
                sortedTaskIdArray
                |> Jotai.jotaiUtils.splitAtom
                |> Store.value getter)


    let rec sortedTaskIdCount =
        Store.readSelector
            $"{nameof Session}/{nameof sortedTaskIdCount}"
            (fun getter ->
                let sortedTaskIdAtoms = Store.value getter sortedTaskIdAtoms
                sortedTaskIdAtoms.Length)


    let rec informationTaskIdArray =
        Store.readSelector
            $"{nameof Session}/{nameof informationTaskIdArray}"
            (fun getter ->
                let sortedTaskIdAtoms = Store.value getter sortedTaskIdAtoms

                let informationSet = Store.value getter informationSet

                let taskInformationArray =
                    sortedTaskIdAtoms
                    |> Store.waitForAll
                    |> Store.value getter
                    |> Array.map Atoms.Task.information
                    |> Store.waitForAll
                    |> Store.value getter

                let taskMap =
                    sortedTaskIdAtoms
                    |> Array.mapi (fun i taskIdAtom -> taskInformationArray.[i], taskIdAtom)
                    |> Array.groupBy fst
                    |> Array.map (fun (information, taskIdAtoms) -> information, taskIdAtoms |> Array.map snd)
                    |> Map.ofSeq

                informationSet
                |> Set.toArray
                |> Array.map
                    (fun information ->
                        let taskIdAtoms =
                            taskMap
                            |> Map.tryFind information
                            |> Option.defaultValue [||]

                        information, taskIdAtoms)
                |> Array.sortBy (fst >> Information.Name)
                |> Array.sortBy (
                    fst
                    >> Option.ofObjUnbox
                    >> Option.map Information.toTag
                ))


    let rec informationTaskIdAtoms =
        Store.readSelector
            $"{nameof Session}/{nameof informationTaskIdAtoms}"
            (fun getter ->
                informationTaskIdArray
                |> Jotai.jotaiUtils.splitAtom
                |> Store.value getter)


    let rec informationTaskIdArrayByKind =
        Store.readSelector
            $"{nameof Session}/{nameof informationTaskIdArrayByKind}"
            (fun getter ->
                let informationTaskIdAtoms = Store.value getter Session.informationTaskIdAtoms
                //                         let informationTaskIdArray = Store.value getter Selectors.Session.informationTaskIdArray
                let informationTaskIdArray =
                    informationTaskIdAtoms
                    |> Store.waitForAll
                    |> Store.value getter

                informationTaskIdArray
                |> Array.indexed
                |> Array.groupBy (fun (_, (information, _)) -> Information.toString information)
                |> Array.map
                    (fun (informationKindName, groups) ->
                        informationKindName,
                        groups
                        |> Array.map (fun (i, _) -> informationTaskIdAtoms.[i])))


    let rec informationTaskIdAtomsByKind =
        Store.readSelector
            $"{nameof Session}/{nameof informationTaskIdAtomsByKind}"
            (fun getter ->
                informationTaskIdArrayByKind
                |> Jotai.jotaiUtils.splitAtom
                |> Store.value getter)


    let rec taskSelectedDateIdMap =
        Store.readSelector
            $"{nameof Session}/{nameof taskSelectedDateIdMap}"
            (fun getter ->
                let sortedTaskIdArray = Store.value getter sortedTaskIdArray

                sortedTaskIdArray
                |> Array.map Atoms.Task.selectionSet
                |> Store.waitForAll
                |> Store.value getter
                |> Array.mapi (fun i dates -> sortedTaskIdArray.[i], dates)
                |> Map.ofArray)


    let rec visibleTaskSelectedDateIdMap =
        Store.selector
            $"{nameof Session}/{nameof visibleTaskSelectedDateIdMap}"
            None
            (fun getter ->
                let taskSelectedDateIdMap = Store.value getter taskSelectedDateIdMap
                let dateIdArray = Store.value getter Selectors.dateIdArray

                taskSelectedDateIdMap
                |> Map.keys
                |> Seq.map
                    (fun taskId ->
                        let dates =
                            dateIdArray
                            |> Array.map (fun dateId -> dateId, taskSelectedDateIdMap.[taskId].Contains dateId)
                            |> Array.filter snd
                            |> Array.map fst
                            |> Set.ofSeq

                        taskId, dates)
                |> Seq.filter (fun (_, dates) -> Set.isEmpty dates |> not)
                |> Map.ofSeq)
            (fun getter setter newValue ->
                let sortedTaskIdArray = Store.value getter sortedTaskIdArray
                let visibleTaskSelectedDateIdMap = Store.value getter visibleTaskSelectedDateIdMap

                let operations =
                    sortedTaskIdArray
                    |> Array.collect
                        (fun taskId ->
                            let dates =
                                visibleTaskSelectedDateIdMap
                                |> Map.tryFind taskId
                                |> Option.defaultValue Set.empty

                            let newDates =
                                newValue
                                |> Map.tryFind taskId
                                |> Option.defaultValue Set.empty

                            let deselect =
                                newDates
                                |> Set.difference dates
                                |> Set.toArray
                                |> Array.map (fun date -> taskId, date, false)

                            let select =
                                dates
                                |> Set.difference newDates
                                |> Set.toArray
                                |> Array.map (fun date -> taskId, date, true)

                            select |> Array.append deselect)

                operations
                |> Array.iter
                    (fun (taskId, dateId, newValue) ->
                        Store.change
                            setter
                            (Atoms.Task.selectionSet taskId)
                            ((if newValue then Set.add else Set.remove) dateId)))
