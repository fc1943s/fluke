namespace Fluke.UI.Frontend.State.Selectors

open FsCore.BaseModel
open FsStore.Bindings
open FsStore.Model
open FsStore.State
open FsCore
open System
open Fluke.Shared
open Fluke.Shared.Domain.Model
open Fluke.Shared.Domain
open Fluke.UI.Frontend.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.Shared.Domain.State
open Fluke.UI.Frontend.State.State
open FsJs
open FsStore


#nowarn "40"


module rec Database =
    let readSelectorFamily name read =
        Atom.readSelectorFamily
            (fun databaseId ->
                StoreAtomPath.ValueAtomPath (
                    Fluke.root,
                    Atoms.Database.collection,
                    Atoms.Database.formatDatabaseId databaseId,
                    AtomName name
                ))
            (fun (databaseId: DatabaseId) -> (read databaseId))

    let rec database =
        readSelectorFamily
            (nameof database)
            (fun (databaseId: DatabaseId) getter ->
                {
                    Id = databaseId
                    Name = Atom.get getter (Atoms.Database.name databaseId)
                    Owner = Atom.get getter (Atoms.Database.owner databaseId)
                    SharedWith = Atom.get getter (Atoms.Database.sharedWith databaseId)
                    Position = Atom.get getter (Atoms.Database.position databaseId)
                })


    let rec nodeType =
        readSelectorFamily
            (nameof nodeType)
            (fun (databaseId: DatabaseId) getter ->
                let database = Atom.get getter (database databaseId)
                let alias = Atom.get getter Selectors.Gun.alias

                match database.Owner with
                | owner when owner = Templates.templatesUser.Username -> DatabaseNodeType.Template
                | Username owner when Some (Gun.Alias owner) = alias -> DatabaseNodeType.Owned
                | _ -> DatabaseNodeType.Shared)



    let rec isReadWrite =
        readSelectorFamily
            (nameof isReadWrite)
            (fun (databaseId: DatabaseId) getter ->
                let alias = Atom.get getter Selectors.Gun.alias

                let access =
                    match alias with
                    | Some (Gun.Alias alias) ->
                        let database = Atom.get getter (database databaseId)

                        if Username alias <> Templates.templatesUser.Username
                           && database.Owner = Templates.templatesUser.Username then
                            None
                        else
                            getAccess database (Username alias)
                    | None -> None

                access = Some Access.ReadWrite)


    let rec taskIdAtoms =
        //        Store.readSelectorFamilyInterval
        readSelectorFamily
            (nameof taskIdAtoms)
            //            Selectors.interval
