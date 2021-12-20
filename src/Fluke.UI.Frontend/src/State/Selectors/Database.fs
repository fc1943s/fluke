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



module Database =
    let inline readSelectorFamily name (defaultValue: 'T option) read =
        Atom.Primitives.atomFamily
            (fun databaseId ->
                let storeAtomPath =
                    StoreAtomPath.ValueAtomPath (
                        Fluke.root,
                        Atoms.Database.collection,
                        Atoms.Database.formatDatabaseId databaseId,
                        AtomName name
                    )

                let wrapper = Atom.selector storeAtomPath (read databaseId) Atom.Primitives.throwReadOnly

                match defaultValue with
                | None -> wrapper
                | Some _defaultValue -> wrapper
                //                    |> Engine.wrapAtomWithInterval defaultValue Selectors.interval
                )

    let rec database =
        readSelectorFamily
            (nameof database)
            None
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
            None
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
            None
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
        readSelectorFamily
            (nameof taskIdAtoms)
            (Some [||])
            (fun (databaseId: DatabaseId) getter ->
                let taskIdAtoms = Atom.get getter Selectors.asyncTaskIdAtoms
                let taskIdArray = taskIdAtoms |> Atom.waitForAll |> Atom.get getter

                let databaseIdArray =
                    taskIdArray
                    |> Array.map Atoms.Task.databaseId
                    |> Atom.waitForAll
                    |> Atom.get getter

                [|
                    0 .. taskIdArray.Length - 1
                |]
                |> Array.filter (fun i -> databaseId = databaseIdArray.[i])
                |> Array.map (fun i -> taskIdAtoms.[i]))

    let rec taskIdAtomsByArchive =
        readSelectorFamily
            (nameof taskIdAtomsByArchive)
            None
            (fun (databaseId: DatabaseId) getter ->
                let archive = Atom.get getter Atoms.User.archive
                let taskIdAtoms = Atom.get getter (taskIdAtoms databaseId)
                let taskIdArray = taskIdAtoms |> Atom.waitForAll |> Atom.get getter

                let archivedArray =
                    taskIdArray
                    |> Array.map Atoms.Task.archived
                    |> Atom.waitForAll
                    |> Atom.get getter

                [|
                    0 .. taskIdArray.Length - 1
                |]
                |> Array.filter
                    (fun i ->
                        match archive, archivedArray.[i] with
                        | Some archive, Some archived -> archive = archived
                        | None, Some archived -> archived = false
                        | _ -> false)
                |> Array.map (fun i -> taskIdAtoms.[i]))

    let rec informationAttachmentIdMap =
        readSelectorFamily
            (nameof informationAttachmentIdMap)
            (Some Map.empty)
            (fun (databaseId: DatabaseId) getter ->
                let attachmentIdArray =
                    Selectors.asyncAttachmentIdAtoms
                    |> Atom.get getter
                    |> Atom.waitForAll
                    |> Atom.get getter

                let attachmentIdParentArray =
                    attachmentIdArray
                    |> Array.map Atoms.Attachment.parent
                    |> Atom.waitForAll
                    |> Atom.get getter

                attachmentIdArray
                |> Array.mapi
                    (fun i attachmentId ->
                        match attachmentIdParentArray.[i] with
                        | Some (AttachmentParent.Information (databaseId', information)) when databaseId' = databaseId ->
                            Some (information, attachmentId)
                        | _ -> None)
                |> Array.choose id
                |> Array.groupBy fst
                |> Array.map (fun (information, items) -> information, items |> Array.map snd |> Set.ofArray)
                |> Map.ofSeq)

    let rec informationAttachmentIdMapByArchive =
        readSelectorFamily
            (nameof informationAttachmentIdMapByArchive)
            None
            (fun (databaseId: DatabaseId) getter ->
                let archive = Atom.get getter Atoms.User.archive

                let informationAttachmentIdMap = Atom.get getter (informationAttachmentIdMap databaseId)

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

    let rec databaseState =
        readSelectorFamily
            (nameof databaseState)
            None
            (fun (databaseId: DatabaseId) getter ->
                let database = Atom.get getter (database databaseId)

                let taskIdAtoms = Atom.get getter (taskIdAtoms databaseId)

                let taskStateList =
                    taskIdAtoms
                    |> Array.toList
                    |> List.map (Atom.get getter)
                    |> List.map Task.taskState
                    |> List.map (Atom.get getter)

                let informationAttachmentIdMap = Atom.get getter (informationAttachmentIdMap databaseId)

                let attachmentIdArray =
                    informationAttachmentIdMap
                    |> Map.values
                    |> Seq.collect id
                    |> Seq.toArray

                let attachmentStateMap =
                    attachmentIdArray
                    |> Array.map Attachment.attachmentState
                    |> Atom.waitForAll
                    |> Atom.get getter
                    |> Array.choose id
                    |> Array.mapi (fun i attachmentState -> attachmentIdArray.[i], attachmentState)
                    |> Map.ofArray

                let informationStateMap =
                    informationAttachmentIdMap
                    |> Map.map
                        (fun information attachmentIdSet ->
                            let attachmentStateList =
                                attachmentIdSet
                                |> Set.toList
                                |> List.choose attachmentStateMap.TryFind

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
                       |> Map.exists (fun _ taskState -> taskState.Task |> Task.Loaded |> not) then
                        Error "Database is not fully synced"
                    else
                        Ok databaseState)
