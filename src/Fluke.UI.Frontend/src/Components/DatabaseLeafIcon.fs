namespace Fluke.UI.Frontend.Components

open Fluke.Shared
open Fluke.Shared.Domain.State
open Fluke.Shared.Domain.UserInteraction
open Feliz
open Fable.React
open FsCore.BaseModel
open FsStore
open FsStore.Bindings
open FsStore.Hooks
open FsUi.Bindings
open Fluke.UI.Frontend.State
open FsUi.Components


module DatabaseLeafIcon =
    [<ReactComponent>]
    let DatabaseLeafIcon databaseId =
        let alias = Store.useValue Selectors.Gun.alias
        let owner = Store.useValue (Atoms.Database.owner databaseId)
        let sharedWith = Store.useValue (Atoms.Database.sharedWith databaseId)
        let position = Store.useValue (Atoms.Database.position databaseId)

        let newSharedWith, isPrivate =
            React.useMemo (
                (fun () ->
                    let newSharedWith =
                        if owner = Templates.templatesUser.Username then
                            [
                                match alias with
                                | Some (Gun.Alias alias) -> yield Username alias
                                | _ -> ()
                            ]
                        else
                            match sharedWith with
                            | DatabaseAccess.Public -> []
                            | DatabaseAccess.Private accessList -> accessList |> List.map fst

                    let isPrivate =
                        match alias, sharedWith with
                        | _, DatabaseAccess.Public -> false
                        | Some (Gun.Alias alias), _ ->
                            newSharedWith
                            |> List.exists (fun (Username share) -> share <> alias)
                            |> not
                        | _ -> true

                    newSharedWith, isPrivate),
                [|
                    box alias
                    box owner
                    box sharedWith
                |]
            )


        Ui.stack
            (fun x ->
                x.display <- "inline"
                x.spacing <- "4px"
                x.direction <- "row")
            [

                match isPrivate with
                | false ->
                    Tooltip.wrap
                        (Ui.box
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
                            Ui.box
                                (fun _ -> ())
                                [
                                    Ui.icon
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
                            Ui.box
                                (fun _ -> ())
                                [
                                    Ui.icon
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
                            Ui.box
                                (fun _ -> ())
                                [
                                    Ui.icon
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
                            Ui.box
                                (fun _ -> ())
                                [
                                    Ui.icon
                                        (fun x ->
                                            x.``as`` <- Icons.bs.BsPlayFill
                                            x.color <- "_green"
                                            x.marginLeft <- "-3px")
                                        []
                                ]
                        ]
            ]
