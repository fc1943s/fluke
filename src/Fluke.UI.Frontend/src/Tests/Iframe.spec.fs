namespace Fluke.UI.Frontend.Tests

open Fable.Core.JsInterop
open System
open Fluke.Shared.Domain.UserInteraction
open Fluke.UI.Frontend.Bindings.Cypress
open FsCore.Model
open Fluke.UI.Frontend.State.State
open Fluke.UI.Frontend.Components


module Iframe =
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

                    Cy2.expectLocation $"{homeUrl}/"
                    Cy.get("body").should "have.css" "background-color" "rgb(222, 222, 222)"

                    Cy.focused().click None |> ignore

                    Cy.window ()
                    |> Promise.iter (fun window -> window?Debug <- false)

                    Cy.visit homeUrl))
