namespace Fluke.UI.Backend

open Saturn
open Fluke.Shared
open Fable.Remoting.Server
open Fable.Remoting.Giraffe


module Server =
    open Domain.State
    open Domain.UserInteraction
    open Templates

    module Data =
        let getTreeStateList (moment: FlukeDateTime) =
            let users = TempData.getUsers ()

            let dslData = PrivateData.Tasks.getDslData moment |> fst

            let sharedDslData =
                SharedPrivateData.SharedTasks.getDslData moment
                |> fst

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
                    |> mergeDslDataIntoTreeState dslData
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
                    |> mergeDslDataIntoTreeState sharedDslData

                    yield! getTreeMap users.fluke
                           |> Map.toList
                           |> List.map (fun (templateName, dslTemplate) ->
                               treeStateFromDslTemplate users.fluke templateName dslTemplate)
                ]

            treeStateList


    module Sync =
        open Sync

        let api: Api =
            {
                currentUser = async {
                    let consts = PrivateData.PrivateData.getPrivateConsts ()
                    return consts.CurrentUser
                }
                treeStateList =
                    fun user moment ->
                        async {
                            let treeStateList = Data.getTreeStateList moment

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

                            return treesWithAccess
                        }
            }

    let webApp =
        Remoting.createApi ()
        |> Remoting.fromValue Sync.api
        |> Remoting.withBinarySerialization
        |> Remoting.buildHttpHandler

    let app =
        application {
            url (sprintf "http://0.0.0.0:%s/" Sync.serverPort)
            use_router webApp
            use_gzip
        }
