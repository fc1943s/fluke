namespace Fluke.UI.Frontend.Tests

open Fable.Core.JsInterop
open System
open Fluke.Shared.Domain.UserInteraction
open FsJs.Bindings.Cypress
open FsCore
open FsCore.Model
open Fluke.UI.Frontend.State.State
open Fluke.UI.Frontend.Components


module Full =
    describe
        "tests"
        (fun () ->
            let homeUrl = "https://localhost:33922"
            before (fun () -> Cy.visit homeUrl)

            it
                "login"
                (fun () ->
                    Cy.window ()
                    |> Promise.iter (fun window -> window?indexedDB?deleteDatabase "radata")

                    let username = "x@x"
                    let password = "x@x"
                    let dbName = "db1"
                    let taskName = "task1"

                    Cy2.expectLocation $"{homeUrl}/"
                    Cy.get("body").should "have.css" "background-color" "rgb(222, 222, 222)"

                    Cy.focused().click None |> ignore

                    Cy2.selectorTypeText "input[placeholder=Email]" username None
                    Cy2.selectorFocusTypeText "input[placeholder=Password]" password
                    Cy2.clickText "Login"
                    Cy2.waitFor "Wrong user or password"

                    //                    Cy.wait 250

                    Cy2.clickText "Register"
                    Cy2.selectorFocusTypeText "input[placeholder='Confirm Password']" password
                    Cy2.clickText "Confirm"

                    Cy2.waitFor "User registered successfully" (Some {| timeout = cypressTimeout |})

                    Cy.wait 10000

                    Cy.window ()
                    |> Promise.iter (fun window -> window?Debug <- true)

                    Cy2.waitFor (nameof Databases) (Some {| timeout = cypressTimeout |})

                    Cy2.clickText (nameof Databases)
                    Cy2.waitFor "Lane Rendering" (Some {| timeout = cypressTimeout |})

                    Cy.wait 2000

                    Cy2.clickSelector "[data-testid='Add Database']"

                    Cy.wait 2000

                    Cy2.selectorTypeText "input[placeholder^=new-database-]" dbName None
                    Cy2.clickText "Save"

                    Cy2.waitFor dbName (Some {| timeout = cypressTimeout |})

                    Cy.wait 2000

                    Cy2.clickSelectorChildFromText dbName ".chakra-button"
                    Cy2.clickText "Add Task"

                    Cy.wait 15000


                    Cy2.clickSelectorChildFromText dbName ".chakra-button"
                    Cy2.clickText "Add Task"

                    //                    Cy.wait 5000

                    Cy2.clickTextWithinSelector "[data-testid=InformationSelector]" "Select..."

                    //                    Cy.wait 400

                    Cy2.clickSelector ".chakra-radio"


                    Cy2.clickText "Add Project"

                    //                    Cy.wait 15000

                    Cy2.selectorFocusTypeText "input[placeholder^='e.g. home renovation']" "p1"

                    Cy2.clickTextWithinSelector "[data-testid=AreaSelector]" "Select..."
                    Cy2.clickText "Add Area"

                    //                    Cy.wait 200

                    Cy2.selectorFocusTypeText "input[placeholder^='e.g. chores']" "a1"

                    Cy2.clickTextWithinSelector "[data-testid=AreaSelector]" "Save"
                    //                    Cy.wait 200

                    Cy2.clickTextWithinSelector "[data-testid=InformationSelector]" "Save"

                    //                    Cy2.waitForWithinSelector "[data-testid=DatabaseSelector]" "Select..." (Some {| timeout = cypressTimeout |})
//                    Cy2.waitForWithinSelector "[data-testid=DatabaseSelector]" dbName (Some {| timeout = cypressTimeout |})
//                    Cy.wait 3000
//                    Cy.wait 3000

                    Cy2.selectorFocusTypeText "input[placeholder^=new-task-]" taskName

                    Cy2.clickText "Save"

                    Cy.wait 6000

                    Cy2.clickSelectorChildFromText dbName ".chakra-button"
                    Cy2.clickText "Edit Database"

                    Cy.wait 3000

                    Cy2.selectorFocusTypeText "input[placeholder^=new-database-]" $"{dbName}_edit"

                    Cy2.clickText "Save"

                    Cy.wait 15000

                    Cy2.waitFor $"{dbName}_edit" (Some {| timeout = cypressTimeout |})

                    Cy2.clickText $"{dbName}_edit"

                    Cy.wait 15000

                    Cy2.clickText (nameof Settings)

                    Cy.wait 2000

                    Cy2.clickText "Filter Tasks by View"

                    Cy.wait 2000

                    Cy2.clickText (nameof Settings)

                    Cy.wait 6000

                    Cy2.waitFor "1 of 1 visible" (Some {| timeout = cypressTimeout |})
                    Cy2.waitFor taskName (Some {| timeout = cypressTimeout |})

                    Cy2.clickSelectorChildFromText taskName ".chakra-button"
                    Cy2.clickText "Start Session"
                    Cy2.waitFor $"Session: 1 active ({taskName})" (Some {| timeout = cypressTimeout |})


                    Cy.wait 6000

                    Cy2.clickText "Habit Tracker View"
                    Cy.wait 200
                    Cy2.waitFor "1 of 1 visible" (Some {| timeout = cypressTimeout |})

                    Cy.wait 6000

                    Cy2.clickSelectorChildFromText taskName ".chakra-button"
                    Cy2.clickText "Edit Task"

                    Cy.wait 6000

                    Cy2.clickSelectorChildFromText
                        (DateTime.Now
                          |> FlukeDate.FromDateTime
                          |> FlukeDate.Stringify
                          |> String.substring 0 9)
                        ".chakra-button"

                    Cy2.clickText "Delete Session"
                    Cy2.clickText "Confirm"

                    Cy2.waitFor "0 of 1 visible" (Some {| timeout = cypressTimeout |})

                    Cy.wait 6000

                    Cy2.clickText "Priority View"
                    Cy.wait 200

                    Cy2.waitFor "1 of 1 visible" (Some {| timeout = cypressTimeout |})

                    Cy2.clickTestId "[data-testid^='cell-']"

                    Cy2.clickTestId
                        $"[data-testid='cell-button-{Color.Value UserState.Default.CellColorCompleted
                                                     |> Option.get}']"


                    Cy2.clickTestId "[data-testid^='cell-']"

                    Cy2.clickTestId
                        $"[data-testid='cell-button-{Color.Value UserState.Default.CellColorPending
                                                     |> Option.get}']"

                    Cy.wait 200

                    Cy2.selectorTypeText "textarea[placeholder='Add Attachment']" "newcomment" None

                    Cy2.clickTestId "[data-testid='Add Attachment']"

                    Cy2.waitFor "newcomment" (Some {| timeout = cypressTimeout |})

                    Cy2.clickText "Bullet Journal View"
                    Cy.wait 200

                    Cy2.waitFor "1 of 1 visible" (Some {| timeout = cypressTimeout |})

                    Cy2.clickTestId "[data-testid^='cell-']"

                    Cy2.clickTestId
                        $"[data-testid='cell-button-{Color.Value UserState.Default.CellColorDisabled
                                                     |> Option.get}']"

                    Cy2.waitFor "1 of 1 visible" (Some {| timeout = cypressTimeout |})

                    Cy.window ()
                    |> Promise.iter (fun window -> window?Debug <- false)

                    Cy.visit homeUrl))
