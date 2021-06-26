namespace Fluke.UI.Frontend.Components

open Fluke.Shared
open Fluke.Shared.Domain.State
open Fluke.Shared.Domain.UserInteraction
open Feliz
open Fable.React
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State


module DatabaseLeafIcon =
    [<ReactComponent>]
    let DatabaseLeafIcon (input: {| DatabaseId: DatabaseId |}) =
        let username = Store.useValue Atoms.username
        let owner = Store.useValue (Atoms.Database.owner input.DatabaseId)
        let sharedWith = Store.useValue (Atoms.Database.sharedWith input.DatabaseId)
        let position = Store.useValue (Atoms.Database.position input.DatabaseId)

        let newSharedWith, isPrivate =
            React.useMemo (
                (fun () ->
                    let newSharedWith =
                        if owner = Templates.templatesUser.Username then
                            [
                                match username with
                                | Some username -> yield username
                                | _ -> ()
                            ]
                        else
                            match sharedWith with
                            | DatabaseAccess.Public -> []
                            | DatabaseAccess.Private accessList -> accessList |> List.map fst

                    let isPrivate =
                        match username, sharedWith with
                        | _, DatabaseAccess.Public -> false
                        | Some username, _ ->
                            newSharedWith
                            |> List.exists (fun share -> share <> username)
                            |> not
                        | _ -> true

                    newSharedWith, isPrivate),
                [|
                    box username
                    box owner
                    box sharedWith
                |]
            )


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
