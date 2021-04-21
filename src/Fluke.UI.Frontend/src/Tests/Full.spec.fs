namespace Fluke.UI.Frontend.Tests

open Fluke.UI.Frontend.Bindings


module Full =
    open Cypress
    open Fluke.UI.Frontend.Components

    let typeText<'T> (text: string) =
        text
        |> Seq.iter
            (fun letter ->
                let letter = string letter
                Cy.wait 10
                Cy.focused().``type`` letter |> ignore)

        Cy.focused().should "have.value" text null

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
                                .``to``.contain $"{homeUrl}/login")

                    Cy.get("body").should "have.css" "background-color" "rgb(33, 33, 33)"
                    (**)

                    (**)
                    Cy.focused().click () |> ignore

                    Cy
                        .get("input[placeholder=Username]")
                        .should "have.focus"
                    |> ignore

                    typeText username

                    Cy.get("input[placeholder=Password]").focus ()
                    typeText password

                    (Cy.contains "Login" None).click () |> ignore

                    (Cy.contains "Wrong user or password" None)
                        .should "be.visible"
                    |> ignore

                    Cy.wait 250

                    (Cy.contains "Register" None).click () |> ignore

                    (Cy.contains "User registered successfully" None)
                        .should "be.visible"
                    |> ignore
                    (**)

                    (**)
                    (Cy.contains (nameof Databases) None).click ()
                    |> ignore

                    Cy.wait 2000

                    (**)

                    (**)

                    (Cy.contains "Add Database" (Some {| timeout = timeout |}))
                        .click ()
                    |> ignore

                    Cy
                        .get("input[placeholder^=new-database-]")
                        .should "have.focus"
                    |> ignore

                    typeText dbName
                    (Cy.contains "Save" None).click () |> ignore
                    (**)


                    (**)
                    Cy
                        .get(
                            $"[data-testid={nameof Databases}]"
                        )
                        .scrollTo
                        "bottom"
                        {| ensureScrollable = false |}

                    (Cy.contains dbName None).click () |> ignore
                    (**)


                    (**)
                    (Cy.contains "Add Task" None).click () |> ignore

                    Cy
                        .get("input[placeholder^=new-task-]")
                        .should "have.focus"
                    |> ignore

                    typeText taskName

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

                    (Cy.contains taskName None).should "be.visible"
                    |> ignore

                    ))
