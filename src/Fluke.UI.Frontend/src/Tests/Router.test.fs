namespace Fluke.UI.Frontend.Tests

open Fable.ReactTestingLibrary
open Fable.Jester
open Feliz.Recoil
open Feliz.Router
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings
open Fluke.UI.Frontend.Components
open Fluke.UI.Frontend.Tests.Core
open Fluke.UI.Frontend.Recoil
open Fluke.Shared


module Router =
    open Sync
    open TempData

    Jest.describe (
        "router",
        (fun () ->
            let initialSetter (setter: CallbackMethods) =
                promise {
                    setter.set (
                        Atoms.api,
                        Some
                            {
                                currentUser = async { return testUser }
                                databaseStateList = fun _username _moment -> async { return [] }
                            }
                    )

                    setter.set (Atoms.username, Some testUser.Username)
                }

            let getComponent () =
                Chakra.box
                    {|  |}
                    [
                        RouterObserver.RouterObserver ()
                    ]

            let initialize peek = promise { do! peek initialSetter }

            let setView peek (view: View.View) =
                peek (fun (setter: CallbackMethods) -> promise { setter.set (Atoms.view, view) })

            let expectView peek (expected: View.View) =
                peek
                    (fun (setter: CallbackMethods) ->
                        promise {
                            let! view = setter.snapshot.getPromise Atoms.view

                            Jest.expect(string view).toEqual (string expected)
                        })

            let expectUrl (expected: string []) =
                let segments = Router.currentUrl () |> List.toArray

                Jest
                    .expect(string segments)
                    .toEqual (string expected)

            let navigate (segments: string []) = RTL.act (fun () -> Router.navigate segments)

            Jest.test (
                "starting with blank url",
                promise {
                    [||] |> expectUrl

                    let! _subject, peek = getComponent () |> Setup.render
                    do! initialize peek
                    do! peek (fun setter -> promise { setter.set (Atoms.sessionRestored, true) })

                    [|
                        "view"
                        "HabitTracker"
                    |]
                    |> expectUrl

                    do! expectView peek View.View.HabitTracker
                }
            )

            Jest.test (
                "navigating from state",
                promise {
                    let! _subject, peek = getComponent () |> Setup.render
                    do! initialize peek
                    do! peek (fun setter -> promise { setter.set (Atoms.sessionRestored, true) })

                    do! setView peek View.View.BulletJournal

                    [|
                        "view"
                        "BulletJournal"
                    |]
                    |> expectUrl

                    do! expectView peek View.View.BulletJournal
                }
            )

            Jest.test (
                "navigating from url",
                promise {
                    let! _subject, peek = getComponent () |> Setup.render
                    do! initialize peek
                    do! peek (fun setter -> promise { setter.set (Atoms.sessionRestored, true) })

                    [|
                        "view"
                        "Information"
                    |]
                    |> navigate

                    do! expectView peek View.View.Information
                }
            )

            Jest.test (
                "navigating from url (not logged)",
                promise {
                    let! _subject, peek = getComponent () |> Setup.render
                    do! initialize peek
                    do! peek (fun setter -> promise { setter.set (Atoms.username, None) })
                    do! peek (fun setter -> promise { setter.set (Atoms.sessionRestored, true) })

                    [|
                        "view"
                        "Information"
                    |]
                    |> navigate

                    [|
                        "login"
                    |]
                    |> expectUrl
                }
            )

            Jest.test (
                "starting with filled url",
                promise {
                    [|
                        "view"
                        "Information"
                    |]
                    |> navigate

                    let! _subject, peek = getComponent () |> Setup.render
                    do! initialize peek
                    do! peek (fun setter -> promise { setter.set (Atoms.sessionRestored, true) })

                    [|
                        "view"
                        "Information"
                    |]
                    |> expectUrl

                    do! expectView peek View.View.Information

                }
            )

            Jest.test (
                "starting with filled url (not logged)",
                promise {
                    [|
                        "view"
                        "Information"
                    |]
                    |> navigate

                    let! _subject, peek = getComponent () |> Setup.render
                    do! initialize peek
                    do! peek (fun setter -> promise { setter.set (Atoms.username, None) })
                    do! peek (fun setter -> promise { setter.set (Atoms.sessionRestored, true) })

                    [|
                        "login"
                    |]
                    |> expectUrl
                }
            )

            ())
    )
