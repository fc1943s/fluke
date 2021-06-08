namespace Fluke.UI.Frontend.Components

open Fluke.Shared
open Fluke.Shared.Domain.State
open Fluke.Shared.Domain.UserInteraction
open Feliz
open Fable.React
open Feliz.Recoil
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State


module DatabaseLeafIcon =
    [<ReactComponent>]
    let DatabaseLeafIcon (username: Username) (databaseId: DatabaseId) =
        let owner = Recoil.useValue (Atoms.Database.owner (username, databaseId))
        let sharedWith = Recoil.useValue (Atoms.Database.sharedWith (username, databaseId))
        let position = Recoil.useValue (Atoms.Database.position (username, databaseId))

        let newSharedWith =
            if owner = Templates.templatesUser.Username then
                [
                    username
                ]
            else
                match sharedWith with
                | DatabaseAccess.Public -> []
                | DatabaseAccess.Private accessList -> accessList |> List.map fst

        let isPrivate =
            match sharedWith with
            | DatabaseAccess.Public -> false
            | _ ->
                newSharedWith
                |> List.exists (fun share -> share <> username)
                |> not

        Chakra.stack
            (fun x ->
                x.display <- "inline"
                x.spacing <- "4px"
                x.direction <- "row")
            [

                match isPrivate with
                | false ->
                    Tooltip.wrap
                        (Chakra.box
                            (fun _ -> ())
                            [
                                str $"Owner: {owner |> Username.Value}"
                                br []
                                if not newSharedWith.IsEmpty then
                                    str
                                        $"""Shared with: {
                                                              newSharedWith
                                                              |> List.map Username.Value
                                                              |> String.concat ", "
                                        }"""
                            ])
                        [
                            Chakra.box
                                (fun _ -> ())
                                [
                                    Chakra.icon
                                        (fun x ->
                                            x.``as`` <- Icons.hi.HiUsers
                                            x.color <- "#ffb836"
                                            x.marginLeft <- "-3px")
                                        []
                                ]
                        ]
                | _ ->
                    Tooltip.wrap
                        (str "Private")
                        [
                            Chakra.box
                                (fun _ -> ())
                                [
                                    Chakra.icon
                                        (fun x ->
                                            x.``as`` <- Icons.fa.FaUserShield
                                            x.color <- "#a4ff8d"
                                            x.marginLeft <- "-3px")
                                        []
                                ]
                        ]

                match position with
                | Some position ->
                    Tooltip.wrap
                        (str $"Database paused at position {position |> FlukeDateTime.Stringify}")
                        [
                            Chakra.box
                                (fun _ -> ())
                                [
                                    Chakra.icon
                                        (fun x ->
                                            x.``as`` <- Icons.bs.BsPauseFill
                                            x.color <- "#ffb836"
                                            x.marginLeft <- "-3px")
                                        []
                                ]
                        ]
                | None ->
                    Tooltip.wrap
                        (str "Live Database")
                        [
                            Chakra.box
                                (fun _ -> ())
                                [
                                    Chakra.icon
                                        (fun x ->
                                            x.``as`` <- Icons.bs.BsPlayFill
                                            x.color <- "#a4ff8d"
                                            x.marginLeft <- "-3px")
                                        []
                                ]
                        ]
            ]
