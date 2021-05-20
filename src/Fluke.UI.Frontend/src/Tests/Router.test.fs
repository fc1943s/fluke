namespace Fluke.UI.Frontend.Tests

open Fable.ReactTestingLibrary
open Fable.Jester
open Feliz.Recoil
open Feliz.Router
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Tests.Core
open Fluke.UI.Frontend.State
open Fluke.Shared


module Router =

    Jest.describe (
        "router",
        (fun () ->
            let initialSetter (setter: CallbackMethods) =
                promise { setter.set (Atoms.username, Some Templates.templatesUser.Username) }

            let getComponent () =
                Chakra.box
                    (fun _ -> ())
                    [
                        RouterObserver.RouterObserver ()
                    ]

            let initialize peek = promise { do! peek initialSetter }

            let setView peek (view: View.View) =
                peek
                    (fun (setter: CallbackMethods) ->
                        promise { setter.set (Atoms.User.view Templates.templatesUser.Username, view) })

            let expectView peek (expected: View.View) =
                peek
                    (fun (setter: CallbackMethods) ->
                        promise {
                            let! view = setter.snapshot.getPromise (Atoms.User.view Templates.templatesUser.Username)

                            Jest.expect(string view).toEqual (string expected)
                        })

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

                    let! _subject, peek = getComponent () |> Setup.render
                    do! initialize peek

                    do!
                        [|
                            "view"
                            (string TempUI.defaultView)
                        |]
                        |> expectUrl

                    do! expectView peek TempUI.defaultView
                }
            )

            Jest.test (
                "navigating from state",
                promise {
                    let! _subject, peek = getComponent () |> Setup.render
                    do! initialize peek

                    do! setView peek View.View.BulletJournal

                    do!
                        [|
                            "view"
                            "BulletJournal"
                        |]
                        |> expectUrl

                    do! expectView peek View.View.BulletJournal
                }
            )

            Jest.test (
                "navigating from url (logged in)",
                promise {
                    let! _subject, peek = getComponent () |> Setup.render
                    do! initialize peek

                    [|
                        "view"
                        "Information"
                    |]
                    |> navigate

                    do! expectView peek View.View.Information
                }
            )

            Jest.test (
                "navigating from url (not logged in)",
                promise {
                    let! _subject, peek = getComponent () |> Setup.render
                    do! initialize peek
                    do! peek (fun setter -> promise { setter.set (Atoms.username, None) })

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

                    let! _subject, peek = getComponent () |> Setup.render
                    do! initialize peek

                    do!
                        [|
                            "view"
                            "Information"
                        |]
                        |> expectUrl

                    do! expectView peek View.View.Information

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

                    let! _subject, peek = getComponent () |> Setup.render
                    do! initialize peek
                    do! peek (fun setter -> promise { setter.set (Atoms.username, None) })

                    do!
                        [|
                            "login"
                        |]
                        |> expectUrl
                }
            )

            ())
    )
