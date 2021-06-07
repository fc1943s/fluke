namespace Fluke.UI.Frontend.Tests

open Fluke.UI.Frontend.Bindings


module Full =
    open Cypress
    open Fluke.UI.Frontend.Components

    module Cy2 =
        let typeText (fn: unit -> Cy.Chainable2<_>) (text: string) =
            //            Cy.wait 200
            fn().clear {| force = true |} |> ignore
            fn().should "be.empty" null null

            text
            |> Seq.iter
                (fun letter ->
                    Cy.wait 250

                    fn().first().click (Some {| force = true |})
                    |> ignore

                    fn().first().``type`` (string letter) {| force = true |}
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

        let selectorFocusTypeTextWithinSelector parent selector text =
            Cy.get(parent).get(selector).first().focus ()
            typeText (fun () -> Cy.get(parent).get selector) text

        let clickTextWithinSelector selector text =
            Cy
                .get(selector)
                .contains(text)
                .click (Some {| force = true |})
            |> ignore

        let clickText text =
            (Cy.contains text None)
                .click (Some {| force = true |})
            |> ignore

        let clickSelector selector = (Cy.get selector).click None |> ignore

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

                    Cy2.expectLocation $"{homeUrl}/login"
                    Cy.get("body").should "have.css" "background-color" "rgb(33, 33, 33)"

                    Cy.focused().click None |> ignore

                    Cy2.selectorTypeText "input[placeholder=Username]" username None
                    Cy2.selectorFocusTypeText "input[placeholder=Password]" password
                    Cy2.clickText "Login"
                    Cy2.waitFor "Wrong user or password" None

                    //                    Cy.wait 250

                    Cy2.clickText "Register"
                    Cy2.selectorFocusTypeText "input[placeholder='Confirm Password']" password
                    Cy2.clickText "Confirm"
                    Cy2.waitFor "User registered successfully" None

                    Cy2.clickText (nameof Databases)
                    Cy2.waitFor "Lane Rendering" None

                    Cy.wait 200

                    Cy2.clickSelector "[data-testid='Add Database']"

                    Cy2.selectorTypeText "input[placeholder^=new-database-]" dbName None
                    Cy2.clickText "Save"

//                    Cy.wait 500

                    (Cy.contains dbName None)
                        .find(".chakra-button")
                        .click (Some {| force = true |})
                    |> ignore

                    //                    Cy.wait 400

                    Cy2.clickText "Add Task"

                    Cy2.clickTextWithinSelector "[data-testid='TextKey TaskForm']" "Select..."
                    Cy2.clickText "Project"
                    Cy2.clickText "Add Project"

                    Cy2.selectorFocusTypeTextWithinSelector
                        "[data-testid='TextKey ProjectForm']"
                        "input[placeholder^='e.g. home renovation']"
                        "p1"

                    Cy2.clickTextWithinSelector "[data-testid='TextKey ProjectForm']" "Select..."
                    Cy2.clickText "Add Area"

                    //                    Cy.wait 200

                    Cy2.selectorFocusTypeTextWithinSelector
                        "[data-testid='TextKey AreaForm']"
                        "input[placeholder^='e.g. chores']"
                        "a1"

                    Cy2.clickTextWithinSelector "[data-testid='TextKey AreaForm']" "Save"
                    //                    Cy.wait 200

                    Cy2.clickTextWithinSelector "[data-testid='TextKey ProjectForm']" "Save"

                    //                    Cy.wait 200

                    Cy2.selectorFocusTypeTextWithinSelector
                        "[data-testid='TextKey TaskForm']"
                        "input[placeholder^=new-task-]"
                        taskName

                    Cy2.clickTextWithinSelector "[data-testid='TextKey TaskForm']" "Save"

                    //                    Cy.wait 200

                    (Cy.contains dbName None)
                        .find(".chakra-button")
                        .click (Some {| force = true |})
                    |> ignore

                    Cy2.clickText "Edit Database"

                    Cy.wait 200

                    Cy2.selectorFocusTypeTextWithinSelector
                        "[data-testid='TextKey DatabaseForm']"
                        "input[placeholder^=new-database-]"
                        $"{dbName}_edit"

                    Cy2.clickTextWithinSelector "[data-testid='TextKey DatabaseForm']" "Save"

                    Cy2.clickText dbName

                    Cy2.waitFor "1 of 1 tasks visible" None
                    Cy2.waitFor taskName None
                    Cy2.clickText "Habit Tracker View"
                    Cy2.waitFor "0 of 1 tasks visible" (Some {| timeout = timeout |})
                    Cy2.clickText "Priority View"
                    Cy2.waitFor "1 of 1 tasks visible" (Some {| timeout = timeout |})
                    Cy2.clickText "Bullet Journal View"
                    Cy2.waitFor "0 of 1 tasks visible" (Some {| timeout = timeout |})

                    Cy.visit homeUrl))