//            [||]
            (fun (databaseId: DatabaseId) getter ->
                Selectors.asyncTaskIdAtoms
                |> Atom.get getter
                |> Array.filter
                    (fun taskIdAtom ->
                        let taskId = Atom.get getter taskIdAtom
                        let databaseId' = Atom.get getter (Atoms.Task.databaseId taskId)
                        databaseId = databaseId'))


    let rec unarchivedTaskIdAtoms =
        readSelectorFamily
            (nameof unarchivedTaskIdAtoms)
            (fun (databaseId: DatabaseId) getter ->
                let taskIdAtoms = Atom.get getter (taskIdAtoms databaseId)

                taskIdAtoms
                |> Array.filter
                    (fun taskIdAtom ->
                        let taskId = Atom.get getter taskIdAtom
                        let archived = Atom.get getter (Atoms.Task.archived taskId)
                        archived = Some false))


    let rec archivedTaskIdAtoms =
        readSelectorFamily
            (nameof archivedTaskIdAtoms)
            (fun (databaseId: DatabaseId) getter ->
                let taskIdAtoms = Atom.get getter (taskIdAtoms databaseId)

                taskIdAtoms
                |> Array.filter
                    (fun taskIdAtom ->
                        let taskId = Atom.get getter taskIdAtom
                        let archived = Atom.get getter (Atoms.Task.archived taskId)
                        archived = Some true))


    let rec taskIdAtomsByArchive =
        readSelectorFamily
            (nameof taskIdAtomsByArchive)
            (fun (databaseId: DatabaseId) getter ->
                let archive = Atom.get getter Atoms.User.archive

                databaseId
                |> (if archive = Some true then
                        Database.archivedTaskIdAtoms
                    else
                        Database.unarchivedTaskIdAtoms)
                |> Atom.get getter)


    let rec informationAttachmentIdMapByArchive =
        readSelectorFamily
            (nameof informationAttachmentIdMapByArchive)
            (fun (databaseId: DatabaseId) getter ->
                let archive = Atom.get getter Atoms.User.archive

                let informationAttachmentIdMap = Atom.get getter (Database.informationAttachmentIdMap databaseId)

                let attachmentIdArray =
                    informationAttachmentIdMap
                    |> Map.values
                    |> Seq.fold Set.union Set.empty
                    |> Seq.toArray

                let archivedArray =
                    attachmentIdArray
                    |> Array.map Atoms.Attachment.archived
                    |> Atom.waitForAll
                    |> Atom.get getter

                let archivedMap =
                    archivedArray
                    |> Array.zip attachmentIdArray
                    |> Map.ofArray

                informationAttachmentIdMap
                |> Map.map
                    (fun _ attachmentIdSet ->
                        attachmentIdSet
                        |> Set.filter (fun attachmentId -> archivedMap.[attachmentId] = archive)))

    let rec informationAttachmentIdMap =
        //        readSelectorFamilyInterval
        readSelectorFamily
            (nameof informationAttachmentIdMap)
            //            Selectors.interval
//            Map.empty
            (fun (databaseId: DatabaseId) getter ->
                Selectors.asyncAttachmentIdAtoms
                |> Atom.get getter
                |> Array.choose
                    (fun attachmentIdAtom ->
                        let attachmentId = Atom.get getter attachmentIdAtom
                        let parent = Atom.get getter (Atoms.Attachment.parent attachmentId)

                        match parent with
                        | Some (AttachmentParent.Information (databaseId', information)) when databaseId' = databaseId ->
                            Some (information, attachmentId)
                        | _ -> None)
                |> Array.groupBy fst
                |> Array.map (fun (dateId, items) -> dateId, items |> Array.map snd |> Set.ofArray)
                |> Map.ofSeq)

    let rec databaseState =
        readSelectorFamily
            (nameof databaseState)
            (fun (databaseId: DatabaseId) getter ->
                let database = Atom.get getter (Database.database databaseId)

                let taskIdAtoms = Atom.get getter (Database.taskIdAtoms databaseId)

                let taskStateList: TaskState list =
                    taskIdAtoms
                    |> Array.toList
                    |> List.map (Atom.get getter)
                    |> List.map Task.taskState
                    |> List.map (Atom.get getter)

                let informationAttachmentIdMap = Atom.get getter (Database.informationAttachmentIdMap databaseId)

                let informationStateMap =
                    informationAttachmentIdMap
                    |> Map.map
                        (fun information attachmentIdSet ->
                            let attachmentStateList =
                                attachmentIdSet
                                |> Set.toArray
                                |> Array.map Attachment.attachmentState
                                |> Atom.waitForAll
                                |> Atom.get getter
                                |> Array.toList
                                |> List.choose id

                            {
                                Information = information
                                AttachmentStateList = attachmentStateList
                                SortList = []
                            })
                    |> Map.filter
                        (fun _ informationState ->
                            not informationState.AttachmentStateList.IsEmpty
                            || not informationState.SortList.IsEmpty)

                let fileIdList =
                    taskStateList
                    |> List.map (fun taskState -> taskState.AttachmentStateList)
                    |> List.append (
                        informationStateMap
                        |> Map.values
                        |> Seq.toList
                        |> List.map (fun informationState -> informationState.AttachmentStateList)
                    )
                    |> List.append (
                        taskStateList
                        |> List.collect
                            (fun taskState ->
                                taskState.CellStateMap
                                |> Map.values
                                |> Seq.toList
                                |> List.map (fun cellState -> cellState.AttachmentStateList))
                    )
                    |> List.collect
                        (fun attachmentStateList ->
                            attachmentStateList
                            |> List.choose
                                (fun attachmentState ->
                                    match attachmentState.Attachment with
                                    | Attachment.Image fileId -> Some fileId
                                    | _ -> None))

                let byteArrayArray =
                    fileIdList
                    |> List.map Selectors.File.byteArray
                    |> List.toArray
                    |> Atom.waitForAll
                    |> Atom.get getter

                if byteArrayArray |> Array.contains None then
                    Error "Invalid files present"
                else
                    let fileMap =
                        fileIdList
                        |> List.mapi (fun i fileId -> fileId, byteArrayArray.[i].Value)
                        |> Map.ofList

                    let taskStateMap =
                        taskStateList
                        |> List.map
                            (fun taskState ->
                                taskState.Task.Id,
                                { taskState with
                                    CellStateMap =
                                        taskState.CellStateMap
                                        |> Map.map (fun _ cellState -> { cellState with SessionList = [] })
                                })
                        |> Map.ofSeq

                    let databaseState =
                        {
                            Database = database
                            InformationStateMap = informationStateMap
                            TaskStateMap = taskStateMap
                            FileMap = fileMap |> Map.mapValues Js.byteArrayToHexString
                        }

                    if databaseState.TaskStateMap
                       |> Map.exists
                           (fun _ taskState ->
                               taskState.Task.Name
                               |> TaskName.Value
                               |> String.IsNullOrWhiteSpace
                               || taskState.Task.Information
                                  |> Information.Name
                                  |> InformationName.Value
                                  |> String.IsNullOrWhiteSpace) then
                        Error "Database is not fully synced"
                    else
                        Ok databaseState)
