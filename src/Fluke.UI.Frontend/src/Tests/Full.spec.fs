namespace Fluke.UI.Frontend.Tests

open System
open Fluke.Shared.Domain.State
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend
open Fluke.UI.Frontend.Bindings


module Full =
    open Cypress
    open Fluke.UI.Frontend.Components

    module Cy2 =
        let typeText (fn: unit -> Cy.Chainable2<_>) (text: string) =
            Cy.wait 200
            fn().clear {| force = false |} |> ignore
            fn().should "be.empty" null null

            text
            |> Seq.iter
                (fun letter ->
                    Cy.wait 50

                    fn().first().click (Some {| force = false |})
                    |> ignore

                    fn().first().``type`` (string letter) {| force = false |}
                    |> ignore)

            fn().should "have.value" text null

        let waitFocus selector wait =
            //            Cy.wait 50
            Cy.get(selector).should "have.focus" |> ignore

            match wait with
            | Some ms -> Cy.wait ms
            | None -> ()

        let selectorTypeText selector text wait =
            waitFocus selector wait
            typeText (fun () -> Cy.get selector) text

        let selectorFocusTypeText selector text =
            Cy.get(selector).first().focus ()
            typeText (fun () -> Cy.get selector) text

        let clickTestId selector =
            Cy
                .get(selector)
                .first()
                .click (Some {| force = false |})
            |> ignore

        let selectorFocusTypeTextWithinSelector parent selector text =
            Cy.get(parent).get(selector).first().focus ()
            typeText (fun () -> Cy.get(parent).get selector) text

        let clickTextWithinSelector selector text =
            (Cy.get(selector).contains text None)
                .click (Some {| force = false |})
            |> ignore

        let clickText text =
            (Cy.contains text None)
                .click (Some {| force = false |})
            |> ignore

        let clickSelectorChildFromText text selector =
            (Cy.contains text None)
                .find(selector)
                .click (Some {| force = false |})
            |> ignore

        let clickSelector selector =
            (Cy.get selector).first().click None |> ignore

        let waitForWithinSelector selector text options =
            (Cy.get(selector).contains text options)
                .should "be.visible"
            |> ignore

        let waitFor text options =
            (Cy.contains text options).should "be.visible"
            |> ignore

        let expectLocation expected =
            Cy
                .location()
                .should (fun location -> expect(location.href).``to``.contain expected)

    describe
        "tests"
        (fun () ->
            let homeUrl = "https://localhost:33922"
            let timeout = 40000
            before (fun () -> Cy.visit homeUrl)

            it
                "login"
                (fun () ->
                    let username = "x"
                    let password = "x"
                    let dbName = "db1"
                    let taskName = "task1"

                    Cy2.expectLocation $"{homeUrl}/"
                    Cy.get("body").should "have.css" "background-color" "rgb(222, 222, 222)"

                    Cy.focused().click None |> ignore

                    Cy2.selectorTypeText "input[placeholder=Username]" username None
                    Cy2.selectorFocusTypeText "input[placeholder=Password]" password
                    Cy2.clickText "Login"
                    Cy2.waitFor "Wrong user or password" (Some {| timeout = timeout |})

                    //                    Cy.wait 250

                    Cy2.clickText "Register"
                    Cy2.selectorFocusTypeText "input[placeholder='Confirm Password']" password
                    Cy2.clickText "Confirm"

                    Cy2.waitFor "User registered successfully" (Some {| timeout = timeout |})

                    Cy2.waitFor "Databases" (Some {| timeout = timeout |})

                    Cy2.clickText (nameof Databases)
                    Cy2.waitFor "Lane Rendering" (Some {| timeout = timeout |})

                    Cy.wait 6000

                    Cy2.clickSelector "[data-testid='Add Database']"

                    Cy2.selectorTypeText "input[placeholder^=new-database-]" dbName None
                    Cy2.clickText "Save"

                    Cy2.waitFor dbName (Some {| timeout = timeout |})

                    //                    Cy.wait 400

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

                    //                    Cy2.waitForWithinSelector "[data-testid=DatabaseSelector]" "Select..." (Some {| timeout = timeout |})
//                    Cy2.waitForWithinSelector "[data-testid=DatabaseSelector]" dbName (Some {| timeout = timeout |})
//                    Cy.wait 3000
//                    Cy.wait 3000

                    Cy2.selectorFocusTypeText "input[placeholder^=new-task-]" taskName

                    Cy2.clickText "Save"

                    Cy.wait 6000

                    Cy2.clickSelectorChildFromText dbName ".chakra-button"
                    Cy2.clickText "Edit Database"

                    Cy.wait 6000

                    Cy2.clickSelectorChildFromText dbName ".chakra-button"
                    Cy2.clickText "Edit Database"

                    Cy2.selectorFocusTypeText "input[placeholder^=new-database-]" $"{dbName}_edit"

                    //                    Cy.wait 200

                    Cy2.clickText "Save"

                    Cy2.waitFor $"{dbName}_edit" (Some {| timeout = timeout |})
                    Cy2.clickText $"{dbName}_edit"

                    Cy2.clickText (nameof Databases)

                    Cy2.waitFor "1 of 1 visible" (Some {| timeout = timeout |})
                    Cy2.waitFor taskName (Some {| timeout = timeout |})

                    Cy2.clickSelectorChildFromText taskName ".chakra-button"
                    Cy2.clickText "Start Session"
                    Cy2.waitFor $"Session: 1 active ({taskName})" (Some {| timeout = timeout |})

                    Cy2.clickText "Habit Tracker View"
                    Cy.wait 200
                    Cy2.waitFor "1 of 1 visible" (Some {| timeout = timeout |})

                    Cy2.clickSelectorChildFromText taskName ".chakra-button"
                    Cy2.clickText "Edit Task"

                    Cy2.clickSelectorChildFromText
                        (DateTime.Now
                         |> FlukeDate.FromDateTime
                         |> FlukeDate.Stringify)
                        ".chakra-button"

                    Cy2.clickText "Delete Session"

                    Cy2.waitFor "0 of 1 visible" (Some {| timeout = timeout |})

                    Cy2.clickText "Priority View"
                    Cy.wait 200

                    Cy2.waitFor "1 of 1 visible" (Some {| timeout = timeout |})

                    Cy2.clickTestId "[data-testid^='cell-']"

                    Cy2.clickTestId
                        $"[data-testid='cell-button-{TempUI.cellStatusColor (UserStatus (unbox null, Completed))}']"

                    Cy2.clickTestId "[data-testid^='cell-']"
                    Cy2.clickTestId $"[data-testid='cell-button-{(TempUI.cellStatusColor Pending)}']"

                    Cy2.selectorTypeText "textarea[placeholder='Add Attachment']" "newcomment" None

                    Cy2.clickTestId "[data-testid='Add Attachment']"

                    Cy2.waitFor "newcomment" (Some {| timeout = timeout |})

                    Cy2.clickText "Bullet Journal View"
                    Cy.wait 200

                    Cy2.waitFor "1 of 1 visible" (Some {| timeout = timeout |})

                    Cy2.clickTestId "[data-testid^='cell-']"
                    Cy2.clickTestId $"[data-testid='cell-button-{(TempUI.cellStatusColor Disabled)}']"

                    Cy2.waitFor "1 of 1 visible" (Some {| timeout = timeout |})

                    Cy.visit homeUrl))
