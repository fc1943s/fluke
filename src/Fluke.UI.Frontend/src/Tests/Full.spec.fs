namespace Fluke.UI.Frontend.Tests

open Fluke.UI.Frontend.Bindings


module Full =
    open Cypress
    open Fluke.UI.Frontend.Components

    let typeText<'T> (el: Cy.Chainable2<'T>) (text: string) =
        text
        |> Seq.iter
            (fun letter ->
                let letter = string letter
                el.``type`` letter |> ignore)

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

                    (**)
                    Cy
                        .location()
                        .should (fun location ->
                            expect(location.href)
                                .``to``.contain $"{homeUrl}/#/login")

                    Cy.get("body").should "have.css" "background-color" "rgb(33, 33, 33)"
                    (**)

                    (**)
                    Cy.focused().click () |> ignore

                    Cy
                        .get("input[placeholder=Username]")
                        .should "have.focus"
                    |> ignore

                    typeText (Cy.focused ()) username
                    Cy.focused().should "have.value" username null

                    Cy.get("input[placeholder=Password]").focus ()
                    typeText (Cy.focused ()) password
                    (Cy.focused ()).should "have.value" password null

                    (Cy.contains "Sign In" None).click () |> ignore

                    (Cy.contains "Wrong user or password" None)
                        .should "be.visible"
                    |> ignore

                    Cy.wait 250

                    (Cy.contains "Sign Up" None).click () |> ignore

                    (Cy.contains "User registered successfully" None)
                        .should "be.visible"
                    |> ignore
                    (**)

                    (**)

                    (Cy.contains "Add Database" (Some {| timeout = timeout |}))
                        .click ()
                    |> ignore


                    Cy
                        .get("input[placeholder^=new-database-]")
                        .should "have.focus"
                    |> ignore

                    typeText (Cy.focused ()) dbName
                    Cy.focused().should "have.value" dbName null
                    (Cy.contains "Save" None).click () |> ignore
                    (**)

                    (**)
                    (Cy.contains (nameof Databases) None).click ()
                    |> ignore

                    Cy
                        .get(
                            $"[data-testid={nameof Databases}]"
                        )
                        .scrollTo
                        "bottom"
                        {| ensureScrollable = false |}

                    (Cy.contains dbName None).click () |> ignore

                    Cy.wait 1000

                    (**)

                    (**)
                    (Cy.contains "Add Task" None).click () |> ignore

                    Cy
                        .get("input[placeholder^=new-task-]")
                        .should "have.focus"
                    |> ignore

                    typeText (Cy.focused ()) taskName

                    Cy.focused().should "have.value" taskName null

                    (Cy.contains "Save" None).click () |> ignore

                    (**)

                    (Cy.contains "0 of 1 tasks visible" None)
                        .should "be.visible"
                    |> ignore

                    (Cy.contains "Priority View" None).click ()
                    |> ignore

                    (Cy.contains "1 of 1 tasks visible" None)
                        .should "be.visible"
                    |> ignore

                    (Cy.contains "Bullet Journal View" None).click ()
                    |> ignore

                    (Cy.contains "0 of 1 tasks visible" None)
                        .should "be.visible"
                    |> ignore

                    (Cy.contains "Information View" None).click ()
                    |> ignore

                    (Cy.contains "1 of 1 tasks visible" None)
                        .should "be.visible"
                    |> ignore


                    ))
