namespace Fluke.UI.Frontend.Tests

open Fluke.UI.Frontend.Bindings


module Full =
    open Cypress
    open Fluke.UI.Frontend.Components

    module Cy2 =
        let typeText<'T> (text: string) =
            Cy.wait 200
            Cy.focused().clear () |> ignore
            Cy.focused().should "be.empty" null null

            text
            |> Seq.iter
                (fun letter ->
                    Cy.wait 200
                    Cy.focused().``type`` (string letter) |> ignore)

            Cy.focused().should "have.value" text null

        let waitFocus selector wait =
            Cy.wait 50
            Cy.get(selector).should "have.focus" |> ignore

            match wait with
            | Some ms -> Cy.wait ms
            | None -> ()

        let clickTextWithinSelector selector text =
            Cy
                .get(selector)
                .contains(text)
                .click (Some {| force = true |})
            |> ignore

        let clickText text =
            (Cy.contains text None).click None |> ignore

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

                    Cy2.waitFocus "input[placeholder=Username]" None
                    Cy2.typeText username

                    Cy.get("input[placeholder=Password]").focus ()
                    Cy2.typeText password

                    Cy2.clickText "Login"
                    Cy2.waitFor "Wrong user or password" None

                    Cy.wait 250

                    Cy2.clickText "Register"

                    Cy
                        .get("input[placeholder='Confirm Password']")
                        .focus ()

                    Cy2.typeText password
                    Cy2.clickText "Confirm"
                    Cy2.waitFor "User registered successfully" None

                    Cy2.clickText (nameof Databases)
                    Cy2.waitFor "Lane Rendering" (Some {| timeout = timeout |})

                    Cy2.clickSelector "[data-testid='Add Database']"
                    Cy2.waitFocus "input[placeholder^=new-database-]" None
                    Cy2.typeText dbName
                    Cy2.clickText "Save"

                    Cy2.clickText dbName

                    (Cy.contains dbName None)
                        .find(".chakra-button")
                        .click None
                    |> ignore

                    Cy2.clickText "Add Task"

                    Cy2.waitFocus "input[placeholder^=new-task-]" (Some 250)
                    Cy2.typeText taskName

                    Cy2.clickTextWithinSelector "[data-testid='TextKey TaskForm']" "Select..."
                    Cy2.clickText "Project"
                    Cy2.clickText "Add Project"

                    Cy2.waitFocus "input[placeholder^='e.g.']" (Some 250)
                    Cy2.typeText "p1"

                    Cy2.clickTextWithinSelector "[data-testid='TextKey ProjectForm']" "Select..."
                    Cy2.clickText "Add Area"

                    Cy2.waitFocus "input[placeholder^='e.g.']" None
                    Cy2.typeText "a1"
                    Cy2.clickTextWithinSelector "[data-testid='TextKey AreaForm']" "Save"
                    Cy2.clickTextWithinSelector "[data-testid='TextKey ProjectForm']" "Save"
                    Cy2.clickTextWithinSelector "[data-testid='TextKey TaskForm']" "Save"

                    (Cy.contains dbName None)
                        .find(".chakra-button")
                        .click None
                    |> ignore

                    Cy2.clickText "Edit Database"

                    Cy2.waitFocus "input[placeholder^=new-database-]" None
                    Cy2.typeText $"{dbName}_edit"
                    Cy2.clickTextWithinSelector "[data-testid='TextKey DatabaseForm']" "Save"

                    Cy2.waitFor "1 of 1 tasks visible" None
                    Cy2.waitFor taskName None
                    Cy2.clickText "Habit Tracker View"
                    Cy2.waitFor "0 of 1 tasks visible" None
                    Cy2.clickText "Priority View"
                    Cy2.waitFor "1 of 1 tasks visible" None
                    Cy2.clickText "Bullet Journal View"
                    Cy2.waitFor "0 of 1 tasks visible" None

                    Cy.visit homeUrl))
