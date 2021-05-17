namespace Fluke.UI.Frontend.Hooks

open Feliz
open Fable.Core
open Feliz.Recoil
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.State
open Fable.Core.JsInterop
open Fluke.UI.Frontend.Hooks
open Fluke.Shared


module Auth =
    let useLogout () =
        let gunNamespace = Recoil.useValue Selectors.gunNamespace
        let setUsername = Recoil.useSetState Atoms.username
        let resetGunKeys = Recoil.useResetState Atoms.gunKeys

        Recoil.useCallbackRef
            (fun _setter _ ->
                promise {
                    printfn "before leave"
                    gunNamespace.ref.leave ()
                    setUsername None
                    resetGunKeys ()
                })

    let usePostSignIn () =
        let gunNamespace = Recoil.useValue Selectors.gunNamespace
        let setUsername = Recoil.useSetState Atoms.username
        let setGunKeys = Recoil.useSetState Atoms.gunKeys

        Recoil.useCallbackRef
            (fun _setter username ->
                promise {
                    setUsername (Some username)
                    setGunKeys gunNamespace.ref.``_``.sea
                })

    let useSignIn () =
        let gunNamespace = Recoil.useValue Selectors.gunNamespace
        let postSignIn = usePostSignIn ()

        Recoil.useCallbackRef
            (fun _setter username password ->
                promise {
                    let! ack = Gun.authUser gunNamespace.ref username password

                    return!
                        promise {
                            match ack.err with
                            | None ->
                                do! postSignIn (Username username)
                                return Ok ()
                            | Some error -> return Error error
                        }
                })


    let useHydrateTemplates () =
        let hydrateDatabase = Hydrate.useHydrateDatabase ()
        let hydrateTask = Hydrate.useHydrateTask ()

        Recoil.useCallbackRef
            (fun setter ->
                promise {
                    TestUser.fetchTemplatesDatabaseStateMap ()
                    |> Map.values
                    |> Seq.iter
                        (fun databaseState ->
                            hydrateDatabase Recoil.AtomScope.ReadOnly databaseState.Database

                            databaseState.TaskStateMap
                            |> Map.values
                            |> Seq.iter
                                (fun taskState ->
                                    hydrateTask Recoil.AtomScope.ReadOnly databaseState.Database.Id taskState.Task

                                    taskState.CellStateMap
                                    |> Map.iter
                                        (fun dateId cellState ->
                                            setter.set (Atoms.Cell.status (taskState.Task.Id, dateId), cellState.Status)

                                            setter.set (
                                                Atoms.Cell.attachments (taskState.Task.Id, dateId),
                                                cellState.Attachments
                                            )

                                            setter.set (
                                                Atoms.Cell.sessions (taskState.Task.Id, dateId),
                                                cellState.Sessions
                                            ))))
                })

    let useSignUp () =
        let signIn = useSignIn ()
        let gunNamespace = Recoil.useValue Selectors.gunNamespace
        let hydrateTemplates = useHydrateTemplates ()

        Recoil.useCallbackRef
            (fun _ username password ->
                promise {
                    if username = "" || username = "" then
                        return Error "Required fields"
                    elif username = (Templates.templatesUser.Username |> Username.Value) then
                        return Error "Invalid username"
                    else
                        printfn $"Auth.useSignUp. gun.user() result: {JS.JSON.stringify gunNamespace.ref}"

                        let! ack = Gun.createUser gunNamespace.ref username password
                        printfn "Auth.useSignUp. Gun.createUser signUpAck:"
                        Browser.Dom.console.log ack
                        Browser.Dom.window?signUpAck <- ack

                        return!
                            promise {
                                match ack.err with
                                | None ->
                                    match! signIn username password with
                                    | Ok () ->
                                        do! hydrateTemplates ()

                                        //                                        gunNamespace
//                                            .ref
//                                            .get("fluke")
//                                            .put {| username = username |}
//                                        |> ignore

                                        return Ok ()
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

                                | Some error -> return Error error
                            }
                })
