namespace Fluke.UI.Frontend.State.Selectors

open FsStore.Model
open FsStore.State
open FsCore
open System
open Fluke.Shared
open Fluke.Shared.View
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend
open Fluke.UI.Frontend.State.State
open FsCore.BaseModel
open FsStore


#nowarn "40"


module rec Session =
    let readSelector name read =
        Atom.readSelector (StoreAtomPath.ValueAtomPath (Fluke.root, Atoms.Session.collection, [], AtomName name)) read

    let rec devicePingList =
        readSelector
            (nameof devicePingList)
            (fun getter ->
                let deviceIdArray =
                    Selectors.asyncDeviceIdAtoms
                    |> Atom.get getter
                    |> Atom.waitForAll
                    |> Atom.get getter

                let pingArray =
                    deviceIdArray
                    |> Array.map Atoms.Device.devicePing
                    |> Atom.waitForAll
                    |> Atom.get getter

                deviceIdArray
                |> Array.toList
                |> List.mapi (fun i deviceId -> deviceId, pingArray.[i]))

    let rec databaseIdAtoms =
        readSelector
            (nameof databaseIdAtoms)
            (fun getter ->
                let asyncDatabaseIdAtoms = Atom.get getter Selectors.asyncDatabaseIdAtoms
                let hideTemplates = Atom.get getter Atoms.User.hideTemplates

                asyncDatabaseIdAtoms
                |> Array.filter
                    (fun databaseIdAtom ->
                        let databaseId = Atom.get getter databaseIdAtom
                        let database = Atom.get getter (Database.database databaseId)

                        let valid =
                            database.Name
                            |> DatabaseName.ValueOrDefault
                            |> String.IsNullOrWhiteSpace
                            |> not
                            && database.Owner
                               |> Username.Value
                               |> String.IsNullOrWhiteSpace
                               |> not

                        if not valid then
                            false
                        else
                            let nodeType = Atom.get getter (Database.nodeType databaseId)

                            nodeType <> DatabaseNodeType.Template
                            || hideTemplates = Some false))


    let rec selectedTaskIdAtoms =
        readSelector
            (nameof selectedTaskIdAtoms)
            (fun getter ->
                let selectedDatabaseIdSet = Atom.get getter Atoms.User.selectedDatabaseIdSet

                selectedDatabaseIdSet
                |> Set.toArray
                |> Array.map Database.taskIdAtoms
                |> Atom.waitForAll
                |> Atom.get getter
                |> Array.collect id)


    let rec selectedTaskIdListByArchive =
        readSelector
            (nameof selectedTaskIdListByArchive)
            (fun getter ->
                let selectedDatabaseIdSet = Atom.get getter Atoms.User.selectedDatabaseIdSet

                selectedDatabaseIdSet
                |> Set.toArray
                |> Array.map Database.taskIdAtomsByArchive
                |> Atom.waitForAll
                |> Atom.get getter
                |> Array.collect id
                |> Atom.waitForAll
                |> Atom.get getter
                |> Array.toList)


    let rec informationSet =
        readSelector
            (nameof informationSet)
            (fun getter ->
                let selectedDatabaseIdSet = Atom.get getter Atoms.User.selectedDatabaseIdSet

                let informationAttachmentIdMapArray =
                    selectedDatabaseIdSet
                    |> Set.toArray
                    |> Array.map Database.informationAttachmentIdMap
                    |> Atom.waitForAll
                    |> Atom.get getter
                    |> Array.collect (
                        Map.filter (fun _ attachmentIdSet -> attachmentIdSet |> Set.isEmpty |> not)
                        >> Map.keys
                        >> Seq.toArray
                    )

                let selectedTaskIdAtoms = Atom.get getter Session.selectedTaskIdAtoms

                let taskInformationArray =
                    selectedTaskIdAtoms
                    |> Array.map (Atom.get getter)
                    |> Array.map Atoms.Task.information
                    |> Atom.waitForAll
                    |> Atom.get getter

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
        //        Store.readSelectorInterval
        readSelector
            (nameof activeSessions)
            (fun getter ->
                //            Selectors.interval
//            []
                let selectedTaskIdArray =
                    Atom.get getter selectedTaskIdListByArchive
                    |> List.toArray

                let durationArray =
                    selectedTaskIdArray
                    |> Array.map Task.activeSession
                    |> Atom.waitForAll
                    |> Atom.get getter

                let nameArray =
                    selectedTaskIdArray
                    |> Array.map Atoms.Task.name
                    |> Atom.waitForAll
                    |> Atom.get getter

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
        readSelector
            (nameof filteredTaskIdSet)
            (fun getter ->
                //        Store.readSelectorInterval
//            Fluke.root
//            (nameof filteredTaskIdSet)
//            Selectors.interval
//            Set.empty
//            (fun getter ->
                let logger = Atom.get getter Selectors.logger
                let filter = Atom.get getter Atoms.User.filter

                let selectedTaskIdListByArchive =
                    Atom.get getter selectedTaskIdListByArchive
                    |> List.toArray

                let selectedTaskStateArray =
                    selectedTaskIdListByArchive
                    |> Array.map Task.taskState
                    |> Atom.waitForAll
                    |> Atom.get getter

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


                let getLocals () =
                    $"filteredTaskIdArray.Length={filteredTaskIdArray.Length} selectedTaskStateArray.Length={selectedTaskStateArray.Length} {getLocals ()}"

                logger.Trace (fun () -> $"{nameof Fluke} | Session.filteredTaskIdSet selector read") getLocals

                filteredTaskIdArray |> Set.ofSeq)


    let rec filteredTaskIdCount =
        readSelector
            (nameof filteredTaskIdCount)
            (fun getter ->
                let filteredTaskIdSet = Atom.get getter filteredTaskIdSet
                filteredTaskIdSet.Count)


    let rec sortedTaskIdArray =
        readSelector
            (nameof sortedTaskIdArray)
            (fun getter ->
                //        Store.readSelectorInterval
//            Fluke.root
//            (nameof sortedTaskIdArray)
//            Selectors.interval
//            [||]
//            (fun getter ->
                let position = Atom.get getter Atoms.Session.position

                match position with
                | Some position ->
                    let logger = Atom.get getter Selectors.logger
                    let filteredTaskIdSet = Atom.get getter filteredTaskIdSet

                    let getLocals () =
                        $"filteredTaskIdSet.Count={filteredTaskIdSet.Count} {getLocals ()}"

                    logger.Trace
                        (fun () -> $"{nameof Fluke} | Session.sortedTaskIdArray readSelector. sortedTaskIdArray")
                        getLocals

                    let filteredTaskIdArray = filteredTaskIdSet |> Set.toArray

                    let statusMapArray =
                        filteredTaskIdArray
                        |> Array.map Task.cellStatusMap
                        |> Atom.waitForAll
                        |> Atom.get getter

                    let taskStateArray =
                        filteredTaskIdArray
                        |> Array.map Task.taskState
                        |> Atom.waitForAll
                        |> Atom.get getter

                    let lanes =
                        statusMapArray
                        |> Array.zip taskStateArray
                        |> Array.toList

                    let view = Atom.get getter Atoms.User.view
                    let dayStart = Atom.get getter Atoms.User.dayStart
                    let informationSet = Atom.get getter Session.informationSet

                    let result =
                        sortLanes
                            {|
                                View = view
                                DayStart = dayStart
                                Position = position
                                InformationSet = informationSet
                                Lanes = lanes
                            |}

                    let getLocals () =
                        $"result.Length={result.Length} {getLocals ()}"

                    logger.Trace (fun () -> $"{nameof Fluke} | Session.sortedTaskIdArray") getLocals

                    result
                    |> List.map (fun (taskState, _) -> taskState.Task.Id)
                    |> List.toArray
                | _ -> [||])

    let rec sortedTaskIdAtoms =
        readSelector (nameof sortedTaskIdAtoms) (fun getter -> sortedTaskIdArray |> Atom.split |> Atom.get getter)

    let rec sortedTaskIdCount =
        readSelector
            (nameof sortedTaskIdCount)
            (fun getter ->
                let sortedTaskIdAtoms = Atom.get getter sortedTaskIdAtoms
                sortedTaskIdAtoms.Length)

    let rec informationTaskIdArray =
        readSelector
            (nameof informationTaskIdArray)
            (fun getter ->
                let sortedTaskIdAtoms = Atom.get getter sortedTaskIdAtoms

                let informationSet = Atom.get getter informationSet

                let taskInformationArray =
                    sortedTaskIdAtoms
                    |> Atom.waitForAll
                    |> Atom.get getter
                    |> Array.map Atoms.Task.information
                    |> Atom.waitForAll
                    |> Atom.get getter

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
        readSelector
            (nameof informationTaskIdAtoms)
            (fun getter ->
                informationTaskIdArray
                |> Atom.split
                |> Atom.get getter)

    let rec informationTaskIdArrayByKind =
        readSelector
            (nameof informationTaskIdArrayByKind)
            (fun getter ->
                let informationTaskIdAtoms = Atom.get getter Session.informationTaskIdAtoms
                //                         let informationTaskIdArray = Atom.get getter Selectors.Session.informationTaskIdArray
                let informationTaskIdArray =
                    informationTaskIdAtoms
                    |> Atom.waitForAll
                    |> Atom.get getter

                informationTaskIdArray
                |> Array.indexed
                |> Array.groupBy (fun (_, (information, _)) -> Information.toString information)
                |> Array.map
                    (fun (informationKindName, groups) ->
                        informationKindName,
                        groups
                        |> Array.map (fun (i, _) -> informationTaskIdAtoms.[i])))

    let rec informationTaskIdAtomsByKind =
        readSelector
            (nameof informationTaskIdAtomsByKind)
            (fun getter ->
                informationTaskIdArrayByKind
                |> Atom.split
                |> Atom.get getter)


    let rec taskSelectedDateMap =
        readSelector
            (nameof taskSelectedDateMap)
            (fun getter ->
                let sortedTaskIdArray = Atom.get getter sortedTaskIdArray

                sortedTaskIdArray
                |> Array.map Atoms.Task.selectionSet
                |> Atom.waitForAll
                |> Atom.get getter
                |> Array.mapi (fun i dates -> sortedTaskIdArray.[i], dates)
                |> Map.ofArray)


    let rec visibleTaskSelectedDateMap =
        Atom.selector
            (StoreAtomPath.ValueAtomPath (
                Fluke.root,
                Atoms.Session.collection,
                [],
                AtomName (nameof visibleTaskSelectedDateMap)
            ))
            (fun getter ->
                let taskSelectedDateMap = Atom.get getter taskSelectedDateMap
                let dateArray = Atom.get getter Selectors.dateArray

                taskSelectedDateMap
                |> Map.keys
                |> Seq.map
                    (fun taskId ->
                        let dates =
                            dateArray
                            |> Array.map (fun date -> date, taskSelectedDateMap.[taskId].Contains date)
                            |> Array.filter snd
                            |> Array.map fst
                            |> Set.ofSeq

                        taskId, dates)
                |> Seq.filter (fun (_, dates) -> Set.isEmpty dates |> not)
                |> Map.ofSeq)
            (fun getter setter newValue ->
                let sortedTaskIdArray = Atom.get getter sortedTaskIdArray
                let visibleTaskSelectedDateMap = Atom.get getter visibleTaskSelectedDateMap

                let operations =
                    sortedTaskIdArray
                    |> Array.collect
                        (fun taskId ->
                            let dates =
                                visibleTaskSelectedDateMap
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
                    (fun (taskId, date, newValue) ->
                        Atom.change
                            setter
                            (Atoms.Task.selectionSet taskId)
                            ((if newValue then Set.add else Set.remove) date)))
