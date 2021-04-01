namespace Fluke.UI.Frontend.Tests

open Fluke.UI.Frontend.Bindings


module Full =
    open Cypress

    describe
        "tests"
        (fun () ->
            let homeUrl = "https://localhost:33922"
            let timeout = 40000
            before (fun () -> Cy.visit homeUrl)

            it
                "login"
                (fun () ->
                    Cy
                        .location()
                        .should (fun location ->
                            expect(location.href)
                                .``to``.contain $"{homeUrl}/#/login")

                    Cy.get("body").should "have.css" "background-color" "rgb(33, 33, 33)"
                    Cy.focused().click ()
                    Cy.focused().``type`` "x"
                    Cy.get("input[type=password]").``type`` "x"
                    (Cy.contains "Sign In" None).click ()
                    Cy.contains "Wrong user or password" |> ignore
                    Cy.wait 300
                    (Cy.contains "Sign Up" None).click ()

                    Cy.contains "User registered successfully"
                    |> ignore

                    Cy.contains "Add Database" (Some {| timeout = timeout |})
                    |> ignore))
