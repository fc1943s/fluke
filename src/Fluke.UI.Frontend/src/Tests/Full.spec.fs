namespace Fluke.UI.Frontend.Tests

open Fluke.UI.Frontend.Bindings


module Full =
    open Cypress
    open Fluke.UI.Frontend.Components

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

                    Cy
                        .location()
                        .should (fun location ->
                            expect(location.href)
                                .``to``.contain $"{homeUrl}/#/login")

                    Cy.get("body").should "have.css" "background-color" "rgb(33, 33, 33)"

                    Cy.focused().click () |> ignore
                    Cy.wait 250

                    (Cy.focused().``type`` username).should "have.value" username null

                    (Cy.get("input[type=password]").``type`` password)
                        .should
                        "have.value"
                        password
                        null

                    (Cy.contains "Sign In" None).click () |> ignore
                    Cy.contains "Wrong user or password" |> ignore
                    Cy.wait 250
                    (Cy.contains "Sign Up" None).click () |> ignore

                    Cy.contains "User registered successfully"
                    |> ignore

                    (Cy.contains (nameof Databases) None).click ()
                    |> ignore

                    (Cy.contains "Add Database" (Some {| timeout = timeout |}))
                        .click ()
                    |> ignore

                    Cy.wait 3000

                    ((Cy.focused().clear().``type`` dbName).should "have.value" dbName null)

                    (Cy.contains "Save" None).click () |> ignore

                    Cy.wait 1250

                    Cy
                        .get(
                            $"[data-testid={nameof Databases}]"
                        )
                        .scrollTo
                        "bottom"
                        {| ensureScrollable = false |}

                    (Cy.contains dbName None).click () |> ignore
                    Cy.wait 250
                    (Cy.contains "Add Task" None).click () |> ignore
                    Cy.wait 250

                    (Cy.focused().clear().``type`` taskName).should "have.value" taskName null

                    (Cy.contains "Save" None).click () |> ignore

                    ))
