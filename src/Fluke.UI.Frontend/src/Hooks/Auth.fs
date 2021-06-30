namespace Fluke.UI.Frontend.Hooks

open System
open Fable.Core.JsInterop
open Fable.Core
open Fluke.Shared
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open Fluke.UI.Frontend.Hooks


module Auth =
    let useLogout () =
        Store.useCallback (
            (fun getter setter () ->
                promise {
                    let gunNamespace = Store.value getter Store.Selectors.gunNamespace
                    printfn "before leave"
                    gunNamespace.leave ()
                    Store.set setter Store.Atoms.username None
                    Store.set setter Store.Atoms.gunKeys Gun.GunKeys.Default
                }),
            [||]
        )

    let usePostSignIn () =
        Store.useCallback (
            (fun getter setter username ->
                promise {
                    let gunNamespace = Store.value getter Store.Selectors.gunNamespace
                    let keys = gunNamespace.__.sea

                    match keys with
                    | Some keys ->
                        Store.set setter Store.Atoms.gunKeys keys
                        Store.set setter Store.Atoms.username (Some username)
                        return Ok (username, keys)
                    | None -> return Error $"No keys found for user {gunNamespace.is}"
                }),
            [||]
        )

    let useSignIn () =
        let postSignIn = usePostSignIn ()

        Store.useCallback (
            (fun getter _ (username, password) ->
                promise {
                    let gunNamespace = Store.value getter Store.Selectors.gunNamespace
                    let! ack = Gun.authUser gunNamespace username password

                    match ack with
                    | { err = None } -> return! postSignIn (Username username)
                    | { err = Some error } -> return Error error
                }),
            [|
                box postSignIn
            |]
        )

    let useChangePassword () =
        Store.useCallback (
            (fun getter _ (password, newPassword) ->
                promise {
                    let username = Store.value getter Store.Atoms.username

                    match username with
                    | Some (Username username) ->
                        let gunNamespace = Store.value getter Store.Selectors.gunNamespace
                        let! ack = Gun.changeUserPassword gunNamespace username password newPassword

                        return!
                            promise {
                                match ack with
                                | { ok = Some 1; err = None } -> return Ok ()
                                | { err = Some error } -> return Error error
                                | _ -> return Error $"invalid ack {JS.JSON.stringify ack}"
                            }
                    | _ -> return Error "Invalid username"
                }),
            [||]
        )

    let useDeleteUser () =
        let logout = useLogout ()

        Store.useCallback (
            (fun getter _ password ->
                promise {
                    let username = Store.value getter Store.Atoms.username

                    match username with
                    | Some (Username username) ->
                        let gunNamespace = Store.value getter Store.Selectors.gunNamespace
                        let! ack = Gun.deleteUser gunNamespace username password
                        printfn $"ack={JS.JSON.stringify ack}"

                        return!
                            promise {
                                match ack with
                                | { ok = Some 0; err = None } ->
                                    do! logout ()
                                    return Ok ()
                                | { err = Some error } -> return Error error
                                | _ -> return Error $"invalid ack {JS.JSON.stringify ack}"
                            }
                    | _ -> return Error "Invalid username"
                }),
            [|
                box logout
            |]
        )

    let useSignUp () =
        let signIn = useSignIn ()
        let hydrateTemplates = Hydrate.useHydrateTemplates ()

        Store.useCallback (
            (fun getter setter (username, password) ->
                promise {
                    if username = "" || username = "" then
                        return Error "Required fields"
                    elif username = (Templates.templatesUser.Username |> Username.Value) then
                        return Error "Invalid username"
                    else
                        let gunNamespace = Store.value getter Store.Selectors.gunNamespace

                        printfn $"Auth.useSignUp. gunNamespace={JS.JSON.stringify gunNamespace}"

                        let! ack = Gun.createUser gunNamespace username password

                        printfn $"Auth.useSignUp. Gun.createUser signUpAck={JS.JSON.stringify ack}"
                        JS.consoleLog ("ack", ack)

                        match JS.window id with
                        | Some window -> window?signUpAck <- ack
                        | None -> ()

                        return!
                            promise {
                                match ack with
                                | {
                                      err = None
                                      ok = Some 0
                                      pub = Some _
                                  } ->
                                    match! signIn (username, password) with
                                    | Ok (username, keys) ->
                                        do! hydrateTemplates ()

                                        Store.set setter Atoms.User.cellSize UserState.Default.CellSize
                                        Store.set setter Atoms.User.darkMode UserState.Default.DarkMode
                                        Store.set setter Atoms.User.daysAfter UserState.Default.DaysAfter
                                        Store.set setter Atoms.User.daysBefore UserState.Default.DaysBefore
                                        Store.set setter Atoms.User.dayStart UserState.Default.DayStart
                                        Store.set setter Atoms.User.expandedDatabaseIdSet UserState.Default.ExpandedDatabaseIdSet
                                        Store.set setter Atoms.User.filterTasksByView UserState.Default.FilterTasksByView
                                        Store.set setter Atoms.User.fontSize UserState.Default.FontSize
                                        Store.set setter Atoms.User.hideSchedulingOverlay UserState.Default.HideSchedulingOverlay
                                        Store.set setter Atoms.User.hideTemplates UserState.Default.HideTemplates
                                        Store.set setter Atoms.User.informationAttachmentMap UserState.Default.InformationAttachmentMap
                                        Store.set setter Atoms.User.language UserState.Default.Language
                                        Store.set setter Atoms.User.leftDock UserState.Default.LeftDock
                                        Store.set setter Atoms.User.rightDock UserState.Default.RightDock
                                        Store.set setter Atoms.User.searchText UserState.Default.SearchText
                                        Store.set setter Atoms.User.selectedDatabaseIdSet UserState.Default.SelectedDatabaseIdSet
                                        Store.set setter Atoms.User.sessionBreakDuration UserState.Default.SessionBreakDuration
                                        Store.set setter Atoms.User.sessionDuration UserState.Default.SessionDuration
                                        Store.set setter Atoms.User.showViewOptions UserState.Default.ShowViewOptions
                                        Store.set setter Atoms.User.systemUiFont UserState.Default.SystemUiFont
                                        Store.set setter Atoms.User.view UserState.Default.View
                                        Store.set setter Atoms.User.weekStart UserState.Default.WeekStart


                                        JS.setTimeout
                                            (fun () ->
                                                Store.set
                                                    setter
                                                    Atoms.User.color
                                                    (Some (String.Format ("#{0:X6}", Random().Next 0x1000000))))
                                            0
                                        |> ignore



                                        //                                        gunNamespace
                                        //                                            .ref
                                        //                                            .get("fluke")
                                        //                                            .put {| username = username |}
                                        //                                        |> ignore

                                        return Ok (username, keys)
                                    | Error error -> return Error error
                                //                                    do! postSignIn (UserInteraction.Username username)

                                //                                    gunNamespace
                                //                                        .get("fluke")
                                //                                        .put {| username = username |}
                                //                                    |> ignore
                                //                                let usernamePut =
                                //                                    gunNamespace
                                //                                        .ref
                                //                                        .get("fluke")
                                //                                        .put {| username = username |}
                                //
                                //                                printfn $"sign up username put = {JS.JSON.stringify usernamePut}"

                                | { err = Some err } -> return Error err
                                | _ -> return Error $"Invalid ack: {JS.JSON.stringify ack}"
                            }
                }),
            [|
                box hydrateTemplates
                box signIn
            |]
        )
