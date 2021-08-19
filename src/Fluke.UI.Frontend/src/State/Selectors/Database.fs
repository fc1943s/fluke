namespace Fluke.UI.Frontend.State.Selectors

open FsCore.BaseModel
open FsStore.Bindings
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
    let rec database =
        Store.readSelectorFamily
            Fluke.root
            (nameof database)
            (fun (databaseId: DatabaseId) getter ->
                {
                    Id = databaseId
                    Name = Store.value getter (Atoms.Database.name databaseId)
                    Owner = Store.value getter (Atoms.Database.owner databaseId)
                    SharedWith = Store.value getter (Atoms.Database.sharedWith databaseId)
                    Position = Store.value getter (Atoms.Database.position databaseId)
                })


    let rec nodeType =
        Store.readSelectorFamily
            Fluke.root
            (nameof nodeType)
            (fun (databaseId: DatabaseId) getter ->
                let database = Store.value getter (database databaseId)
                let alias = Store.value getter Selectors.Gun.alias

                match database.Owner with
                | owner when owner = Templates.templatesUser.Username -> DatabaseNodeType.Template
                | Username owner when Some (Gun.Alias owner) = alias -> DatabaseNodeType.Owned
                | _ -> DatabaseNodeType.Shared)



    let rec isReadWrite =
        Store.readSelectorFamily
            Fluke.root
            (nameof isReadWrite)
            (fun (databaseId: DatabaseId) getter ->
                let alias = Store.value getter Selectors.Gun.alias

                let access =
                    match alias with
                    | Some (Gun.Alias alias) ->
                        let database = Store.value getter (database databaseId)

                        if Username alias <> Templates.templatesUser.Username
                           && database.Owner = Templates.templatesUser.Username then
                            None
                        else
                            getAccess database (Username alias)
                    | None -> None

                access = Some Access.ReadWrite)


    let rec taskIdAtoms =
        Store.readSelectorFamilyInterval
            Fluke.root
            (nameof taskIdAtoms)
            Selectors.interval
            [||]
            (fun (databaseId: DatabaseId) getter ->
                Selectors.asyncTaskIdAtoms
                |> Store.value getter
                |> Array.filter
                    (fun taskIdAtom ->
                        let taskId = Store.value getter taskIdAtom
                        let databaseId' = Store.value getter (Atoms.Task.databaseId taskId)
                        databaseId = databaseId'))


    let rec unarchivedTaskIdAtoms =
        Store.readSelectorFamily
            Fluke.root
            (nameof unarchivedTaskIdAtoms)
            (fun (databaseId: DatabaseId) getter ->
                let taskIdAtoms = Store.value getter (taskIdAtoms databaseId)

                taskIdAtoms
                |> Array.filter
                    (fun taskIdAtom ->
                        let taskId = Store.value getter taskIdAtom
                        let archived = Store.value getter (Atoms.Task.archived taskId)
                        archived = Some false))


    let rec archivedTaskIdAtoms =
        Store.readSelectorFamily
            Fluke.root
            (nameof archivedTaskIdAtoms)
            (fun (databaseId: DatabaseId) getter ->
                let taskIdAtoms = Store.value getter (taskIdAtoms databaseId)

                taskIdAtoms
                |> Array.filter
                    (fun taskIdAtom ->
                        let taskId = Store.value getter taskIdAtom
                        let archived = Store.value getter (Atoms.Task.archived taskId)
                        archived = Some true))


    let rec taskIdAtomsByArchive =
        Store.readSelectorFamily
            Fluke.root
            (nameof taskIdAtomsByArchive)
            (fun (databaseId: DatabaseId) getter ->
                let archive = Store.value getter Atoms.User.archive

                databaseId
                |> (if archive = Some true then
                        Database.archivedTaskIdAtoms
                    else
                        Database.unarchivedTaskIdAtoms)
                |> Store.value getter)


    let rec informationAttachmentIdMapByArchive =
        Store.readSelectorFamily
            Fluke.root
            (nameof informationAttachmentIdMapByArchive)
            (fun (databaseId: DatabaseId) getter ->
                let archive = Store.value getter Atoms.User.archive

                let informationAttachmentIdMap = Store.value getter (Database.informationAttachmentIdMap databaseId)

                let attachmentIdArray =
                    informationAttachmentIdMap
                    |> Map.values
                    |> Seq.fold Set.union Set.empty
                    |> Seq.toArray

                let archivedArray =
                    attachmentIdArray
                    |> Array.map Atoms.Attachment.archived
                    |> Store.waitForAll
                    |> Store.value getter

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
        Store.readSelectorFamilyInterval
            Fluke.root
            (nameof informationAttachmentIdMap)
            Selectors.interval
            Map.empty
            (fun (databaseId: DatabaseId) getter ->
                Selectors.asyncAttachmentIdAtoms
                |> Store.value getter
                |> Array.choose
                    (fun attachmentIdAtom ->
                        let attachmentId = Store.value getter attachmentIdAtom
                        let parent = Store.value getter (Atoms.Attachment.parent attachmentId)

                        match parent with
                        | Some (AttachmentParent.Information (databaseId', information)) when databaseId' = databaseId ->
                            Some (information, attachmentId)
                        | _ -> None)
                |> Array.groupBy fst
                |> Array.map (fun (dateId, items) -> dateId, items |> Array.map snd |> Set.ofArray)
                |> Map.ofSeq)

    let rec databaseState =
        Store.readSelectorFamily
            Fluke.root
            (nameof databaseState)
            (fun (databaseId: DatabaseId) getter ->
                let database = Store.value getter (Database.database databaseId)

                let taskIdAtoms = Store.value getter (Database.taskIdAtoms databaseId)

                let taskStateList: TaskState list =
                    taskIdAtoms
                    |> Array.toList
                    |> List.map (Store.value getter)
                    |> List.map Task.taskState
                    |> List.map (Store.value getter)

                let informationAttachmentIdMap = Store.value getter (Database.informationAttachmentIdMap databaseId)

                let informationStateMap =
                    informationAttachmentIdMap
                    |> Map.map
                        (fun information attachmentIdSet ->
                            let attachmentStateList =
                                attachmentIdSet
                                |> Set.toArray
                                |> Array.map Attachment.attachmentState
                                |> Store.waitForAll
                                |> Store.value getter
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
                    |> Store.waitForAll
                    |> Store.value getter

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
