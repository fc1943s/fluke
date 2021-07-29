namespace Fluke.UI.Frontend.Components

open Fluke.Shared
open Fluke.Shared.Domain.State
open Fluke.Shared.Domain.UserInteraction
open Feliz
open Fable.React
open FsCore.Model
open FsStore
open FsUi.Bindings
open Fluke.UI.Frontend.State
open FsUi.Components


module DatabaseLeafIcon =
    [<ReactComponent>]
    let DatabaseLeafIcon databaseId =
        let username = Store.useValue Atoms.username
        let owner = Store.useValue (Atoms.Database.owner databaseId)
        let sharedWith = Store.useValue (Atoms.Database.sharedWith databaseId)
        let position = Store.useValue (Atoms.Database.position databaseId)

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


        UI.stack
            (fun x ->
                x.display <- "inline"
                x.spacing <- "4px"
                x.direction <- "row")
            [

                match isPrivate with
                | false ->
                    Tooltip.wrap
                        (UI.box
                            (fun _ -> ())
                            [
                                str $"Owner: {owner |> Username.ValueOrDefault}"
                                br []
                                if not newSharedWith.IsEmpty then
                                    str
                                        $"""Shared with: {newSharedWith
                                                          |> List.map Username.ValueOrDefault
                                                          |> String.concat ", "}"""
                            ])
                        [
                            UI.box
                                (fun _ -> ())
                                [
                                    UI.icon
                                        (fun x ->
                                            x.``as`` <- Icons.hi.HiUsers
                                            x.color <- "_orange"
                                            x.marginLeft <- "-3px")
                                        []
                                ]
                        ]
                | _ ->
                    Tooltip.wrap
                        (str "Private")
                        [
                            UI.box
                                (fun _ -> ())
                                [
                                    UI.icon
                                        (fun x ->
                                            x.``as`` <- Icons.fa.FaUserShield
                                            x.color <- "_green"
                                            x.marginLeft <- "-3px")
                                        []
                                ]
                        ]

                match position with
                | Some position ->
                    Tooltip.wrap
                        (str $"Database paused at position {position |> FlukeDateTime.Stringify}")
                        [
                            UI.box
                                (fun _ -> ())
                                [
                                    UI.icon
                                        (fun x ->
                                            x.``as`` <- Icons.bs.BsPauseFill
                                            x.color <- "_orange"
                                            x.marginLeft <- "-3px")
                                        []
                                ]
                        ]
                | None ->
                    Tooltip.wrap
                        (str "Live Database")
                        [
                            UI.box
                                (fun _ -> ())
                                [
                                    UI.icon
                                        (fun x ->
                                            x.``as`` <- Icons.bs.BsPlayFill
                                            x.color <- "_green"
                                            x.marginLeft <- "-3px")
                                        []
                                ]
                        ]
            ]
