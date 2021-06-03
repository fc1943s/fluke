namespace Fluke.UI.Frontend.Tests

open Fable.ReactTestingLibrary
open Fable.React
open Fable.Jester
open Feliz.Recoil
open Feliz.Router
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Tests.Core
open Fluke.UI.Frontend.State
open Fluke.Shared
open Feliz
open Fluke.Shared.Domain.UserInteraction
open Microsoft.FSharp.Core.Operators


module Router =
    Jest.describe (
        "router",
        (fun () ->
            let getComponent () =
                React.fragment [
                    (React.memo
                        (fun () ->
                            //                            React.useEffect (
//                                (fun () ->
//                                    promise {
//                                        let! gun = Recoil.getGun2 () |> Async.StartAsPromise
//                                        let user = gun.user ()
//                                        let username = Templates.templatesUser.Username |> Username.Value
//                                        let! _ = Gun.createUser user username username
//                                        let! _ = Gun.authUser user username username
//                                        ()
//                                    }
//                                    |> Promise.start),
//                                [||]
//                            )

                            nothing)
                        ())
                    Chakra.box
                        (fun _ -> ())
                        [
                            RouterObserver.RouterObserver ()
                        ]
                ]

            let initialSetter (setter: CallbackMethods) =
                promise { setter.set (Atoms.username, Some Templates.templatesUser.Username) }

            let initialize () =
                promise {
                    let subject, setter = getComponent () |> Setup.render
                    do! RTL.waitFor (initialSetter (setter.current ()))
                    return subject, setter
                }

            let setView (setter: CallbackMethods) (view: View.View) =
                RTL.act (fun () -> setter.set (Atoms.User.view Templates.templatesUser.Username, view))
                RTL.waitFor id

            let expectView (setter: CallbackMethods) (expected: View.View) =
                promise {
                    let! view = setter.snapshot.getPromise (Atoms.User.view Templates.templatesUser.Username)

                    Jest.expect(string view).toEqual (string expected)
                }

            let expectUrl (expected: string []) =
                promise {
                    do! RTL.waitFor id
                    let segments = Router.currentPath () |> List.toArray

                    Jest
                        .expect(string segments)
                        .toEqual (string expected)
                }

            let navigate (segments: string []) =
                RTL.act (fun () -> Router.navigatePath segments)

            Jest.beforeEach (
                promise {
                    printfn "Before each"
                    Browser.Dom.window.localStorage.clear ()
                //                    Jest.clearAllTimers ()
//                    JsInterop.emitJsExpr () "jest.clearAllMocks()"
                }
            )


            Jest.test (
                "starting with blank url",
                promise {
                    do! [||] |> expectUrl

                    let! _subject, setter = initialize ()

                    do!
                        [|
                            "view"
                            (string TempUI.defaultView)
                        |]
                        |> expectUrl

                    do! expectView (setter.current ()) TempUI.defaultView
                }
            )

            Jest.test (
                "navigating from state",
                promise {
                    let! _subject, setter = initialize ()

                    do! setView (setter.current ()) View.View.BulletJournal

                    do!
                        [|
                            "view"
                            "BulletJournal"
                        |]
                        |> expectUrl

                    do! expectView (setter.current ()) View.View.BulletJournal
                }
            )

            Jest.test (
                "navigating from url (logged in)",
                promise {
                    let! _subject, setter = initialize ()

                    [|
                        "view"
                        "Information"
                    |]
                    |> navigate

                    do! expectView (setter.current ()) View.View.Information
                }
            )

            Jest.test (
                "navigating from url (not logged in)",
                promise {
                    let! _subject, setter = initialize ()

                    RTL.act (fun () -> (setter.current ()).set (Atoms.username, None))
                    do! RTL.waitFor id

                    [|
                        "view"
                        "Information"
                    |]
                    |> navigate

                    do!
                        [|
                            "login"
                        |]
                        |> expectUrl
                }
            )

            Jest.test (
                "starting with filled url (logged in)",
                promise {

                    [|
                        "view"
                        "Information"
                    |]
                    |> navigate

                    let! _subject, setter = initialize ()

                    do!
                        [|
                            "view"
                            "Information"
                        |]
                        |> expectUrl

                    do! expectView (setter.current ()) View.View.Information

                }
            )

            Jest.test (
                "starting with filled url (not logged in)",
                promise {
                    [|
                        "view"
                        "Information"
                    |]
                    |> navigate

                    let! _subject, setter = initialize ()

                    RTL.act (fun () -> (setter.current ()).set (Atoms.username, None))
                    do! RTL.waitFor id

                    do!
                        [|
                            "login"
                        |]
                        |> expectUrl
                }
            )

            ())
    )
